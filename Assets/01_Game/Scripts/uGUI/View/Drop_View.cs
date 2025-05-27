using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using R3;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Drop_View : MonoBehaviour
{
    // -----
    // -----SerializeField
    [SerializeField] private GameObject[] _moduleOptionObjects = new GameObject[3]; // 各モジュール選択肢のルートGameObject
    [SerializeField] private TextMeshProUGUI _instructionsText; // 説明文テキスト

    // -----Field
    private List<Button> _buttons = new List<Button>();
    private List<Detailed_View> _detailedViews = new List<Detailed_View>();
    private List<int> _currentDisplayedModuleIds = new List<int>(); // 現在表示しているモジュールのIDリスト

    // -----Events (PresenterがR3で購読する)
    // ユーザーがモジュールを選択した際に、選択されたモジュールのIDを通知
    // UniRx.ISubject<int> ではなく R3.Subject<int> を使用
    public Subject<int> OnModuleSelected { get; private set; } = new Subject<int>(); // R3.Subject を初期化

    // -----UnityMessage
    private void Awake()
    {
        // 各オプションオブジェクトからButtonとDetailed_Viewを取得し、初期化
        InitOptionViews();

    }

    // -----Private
    private void InitOptionViews()
    {
        _buttons.Clear();
        _detailedViews.Clear();

        for (int i = 0; i < _moduleOptionObjects.Length; i++)
        {
            GameObject obj = _moduleOptionObjects[i];
            if (obj == null)
            {
                Debug.LogError($"_moduleOptionObjects[{i}] is null. Please assign it in the Inspector.");
                continue;
            }

            Button button = obj.GetComponent<Button>();
            Detailed_View detailedView = obj.GetComponent<Detailed_View>();

            if (button == null) Debug.LogError($"Button component not found on _moduleOptionObjects[{i}].");
            if (detailedView == null) Debug.LogError($"Detailed_View component not found on _moduleOptionObjects[{i}].");

            if (button != null && detailedView != null)
            {
                _buttons.Add(button);
                _detailedViews.Add(detailedView);

                // ボタンクリックイベントをR3で購読
                int index = i; // クロージャのためにインデックスをキャプチャ
                button.OnClickAsObservable()
                      .Subscribe(_ => OnOptionButtonClicked(index))
                      .AddTo(this); // オブジェクト破棄時に購読を解除
            }
            //obj.SetActive(false); // 各オプションも初期は非表示
        }
    }

    /// <summary>
    /// オプションボタンがクリックされた際のハンドラ。
    /// </summary>
    /// <param name="index">クリックされたボタンのインデックス。</param>
    private void OnOptionButtonClicked(int index)
    {
        if (index < 0 || index >= _currentDisplayedModuleIds.Count)
        {
            //// 代替オプションがクリックされた場合の処理など
            //if (_defaultOptionObject.activeSelf && index == _moduleOptionObjects.Length) // 例: 3つのオプションの後ろに代替オプションがある場合
            //{
            //    // ここで代替オプションが選択された場合のロジックを実行
            //    // 例: 経験値獲得、コイン獲得などをPresenterに通知
            //    Debug.Log("Default option selected.");
            //    OnModuleSelected.OnNext(-1); // 仮に-1を代替オプションのIDとする
            //}
            Debug.LogWarning($"Invalid option index clicked: {index}");
            return;
        }

        int selectedModuleId = _currentDisplayedModuleIds[index];
        OnModuleSelected.OnNext(selectedModuleId); // 選択されたモジュールIDをイベントとして発火

    }

    // -----Public
    /// <summary>
    /// ドロップ選択UIを表示します。
    /// Presenterから提供されるモジュールデータに基づいてUIを更新します。
    /// </summary>
    /// <param name="moduleDatas">表示するモジュールのデータリスト（ModuleDataとRuntimeModuleDataを結合したデータ）。</param>
    /// <param name="showDefaultOption">代替オプションを表示するかどうか。</param>
    public void Show(List<(ModuleData master, RuntimeModuleData runtime)> moduleDatas, bool showDefaultOption)
    {

        _currentDisplayedModuleIds.Clear(); // 表示IDリストをクリア

        //// まず全てのオプションを非表示に
        //foreach (var obj in _moduleOptionObjects) obj.SetActive(false);
        //_defaultOptionObject.SetActive(false);


        // 渡されたデータに基づいて各オプションUIを設定
        for (int i = 0; i < moduleDatas.Count && i < _detailedViews.Count; i++)
        {
            var (master, runtime) = moduleDatas[i];
            if (master != null && runtime != null)
            {
                _moduleOptionObjects[i].SetActive(true);
                _detailedViews[i].SetInfo(master, runtime); // MasterDataとRuntimeDataの両方を渡す
                _currentDisplayedModuleIds.Add(master.Id); // 表示中のモジュールIDを記録
            }
        }

        // 代替オプションの表示
        if (showDefaultOption)
        {
            //_defaultOptionObject.SetActive(true);
            // 必要であれば_defaultOptionObject内のDetailed_Viewやテキストを設定
        }
    }

}