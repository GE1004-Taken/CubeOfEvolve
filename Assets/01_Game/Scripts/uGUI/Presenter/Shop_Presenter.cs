// App.GameSystem.Presenters/Shop_Presenter.cs
using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using App.GameSystem.UI;
using R3; // R3のusingディレクティブ
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace App.GameSystem.Presenters
{
    /// <summary>
    /// ショップ画面のプレゼンターを担当するクラス。
    /// ViewからのイベントをR3で購読し、Model（RuntimeModuleManager, PlayerCore）を操作し、
    /// Viewに表示データを渡す。
    /// また、RuntimeModuleのレベル変更を監視し、ショップ表示を更新する。
    /// </summary>
    public class Shop_Presenter : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private Shop_View _shopView;
        [SerializeField] private ModuleDataStore _moduleDataStore;
        [SerializeField] private RuntimeModuleManager _runtimeModuleManager;
        [SerializeField] private PlayerCore _playerCore;

        private CompositeDisposable _disposables = new CompositeDisposable();
        // 各モジュールのレベル・数量変更購読用。コレクション変更時にクリアするためCompositeDisposableを使用
        private CompositeDisposable _moduleLevelAndQuantityChangeDisposables = new CompositeDisposable();

        void Awake()
        {
            // 依存関係の取得とチェックはAwakeの早い段階で行う
            if (_shopView == null) _shopView = FindObjectOfType<Shop_View>();
            if (_moduleDataStore == null) Debug.LogError("Shop_Presenter: ModuleDataStore is not assigned in Inspector!", this);
            if (_runtimeModuleManager == null) _runtimeModuleManager = RuntimeModuleManager.Instance;
            if (_playerCore == null) _playerCore = FindObjectOfType<PlayerCore>();

            // 各依存関係が揃っているか最終チェック
            if (_shopView == null || _moduleDataStore == null || _runtimeModuleManager == null || _playerCore == null)
            {
                Debug.LogError("Shop_Presenter: One or more dependencies are missing. Please check Inspector assignments and scene setup. Disabling this component.", this);
                enabled = false;
                return;
            }

            // Viewからのモジュール購入リクエストを購読
            _shopView.OnModulePurchaseRequested
                .Subscribe(moduleId => HandleModulePurchaseRequested(moduleId))
                .AddTo(_disposables);

            // PlayerCore の Money が変更されたらUIを更新する購読
            if (_playerCore.Money != null)
            {
                _playerCore.Money.Subscribe(money => UpdateShopUIOnCoinChange(money)).AddTo(_disposables);
            }
            else
            {
                Debug.LogError("Shop_Presenter: PlayerCore.Money ReactiveProperty is null. Cannot subscribe to money changes.", this);
            }

            // RuntimeModuleManagerが管理するモジュールコレクション全体の変更を監視し、ショップUIを更新する
            // モジュールの追加、削除、または既存モジュールのレベル・数量変更がRuntimeModuleManagerから通知された場合に発火
            _runtimeModuleManager.OnAllRuntimeModuleDataChanged
                .Subscribe(_ => {
                    Debug.Log("RuntimeModuleData collection changed. Re-subscribing to module changes and updating shop UI.");
                    // 既存のモジュールレベル・数量変更購読を全て解除
                    _moduleLevelAndQuantityChangeDisposables.Clear();

                    // 現在の全てのモジュールに対してレベル・数量変更を購読
                    foreach (var rmd in _runtimeModuleManager.AllRuntimeModuleData)
                    {
                        SubscribeToModuleChanges(rmd);
                    }
                    PrepareAndShowShopUI(); // ショップを再表示してリストを更新
                })
                .AddTo(_disposables);

            // 初期表示のために、コレクションが初期化された後に一度PrepareAndShowShopUIを呼び出す
            // OnAllRuntimeModuleDataChangedがAwake後に発火するため、この行は不要な場合もありますが、
            // 確実に初期表示を行うために残しておきます。
            PrepareAndShowShopUI();
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _moduleLevelAndQuantityChangeDisposables.Dispose(); // 各モジュールのレベル・数量変更購読も解除
        }

        /// <summary>
        /// 各RuntimeModuleDataのレベルと数量変更を購読するヘルパーメソッド
        /// </summary>
        /// <param name="runtimeModuleData"></param>
        private void SubscribeToModuleChanges(RuntimeModuleData runtimeModuleData)
        {
            // Levelの変更を購読
            if (runtimeModuleData.Level != null)
            {
                runtimeModuleData.Level
                    .Subscribe(level => {
                        Debug.Log($"Module {runtimeModuleData.Id} ({_moduleDataStore.FindWithId(runtimeModuleData.Id)?.ViewName}) level changed to {level}. Updating shop UI.");
                        PrepareAndShowShopUI(); // レベルが変更されたらショップを再表示
                    })
                    .AddTo(_moduleLevelAndQuantityChangeDisposables); // 個別モジュールの購読は専用のDisposableBagに追加
            }
            else
            {
                Debug.LogWarning($"RuntimeModuleData ID {runtimeModuleData.Id} does not expose its Level as a ReactiveProperty.", this);
            }

            // Quantityの変更も購読 (もし数量の変化でボタンの状態を変えたい場合)
            if (runtimeModuleData.Quantity != null)
            {
                runtimeModuleData.Quantity
                    .Subscribe(quantity => {
                        Debug.Log($"Module {runtimeModuleData.Id} ({_moduleDataStore.FindWithId(runtimeModuleData.Id)?.ViewName}) quantity changed to {quantity}. Updating purchase button interactability.");
                        // 数量変更だけならショップリスト全体ではなく、ボタンのインタラクト性のみ更新
                        UpdatePurchaseButtonsInteractability();
                    })
                    .AddTo(_moduleLevelAndQuantityChangeDisposables); // 個別モジュールの購読は専用のDisposableBagに追加
            }
            else
            {
                Debug.LogWarning($"RuntimeModuleData ID {runtimeModuleData.Id} does not expose its Quantity as a ReactiveProperty.", this);
            }
        }

        /// <summary>
        /// ショップ画面を表示する準備をし、Viewに表示を依頼します。
        /// このメソッドは外部から呼び出されます（例: GameManagerやUIController）。
        /// また、RuntimeModuleDataの変更によっても自動的に呼び出されることがあります。
        /// </summary>
        public void PrepareAndShowShopUI()
        {
            if (_shopView == null || _moduleDataStore == null || _runtimeModuleManager == null || _playerCore == null)
            {
                Debug.LogError("Shop_Presenter dependencies not met! Cannot prepare shop UI. Check Awake logs.", this);
                return;
            }

            // ★変更点: レベル1以上のモジュールのみをViewに渡す★
            // AllRuntimeModuleDataはIReadOnlyList<RuntimeModuleData>型になっている
            List<RuntimeModuleData> shopRuntimeModules = _runtimeModuleManager.AllRuntimeModuleData
                .Where(rmd => rmd != null && rmd.CurrentLevelValue > 0) // CurrentLevelValueを使用
                .ToList();

            _shopView.DisplayShopModules(shopRuntimeModules);
            _shopView.UpdatePlayerCoins(_playerCore.Money.CurrentValue);
            UpdatePurchaseButtonsInteractability();
        }

        /// <summary>
        /// プレイヤーのコイン変更時にショップUIを更新する。
        /// </summary>
        /// <param name="newCoins">新しいコイン量。</param>
        private void UpdateShopUIOnCoinChange(int newCoins)
        {
            if (_shopView == null) return;
            _shopView.UpdatePlayerCoins(newCoins);
            UpdatePurchaseButtonsInteractability(); // コイン量に応じてボタンの有効/無効を切り替える
        }

        /// <summary>
        /// 各モジュールの購入ボタンのインタラクト可能状態を更新します。
        /// </summary>
        private void UpdatePurchaseButtonsInteractability()
        {
            if (_playerCore == null || _moduleDataStore == null || _moduleDataStore.DataBase == null || _moduleDataStore.DataBase.ItemList == null)
            {
                Debug.LogError("Shop_Presenter: Required data for updating purchase button interactability is missing.", this);
                return;
            }

            // ショップに表示されているすべてのモジュール（レベル1以上のもの）についてチェック
            foreach (var runtimeData in _runtimeModuleManager.AllRuntimeModuleData
                                                            .Where(rmd => rmd != null && rmd.CurrentLevelValue > 0))
            {
                ModuleData masterData = _moduleDataStore.FindWithId(runtimeData.Id);
                if (masterData == null) continue;

                bool canAfford = _playerCore.Money.CurrentValue >= masterData.BasePrice;

                // レベルが1以上でショップに表示されているモジュールは、所持金が足りれば購入可能
                // 複数回購入できるため、常にインタラクト可能とする（所持金が足りる限り）。
                _shopView.SetPurchaseButtonInteractable(runtimeData.Id, canAfford);
            }
        }

        /// <summary>
        /// モジュール購入リクエストを受け取った際のハンドラ。
        /// </summary>
        /// <param name="moduleId">購入がリクエストされたモジュールのID。</param>
        private void HandleModulePurchaseRequested(int moduleId)
        {
            ModuleData masterData = _moduleDataStore.FindWithId(moduleId);
            if (masterData == null)
            {
                Debug.LogError($"Shop_Presenter: Master data for module ID {moduleId} not found. Cannot process purchase.", this);
                return;
            }

            RuntimeModuleData runtimeModule = _runtimeModuleManager.GetRuntimeModuleData(moduleId);
            if (runtimeModule == null)
            {
                Debug.LogError($"Shop_Presenter: Runtime data for module ID {moduleId} not found. This should not happen if modules are initialized to all players.", this);
                return;
            }

            // レベルが0のモジュールは購入できない
            if (runtimeModule.CurrentLevelValue == 0) // CurrentLevelValueを使用
            {
                Debug.LogWarning($"Shop_Presenter: Module ID {moduleId} ({masterData.ViewName}) is at level 0. Cannot purchase from shop. Please upgrade it first (e.g., via drops).", this);
                return;
            }

            // 購入可能か判定（所持金が足りるか）
            if (_playerCore.Money.CurrentValue >= masterData.BasePrice)
            {
                _playerCore.PayMoney(masterData.BasePrice);
                Debug.Log($"Shop_Presenter: Player purchased module ID {moduleId} ({masterData.ViewName}) for {masterData.BasePrice} coins. Remaining coins: {_playerCore.Money.CurrentValue}.", this);

                // モジュールをプレイヤーのランタイムモジュールに追加（数量を1増やす）
                _runtimeModuleManager.ChangeModuleQuantity(moduleId, 1);

                // 購入成功のフィードバック (UI更新など)
                UpdatePurchaseButtonsInteractability();
            }
            else
            {
                Debug.Log($"Shop_Presenter: Not enough coins to purchase module ID {moduleId} ({masterData.ViewName}). Required: {masterData.BasePrice}, Have: {_playerCore.Money.CurrentValue}.", this);
            }
        }
    }
}