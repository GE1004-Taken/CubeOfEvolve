using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using R3;
using R3.Triggers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.View
{
    public class ViewBuildCanvas : MonoBehaviour
    {
        // ----- SerializedField
        [SerializeField] private GameObject _moduleItemPrefab; // 各モジュール表示用のプレハブ (Detailed_ViewとButtonを含む)。
        [SerializeField] private Transform _contentParent; // モジュールリストの親Transform。

        // ----- Events
        public Subject<int> OnModuleChoiceRequested { get; private set; } = new Subject<int>(); // モジュール購入リクエストを通知するSubject。
        public Subject<int> OnModuleHovered { get; private set; } = new Subject<int>(); // モジュール購入リクエストを通知するSubject。

        // ----- Private Members (内部データ)
        private List<GameObject> _instantiatedModuleItems = new List<GameObject>(); // 生成されたモジュールアイテムのリスト。
        private Dictionary<int, Button> _choiceButtons = new Dictionary<int, Button>(); // モジュールIDと購入ボタンのマッピング。
        private CompositeDisposable _disposables = new CompositeDisposable(); // R3購読管理用。

        // ----- UnityMessage

        private void OnDestroy()
        {
            _disposables.Dispose(); // オブジェクト破棄時に全ての購読を解除。
        }

        // ----- Public
        /// <summary>
        /// ショップに表示するモジュールリストを設定し、UIを更新します。
        /// 1個以上所持しているのモジュールのみ、実際のランタイムデータに基づいて表示されます。
        /// </summary>
        /// <param name="buildRuntimeModules">ショップに表示するRuntimeModuleDataのリスト。</param>
        public void DisplayBuildModules(List<RuntimeModuleData> buildRuntimeModules, ModuleDataStore moduleDataStore)
        {
            // 既存のモジュールアイテムを全てクリア
            foreach (var item in _instantiatedModuleItems)
            {
                Destroy(item);
            }
            _instantiatedModuleItems.Clear();
            _choiceButtons.Clear();
            _disposables.Clear(); // 新しいボタン購読のために既存の購読をクリア。

            // 各モジュールデータを基にUI要素を生成・設定
            foreach (var runtimeData in buildRuntimeModules)
            {
                if (runtimeData == null)
                {
                    Debug.LogWarning("Build_View: ショップのランタイムモジュールリストにnullデータが提供されました。スキップします。", this);
                    continue;
                }

                // 対応するマスターデータを取得
                ModuleData masterData = moduleDataStore.FindWithId(runtimeData.Id);
                if (masterData == null)
                {
                    Debug.LogError($"Build_View: ModuleDataStoreにランタイムモジュールID {runtimeData.Id} のマスターデータが見つかりません。モジュールを表示できません。", this);
                    continue;
                }

                GameObject moduleItem = Instantiate(_moduleItemPrefab, _contentParent);
                _instantiatedModuleItems.Add(moduleItem);

                ViewInfo InfoView = moduleItem.GetComponent<ViewInfo>();
                Button choiceButton = moduleItem.GetComponentInChildren<Button>(); // 子要素からボタンを探す。

                if (InfoView == null)
                {
                    Debug.LogError($"Build_View: _moduleItemPrefabにDetailed_Viewコンポーネントがありません。モジュールID: {masterData.Id}", moduleItem);
                    continue;
                }
                if (choiceButton == null)
                {
                    Debug.LogError($"Build_View: _moduleItemPrefabの子要素にButtonコンポーネントがありません。モジュールID: {masterData.Id}", moduleItem);
                    continue;
                }

                // 実際のRuntimeModuleDataをDetailed_Viewに渡す
                InfoView.SetInfo(masterData, runtimeData);

                // 購入ボタンにイベントを登録
                _choiceButtons.Add(masterData.Id, choiceButton);
                int moduleId = masterData.Id; // クロージャのためにコピー。
                choiceButton.OnClickAsObservable()
                    .Subscribe(_ => OnModuleChoiceButtonClicked(moduleId))
                    .AddTo(_disposables); // _disposables に追加。
                // OnEnter
                choiceButton.OnPointerEnterAsObservable()
                    .Subscribe(_ => OnBuildItemHovered(moduleId))
                    .AddTo(_disposables);

            }
        }

        /// <summary>
        /// 特定のモジュールの選択ボタンの有効/無効を切り替えます。
        /// </summary>
        /// <param name="moduleId">対象のモジュールID。</param>
        /// <param name="isInteractable">ボタンを操作可能にするか。</param>
        public void SetChoiceButtonInteractable(int moduleId, bool isInteractable)
        {
            if (_choiceButtons.TryGetValue(moduleId, out Button button))
            {
                button.interactable = isInteractable;
            }
        }

        // ----- Private

        /// <summary>
        /// モジュール選択ボタンがクリックされたときに呼び出されるハンドラです。
        /// </summary>
        /// <param name="moduleId">選択がリクエストされたモジュールのID。</param>
        private void OnModuleChoiceButtonClicked(int moduleId)
        {
            OnModuleChoiceRequested.OnNext(moduleId); // Presenterに通知。
        }

        /// <summary>
        /// モジュールにカーソルを重ねた際のハンドラ。
        /// </summary>
        /// <param name="moduleId"></param>
        private void OnBuildItemHovered(int moduleId)
        {
            OnModuleHovered.OnNext(moduleId); // 選択されたモジュールIDをイベントとして発火。
        }
    }
}