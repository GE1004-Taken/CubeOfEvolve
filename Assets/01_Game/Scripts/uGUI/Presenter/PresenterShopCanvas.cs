using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Assets.AT;
using Assets.IGC2025.Scripts.View;
using R3;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Assets.IGC2025.Scripts.Presenter
{
    /// <summary>
    /// ショップ画面のプレゼンタークラスです。
    /// ショップの表示ロジック、プレイヤーの所持金やモジュールデータとの連携、購入処理などを担当します。
    /// </summary>
    public class PresenterShopCanvas : MonoBehaviour
    {
        // ----- Serializable Fields (シリアライズフィールド)
        [Header("Models")]
        [SerializeField] private ViewShopCanvas _shopView; // ショップのUI表示を管理するViewへの参照。
        [SerializeField] private ModuleDataStore _moduleDataStore; // モジュールのマスターデータを保持するデータストアへの参照。
        [SerializeField] private RuntimeModuleManager _runtimeModuleManager; // ランタイムモジュールデータを管理するマネージャーへの参照。
        [SerializeField] private PlayerCore _playerCore; // プレイヤーのコアデータ（所持金など）への参照。

        [Header("Views")]
        [SerializeField] private TextScaleAnimation _moneyTextScaleAnimation; // 所持金表示のテキストアニメーションコンポーネント。
        [SerializeField] private TextMeshProUGUI _hoveredModuleInfoText; // ホバー中のモジュール情報テキスト。

        // ----- Private Fields (プライベートフィールド)
        private CompositeDisposable _disposables = new CompositeDisposable(); // オブジェクト破棄時に購読をまとめて解除するためのDisposable。
        private CompositeDisposable _moduleLevelAndQuantityChangeDisposables = new CompositeDisposable(); // モジュールレベルや数量変更の購読を管理するためのDisposable。

        // ----- Unity Messages (Unityイベントメッセージ)

        private void Awake()
        {
            // 依存関係のチェックとエラーログ
            if (_shopView == null) Debug.LogError("Shop_Presenter: ShopViewがInspectorで設定されていません！", this);
            if (_moduleDataStore == null) Debug.LogError("Shop_Presenter: ModuleDataStoreがInspectorで設定されていません！", this);
            if (_runtimeModuleManager == null) _runtimeModuleManager = RuntimeModuleManager.Instance; // インスタンスを自動取得
            if (_playerCore == null) Debug.LogError("Shop_Presenter: PlayerCoreがInspectorで設定されていません！", this);

            // 必須の依存関係が一つでも不足している場合、コンポーネントを無効にします。
            if (_shopView == null || _moduleDataStore == null || _runtimeModuleManager == null || _playerCore == null)
            {
                Debug.LogError("Shop_Presenter: 依存関係が不足しています。Inspectorの設定とシーンのセットアップを確認してください。このコンポーネントを無効にします。", this);
                enabled = false;
                return;
            }

            // プレイヤーの所持金が変更された際に、テキストアニメーションを更新します。
            _playerCore.Money
                .Subscribe(x => _moneyTextScaleAnimation.AnimateFloatAndText(x, 1f))
                .AddTo(_disposables);

            // ランタイムモジュールデータ全体が変更された際に、モジュールの変更購読を再設定し、ショップUIを更新します。
            _runtimeModuleManager.OnAllRuntimeModuleDataChanged
                .Subscribe(_ =>
                {
                    Debug.Log("Shop_Presenter: ランタイムモジュールデータコレクションが変更されました。モジュールの変更購読を再設定し、ショップUIを更新します。");
                    _moduleLevelAndQuantityChangeDisposables.Clear(); // 既存の購読をクリア
                    foreach (var rmd in _runtimeModuleManager.AllRuntimeModuleData)
                    {
                        SubscribeToModuleChanges(rmd); // 各モジュールの変更を購読
                    }
                    DisplayShopContent(); // ショップ内容を再表示
                })
                .AddTo(_disposables);

            // ショップUIの初期準備と表示を行います。
            PrepareAndShowShopUI();
        }


        private void Start()
        {
            // Viewからのモジュール購入要求イベントを購読し、購入処理を呼び出します。
            _shopView.OnModulePurchaseRequested
                .Subscribe(moduleId => HandleModulePurchaseRequested(moduleId))
                .AddTo(_disposables);

            // Viewからのモジュールホバーイベントを購読し、ホバー情報を表示します。
            _shopView.OnModuleHovered
                .Subscribe(x => HandleModuleHovered(x))
                .AddTo(this); // このGameObjectが破棄されたら自動的に購読解除
        }


        private void OnDestroy()
        {
            _disposables.Dispose(); // メインの購読を解除
            _moduleLevelAndQuantityChangeDisposables.Dispose(); // モジュール変更に関する購読を解除
        }

        // ----- Private Methods (プライベートメソッド)
        /// <summary>
        /// 指定されたランタイムモジュールデータの変更（レベルなど）を購読し、ショップUIを更新します。
        /// </summary>
        /// <param name="runtimeModuleData">購読対象のランタイムモジュールデータ。</param>
        private void SubscribeToModuleChanges(RuntimeModuleData runtimeModuleData)
        {
            if (runtimeModuleData.Level != null)
            {
                runtimeModuleData.Level
                    .Subscribe(level =>
                    {
                        Debug.Log($"Shop_Presenter: モジュールID {runtimeModuleData.Id} ({_moduleDataStore.FindWithId(runtimeModuleData.Id)?.ViewName}) のレベルが {level} に変更されました。ショップUIを更新します。");
                        PrepareAndShowShopUI(); // レベル変更時にショップUIを再準備・表示
                    })
                    .AddTo(_moduleLevelAndQuantityChangeDisposables); // モジュールレベル変更購読用のDisposableに追加
            }
            else
            {
                Debug.LogWarning($"Shop_Presenter: ランタイムモジュールデータID {runtimeModuleData.Id} はLevelをReactivePropertyとして公開していません。", this);
            }
        }

        /// <summary>
        /// ショップUIの準備と表示を行います。
        /// 主にショップに表示するモジュールのデータ取得とViewへの引き渡しを行います。
        /// </summary>
        private void PrepareAndShowShopUI()
        {
            if (_shopView == null || _moduleDataStore == null || _runtimeModuleManager == null || _playerCore == null)
            {
                Debug.LogError("Shop_Presenter: ショップUIを準備するための依存関係が満たされていません！Awakeのログを確認してください。", this);
                return;
            }

            DisplayShopContent(); // ショップの内容を表示
        }

        /// <summary>
        /// 現在のランタイムモジュールデータに基づいてショップのコンテンツを表示します。
        /// </summary>
        private void DisplayShopContent()
        {
            // ショップに表示するランタイムモジュールデータをフィルタリングします。（例: レベルが0より大きいモジュール）
            List<RuntimeModuleData> shopRuntimeModules = _runtimeModuleManager.AllRuntimeModuleData
                .Where(rmd => rmd != null && rmd.CurrentLevelValue > 0)
                .ToList();

            // Viewにモジュール表示を依頼します。
            _shopView.DisplayShopModules(shopRuntimeModules, _moduleDataStore);
            // 購入ボタンのインタラクト可能性を更新します。
            UpdatePurchaseButtonsInteractability();
        }

        /// <summary>
        /// プレイヤーの所持金と各モジュールの価格に基づいて、購入ボタンのインタラクト可能性を更新します。
        /// </summary>
        private void UpdatePurchaseButtonsInteractability()
        {
            if (_playerCore == null || _moduleDataStore == null || _moduleDataStore.DataBase?.ItemList == null)
            {
                Debug.LogError("Shop_Presenter: 購入ボタンのインタラクト可能性を更新するための必要なデータが不足しています。", this);
                return;
            }

            // 現在プレイヤーが所持している（レベルが1以上の）モジュールについて、購入ボタンの状態を更新します。
            foreach (var runtimeData in _runtimeModuleManager.AllRuntimeModuleData
                .Where(rmd => rmd != null && rmd.CurrentLevelValue > 0))
            {
                ModuleData masterData = _moduleDataStore.FindWithId(runtimeData.Id);
                if (masterData == null) continue;

                // プレイヤーが購入できるかどうかを判断します。
                bool canAfford = _playerCore.Money.CurrentValue >= masterData.BasePrice; // ここでは簡略化のため定価を使用。後述の割引計算を適用することも可能。
                _shopView.SetPurchaseButtonInteractable(runtimeData.Id, canAfford); // Viewにボタンの状態更新を依頼します。
            }
        }

        /// <summary>
        /// モジュールの購入要求がViewからあった際に処理します。
        /// プレイヤーの所持金チェック、価格計算、購入処理、およびUI更新を行います。
        /// </summary>
        /// <param name="moduleId">購入が要求されたモジュールのID。</param>
        private void HandleModulePurchaseRequested(int moduleId)
        {
            ModuleData masterData = _moduleDataStore.FindWithId(moduleId);
            if (masterData == null)
            {
                Debug.LogError($"Shop_Presenter: モジュールID {moduleId} のマスターデータが見つかりません。購入できません。", this);
                return;
            }

            RuntimeModuleData runtimeModule = _runtimeModuleManager.GetRuntimeModuleData(moduleId);
            if (runtimeModule == null)
            {
                Debug.LogError($"Shop_Presenter: モジュールID {moduleId} のランタイムデータが見つかりません。購入できません。", this);
                return;
            }

            // レベル0のモジュールは購入できないというロジック (例: 未開放のモジュールはショップに表示されない、あるいは購入できない)
            if (runtimeModule.CurrentLevelValue == 0)
            {
                Debug.LogWarning($"Shop_Presenter: モジュールID {moduleId} ({masterData.ViewName}) はレベル0です。購入できません。", this);
                return;
            }

            /// <summary>
            /// モジュールの購入価格を計算します。
            /// レベルに応じて割引を適用するロジックの例です。
            /// </summary>
            /// <param name="maxDiscountRate">最大割引率 (例: 0.5fで50%割引)。</param>
            /// <returns>計算された購入価格。</returns>
            float CalculatePrice(float maxDiscountRate)
            {
                // レベル1の場合は定価
                if (runtimeModule.CurrentLevelValue <= 1) return masterData.BasePrice;
                // レベル5以上の場合は最大割引
                if (runtimeModule.CurrentLevelValue >= 5) return masterData.BasePrice * (1f - maxDiscountRate);

                // レベル2から4の間で割引率を線形補間
                float discountProgress = (runtimeModule.CurrentLevelValue - 1) / 4f; // 1 (Lv2) から 4 (Lv5) まで進捗
                float currentDiscountRate = maxDiscountRate * discountProgress;
                return masterData.BasePrice * (1f - currentDiscountRate);
            }

            var payPrice = CalculatePrice(0.5f); // 最大50%の割引を適用して価格を計算

            // プレイヤーがモジュールを購入できるかチェックします。
            if (_playerCore.Money.CurrentValue >= payPrice)
            {
                _playerCore.PayMoney((int)payPrice); // プレイヤーから代金を支払います。
                Debug.Log($"Shop_Presenter: プレイヤーがモジュールID {moduleId} ({masterData.ViewName}) を {payPrice:F0} 金で購入しました。残り金: {_playerCore.Money.CurrentValue}。", this);
                _runtimeModuleManager.ChangeModuleQuantity(moduleId, 1); // モジュールの数量を増やします。
                UpdatePurchaseButtonsInteractability(); // 購入ボタンのインタラクト可能性を更新します。
            }
            else
            {
                Debug.Log($"Shop_Presenter: モジュールID {moduleId} ({masterData.ViewName}) を購入するのに金が不足しています。現在の所持金: {_playerCore.Money.CurrentValue}、必要金額: {payPrice:F0}。", this);
            }
        }

        /// <summary>
        /// モジュールにマウスがホバーされた際に、そのモジュールの詳細情報をテキストで表示します。
        /// </summary>
        /// <param name="hoveredModuleId">ホバーされたモジュールのID。</param>
        private void HandleModuleHovered(int hoveredModuleId)
        {
            // ホバー情報テキストが設定されていれば、モジュールの説明文を表示します。
            if (_hoveredModuleInfoText != null)
            {
                ModuleData hoveredMasterData = _moduleDataStore.FindWithId(hoveredModuleId);
                _hoveredModuleInfoText.text = hoveredMasterData?.Description ?? "情報なし"; // 説明がなければ「情報なし」と表示
            }
        }
    }
}