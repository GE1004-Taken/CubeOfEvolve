using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Assets.AT;
using Assets.IGC2025.Scripts.View;
using R3;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UltimateClean;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.Presenter
{
    public class PresenterBuildCanvas : MonoBehaviour
    {
        // ----- SerializedField

        [Header("Models")]
        [SerializeField] private PlayerBuilder _builder;
        [SerializeField] private ViewBuildCanvas _buildView; // ビルドUIを表示するViewコンポーネント。
        [SerializeField] private ModuleDataStore _moduleDataStore; // モジュールマスターデータを管理するデータストア。
        [SerializeField] private RuntimeModuleManager _runtimeModuleManager; // ランタイムモジュールデータを管理するマネージャー。
        [SerializeField] private PlayerCore _playerCore; // プレイヤーのコアデータ（所持金など）への参照。

        [Header("Views")]
        //[SerializeField] private TextScaleAnimation _moneyTextScaleAnimation; // 所持金表示のテキストアニメーションコンポーネント。
        //[SerializeField] private Button _exitButton;

        [Header("Views_Hovered")]
        //[SerializeField] private TextMeshProUGUI _unitName;
        //[SerializeField] private TextMeshProUGUI _infoText; // 説明文
        //[SerializeField] private TextMeshProUGUI _level; // 
        //[SerializeField] private TextMeshProUGUI _quantity; // 
        //[SerializeField] private Image _image; // 
        //[SerializeField] private Image _icon; // 
        //[SerializeField] private TextMeshProUGUI _atk; // 
        //[SerializeField] private TextMeshProUGUI _rpd; // 
        //[SerializeField] private TextMeshProUGUI _prc; // 


        // ----- Private Members (内部データ)
        private CompositeDisposable _disposables = new CompositeDisposable(); // 全体の購読解除を管理するCompositeDisposable。
        private CompositeDisposable _moduleLevelAndQuantityChangeDisposables = new CompositeDisposable(); // 各モジュールのレベル・数量変更購読を管理するCompositeDisposable。

        // ----- UnityMessage

        //private void Start()
        //{
        //    // プレイヤーの所持金が変更された際に、テキストアニメーションを更新します。
        //    _playerCore.Money
        //        .Subscribe(x => _moneyTextScaleAnimation.AnimateFloatAndText(x, 1f))
        //        .AddTo(_disposables);
        //}
        private void Awake()
        {
            // 依存関係の取得とチェック
            if (_builder == null) Debug.LogError("Build_Presenter: PlayerBuilderがアタッチされていません！", this);
            if (_buildView == null) Debug.LogError("Build_Presenter: BuildViewがInspectorで設定されていません！", this);
            if (_moduleDataStore == null) Debug.LogError("Build_Presenter: ModuleDataStoreがInspectorで設定されていません！", this);
            if (_runtimeModuleManager == null) _runtimeModuleManager = RuntimeModuleManager.Instance;
            //if (_exitButton == null) Debug.LogError("Build_Presenter: ExitButtonがInspectorで設定されていません！", this);

            // 各依存関係が揃っているか最終チェック
            if (_buildView == null || _moduleDataStore == null || _runtimeModuleManager == null/* || _exitButton == null*/)
            {
                Debug.LogError("Build_Presenter: 依存関係が不足しています。Inspectorの設定とシーンのセットアップを確認してください。このコンポーネントを無効にします。", this);
                enabled = false;
                return;
            }

            // Viewからのモジュール選択リクエストを購読
            _buildView.OnModuleChoiceRequested
                .Subscribe(moduleId => HandleModuleChoiceRequested(moduleId))
                .AddTo(_disposables);

            //_buildView.OnModuleHovered
            //    .Subscribe(moduleId => HandleModuleHovered(moduleId))
            //    .AddTo(_disposables);

            // RuntimeModuleManagerが管理するモジュールコレクション全体の変更を監視し、ビルドUIを更新する
            _runtimeModuleManager.OnAllRuntimeModuleDataChanged
                .Subscribe(_ =>
                {
                    Debug.Log("RuntimeModuleDataコレクションが変更されました。モジュールの変更購読を再設定し、ビルドUIを更新します。");
                    // 既存のモジュールレベル・数量変更購読を全て解除
                    _moduleLevelAndQuantityChangeDisposables.Clear();

                    // 現在の全てのモジュールに対してレベル・数量変更を購読
                    foreach (var rmd in _runtimeModuleManager.AllRuntimeModuleData)
                    {
                        SubscribeToModuleChanges(rmd);
                    }
                    DisplayBuildUI(); // ビルド画面を再表示してリストを更新
                })
                .AddTo(_disposables);

            // 初期表示のためにビルドUIを準備して表示
            DisplayBuildUI();
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _moduleLevelAndQuantityChangeDisposables.Dispose(); // 各モジュールのレベル・数量変更購読も解除
        }

        #region ModelToView

        /// <summary>
        /// 各RuntimeModuleDataのレベルと数量変更を購読するヘルパーメソッドです。
        /// </summary>
        /// <param name="runtimeModuleData">購読対象のRuntimeModuleData。</param>
        private void SubscribeToModuleChanges(RuntimeModuleData runtimeModuleData)
        {
            // LevelまたはQuantityの変更を購読
            if (runtimeModuleData.Level != null)
            {
                runtimeModuleData.Level
                    .Subscribe(level =>
                    {
                        Debug.Log($"モジュールID {runtimeModuleData.Id} ({_moduleDataStore.FindWithId(runtimeModuleData.Id)?.ViewName}) のレベルが {level} に変更されました。ビルドUIを更新します。");
                        DisplayBuildUI(); // レベルが変更されたらビルド画面を再表示
                    })
                    .AddTo(_moduleLevelAndQuantityChangeDisposables); // 個別モジュールの購読は専用のDisposableBagに追加
            }
            if (runtimeModuleData.Quantity != null) // 数量の監視も重要なので追加
            {
                runtimeModuleData.Quantity
                    .Subscribe(quantity =>
                    {
                        Debug.Log($"モジュールID {runtimeModuleData.Id} ({_moduleDataStore.FindWithId(runtimeModuleData.Id)?.ViewName}) の数量が {quantity} に変更されました。ビルドUIを更新します。");
                        DisplayBuildUI(); // 数量が変更されたらビルド画面を再表示
                    })
                    .AddTo(_moduleLevelAndQuantityChangeDisposables);
            }
            else
            {
                Debug.LogWarning($"RuntimeModuleData ID {runtimeModuleData.Id} はLevelまたはQuantityをReactivePropertyとして公開していません。", this);
            }
        }

        /// <summary>
        /// ビルド画面を表示する準備をし、Viewに表示を依頼します。
        /// このメソッドは外部から呼び出されます（例: GameManagerやUIController）。
        /// また、RuntimeModuleDataの変更によっても自動的に呼び出されることがあります。
        /// </summary>
        private void DisplayBuildUI()
        {
            // 参照NullCheck
            if (_buildView == null || _moduleDataStore == null || _runtimeModuleManager == null)
            {
                Debug.LogError("Build_Presenter: ビルドUIを準備するための依存関係が満たされていません！Awakeのログを確認してください。", this);
                return;
            }

            // 所持数1以上のモジュールのみをViewに渡す
            List<RuntimeModuleData> choiceRuntimeModules = _runtimeModuleManager.AllRuntimeModuleData
                .Where(rmd => rmd != null && rmd.CurrentQuantityValue > 0)
                .ToList();

            _buildView.DisplayBuildModules(choiceRuntimeModules, _moduleDataStore);
            UpdateChoiceButtonsInteractability();
        }

        /// <summary>
        /// 各モジュールの選択ボタンのインタラクト可能状態を更新します。
        /// </summary>
        private void UpdateChoiceButtonsInteractability()
        {
            if (_moduleDataStore == null || _moduleDataStore.DataBase == null || _moduleDataStore.DataBase.ItemList == null)
            {
                Debug.LogError("Build_Presenter: 選択ボタンのインタラクト可能性を更新するための必要なデータが不足しています。", this);
                return;
            }

            // ビルド画面に表示されているすべてのモジュール（1個以上のもの）についてチェック
            foreach (var runtimeData in _runtimeModuleManager.AllRuntimeModuleData
                                                             .Where(rmd => rmd != null && rmd.CurrentQuantityValue > 0))
            {
                ModuleData masterData = _moduleDataStore.FindWithId(runtimeData.Id);
                if (masterData == null) continue;

                // 所持数が0でなければ選択可能
                bool canChoose = runtimeData.CurrentQuantityValue > 0;

                _buildView.SetChoiceButtonInteractable(runtimeData.Id, canChoose);
            }
        }

        #endregion

        #region ViewToModel

        /// <summary>
        /// モジュール選択リクエストを受け取った際のハンドラです。
        /// </summary>
        /// <param name="moduleId">選択がリクエストされたモジュールのID。</param>
        private void HandleModuleChoiceRequested(int moduleId)
        {
            ModuleData masterData = _moduleDataStore.FindWithId(moduleId);
            if (masterData == null)
            {
                Debug.LogError($"Build_Presenter: モジュールID {moduleId} のマスターデータが見つかりません。選択を処理できません。", this);
                return;
            }

            RuntimeModuleData runtimeModule = _runtimeModuleManager.GetRuntimeModuleData(moduleId);
            if (runtimeModule == null)
            {
                Debug.LogError($"Build_Presenter: モジュールID {moduleId} のランタイムデータが見つかりません。これは全てのプレイヤーにモジュールが初期化されている場合は発生しないはずです。", this);
                return;
            }

            // 所持数0のモジュールは選択できない
            if (runtimeModule.CurrentQuantityValue == 0)
            {
                Debug.LogWarning($"Build_Presenter: モジュールID {moduleId} ({masterData.ViewName}) は持っていないため選択できません。", this);
                return;
            }

            // 選択画面の消去
            //_exitButton.onClick.Invoke();

            // ☆注意：ビルド画面に移行する処理
            // ☆注意：設置後に所持数を減らす処理

            _builder?.SetModuleData(masterData);

            Debug.Log($"Build_Presenter: プレイヤーがモジュールID {moduleId} ({masterData.ViewName}) を選択しました。", this);

            // 選択成功時のフィードバック (UI更新など)
            UpdateChoiceButtonsInteractability();
        }

        /// <summary>
        /// モジュールにマウスオーバーした際のイベントハンドラ。
        /// 説明文を更新します。
        /// </summary>
        /// <param name="EnterModuleId">マウスオーバーされたモジュールのID。</param>
        private void HandleModuleHovered(int EnterModuleId)
        {
            var module = _moduleDataStore.FindWithId(EnterModuleId);
            var Rruntime = RuntimeModuleManager.Instance.GetRuntimeModuleData(EnterModuleId);

            //_unitName.text = module.ViewName;
            //_infoText.text = module.Description;
            //_level.text = $"{Rruntime.CurrentLevelValue}";
            //_quantity.text = $"{Rruntime.CurrentQuantityValue}";
            //_image.sprite = module.MainSprite;
            //_icon.sprite = module.BlockSprite;
            //_atk.text = $"{module.ModuleState?.Attack ?? 0}";
            //_rpd.text = $"{module.ModuleState?.Interval ?? 0}";
            //_prc.text = $"{module.BasePrice}";
        }

        #endregion

        // -----Public
        public void inInventory()
        {
            GameSoundManager.Instance.PlaySE("inv_in", "SE");
        }

        public void outInventory()
        {
            GameSoundManager.Instance.PlaySE("inv_out", "SE");
        }

    }
}