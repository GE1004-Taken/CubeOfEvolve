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
    /// ドロップ選択画面のビューを担当するクラス。
    /// モジュールオプションの表示、UIの表示・非表示、選択ボタンクリックイベントの通知を行います。
    /// </summary>
    public class Drop_View : MonoBehaviour
    {
        // ----- SerializedField (Unity Inspectorで設定)
        [SerializeField] private GameObject[] _moduleOptionObjects = new GameObject[3]; // 各モジュール選択肢のルートGameObject。

        // ----- Private Members (内部データ)
        private List<Button> _buttons = new List<Button>(); // 各オプションのボタンリスト。
        private List<Detailed_View> _detailedViews = new List<Detailed_View>(); // 各オプションの詳細表示ビューリスト。
        private List<int> _currentDisplayedModuleIds = new List<int>(); // 現在表示しているモジュールのIDリスト。

        // ----- Events (PresenterがR3で購読する)
        public Subject<int> OnModuleSelected { get; private set; } = new Subject<int>(); // ユーザーがモジュールを選択した際に、選択されたモジュールのIDを通知するSubject。
        public Subject<int> OnModuleHovered { get; private set; } = new Subject<int>(); // カーソルを合わせたモジュールのIDを通知するSubject。

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
                          .Subscribe(_ => OnButtonClicked(index))
                          .AddTo(this); // オブジェクト破棄時に購読を解除。
                    button.OnPointerEnterAsObservable()
                        .Subscribe(_ => OnModuleOptionHovered(index))
                        .AddTo(this);
                }
            }
        }

        /// <summary>
        /// モジュールを選択した際のハンドラ。
        /// </summary>
        /// <param name="index">クリックされたボタンのインデックス。</param>
        private void OnButtonClicked(int index)
        {
            if (index < 0 || index >= _currentDisplayedModuleIds.Count)
            {
                Debug.LogWarning($"無効なオプションインデックスがクリックされました: {index}");
                return;
            } // 範囲確認

            int selectedModuleId = _currentDisplayedModuleIds[index];
            OnModuleSelected.OnNext(selectedModuleId); // 選択されたモジュールIDをイベントとして発火。

        }

        /// <summary>
        /// モジュールにカーソルを重ねた際のハンドラ。
        /// </summary>
        /// <param name="index"></param>
        private void OnModuleOptionHovered(int index)
        {
            if (index < 0 || index >= _currentDisplayedModuleIds.Count)
            {
                Debug.LogWarning($"無効なオプションインデックスがクリックされました: {index}");
                return;
            } // 範囲確認

            int selectedModuleId = _currentDisplayedModuleIds[index];
            OnModuleHovered.OnNext(selectedModuleId); // 選択されたモジュールIDをイベントとして発火。
        }

        // ----- Public Methods (Presenterから呼び出される)
        /// <summary>
        /// ドロップ選択UIを表示します。
        /// Presenterから提供されるモジュールデータに基づいてUIを更新します。
        /// </summary>
        /// <param name="moduleDatas">表示するモジュールのデータリスト（ModuleDataとRuntimeModuleDataを結合したデータ）。</param>
        /// <param name="showDefaultOption">代替オプションを表示するかどうか。</param>
        public void UpdateModuleView(List<(ModuleData master, RuntimeModuleData runtime)> moduleDatas)
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
        }
    }
}
