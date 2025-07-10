using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Assets.AT;
using Assets.IGC2025.Scripts.View;
using Game.Utils;
using R3;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.Presenter
{
    public class PresenterShopCanvas : MonoBehaviour
    {
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

        private CompositeDisposable _disposables = new CompositeDisposable();
        private CompositeDisposable _moduleLevelAndQuantityChangeDisposables = new CompositeDisposable();

        private int _currentSelectedModuleId = -1;

        private void Awake()
        {
            if (_shopView == null || _moduleDataStore == null || _playerCore == null)
            {
                Debug.LogError("依存関係が不足しています。", this);
                enabled = false;
                return;
            }

            if (_runtimeModuleManager == null)
                _runtimeModuleManager = RuntimeModuleManager.Instance;

            _runtimeModuleManager.OnAllRuntimeModuleDataChanged
                .Subscribe(_ =>
                {
                    _moduleLevelAndQuantityChangeDisposables.Clear();
                    foreach (var rmd in _runtimeModuleManager.AllRuntimeModuleData)
                    {
                        SubscribeToModuleChanges(rmd);
                    }
                    DisplayShopContent();
                })
                .AddTo(_disposables);

            PrepareAndShowShopUI();
        }

        private void Start()
        {
            _shopView.OnModulePurchaseRequested
                .Subscribe(moduleId => HandleModulePurchaseRequested(moduleId))
                .AddTo(_disposables);

            _shopView.OnModuleDetailRequested
                .Subscribe(id => ShowModuleDetailPanel(id))
                .AddTo(_disposables);

            _playerCore.Money
                .Subscribe(x => _moneyTextScaleAnimation.AnimateFloatAndText(x, 1f))
                .AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _moduleLevelAndQuantityChangeDisposables.Dispose();
        }

        private void SubscribeToModuleChanges(RuntimeModuleData runtimeModuleData)
        {
            if (runtimeModuleData.Level != null)
            {
                runtimeModuleData.Level
                    .Subscribe(level =>
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
            List<RuntimeModuleData> shopRuntimeModules = _runtimeModuleManager.AllRuntimeModuleData
                .Where(rmd => rmd != null && rmd.CurrentLevelValue > 0)
                .ToList();

            _shopView.DisplayShopModules(shopRuntimeModules, _moduleDataStore);
            UpdatePurchaseButtonsInteractability();
        }

        private void UpdatePurchaseButtonsInteractability()
        {
            if (_playerCore == null || _moduleDataStore?.DataBase?.ItemList == null)
                return;

            foreach (var runtimeData in _runtimeModuleManager.AllRuntimeModuleData
                .Where(rmd => rmd != null && rmd.CurrentLevelValue > 0))
            {
                ModuleData masterData = _moduleDataStore.FindWithId(runtimeData.Id);
                if (masterData == null) continue;

                bool canAfford = _playerCore.Money.CurrentValue >= masterData.BasePrice;
                _shopView.SetPurchaseButtonInteractable(runtimeData.Id, canAfford);
            }
        }

        private void ShowModuleDetailPanel(int moduleId)
        {
            var module = _moduleDataStore.FindWithId(moduleId);
            var runtime = _runtimeModuleManager.GetRuntimeModuleData(moduleId);

            if (module == null || runtime == null) return;

            int level = runtime.CurrentLevelValue;

            // 攻撃力のスケーリング
            float scaledAtk = StateValueCalculator.CalcStateValue(
                baseValue: module.ModuleState?.Attack ?? 0f,
                currentLevel: level,
                maxLevel: 5,
                maxRate: 0.5f // 最大+50%の成長
            );

            // 価格スケーリング（最大50%増）
            float scaledPrice = StateValueCalculator.CalcStateValue(
                baseValue: module.BasePrice,
                currentLevel: level,
                maxLevel: 5,
                maxRate: 0.5f
            );

            // UIに反映
            _unitName.text = module.ViewName;
            _infoText.text = module.Description;
            _level.text = $"{level}";
            _image.sprite = module.MainSprite;
            _icon.sprite = module.BlockSprite;

            _atk.text = $"{(int)scaledAtk}";
            _rpd.text = $"{Mathf.FloorToInt(module?.ModuleState?.Interval ?? 0)}";
            _rng.text = $"{Mathf.FloorToInt(module.ModuleState?.SearchRange ?? 0)}";
            _prc.text = $"{(int)scaledPrice}";

            _currentSelectedModuleId = moduleId;

            _confirmPurchaseButton.onClick.RemoveAllListeners();
            _confirmPurchaseButton.onClick.AddListener(() => HandleModulePurchaseRequested(moduleId));
        }


        private void HandleModulePurchaseRequested(int moduleId)
        {
            ModuleData masterData = _moduleDataStore.FindWithId(moduleId);
            if (masterData == null) return;

            RuntimeModuleData runtimeModule = _runtimeModuleManager.GetRuntimeModuleData(moduleId);
            if (runtimeModule == null || runtimeModule.CurrentLevelValue == 0) return;

            float CalculatePrice(float maxIncreaseRate, int maxLevel = 5)
            {
                int currentLevel = runtimeModule.CurrentLevelValue;

                if (currentLevel <= 1) return masterData.BasePrice;
                if (currentLevel >= maxLevel) return masterData.BasePrice * (1f + maxIncreaseRate);

                float progress = (currentLevel - 1f) / (maxLevel - 1f);
                return masterData.BasePrice * (1f + maxIncreaseRate * progress);
            }

            var payPrice = CalculatePrice(0.5f);
            if (_playerCore.Money.CurrentValue >= payPrice)
            {
                _playerCore.PayMoney((int)payPrice);
                _runtimeModuleManager.ChangeModuleQuantity(moduleId, 1);
                UpdatePurchaseButtonsInteractability();
            }
        }

        public void inShopSE()
        {
            GameSoundManager.Instance.PlaySE("Sys_menu_in", "System");
        }

        public void outShopSE()
        {
            GameSoundManager.Instance.PlaySE("Sys_menu_out", "System");
        }
    }
}
