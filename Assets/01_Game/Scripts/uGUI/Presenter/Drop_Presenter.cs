using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using R3;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ドロップ選択画面のプレゼンターを担当するクラス。
/// ViewからのイベントをR3で購読し、Model（RuntimeModuleManager, ModuleDataStore）を操作し、
/// Viewに表示データを渡します。
/// </summary>
public class Drop_Presenter : MonoBehaviour
{
    // ----- SerializedField (Unity Inspectorで設定)
    [Header("Dependencies")]
    [SerializeField] private Drop_View _dropView; // ドロップUIを表示するViewコンポーネント。
    [SerializeField] private RuntimeModuleManager _runtimeModuleManager; // ランタイムモジュールデータを管理するマネージャー。
    [SerializeField] private ModuleDataStore _moduleDataStore; // モジュールマスターデータを管理するデータストア。

    // ----- Private Members (内部データ)
    private const int NUMBER_OF_OPTIONS = 3; // 提示するモジュールの数。

    // ----- MonoBehaviour Lifecycle (MonoBehaviourライフサイクル)
    /// <summary>
    /// Awakeはスクリプトインスタンスがロードされたときに呼び出されます。
    /// 依存関係の取得とViewイベントの購読を行います。
    /// </summary>
    void Awake()
    {
        // 依存関係が未設定の場合、シーンから取得を試みる
        if (_dropView == null) _dropView = FindObjectOfType<Drop_View>();
        if (_runtimeModuleManager == null) _runtimeModuleManager = RuntimeModuleManager.Instance;
        // _moduleDataStore は GameManager などから渡されるか、直接参照する想定です。

        // ViewからのイベントをR3で購読
        if (_dropView != null)
        {
            _dropView.OnModuleSelected
                .Subscribe(selectedModuleId => HandleModuleSelected(selectedModuleId))
                .AddTo(this); // R3 の AddTo(CompositeDisposable) を使用。
        }
        else
        {
            Debug.LogError("Drop_Presenter: Drop_Viewが設定されていません。ドロップ選択イベントを購読できません。", this);
        }

        // 必須の依存関係が揃っているかチェック
        if (_runtimeModuleManager == null || _moduleDataStore == null)
        {
            Debug.LogError("Drop_Presenter: RuntimeModuleManagerまたはModuleDataStoreが設定されていません。このコンポーネントを無効にします。", this);
            enabled = false;
        }
    }

    // ----- Public Methods (外部から呼び出されるメソッド)
    /// <summary>
    /// ドロップ選択UIを表示する準備をし、Viewに表示を依頼します。
    /// このメソッドは、例えばプレイヤーが特定のアイテムを拾った際にGameManagerなどから呼び出されます。
    /// </summary>
    public void PrepareAndShowDropUI()
    {
        if (_runtimeModuleManager == null || _moduleDataStore == null || _dropView == null)
        {
            Debug.LogError("Drop_Presenter: プレゼンターの依存関係が満たされていません！ドロップUIを表示できません。", this);
            return;
        }

        // 1. 提示するモジュールを選択するロジック
        List<int> candidateModuleIds = GetRandomAvailableModuleIds(NUMBER_OF_OPTIONS);

        // 2. Viewに渡すためのデータ準備
        List<(ModuleData master, RuntimeModuleData runtime)> displayDatas = new List<(ModuleData, RuntimeModuleData)>();
        bool showDefaultOption = false; // 代替オプション（例: コイン獲得）を表示するかどうか。

        if (candidateModuleIds.Count == 0)
        {
            // 選択肢がない場合、代替オプションを表示
            showDefaultOption = true;
            Debug.Log("Drop_Presenter: アップグレード可能なモジュールが見つかりませんでした。代替オプションを表示します。");
        }
        else
        {
            foreach (int moduleId in candidateModuleIds)
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
        _dropView.Show(displayDatas, showDefaultOption);
    }

    // ----- Private Methods (内部処理)
    /// <summary>
    /// ユーザーがモジュールを選択した際のイベントハンドラ。
    /// Viewからのイベント（R3で購読）によって呼び出されます。
    /// </summary>
    /// <param name="selectedModuleId">選択されたモジュールのID。</param>
    private void HandleModuleSelected(int selectedModuleId)
    {
        if (selectedModuleId == -1) // 代替オプションが選択された場合
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
            // ここでマスターデータに maxLevel のような情報がある前提
            // 仮に、レベルが5未満のモジュールをアップグレード可能とする
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
}