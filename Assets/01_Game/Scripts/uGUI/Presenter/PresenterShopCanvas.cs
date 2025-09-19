using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Assets.AT;
using Assets.IGC2025.Scripts.View;
using Game.Utils;
using R3;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.Presenter
{
    public class PresenterShopCanvas : MonoBehaviour, IPresenter
    {
        // -----SerializeField
        [Header("Models")]
        [SerializeField] private ViewShopCanvas _shopView;
        [SerializeField] private ModuleDataStore _moduleDataStore;
        [SerializeField] private RuntimeModuleManager _runtimeModuleManager;
        [SerializeField] private PlayerCore _playerCore;

        [Header("Views")]
        [SerializeField] private TextScaleAnimation _moneyTextScaleAnimation;

        [Header("Views_Hovered")]
        [SerializeField] private TextMeshProUGUI _unitName;
        [SerializeField] private TextMeshProUGUI _infoText;
        [SerializeField] private TextMeshProUGUI _level;
        [SerializeField] private Image _image;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _atk;
        [SerializeField] private TextMeshProUGUI _rpd;
        [SerializeField] private TextMeshProUGUI _rng;
        [SerializeField] private TextMeshProUGUI _prc;
        [SerializeField] private Button _confirmPurchaseButton;

        // -----購入演出
        [Header("Purchase Feedback")]
        [SerializeField] private string _purchaseSuccessSE = "Shop_buy_ok";
        [SerializeField] private string _purchaseFailedSE = "Shop_buy_ng";
        [SerializeField, Range(1f, 1.5f)] private float _buttonPulseScale = 1.08f;
        [SerializeField, Range(0.03f, 0.3f)] private float _buttonPulseDuration = 0.08f;
        [SerializeField] private GameObject _cannotAffordPanel;

        // -----Field
        private CompositeDisposable _disposables = new CompositeDisposable();
        private CompositeDisposable _moduleLevelAndQuantityChangeDisposables = new CompositeDisposable();

        private int _currentSelectedModuleId = -1;
        public bool IsInitialized { get; private set; } = false;

        // -----UnityMessage
        private void OnDestroy()
        {
            _disposables.Dispose();
            _moduleLevelAndQuantityChangeDisposables.Dispose();
        }

        // -----PublicMethod
        public void Initialize()
        {
            if (IsInitialized) return;

            if (_runtimeModuleManager == null)
                _runtimeModuleManager = RuntimeModuleManager.Instance;

            if (_shopView == null || _moduleDataStore == null || _playerCore == null || _runtimeModuleManager == null)
            {
                Debug.LogError($"{nameof(PresenterShopCanvas)}: 依存が不足しています。", this);
                enabled = false;
                return;
            }

            _runtimeModuleManager.OnAllRuntimeModuleDataChanged
                .Subscribe(_ =>
                {
                    _moduleLevelAndQuantityChangeDisposables.Clear();
                    if (_runtimeModuleManager.AllRuntimeModuleData != null)
                    {
                        foreach (var rmd in _runtimeModuleManager.AllRuntimeModuleData)
                            SubscribeToModuleChanges(rmd);
                    }
                    DisplayShopContent();
                    // 所持金によるボタン無効化はやめる（選択は常に可能）
                })
                .AddTo(_disposables);

            _shopView.OnModulePurchaseRequested
                .Subscribe(moduleId => HandleModulePurchaseRequested(moduleId))
                .AddTo(_disposables);

            _shopView.OnModuleDetailRequested
                .Subscribe(id => ShowModuleDetailPanel(id))
                .AddTo(_disposables);

            _playerCore.Money
                .Subscribe(x =>
                {
                    if (_moneyTextScaleAnimation != null)
                        _moneyTextScaleAnimation.AnimateFloatAndText(x, 1f);
                    if (_currentSelectedModuleId >= 0)
                        UpdateAffordUIFor(_currentSelectedModuleId);
                })
                .AddTo(_disposables);

            PrepareAndShowShopUI();

            IsInitialized = true;
        }


        // -----PrivateMethod
        private void SubscribeToModuleChanges(RuntimeModuleData runtimeModuleData)
        {
            if (runtimeModuleData?.Level != null)
            {
                runtimeModuleData.Level
                    .Subscribe(_ =>
                    {
                        PrepareAndShowShopUI();
                    })
                    .AddTo(_moduleLevelAndQuantityChangeDisposables);
            }
        }

        private void PrepareAndShowShopUI()
        {
            if (_shopView == null || _moduleDataStore == null || _runtimeModuleManager == null || _playerCore == null)
                return;

            DisplayShopContent();
        }

        private void DisplayShopContent()
        {
            var list = _runtimeModuleManager.AllRuntimeModuleData?
                .Where(rmd => rmd != null && rmd.CurrentLevelValue > 0)
                .ToList() ?? new List<RuntimeModuleData>();

            _shopView.DisplayShopModules(list, _moduleDataStore);
        }

        private void ShowModuleDetailPanel(int moduleId)
        {
            var module = _moduleDataStore.FindWithId(moduleId);
            var runtime = _runtimeModuleManager.GetRuntimeModuleData(moduleId);
            if (module == null || runtime == null) return;

            int level = runtime.CurrentLevelValue;

            float scaledAtk = StateValueCalculator.CalcStateValue(
                baseValue: module.ModuleState?.Attack ?? 0f,
                currentLevel: level, maxLevel: 5, maxRate: 0.5f);

            float scaledPrice = StateValueCalculator.CalcStateValue(
                baseValue: module.BasePrice,
                currentLevel: level, maxLevel: 5, maxRate: 0.5f);

            if (_unitName != null) _unitName.text = module.ViewName;
            if (_infoText != null) _infoText.text = module.Description;
            if (_level != null) _level.text = $"{level}";
            if (_image != null) _image.sprite = module.MainSprite;
            if (_icon != null) _icon.sprite = module.BlockSprite;
            if (_atk != null) _atk.text = $"{(int)scaledAtk}";
            if (_rpd != null) _rpd.text = $"{Mathf.FloorToInt(module?.ModuleState?.Interval ?? 0)}";
            if (_rng != null) _rng.text = $"{Mathf.FloorToInt(module?.ModuleState?.SearchRange ?? 0)}";
            if (_prc != null) _prc.text = $"{(int)scaledPrice}";

            _currentSelectedModuleId = moduleId;

            UpdateAffordUIFor(moduleId);

            if (_confirmPurchaseButton != null)
            {
                _confirmPurchaseButton.onClick.RemoveAllListeners();
                _confirmPurchaseButton.onClick.AddListener(() => HandleModulePurchaseRequested(moduleId));
            }
        }

        private void HandleModulePurchaseRequested(int moduleId)
        {
            var masterData = _moduleDataStore.FindWithId(moduleId);
            if (masterData == null) return;

            var runtimeModule = _runtimeModuleManager.GetRuntimeModuleData(moduleId);
            if (runtimeModule == null || runtimeModule.CurrentLevelValue == 0) return;

            var payPrice = StateValueCalculator.CalcStateValue(
                baseValue: masterData.BasePrice,
                currentLevel: runtimeModule.Level.CurrentValue,
                maxLevel: 5, maxRate: 0.5f);

            bool canPay = _playerCore.Money.CurrentValue >= payPrice;

            if (canPay)
            {
                _playerCore.PayMoney((int)payPrice);
                _runtimeModuleManager.ChangeModuleQuantity(moduleId, 1);

                // 成功演出
                if (!string.IsNullOrEmpty(_purchaseSuccessSE))
                    GameSoundManager.Instance.PlaySE(_purchaseSuccessSE, "System");
                if (_confirmPurchaseButton != null)
                    StartCoroutine(PulseButton(_confirmPurchaseButton.transform));
            }
            else
            {
                // 失敗演出
                if (!string.IsNullOrEmpty(_purchaseFailedSE))
                    GameSoundManager.Instance.PlaySE(_purchaseFailedSE, "System");
                if (_confirmPurchaseButton != null)
                    StartCoroutine(PulseButton(_confirmPurchaseButton.transform));
            }
            UpdateAffordUIFor(moduleId);
        }

        // シンプルなパルスアニメ（外部依存なし）
        private IEnumerator PulseButton(Transform t)
        {
            if (t == null) yield break;
            var original = t.localScale;
            var up = original * _buttonPulseScale;

            float t1 = 0f;
            while (t1 < _buttonPulseDuration)
            {
                t1 += Time.unscaledDeltaTime;
                float r = Mathf.Clamp01(t1 / _buttonPulseDuration);
                t.localScale = Vector3.Lerp(original, up, r);
                yield return null;
            }

            float t2 = 0f;
            while (t2 < _buttonPulseDuration)
            {
                t2 += Time.unscaledDeltaTime;
                float r = Mathf.Clamp01(t2 / _buttonPulseDuration);
                t.localScale = Vector3.Lerp(up, original, r);
                yield return null;
            }

            t.localScale = original;
        }

        private void UpdateAffordUIFor(int moduleId)
        {
            if (moduleId < 0) return;
            var module = _moduleDataStore.FindWithId(moduleId);
            var runtime = _runtimeModuleManager.GetRuntimeModuleData(moduleId);
            if (module == null || runtime == null) return;

            int level = runtime.CurrentLevelValue;
            float price = StateValueCalculator.CalcStateValue(
                baseValue: module.BasePrice,
                currentLevel: level, maxLevel: 5, maxRate: 0.5f);

            int money = _playerCore.Money.CurrentValue;
            bool canAfford = money >= price;

            if (_cannotAffordPanel != null)
                _cannotAffordPanel.SetActive(!canAfford);

        }

    }
}
