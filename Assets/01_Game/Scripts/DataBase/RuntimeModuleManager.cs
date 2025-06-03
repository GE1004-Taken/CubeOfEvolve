using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using R3;
using System.Collections.Generic;
using UnityEngine;

namespace App.GameSystem.Modules
{
    /// <summary>
    /// ゲーム実行中のモジュールデータを一元的に管理するマネージャー。
    /// シングルトンパターンで実装されており、各種モジュールの状態変化を監視・操作します。
    /// </summary>
    public class RuntimeModuleManager : MonoBehaviour
    {
        // ----- Singleton
        public static RuntimeModuleManager Instance { get; private set; } 

        // ----- SerializedField 
        [SerializeField] private ModuleDataStore _moduleDataStore; // モジュールマスターデータを格納するデータストア。

        // ----- Private Members (内部データ)
        private readonly List<RuntimeModuleData> _allRuntimeModuleDataInternal = new List<RuntimeModuleData>(); // 全てのRuntimeModuleDataを管理する内部リスト。
        private readonly Subject<Unit> _collectionChangedSubject = new Subject<Unit>(); // モジュールコレクションの変更を通知するためのSubject。
        private Dictionary<int, RuntimeModuleData> _runtimeModuleDictionary = new Dictionary<int, RuntimeModuleData>(); // モジュールIDをキーとしたRuntimeModuleDataの高速アクセス用辞書。

        // ----- Public Properties (公開プロパティ)
        public IReadOnlyList<RuntimeModuleData> AllRuntimeModuleData => _allRuntimeModuleDataInternal;
        public Observable<Unit> OnAllRuntimeModuleDataChanged => _collectionChangedSubject.AsObservable();

        // ----- UnityMessage
        void Awake()
        {
            // シングルトン初期化
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            // 参照NullCheck
            if (_moduleDataStore == null)
            {
                Debug.LogError("RuntimeModuleManager: ModuleDataStoreが設定されていません！", this);
            }

            // 初期化
            InitializeAllModules();
        }

        void OnDestroy()
        {
            _collectionChangedSubject.Dispose(); // リソース開放
        }

        // ----- Private Methods (プライベートメソッド)
        /// <summary>
        /// 全てのマスターモジュールデータを基にRuntimeModuleDataを初期化します。
        /// ゲーム開始時に一度だけ呼ばれることを想定しています。
        /// </summary>
        private void InitializeAllModules()
        {
            // 参照NullCheck
            if (_moduleDataStore == null || _moduleDataStore.DataBase == null || _moduleDataStore.DataBase.ItemList == null)
            {
                Debug.LogError("RuntimeModuleManager: モジュールの初期化に必要なModuleDataStoreデータが利用できません。", this);
                return;
            }

            foreach (var masterData in _moduleDataStore.DataBase.ItemList)
            {
                if (!_runtimeModuleDictionary.ContainsKey(masterData.Id))
                {
                    RuntimeModuleData newRmd = new RuntimeModuleData(masterData);
                    _runtimeModuleDictionary.Add(masterData.Id, newRmd);
                    _allRuntimeModuleDataInternal.Add(newRmd); // 内部リストに追加。
                }
            }

            // 全ての要素を追加し終えた後に一度だけ変更を通知。
            _collectionChangedSubject.OnNext(Unit.Default);
            Debug.Log($"RuntimeModuleManager: {_allRuntimeModuleDataInternal.Count}個のモジュールを初期化しました。");
        }

        // ----- Public Methods (公開メソッド)
        /// <summary>
        /// 指定されたIDのRuntimeModuleDataを取得します。
        /// </summary>
        /// <param name="moduleId">取得するモジュールのID。</param>
        /// <returns>指定されたIDのRuntimeModuleData。見つからない場合はnull。</returns>
        public RuntimeModuleData GetRuntimeModuleData(int moduleId)
        {
            _runtimeModuleDictionary.TryGetValue(moduleId, out var rmd);
            return rmd;
        }

        /// <summary>
        /// モジュールの数量を変更します。
        /// </summary>
        /// <param name="moduleId">対象モジュールのID。</param>
        /// <param name="amount">数量の変更量。</param>
        public void ChangeModuleQuantity(int moduleId, int amount)
        {
            if (_runtimeModuleDictionary.TryGetValue(moduleId, out RuntimeModuleData rmd))
            {
                rmd.ChangeQuantity(amount); // RuntimeModuleData内のReactivePropertyを更新。
                // 個別のRuntimeModuleDataが変更された場合、コレクションの変更も通知。
                _collectionChangedSubject.OnNext(Unit.Default);
                Debug.Log($"RuntimeModuleManager: モジュールID {moduleId} の数量を {amount} 変更しました。現在の数量: {rmd.CurrentQuantityValue}");
            }
            else
            {
                Debug.LogWarning($"RuntimeModuleManager: ID {moduleId} のモジュールが見つかりません。数量を変更できません。", this);
            }
        }

        /// <summary>
        /// モジュールのレベルを上げる関数。
        /// </summary>
        /// <param name="moduleId">対象モジュールのID。</param>
        public void LevelUpModule(int moduleId)
        {
            if (_runtimeModuleDictionary.TryGetValue(moduleId, out RuntimeModuleData rmd))// 高速アクセス
            {
                rmd.LevelUp(); // RuntimeModuleData内のReactivePropertyを更新。
                // 個別のRuntimeModuleDataが変更された場合、コレクションの変更も通知。
                _collectionChangedSubject.OnNext(Unit.Default);
                Debug.Log($"RuntimeModuleManager: モジュールID {moduleId} のレベルを上げました。現在のレベル: {rmd.CurrentLevelValue}");
            }
            else
            {
                Debug.LogWarning($"RuntimeModuleManager: ID {moduleId} のモジュールが見つかりません。レベルアップできません。", this);
            }
        }
    }
}