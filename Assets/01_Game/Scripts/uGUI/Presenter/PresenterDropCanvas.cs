using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Assets.IGC2025.Scripts.View;
using R3;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.IGC2025.Scripts.Presenter
{
    public class PresenterDropCanvas : MonoBehaviour
    {
        // ----- SerializedField
        [Header("Models")]
        [SerializeField] private RuntimeModuleManager _runtimeModuleManager; // ランタイムモジュールデータを管理するマネージャー。
        [SerializeField] private ModuleDataStore _moduleDataStore; // モジュールマスターデータを管理するデータストア。
        [Header("Views")]
        [SerializeField] private ViewDropCanvas _dropView; // ドロップUIを表示するViewコンポーネント。
        [SerializeField] private TextMeshProUGUI _hoveredModuleInfoText; // 説明文

        // ----- Private Members (内部データ)
        private const int NUMBER_OF_OPTIONS = 3; // 提示するモジュールの数。
        private List<int> _candidateModuleIds = new List<int>();

        // ----- UnityMessage
        private void Start()
        {
            if (_dropView != null)
            {
                _dropView.OnModuleSelected
                    .Subscribe(x => HandleModuleSelected(x))
                    .AddTo(this); // R3 の AddTo(CompositeDisposable) を使用。
                _dropView.OnModuleHovered
                    .Subscribe(x => HandleModuleHovered(x))
                    .AddTo(this);
            }
        }
        private void Awake()
        {
            // 依存関係が未設定の場合、シーンから取得を試みる
            if (_runtimeModuleManager == null) _runtimeModuleManager = RuntimeModuleManager.Instance;

            // 必須の依存関係が揃っているかチェック
            if (_runtimeModuleManager == null || _moduleDataStore == null || _dropView == null)
            {
                Debug.LogError("Drop_Presenter: RuntimeModuleManager, ModuleDataStore, またはDrop_Viewが設定されていません。このコンポーネントを無効にします。", this);
                enabled = false;
            }
        }

        #region ModelToView

        /// <summary>
        /// ドロップ選択UIを表示する準備をし、Viewに表示を依頼します。
        /// このメソッドは、例えばプレイヤーが特定のアイテムを拾った際にGameManagerなどから呼び出されます。
        /// </summary>
        public void PrepareAndShowDropUI()
        {
            // 依存NullCheck
            if (_runtimeModuleManager == null || _moduleDataStore == null || _dropView == null)
            {
                Debug.LogError("Drop_Presenter: プレゼンターの依存関係が満たされていません！ドロップUIを表示できません。", this);
                return;
            }

            // 1. 提示するモジュールを選択するロジック
            _candidateModuleIds = GetRandomAvailableModuleIds(NUMBER_OF_OPTIONS);

            // 2. Viewに渡すためのデータ準備
            List<(ModuleData master, RuntimeModuleData runtime)> displayDatas = new List<(ModuleData, RuntimeModuleData)>();

            if (_candidateModuleIds.Count == 0)
            {
                // 選択肢がない場合。全部のモジュールがレベル5の場合
                Debug.Log("Drop_Presenter: 全部のモジュールがレベル5になっちゃったみたい");
            }
            else
            {
                foreach (int moduleId in _candidateModuleIds)
                {
                    RuntimeModuleData runtime = _runtimeModuleManager.GetRuntimeModuleData(moduleId);
                    ModuleData master = _moduleDataStore.FindWithId(moduleId);

                    if (runtime != null && master != null)
                    {
                        displayDatas.Add((master, runtime));
                    }
                    else
                    {
                        Debug.LogWarning($"Drop_Presenter: データ不整合: 候補にモジュールID {moduleId} が見つかりましたが、マスターデータまたはランタイムデータが不足しています。");
                    }
                }
            }

            // 3. Viewに表示を依頼
            _dropView.UpdateModuleView(displayDatas);
        }

        /// <summary>
        /// ランダムにアップグレード可能なモジュールIDを選択するロジック。
        /// </summary>
        /// <param name="count">選出するモジュールの数。</param>
        /// <returns>選出されたモジュールのIDリスト。</returns>
        private List<int> GetRandomAvailableModuleIds(int count)
        {
            List<int> upgradeableModuleIds = new List<int>();

            // 現在プレイヤーが所持しているモジュールの中から、
            // まだレベル上限に達していないモジュールを抽出するロジック
            foreach (var runtimeModule in _runtimeModuleManager.AllRuntimeModuleData)
            {
                // レベルが5未満のモジュールをアップグレード可能とする
                if (runtimeModule.CurrentLevelValue < 5)
                {
                    upgradeableModuleIds.Add(runtimeModule.Id);
                }
            }

            // 選出ロジック
            List<int> selectedIds = new List<int>();
            if (upgradeableModuleIds.Count == 0)
            {
                return selectedIds; // アップグレード可能なモジュールがない場合。
            }

            // 重複なしでランダムに選ぶ
            HashSet<int> uniqueIndices = new HashSet<int>();
            while (uniqueIndices.Count < count && uniqueIndices.Count < upgradeableModuleIds.Count)
            {
                int randomIndex = Random.Range(0, upgradeableModuleIds.Count);
                uniqueIndices.Add(upgradeableModuleIds[randomIndex]);
            }

            selectedIds.AddRange(uniqueIndices); // HashSetからリストに変換。

            return selectedIds;
        }

        #endregion


        #region ViewToModel

        /// <summary>
        /// ユーザーがモジュールを選択した際のイベントハンドラ。
        /// Viewからのイベント（R3で購読）によって呼び出されます。
        /// </summary>
        /// <param name="selectedModuleId">選択されたモジュールのID。</param>
        private void HandleModuleSelected(int selectedModuleId)
        {
            if (selectedModuleId == -1) // 何でもないものを選択した場合
            {
                Debug.Log("Drop_Presenter: 代替オプションが選択されました。");
                // ここで経験値獲得やコイン獲得などのロジックを呼び出す
                // 例: GameManager.Instance.GainExperience(100);
            }
            else
            {
                Debug.Log($"Drop_Presenter: ユーザーによってモジュールID {selectedModuleId} が選択されました。");

                // RuntimeModuleManager を介してモジュールのレベルアップ処理を実行
                _runtimeModuleManager.LevelUpModule(selectedModuleId);
            }

            // 必要であれば、UIの更新など、ゲーム全体の状態に応じた後処理を呼び出す
            // 例: GameManager.Instance.OnPlayerModuleUpgraded();
        }

        /// <summary>
        /// モジュールにマウスオーバーした際のイベントハンドラ。
        /// 説明文を更新します。
        /// </summary>
        /// <param name="EnterModuleId">マウスオーバーされたモジュールのID。</param>
        private void HandleModuleHovered(int EnterModuleId)
        {
            _hoveredModuleInfoText.text = _moduleDataStore.FindWithId(EnterModuleId).Description;
        }

        #endregion

    }
}