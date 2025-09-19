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
    public sealed class PresenterBuildCanvas : MonoBehaviour, IPresenter
    {
        // -----SerializedField

        [Header("Models")]
        [SerializeField] private PlayerBuilder _builder;
        [SerializeField] private ViewBuildCanvas _buildView;
        [SerializeField] private ModuleDataStore _moduleDataStore;
        [SerializeField] private RuntimeModuleManager _runtimeModuleManager;
        [SerializeField] private PlayerCore _playerCore;

        [Header("Views")]
        [SerializeField] private TextMeshProUGUI _cubeQuantityText;

        // ----- Private Members (内部データ)
        private CompositeDisposable _disposables = new CompositeDisposable(); // 全体の購読解除を管理するCompositeDisposable。
        private CompositeDisposable _moduleLevelAndQuantityChangeDisposables = new CompositeDisposable(); // 各モジュールのレベル・数量変更購読を管理するCompositeDisposable。

        // -----Field
        public bool IsInitialized { get; private set; } = false;

        // -----UnityMessages

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
                        DisplayBuildUI(); // レベルが変更されたらビルド画面を再表示
                    })
                    .AddTo(_moduleLevelAndQuantityChangeDisposables); // 個別モジュールの購読は専用のDisposableBagに追加
            }
            if (runtimeModuleData.Quantity != null) // 数量の監視も重要なので追加
            {
                runtimeModuleData.Quantity
                    .Subscribe(quantity =>
                    {
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
            if (_runtimeModuleManager == null ||
                _moduleDataStore == null || _moduleDataStore.DataBase == null || _moduleDataStore.DataBase.ItemList == null)
            {
                Debug.LogError("Build_Presenter: 選択ボタンの更新に必要な依存が不足しています。", this);
                return;
            }

            foreach (var runtimeData in _runtimeModuleManager.AllRuntimeModuleData
                                                             .Where(rmd => rmd != null && rmd.CurrentQuantityValue > 0))
            {
                var masterData = _moduleDataStore.FindWithId(runtimeData.Id);
                if (masterData == null) continue;
                _buildView.SetChoiceButtonInteractable(runtimeData.Id, runtimeData.CurrentQuantityValue > 0);
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

            _builder?.SetModuleData(masterData);

            // 選択成功時のフィードバック (UI更新など)
            UpdateChoiceButtonsInteractability();
        }

        /// <summary>
        /// モジュールにマウスオーバーした際のイベントハンドラ
        /// </summary>
        /// <param name="EnterModuleId">マウスオーバーされたモジュールのID。</param>
        private void HandleModuleHovered(int EnterModuleId)
        {
            var module = _moduleDataStore.FindWithId(EnterModuleId);
            var Rruntime = RuntimeModuleManager.Instance.GetRuntimeModuleData(EnterModuleId);
        }

        #endregion

        // -----PublicMethod
        public void InInventory()
        {
            GameSoundManager.Instance.PlaySE("inv_in", "SE");
        }
        public void OutInventory()
        {
            GameSoundManager.Instance.PlaySE("inv_out", "SE");
        }

        public void Initialize()
        {
            if (IsInitialized) return;

            // 所持金
            _playerCore.CubeCount
                .CombineLatest(_playerCore.MaxCubeCount, (cube, maxCube) => new { cube, maxCube })
                .Subscribe(x =>
                {
                    _cubeQuantityText.text = $"{_playerCore.MaxCubeCount.CurrentValue - x.cube}";
                }).AddTo(_disposables);

            // 依存関係の取得とチェック
            if (_runtimeModuleManager == null) _runtimeModuleManager = RuntimeModuleManager.Instance;

            if (_buildView == null || _moduleDataStore == null || _runtimeModuleManager == null)
            {
                Debug.LogError($"{nameof(PresenterBuildCanvas)}: 依存不足で初期化を中断します。", this);
                enabled = false;
                return;
            }

            // Viewからのモジュール選択リクエストを購読
            _buildView.OnModuleChoiceRequested
                .Subscribe(moduleId => HandleModuleChoiceRequested(moduleId))
                .AddTo(_disposables);

            // RuntimeModuleManagerが管理するモジュールコレクション全体の変更を監視し、ビルドUIを更新する
            _runtimeModuleManager.OnAllRuntimeModuleDataChanged
                .Subscribe(_ =>
                {

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

            // 完了
            IsInitialized = true;

        }
    }
}