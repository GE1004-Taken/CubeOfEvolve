using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using MVRP.AT.View;
using R3;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace MVRP.AT.Presenter
{
    /// <summary>
    /// ショップ画面のプレゼンターを担当するクラス。
    /// ViewからのイベントをR3で購読し、Model（RuntimeModuleManager, PlayerCore）を操作し、
    /// Viewに表示データを渡します。また、RuntimeModuleのレベル変更を監視し、ショップ表示を更新します。
    /// </summary>
    public class Shop_Presenter : MonoBehaviour
    {
        // ----- SerializedField

        // Models
        [SerializeField] private Shop_View _shopView; // ショップUIを表示するViewコンポーネント。
        [SerializeField] private ModuleDataStore _moduleDataStore; // モジュールマスターデータを管理するデータストア。
        [SerializeField] private RuntimeModuleManager _runtimeModuleManager; // ランタイムモジュールデータを管理するマネージャー。
        [SerializeField] private PlayerCore _playerCore; // プレイヤーのコアデータ（所持金など）を管理するコンポーネント。

        // Views
        [SerializeField] private TextScaleAnimation _moneyTextScaleAnimation;
        [SerializeField] private TextMeshProUGUI _hoveredModuleInfoText;

        // ----- Private Members (内部データ)
        private CompositeDisposable _disposables = new CompositeDisposable(); // 全体の購読解除を管理するCompositeDisposable。
        private CompositeDisposable _moduleLevelAndQuantityChangeDisposables = new CompositeDisposable(); // 各モジュールのレベル・数量変更購読を管理するCompositeDisposable。

        // ----- UnityMessage
        /// <summary>
        /// Awakeはスクリプトインスタンスがロードされたときに呼び出されます。
        /// 依存関係の取得と初期設定を行います。
        /// </summary>
        void Awake()
        {
            // 依存関係の取得とチェック
            if (_shopView == null) Debug.LogError("Shop_Presenter: ShopViewがInspectorで設定されていません！", this);
            if (_moduleDataStore == null) Debug.LogError("Shop_Presenter: ModuleDataStoreがInspectorで設定されていません！", this);
            if (_runtimeModuleManager == null) _runtimeModuleManager = RuntimeModuleManager.Instance;
            if (_playerCore == null) Debug.LogError("Shop_Presenter: PlayerCoreがInspectorで設定されていません！", this);

            // 各依存関係が揃っているか最終チェック
            if (_shopView == null || _moduleDataStore == null || _runtimeModuleManager == null || _playerCore == null)
            {
                Debug.LogError("Shop_Presenter: 依存関係が不足しています。Inspectorの設定とシーンのセットアップを確認してください。このコンポーネントを無効にします。", this);
                enabled = false;
                return;
            }

            // Viewからのモジュール購入リクエストを購読
            _shopView.OnModulePurchaseRequested
                .Subscribe(moduleId => HandleModulePurchaseRequested(moduleId))
                .AddTo(_disposables);

            // Playerの所持金を監視
            _playerCore.Money
                .Subscribe(x =>
                {
                    // Viewに反映
                    _moneyTextScaleAnimation.AnimateFloatAndText(x, 1f);
                }).AddTo(this);

            _shopView.OnModuleHovered
                .Subscribe(x => HandleModuleHovered(x))
                .AddTo(this);

            // RuntimeModuleManagerが管理するモジュールコレクション全体の変更を監視し、ショップUIを更新する
            _runtimeModuleManager.OnAllRuntimeModuleDataChanged
                .Subscribe(_ => {
                    Debug.Log("RuntimeModuleDataコレクションが変更されました。モジュールの変更購読を再設定し、ショップUIを更新します。");
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

            // 初期表示のためにショップUIを準備して表示
            PrepareAndShowShopUI();
        }

        /// <summary>
        /// OnDestroyはゲームオブジェクトが破棄されるときに呼び出されます。
        /// 全ての購読を解除し、リソースを解放します。
        /// </summary>
        private void OnDestroy()
        {
            _disposables.Dispose();
            _moduleLevelAndQuantityChangeDisposables.Dispose(); // 各モジュールのレベル・数量変更購読も解除
        }

        // ----- Private Methods (プライベートメソッド)
        /// <summary>
        /// 各RuntimeModuleDataのレベルと数量変更を購読するヘルパーメソッドです。
        /// </summary>
        /// <param name="runtimeModuleData">購読対象のRuntimeModuleData。</param>
        private void SubscribeToModuleChanges(RuntimeModuleData runtimeModuleData)
        {
            // Levelの変更を購読
            if (runtimeModuleData.Level != null)
            {
                runtimeModuleData.Level
                    .Subscribe(level => {
                        Debug.Log($"モジュールID {runtimeModuleData.Id} ({_moduleDataStore.FindWithId(runtimeModuleData.Id)?.ViewName}) のレベルが {level} に変更されました。ショップUIを更新します。");
                        PrepareAndShowShopUI(); // レベルが変更されたらショップを再表示
                    })
                    .AddTo(_moduleLevelAndQuantityChangeDisposables); // 個別モジュールの購読は専用のDisposableBagに追加
            }
            else
            {
                Debug.LogWarning($"RuntimeModuleData ID {runtimeModuleData.Id} はLevelをReactivePropertyとして公開していません。", this);
            }
        }

        /// <summary>
        /// 各モジュールの購入ボタンのインタラクト可能状態を更新します。
        /// </summary>
        private void UpdatePurchaseButtonsInteractability()
        {
            if (_playerCore == null || _moduleDataStore == null || _moduleDataStore.DataBase == null || _moduleDataStore.DataBase.ItemList == null)
            {
                Debug.LogError("Shop_Presenter: 購入ボタンのインタラクト可能性を更新するための必要なデータが不足しています。", this);
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
        /// モジュール購入リクエストを受け取った際のハンドラです。
        /// </summary>
        /// <param name="moduleId">購入がリクエストされたモジュールのID。</param>
        private void HandleModulePurchaseRequested(int moduleId)
        {
            ModuleData masterData = _moduleDataStore.FindWithId(moduleId);
            if (masterData == null)
            {
                Debug.LogError($"Shop_Presenter: モジュールID {moduleId} のマスターデータが見つかりません。購入を処理できません。", this);
                return;
            }

            RuntimeModuleData runtimeModule = _runtimeModuleManager.GetRuntimeModuleData(moduleId);
            if (runtimeModule == null)
            {
                Debug.LogError($"Shop_Presenter: モジュールID {moduleId} のランタイムデータが見つかりません。これは全てのプレイヤーにモジュールが初期化されている場合は発生しないはずです。", this);
                return;
            }

            // レベルが0のモジュールは購入できない
            if (runtimeModule.CurrentLevelValue == 0)
            {
                Debug.LogWarning($"Shop_Presenter: モジュールID {moduleId} ({masterData.ViewName}) はレベル0です。ショップから購入できません。まずアップグレードしてください（例: ドロップ経由で）。", this);
                return;
            }

            // 購入可能か判定（所持金が足りるか）
            if (_playerCore.Money.CurrentValue >= masterData.BasePrice)
            {
                _playerCore.PayMoney(masterData.BasePrice);
                Debug.Log($"Shop_Presenter: プレイヤーがモジュールID {moduleId} ({masterData.ViewName}) を {masterData.BasePrice} 金で購入しました。残り金: {_playerCore.Money.CurrentValue}。", this);

                // モジュールをプレイヤーのランタイムモジュールに追加（数量を1増やす）
                _runtimeModuleManager.ChangeModuleQuantity(moduleId, 1);

                // 購入成功のフィードバック (UI更新など)
                UpdatePurchaseButtonsInteractability();
            }
            else
            {
                Debug.Log($"Shop_Presenter: モジュールID {moduleId} ({masterData.ViewName}) を購入するのに金が不足しています。必要: {masterData.BasePrice}、所持: {_playerCore.Money.CurrentValue}。", this);
            }
        }

        private void HandleModuleHovered(int EnterModuleId)
        {
             _hoveredModuleInfoText.text = _moduleDataStore.FindWithId(EnterModuleId).Description;
        }

        // ----- Public
        /// <summary>
        /// ショップ画面を表示する準備をし、Viewに表示を依頼します。
        /// このメソッドは外部から呼び出されます（例: GameManagerやUIController）。
        /// また、RuntimeModuleDataの変更によっても自動的に呼び出されることがあります。
        /// </summary>
        private void PrepareAndShowShopUI()
        {
            // 参照NullCheck
            if (_shopView == null || _moduleDataStore == null || _runtimeModuleManager == null || _playerCore == null)
            {
                Debug.LogError("Shop_Presenter: ショップUIを準備するための依存関係が満たされていません！Awakeのログを確認してください。", this);
                return;
            }

            // レベル1以上のモジュールのみをViewに渡す
            List<RuntimeModuleData> shopRuntimeModules = _runtimeModuleManager.AllRuntimeModuleData
                .Where(rmd => rmd != null && rmd.CurrentLevelValue > 0)
                .ToList();

            _shopView.DisplayShopModules(shopRuntimeModules);
            UpdatePurchaseButtonsInteractability();
        }
    }
}