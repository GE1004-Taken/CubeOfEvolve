using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using R3;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ドロップ選択画面のビューを担当するクラス。
/// モジュールオプションの表示、UIの表示・非表示、選択ボタンクリックイベントの通知を行います。
/// </summary>
public class Drop_View : MonoBehaviour
{
    // ----- SerializedField (Unity Inspectorで設定)
    [SerializeField] private GameObject[] _moduleOptionObjects = new GameObject[3]; // 各モジュール選択肢のルートGameObject。
    [SerializeField] private TextMeshProUGUI _instructionsText; // 説明文テキスト。

    // ----- Private Members (内部データ)
    private List<Button> _buttons = new List<Button>(); // 各オプションのボタンリスト。
    private List<Detailed_View> _detailedViews = new List<Detailed_View>(); // 各オプションの詳細表示ビューリスト。
    private List<int> _currentDisplayedModuleIds = new List<int>(); // 現在表示しているモジュールのIDリスト。

    // ----- Events (PresenterがR3で購読する)
    public Subject<int> OnModuleSelected { get; private set; } = new Subject<int>(); // ユーザーがモジュールを選択した際に、選択されたモジュールのIDを通知するSubject。

    // ----- MonoBehaviour Lifecycle (MonoBehaviourライフサイクル)
    /// <summary>
    /// Awakeはスクリプトインスタンスがロードされたときに呼び出されます。
    /// 各オプションUIのコンポーネントを取得し、イベントを購読します。
    /// </summary>
    private void Awake()
    {
        InitOptionViews();
    }

    // ----- Private Methods (内部処理)
    /// <summary>
    /// 各モジュールオプションのViewコンポーネントを初期化し、ボタンイベントを購読します。
    /// </summary>
    private void InitOptionViews()
    {
        _buttons.Clear();
        _detailedViews.Clear();

        for (int i = 0; i < _moduleOptionObjects.Length; i++)
        {
            GameObject obj = _moduleOptionObjects[i];
            if (obj == null)
            {
                Debug.LogError($"_moduleOptionObjects[{i}]がnullです。Inspectorで割り当ててください。");
                continue;
            }

            Button button = obj.GetComponent<Button>();
            Detailed_View detailedView = obj.GetComponent<Detailed_View>();

            if (button == null) Debug.LogError($"_moduleOptionObjects[{i}]にButtonコンポーネントが見つかりません。");
            if (detailedView == null) Debug.LogError($"_moduleOptionObjects[{i}]にDetailed_Viewコンポーネントが見つかりません。");

            if (button != null && detailedView != null)
            {
                _buttons.Add(button);
                _detailedViews.Add(detailedView);

                // ボタンクリックイベントをR3で購読
                int index = i; // クロージャのためにインデックスをキャプチャ。
                button.OnClickAsObservable()
                      .Subscribe(_ => OnOptionButtonClicked(index))
                      .AddTo(this); // オブジェクト破棄時に購読を解除。
            }
        }
    }

    /// <summary>
    /// オプションボタンがクリックされた際のハンドラです。
    /// </summary>
    /// <param name="index">クリックされたボタンのインデックス。</param>
    private void OnOptionButtonClicked(int index)
    {
        if (index < 0 || index >= _currentDisplayedModuleIds.Count)
        {
            Debug.LogWarning($"無効なオプションインデックスがクリックされました: {index}");
            return;
        }

        int selectedModuleId = _currentDisplayedModuleIds[index];
        OnModuleSelected.OnNext(selectedModuleId); // 選択されたモジュールIDをイベントとして発火。

        // UIを非表示にするなどの後処理が必要であれば、ここで呼び出す
        //Hide();
    }

    // ----- Public Methods (Presenterから呼び出される)
    /// <summary>
    /// ドロップ選択UIを表示します。
    /// Presenterから提供されるモジュールデータに基づいてUIを更新します。
    /// </summary>
    /// <param name="moduleDatas">表示するモジュールのデータリスト（ModuleDataとRuntimeModuleDataを結合したデータ）。</param>
    /// <param name="showDefaultOption">代替オプションを表示するかどうか。</param>
    public void Show(List<(ModuleData master, RuntimeModuleData runtime)> moduleDatas, bool showDefaultOption)
    {
        _currentDisplayedModuleIds.Clear(); // 表示IDリストをクリア。

        // 渡されたデータに基づいて各オプションUIを設定
        for (int i = 0; i < moduleDatas.Count && i < _detailedViews.Count; i++)
        {
            var (master, runtime) = moduleDatas[i];
            if (master != null && runtime != null)
            {
                _moduleOptionObjects[i].SetActive(true);
                _detailedViews[i].SetInfo(master, runtime); // MasterDataとRuntimeDataの両方を渡す。
                _currentDisplayedModuleIds.Add(master.Id); // 表示中のモジュールIDを記録。
            }
        }

        // 代替オプションの表示 (現在コメントアウトされているため、必要に応じて実装)
        if (showDefaultOption)
        {
            // 例えば、_moduleOptionObjectsの最後の要素を代替オプションとして使用する場合
            // if (_moduleOptionObjects.Length > moduleDatas.Count)
            // {
            //     _moduleOptionObjects[moduleDatas.Count].SetActive(true);
            //     // 代替オプションのテキストなどを設定
            //     _instructionsText.text = "アップグレード可能なモジュールがありません。代わりにコインを獲得します。";
            //     // このボタンがクリックされたときに-1をOnModuleSelectedに発行するようにする
            //     _buttons[moduleDatas.Count].OnClickAsObservable()
            //         .Subscribe(_ => OnModuleSelected.OnNext(-1))
            //         .AddTo(this);
            // }
            Debug.Log("Drop_View: 代替オプション表示のロジックが有効になりましたが、UIの実装はまだです。");
            _instructionsText.text = "アップグレード可能なモジュールがありません。";
        }
        else
        {
            // 通常の指示テキストを表示
            _instructionsText.text = "アップグレードするモジュールを選択してください。";
        }

        // UI全体を表示
        gameObject.SetActive(true);
    }

    /// <summary>
    /// ドロップ選択UIを非表示にします。
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}