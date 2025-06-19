using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using R3;
using R3.Triggers;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.View
{
    public class ViewDropCanvas : MonoBehaviour
    {
        // ----- SerializedField (Unity Inspectorで設定)
        [SerializeField] private GameObject[] _moduleOptionObjects = new GameObject[3]; // 各モジュール選択肢のルートGameObject。

        // ----- Private Members (内部データ)
        private List<Button> _buttons = new List<Button>(); // 各オプションのボタンリスト。
        private List<ViewInfo> _detailedViews = new List<ViewInfo>(); // 各オプションの詳細表示ビューリスト。
        private List<int> _currentDisplayedModuleIds = new List<int>(); // 現在表示しているモジュールのIDリスト。

        // R3の購読を管理するためのCompositeDisposable
        private CompositeDisposable _disposables = new CompositeDisposable();

        // ----- Events (PresenterがR3で購読する)
        public Subject<int> OnModuleSelected { get; private set; } = new Subject<int>(); // ユーザーがモジュールを選択した際に、選択されたモジュールのIDを通知するSubject。
        public Subject<int> OnModuleHovered { get; private set; } = new Subject<int>(); // カーソルを合わせたモジュールのIDを通知するSubject。

        // ----- UnityMessage

        private void Awake()
        {
            InitializeOptionComponents();
        }

        private void OnDestroy()
        {
            _disposables.Dispose(); // オブジェクトが破棄される際に、すべての購読を解除
            OnModuleSelected.Dispose(); // Subjectも忘れずにDispose
            OnModuleHovered.Dispose();  // Subjectも忘れずにDispose
        }

        // ----- Private Methods (内部処理)
        /// <summary>
        /// 各モジュールオプションのViewコンポーネントを取得します。
        /// </summary>
        private void InitializeOptionComponents()
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
                ViewInfo detailedView = obj.GetComponent<ViewInfo>();

                if (button == null) Debug.LogError($"_moduleOptionObjects[{i}]にButtonコンポーネントが見つかりません。");
                if (detailedView == null) Debug.LogError($"_moduleOptionObjects[{i}]にDetailed_Viewコンポーネントが見つかりません。");

                if (button != null && detailedView != null)
                {
                    _buttons.Add(button);
                    _detailedViews.Add(detailedView);
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
            }

            int selectedModuleId = _currentDisplayedModuleIds[index];
            OnModuleSelected.OnNext(selectedModuleId); // 選択されたモジュールIDをイベントとして発火。

            // アイテム選択後、画面が閉じる直前に購読を解除
            HideModuleView();
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
            }

            int selectedModuleId = _currentDisplayedModuleIds[index];
            OnModuleHovered.OnNext(selectedModuleId); // 選択されたモジュールIDをイベントとして発火。
        }

        // ----- Public Methods (Presenterから呼び出される)
        /// <summary>
        /// ドロップ選択UIを表示します。
        /// Presenterから提供されるモジュールデータに基づいてUIを更新します。
        /// </summary>
        /// <param name="moduleDatas">表示するモジュールのデータリスト（ModuleDataとRuntimeModuleDataを結合したデータ）。</param>
        public void UpdateModuleView(List<(ModuleData master, RuntimeModuleData runtime)> moduleDatas)
        {
            // 前回の購読をすべて解除
            _disposables.Clear();

            _currentDisplayedModuleIds.Clear();

            // 渡されたデータに基づいて各オプションUIを設定
            for (int i = 0; i < _moduleOptionObjects.Length; i++) // _moduleOptionObjectsの長さでループ
            {
                GameObject obj = _moduleOptionObjects[i];
                if (obj == null) continue; // nullチェック

                // まずは全て非表示にする
                obj.SetActive(false);

                if (i < moduleDatas.Count) // データがある場合のみ設定
                {
                    var (master, runtime) = moduleDatas[i];
                    if (master != null && runtime != null)
                    {
                        obj.SetActive(true);
                        _detailedViews[i].SetInfo(master, runtime);
                        _currentDisplayedModuleIds.Add(master.Id);

                        // ここでボタンイベントを再購読
                        // クロージャのためにインデックスをキャプチャ
                        int index = i;
                        _buttons[i].OnClickAsObservable()
                                 .Subscribe(_ => OnButtonClicked(index))
                                 .AddTo(_disposables); // CompositeDisposableに追加
                        _buttons[i].OnPointerEnterAsObservable()
                                 .Subscribe(_ => OnModuleOptionHovered(index))
                                 .AddTo(_disposables); // CompositeDisposableに追加
                    }
                }
            }
        }

        /// <summary>
        /// ドロップ選択UIを非表示にし、購読を解除します。
        /// 画面が閉じられる際や、不要になった際に呼び出してください。
        /// </summary>
        public void HideModuleView()
        {
            _disposables.Clear(); // 購読をすべて解除
            foreach (GameObject obj in _moduleOptionObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false); // UIを非表示にする
                }
            }
            _currentDisplayedModuleIds.Clear();
        }
    }
}