// App.GameSystem.Modules/RuntimeModuleManager.cs
using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using System.Collections.Generic;
using UnityEngine;
using R3; // R3のusingディレクティブを追加

namespace App.GameSystem.Modules
{
    public class RuntimeModuleManager : MonoBehaviour
    {
        public static RuntimeModuleManager Instance { get; private set; }

        [SerializeField] private ModuleDataStore _moduleDataStore;

        // 全てのRuntimeModuleDataを管理するためのList
        private readonly List<RuntimeModuleData> _allRuntimeModuleDataInternal = new List<RuntimeModuleData>();

        // コレクションの変更を通知するためのSubject (R3)
        private readonly Subject<Unit> _collectionChangedSubject = new Subject<Unit>();

        // コレクションの要素自体を読み取り専用で公開 (既存のAllRuntimeModuleDataプロパティ名と型を維持に近い形)
        // 他のクラスがコレクションの要素にアクセスするために使用
        public IReadOnlyList<RuntimeModuleData> AllRuntimeModuleData => _allRuntimeModuleDataInternal;

        // コレクション全体の変更イベントを購読するためのObservable (R3)
        // SubjectからObservable<Unit>に変換して公開します。
        // 他のクラスがコレクションの変更を検知するために使用
        public Observable<Unit> OnAllRuntimeModuleDataChanged => _collectionChangedSubject.AsObservable();


        // モジュールIDをキーとした辞書で高速アクセス
        private Dictionary<int, RuntimeModuleData> _runtimeModuleDictionary = new Dictionary<int, RuntimeModuleData>();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            if (_moduleDataStore == null)
            {
                Debug.LogError("RuntimeModuleManager: ModuleDataStore is not assigned!", this);
            }

            InitializeAllModules();
        }

        // MonoBehaviourのOnDestroyでSubjectをDisposeすることを推奨
        void OnDestroy()
        {
            _collectionChangedSubject.Dispose();
        }

        /// <summary>
        /// 全てのマスターモジュールデータを基にRuntimeModuleDataを初期化（レベル0, 数量0）
        /// ゲーム開始時に一度だけ呼ばれることを想定
        /// </summary>
        private void InitializeAllModules()
        {
            if (_moduleDataStore == null || _moduleDataStore.DataBase == null || _moduleDataStore.DataBase.ItemList == null)
            {
                Debug.LogError("RuntimeModuleManager: ModuleDataStore data is not available for initialization.", this);
                return;
            }

            foreach (var masterData in _moduleDataStore.DataBase.ItemList)
            {
                if (!_runtimeModuleDictionary.ContainsKey(masterData.Id))
                {
                    RuntimeModuleData newRmd = new RuntimeModuleData(masterData);
                    _runtimeModuleDictionary.Add(masterData.Id, newRmd);
                    _allRuntimeModuleDataInternal.Add(newRmd); // 内部Listに追加
                }
            }
            // 全ての要素を追加し終えた後に一度だけ変更を通知
            _collectionChangedSubject.OnNext(Unit.Default);
            Debug.Log($"RuntimeModuleManager: Initialized {_allRuntimeModuleDataInternal.Count} modules to level 0, quantity 0.");

            // デバッグ用: 特定のモジュールを初期レベル1に設定してショップに表示されるかテスト
            // if (_runtimeModuleDictionary.TryGetValue(1001, out var debugRmd)) // 仮のID
            // {
            //     debugRmd.SetLevel(1);
            //     debugRmd.SetQuantity(1);
            //     Debug.Log($"Debug: Module 1001 set to Level 1, Quantity 1.");
            // }
        }

        /// <summary>
        /// 指定されたIDのRuntimeModuleDataを取得します。
        /// </summary>
        public RuntimeModuleData GetRuntimeModuleData(int moduleId)
        {
            _runtimeModuleDictionary.TryGetValue(moduleId, out var rmd);
            return rmd;
        }

        /// <summary>
        /// モジュールの数量を変更します。
        /// </summary>
        /// <param name="moduleId">対象モジュールのID。</param>
        /// <param name="amount">変更量。</param>
        public void ChangeModuleQuantity(int moduleId, int amount)
        {
            if (_runtimeModuleDictionary.TryGetValue(moduleId, out RuntimeModuleData rmd))
            {
                rmd.ChangeQuantity(amount); // RuntimeModuleData内のReactivePropertyを更新
                // 個別のRuntimeModuleDataが変更された場合、コレクションの変更も通知
                _collectionChangedSubject.OnNext(Unit.Default);
            }
            else
            {
                Debug.LogWarning($"RuntimeModuleManager: Module with ID {moduleId} not found. Cannot change quantity.", this);
            }
        }

        /// <summary>
        /// モジュールのレベルを上げます。
        /// </summary>
        /// <param name="moduleId">対象モジュールのID。</param>
        public void LevelUpModule(int moduleId)
        {
            if (_runtimeModuleDictionary.TryGetValue(moduleId, out RuntimeModuleData rmd))
            {
                rmd.LevelUp(); // RuntimeModuleData内のReactivePropertyを更新
                // 個別のRuntimeModuleDataが変更された場合、コレクションの変更も通知
                _collectionChangedSubject.OnNext(Unit.Default);
            }
            else
            {
                Debug.LogWarning($"RuntimeModuleManager: Module with ID {moduleId} not found. Cannot level up.", this);
            }
        }
    }
}