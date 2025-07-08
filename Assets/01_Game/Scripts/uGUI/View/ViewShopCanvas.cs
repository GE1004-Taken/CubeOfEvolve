using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Assets.AT;
using R3;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.View
{
    /// <summary>
    /// ショップ画面のビューを担当するクラス。
    /// モジュールリストの表示、UIの表示・非表示、購入詳細表示を行います。
    /// </summary>
    public class ViewShopCanvas : MonoBehaviour
    {
        [SerializeField] private GameObject _moduleItemPrefab; // 各モジュール表示用のプレハブ。
        [SerializeField] private Transform _contentParent; // モジュールリストの親Transform。

        public Subject<int> OnModuleDetailRequested { get; private set; } = new Subject<int>();
        public Subject<int> OnModulePurchaseRequested { get; private set; } = new Subject<int>();

        private List<GameObject> _instantiatedModuleItems = new List<GameObject>();
        private Dictionary<int, Button> _purchaseButtons = new Dictionary<int, Button>();
        private CompositeDisposable _disposables = new CompositeDisposable();

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        /// <summary>
        /// ショップに表示するモジュールリストを設定し、UIを更新します。
        /// </summary>
        public void DisplayShopModules(List<RuntimeModuleData> shopRuntimeModules, ModuleDataStore moduleDataStore)
        {
            foreach (var item in _instantiatedModuleItems)
            {
                Destroy(item);
            }
            _instantiatedModuleItems.Clear();
            _purchaseButtons.Clear();
            _disposables.Clear();

            foreach (var runtimeData in shopRuntimeModules)
            {
                if (runtimeData == null)
                {
                    Debug.LogWarning("Shop_View: nullデータが提供されました。", this);
                    continue;
                }

                ModuleData masterData = moduleDataStore.FindWithId(runtimeData.Id);
                if (masterData == null)
                {
                    Debug.LogError($"Shop_View: ID {runtimeData.Id} のマスターデータが見つかりません。", this);
                    continue;
                }

                GameObject moduleItem = Instantiate(_moduleItemPrefab, _contentParent);
                _instantiatedModuleItems.Add(moduleItem);

                ViewInfo detailedView = moduleItem.GetComponent<ViewInfo>();
                Button purchaseButton = moduleItem.GetComponentInChildren<Button>();

                if (detailedView == null || purchaseButton == null)
                {
                    Debug.LogError($"Shop_View: プレハブのコンポーネントが不足しています。ID: {masterData.Id}", moduleItem);
                    continue;
                }

                detailedView.SetInfo(masterData, runtimeData);
                _purchaseButtons.Add(masterData.Id, purchaseButton);

                int moduleId = masterData.Id;
                purchaseButton.OnClickAsObservable()
                    .Subscribe(_ => OnModuleDetailRequested.OnNext(moduleId))
                    .AddTo(_disposables);
            }
        }

        /// <summary>
        /// 特定のモジュールの購入ボタンの有効/無効を切り替えます。
        /// </summary>
        public void SetPurchaseButtonInteractable(int moduleId, bool isInteractable)
        {
            if (_purchaseButtons.TryGetValue(moduleId, out Button button))
            {
                button.interactable = isInteractable;
            }
        }

        /// <summary>
        /// 詳細UIの購入ボタンから呼ばれるメソッド。
        /// </summary>
        public void RequestPurchase(int moduleId)
        {
            GameSoundManager.Instance.PlaySE("shop_buy1", "SE");
            GameSoundManager.Instance.PlaySE("shop_buy2", "SE");
            OnModulePurchaseRequested.OnNext(moduleId);
        }
    }
}
