using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using R3;
using R3.Triggers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.View
{
    /// <summary>
    /// モジュール選択画面（最大3件）。Prefabベースで生成し、選択またはランダム表示が可能。
    /// </summary>
    public class ViewDropCanvas : MonoBehaviour
    {
        // ----- SerializedField
        [SerializeField] private GameObject _moduleItemPrefab;
        [SerializeField] private Transform _contentParent;

        // ----- Events
        public Subject<int> OnModuleSelected { get; private set; } = new Subject<int>();
        public Subject<int> OnModuleHovered { get; private set; } = new Subject<int>();

        // ----- Internal State
        private List<GameObject> _instantiatedItems = new List<GameObject>();
        private Dictionary<int, Button> _selectionButtons = new Dictionary<int, Button>();
        private List<int> _currentDisplayedModuleIds = new List<int>();
        private CompositeDisposable _disposables = new CompositeDisposable();

        [SerializeField] private int _maxOptions = 3;

        private void OnDestroy()
        {
            _disposables.Dispose();
            OnModuleSelected.Dispose();
            OnModuleHovered.Dispose();
        }

        /// <summary>
        /// 指定されたIDのモジュールを選出・表示する（-1はランダム補完）。
        /// </summary>
        /// <param name="moduleIds">選出したいモジュールID（-1はランダム補完）。</param>
        /// <param name="candidatePool">ランダム補完に使える候補データ。</param>
        /// <param name="dataStore">マスターデータ取得用。</param>
        public void DisplayModulesByIdOrRandom(List<int> moduleIds, List<RuntimeModuleData> candidatePool, ModuleDataStore dataStore)
        {
            Cleanup();

            var randomPool = candidatePool.OrderBy(_ => Random.value).ToList();
            int randomIndex = 0;

            for (int i = 0; i < Mathf.Min(_maxOptions, moduleIds.Count); i++)
            {
                int requestedId = moduleIds[i];
                if (TryGetModuleData(requestedId, out var moduleId, out var master, out var runtime))
                {
                    CreateModuleUI(moduleId, master, runtime);
                }
            }

            // -----ローカル関数定義

            void Cleanup()
            {
                foreach (var obj in _instantiatedItems)
                    Destroy(obj);

                _instantiatedItems.Clear();
                _selectionButtons.Clear();
                _currentDisplayedModuleIds.Clear();
                _disposables.Clear();
            }

            bool TryGetModuleData(int idInput, out int moduleId, out ModuleData master, out RuntimeModuleData runtime)
            {
                moduleId = -1;
                master = null;
                runtime = null;

                if (idInput == -1)
                {
                    while (randomIndex < randomPool.Count && _currentDisplayedModuleIds.Contains(randomPool[randomIndex].Id))
                        randomIndex++;

                    if (randomIndex >= randomPool.Count)
                    {
                        Debug.LogWarning("ViewDropCanvas: ランダム候補が不足しています。");
                        return false;
                    }

                    runtime = randomPool[randomIndex];
                    master = dataStore.FindWithId(runtime.Id);
                    moduleId = runtime.Id;
                }
                else
                {
                    runtime = candidatePool.FirstOrDefault(c => c.Id == idInput);
                    master = dataStore.FindWithId(idInput);
                    moduleId = idInput;

                    if (runtime == null || master == null)
                    {
                        Debug.LogWarning($"ViewDropCanvas: 指定ID {idInput} に該当するデータが見つかりません。スキップ。");
                        return false;
                    }
                }

                return true;
            }

            void CreateModuleUI(int moduleId, ModuleData master, RuntimeModuleData runtime)
            {
                GameObject item = Instantiate(_moduleItemPrefab, _contentParent);
                _instantiatedItems.Add(item);

                ViewInfo view = item.GetComponent<ViewInfo>();
                Button button = item.GetComponentInChildren<Button>();

                if (view == null || button == null)
                {
                    Debug.LogError($"ViewDropCanvas: Prefabに必要なコンポーネントがありません（ViewInfo/Button）。ID: {moduleId}");
                    return;
                }

                view.SetInfo(master, runtime);
                _selectionButtons[moduleId] = button;
                _currentDisplayedModuleIds.Add(moduleId);

                Transform indicatorTransform = item.transform.Find("LevelZeroIndicator");
                if (indicatorTransform != null)
                {
                    GameObject indicator = indicatorTransform.gameObject;
                    indicator.SetActive(runtime.CurrentLevelValue == 0);
                }

                button.OnClickAsObservable()
                    .Subscribe(_ => OnModuleSelected.OnNext(moduleId))
                    .AddTo(_disposables);

                button.OnPointerEnterAsObservable()
                    .Subscribe(_ => OnModuleHovered.OnNext(moduleId))
                    .AddTo(_disposables);
            }

        }
    }
}
