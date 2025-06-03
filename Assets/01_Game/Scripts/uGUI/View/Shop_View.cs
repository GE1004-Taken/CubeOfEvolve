using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using R3;
using R3.Triggers;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MVRP.AT.View
{
    /// <summary>
    /// ショップ画面のビューを担当するクラス。
    /// モジュールリストの表示、UIの表示・非表示、購入ボタンクリックイベントの通知を行います。
    /// </summary>
    public class Shop_View : MonoBehaviour
    {
        // ----- SerializedField
        [SerializeField] private GameObject _moduleItemPrefab; // 各モジュール表示用のプレハブ (Detailed_ViewとButtonを含む)。
        [SerializeField] private Transform _contentParent; // モジュールリストの親Transform。
        [SerializeField] private ModuleDataStore _moduleDataStore; // マスターデータを取得するために必要。

        // ----- Events (PresenterがR3で購読する)
        public Subject<int> OnModulePurchaseRequested { get; private set; } = new Subject<int>(); // モジュール購入リクエストを通知するSubject。
        public Subject<int> OnModuleHovered { get; private set; } = new Subject<int>(); // モジュール購入リクエストを通知するSubject。

        // ----- Private Members (内部データ)
        private List<GameObject> _instantiatedModuleItems = new List<GameObject>(); // 生成されたモジュールアイテムのリスト。
        private Dictionary<int, Button> _purchaseButtons = new Dictionary<int, Button>(); // モジュールIDと購入ボタンのマッピング。
        private CompositeDisposable _disposables = new CompositeDisposable(); // R3購読管理用。

        // ----- UnityMessage
        
        private void Awake()
        {
            if (_moduleDataStore == null)
            {
                Debug.LogError("Shop_View: ModuleDataStoreがInspectorで設定されていません！モジュールの詳細を表示できません。", this);
                enabled = false;
            }
        }

        private void OnDestroy()
        {
            _disposables.Dispose(); // オブジェクト破棄時に全ての購読を解除。
        }

        // ----- Public Methods (Presenterから呼び出される)

        /// <summary>
        /// ショップに表示するモジュールリストを設定し、UIを更新します。
        /// レベル1以上のモジュールのみ、実際のランタイムデータに基づいて表示されます。
        /// </summary>
        /// <param name="shopRuntimeModules">ショップに表示するRuntimeModuleDataのリスト。</param>
        public void DisplayShopModules(List<RuntimeModuleData> shopRuntimeModules)
        {
            // 既存のモジュールアイテムを全てクリア
            foreach (var item in _instantiatedModuleItems)
            {
                Destroy(item);
            }
            _instantiatedModuleItems.Clear();
            _purchaseButtons.Clear();
            _disposables.Clear(); // 新しいボタン購読のために既存の購読をクリア。

            // 各モジュールデータを基にUI要素を生成・設定
            foreach (var runtimeData in shopRuntimeModules)
            {
                if (runtimeData == null)
                {
                    Debug.LogWarning("Shop_View: ショップのランタイムモジュールリストにnullデータが提供されました。スキップします。", this);
                    continue;
                }

                // 対応するマスターデータを取得
                ModuleData masterData = _moduleDataStore.FindWithId(runtimeData.Id);
                if (masterData == null)
                {
                    Debug.LogError($"Shop_View: ModuleDataStoreにランタイムモジュールID {runtimeData.Id} のマスターデータが見つかりません。モジュールを表示できません。", this);
                    continue;
                }

                GameObject moduleItem = Instantiate(_moduleItemPrefab, _contentParent);
                _instantiatedModuleItems.Add(moduleItem);

                Detailed_View detailedView = moduleItem.GetComponent<Detailed_View>();
                Button purchaseButton = moduleItem.GetComponentInChildren<Button>(); // 子要素からボタンを探す。

                if (detailedView == null)
                {
                    Debug.LogError($"Shop_View: _moduleItemPrefabにDetailed_Viewコンポーネントがありません。モジュールID: {masterData.Id}", moduleItem);
                    continue;
                }
                if (purchaseButton == null)
                {
                    Debug.LogError($"Shop_View: _moduleItemPrefabの子要素にButtonコンポーネントがありません。モジュールID: {masterData.Id}", moduleItem);
                    continue;
                }

                // 実際のRuntimeModuleDataをDetailed_Viewに渡す
                detailedView.SetInfo(masterData, runtimeData);

                // 購入ボタンにイベントを登録
                _purchaseButtons.Add(masterData.Id, purchaseButton);
                int moduleId = masterData.Id; // クロージャのためにコピー。
                purchaseButton.OnClickAsObservable()
                    .Subscribe(_ => OnModulePurchaseButtonClicked(moduleId))
                    .AddTo(_disposables); // _disposables に追加。
                // OnEnter
                purchaseButton.OnPointerEnterAsObservable()
                    .Subscribe(_ => OnShopItemHovered(moduleId))
                    .AddTo(_disposables);

                // ボタンのテキストを設定 (例: "購入 - 100G")
                TextMeshProUGUI buttonText = purchaseButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = $"購入 - {masterData.BasePrice}G";
                }
            }
        }

        /// <summary>
        /// 特定のモジュールの購入ボタンの有効/無効を切り替えます。
        /// </summary>
        /// <param name="moduleId">対象のモジュールID。</param>
        /// <param name="isInteractable">ボタンを操作可能にするか。</param>
        public void SetPurchaseButtonInteractable(int moduleId, bool isInteractable)
        {
            if (_purchaseButtons.TryGetValue(moduleId, out Button button))
            {
                button.interactable = isInteractable;
            }
        }

        // ----- Private Methods (UIイベントハンドラ)

        /// <summary>
        /// モジュール購入ボタンがクリックされたときに呼び出されるハンドラです。
        /// </summary>
        /// <param name="moduleId">購入がリクエストされたモジュールのID。</param>
        private void OnModulePurchaseButtonClicked(int moduleId)
        {
            OnModulePurchaseRequested.OnNext(moduleId); // Presenterに通知。
        }

        /// <summary>
        /// モジュールにカーソルを重ねた際のハンドラ。
        /// </summary>
        /// <param name="moduleId"></param>
        private void OnShopItemHovered(int moduleId)
        {
            OnModuleHovered.OnNext(moduleId); // 選択されたモジュールIDをイベントとして発火。
        }
    }
}