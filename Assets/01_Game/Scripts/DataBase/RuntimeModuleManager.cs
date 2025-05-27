// RuntimeModuleManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // LINQ メソッド (Select, ToDictionary など) のために必要
using App.BaseSystem.DataStores.ScriptableObjects.Modules; // ModuleDataStore の名前空間

namespace App.GameSystem.Modules
{
    /// <summary>
    /// プレイヤーが所有するモジュールのランタイムデータを管理するクラス。
    /// 初期化、データ取得、操作、セーブ/ロードデータの生成・適用を担当する。
    /// </summary>
    public class RuntimeModuleManager : MonoBehaviour
    {
        // ------------------ 依存関係
        // ModuleDataStore への参照（マスターデータを取得するため）
        [SerializeField] private ModuleDataStore _moduleDataStore;

        // ------------------ ランタイムデータの保持
        // モジュールIDをキーに、RuntimeModuleData インスタンスを管理
        private Dictionary<int, RuntimeModuleData> _runtimeModules = new Dictionary<int, RuntimeModuleData>();

        // ------------------ シングルトンパターン（例）
        // GameManagerのような上位クラスがこれを管理する場合、シングルトンは不要なこともあります
        public static RuntimeModuleManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Initialize();
                Instance = this;
                // シーン遷移してもデータを保持したい場合はコメント解除
                //DontDestroyOnLoad(gameObject);
            }
        }
        // ------------------ シングルトンパターン終わり

        // ------------------ 初期化
       
        /// ランタイムモジュールデータを初期化します。
        /// ModuleDataStoreに含まれる全てのモジュールを初期状態でプレイヤーが持つように設定します。
        /// 通常、ゲーム開始時やシーンロード時に呼び出されます。
        /// </summary>
        public void Initialize()
        {
            if (_moduleDataStore == null)
            {
                Debug.LogError("RuntimeModuleManager: ModuleDataStore is not assigned in Inspector! Cannot initialize modules.");
                return;
            }
            if (_moduleDataStore.DaraBase == null)
            {
                Debug.LogError("RuntimeModuleManager: ModuleDataStore.DaraBase is NULL! Master data is not loaded. Cannot initialize modules.");
                return;
            }
            if (_moduleDataStore.DaraBase.ItemList == null || _moduleDataStore.DaraBase.ItemList.Count == 0)
            {
                Debug.LogWarning("RuntimeModuleManager: ModuleDataStore.DaraBase.ItemList is EMPTY. No master ModuleData to initialize with.");
                return;
            }

            _runtimeModules.Clear(); // 既存のランタイムデータをクリア

            // (変更点: Storeから全てのモジュールを取得し、ランタイムデータとして追加)
            foreach (ModuleData masterModuleData in _moduleDataStore.DaraBase.ItemList)
            {
                if (masterModuleData != null)
                {
                    RuntimeModuleData runtimeModule = new RuntimeModuleData(masterModuleData);
                    // 既に同じIDが存在しないかチェック (エラー防止、通常は発生しないはずだが念のため)
                    if (_runtimeModules.ContainsKey(runtimeModule.Id))
                    {
                        Debug.LogWarning($"RuntimeModuleManager: Duplicate module ID {runtimeModule.Id} found in ModuleDataStore.DaraBase.ItemList. Skipping duplicate.");
                    }
                    else
                    {
                        _runtimeModules.Add(runtimeModule.Id, runtimeModule);
                    }
                }
                else
                {
                    Debug.LogWarning("RuntimeModuleManager: Null ModuleData found in ModuleDataStore.DaraBase.ItemList. Skipping.");
                }
            }

            Debug.Log($"RuntimeModuleManager Initialized. Managing {_runtimeModules.Count} player runtime modules, taken from ModuleDataStore.");
        }

        // ------------------ ランタイムデータへのアクセス
        /// <summary>
        /// 指定されたIDのランタイムモジュールデータを取得します。
        /// </summary>
        /// <param name="id">取得したいモジュールのID。</param>
        /// <returns>対応するRuntimeModuleDataインスタンス。見つからない場合はnull。</returns>
        public RuntimeModuleData GetRuntimeModuleData(int id)
        {
            _runtimeModules.TryGetValue(id, out RuntimeModuleData module);
            return module;
        }

        /// <summary>
        /// 全てのランタイムモジュールデータを読み取り専用のコレクションとして取得します。
        /// </summary>
        public IReadOnlyCollection<RuntimeModuleData> AllRuntimeModuleData => _runtimeModules.Values;

        // ------------------ ランタイムデータの操作（プレイヤーモジュール特有のビジネスロジック）
        /// <summary>
        /// 指定されたモジュールのレベルを1上げます。
        /// </summary>
        /// <param name="id">レベルアップさせるモジュールのID。</param>
        public void LevelUpModule(int id)
        {
            RuntimeModuleData module = GetRuntimeModuleData(id);
            if (module != null)
            {
                module.CurrentLevel++; // ランタイムデータを更新

                // マスターデータを参照して、レベルアップ後の影響を計算するなど
                ModuleData masterData = _moduleDataStore.FindWithId(id);
                string moduleName = masterData?.Name ?? "Unknown Module";
                Debug.Log($"Player's Module {moduleName} (ID:{id}) level up to {module.CurrentLevel}!");
            }
            else
            {
                Debug.LogWarning($"Attempted to level up non-existent player module with ID: {id}");
            }
        }

        /// <summary>
        /// 指定されたモジュールの所持数を変更します。
        /// </summary>
        /// <param name="id">所持数を変更するモジュールのID。</param>
        /// <param name="changeAmount">変更量（加算または減算）。</param>
        public void ChangeModuleQuantity(int id, int changeAmount)
        {
            RuntimeModuleData module = GetRuntimeModuleData(id);
            if (module != null)
            {
                module.Quantity += changeAmount;
                if (module.Quantity < 0) module.Quantity = 0; // 所持数が負にならないように

                ModuleData masterData = _moduleDataStore.FindWithId(id);
                string moduleName = masterData?.Name ?? "Unknown Module";
                Debug.Log($"Player's Module {moduleName} (ID:{id}) quantity changed by {changeAmount}. Current: {module.Quantity}");

                if (module.Quantity == 0)
                {
                    Debug.Log($"Player's Module {moduleName} (ID:{id}) ran out.");
                    // 必要であれば、ここでモジュールを完全に削除するロジックを呼び出す
                    // RemoveModule(id);
                }
            }
            else
            {
                Debug.LogWarning($"Attempted to change quantity of non-existent player module with ID: {id}");
            }
        }

        // モジュールを完全に削除する例 (所持数が0になった場合など)
        public void RemoveModule(int id)
        {
            if (_runtimeModules.Remove(id))
            {
                ModuleData masterData = _moduleDataStore.FindWithId(id);
                string moduleName = masterData?.Name ?? "Unknown Module";
                Debug.Log($"Player's Module {moduleName} (ID:{id}) has been removed.");
            }
            else
            {
                Debug.LogWarning($"Attempted to remove non-existent player module with ID: {id}");
            }
        }


        

        // ------------------ セーブ/ロード処理のためのデータ提供/適用
        /// <summary>
        /// 現在の全てのプレイヤーモジュールデータをセーブデータ形式に変換し、提供します。
        /// </summary>
        /// <returns>保存すべきモジュールの状態のリスト。</returns>
        public List<RuntimeModuleData.ModuleSaveState> GeneratePlayerModuleSaveData()
        {
            return _runtimeModules.Values
                .Select(m => new RuntimeModuleData.ModuleSaveState
                {
                    id = m.Id,
                    level = m.CurrentLevel,
                    quantity = m.Quantity // Quantity もセーブ対象に追加
                })
                .ToList();
        }

        /// <summary>
        /// ロードされたセーブデータを受け取り、プレイヤーモジュールデータを更新します。
        /// </summary>
        /// <param name="savedStates">ロードされたモジュールのセーブデータのリスト。</param>
        public void ApplyPlayerModuleSaveData(List<RuntimeModuleData.ModuleSaveState> savedStates)
        {
            // まずは既存のランタイムデータをクリアし、セーブデータで上書きする準備
            _runtimeModules.Clear();

            // ロードされたセーブデータの内容で、対応するランタイムモジュールを生成・更新
            foreach (var state in savedStates)
            {
                // セーブデータから復元する際、マスターデータが存在するか確認
                ModuleData masterModuleData = _moduleDataStore.FindWithId(state.id);
                if (masterModuleData != null)
                {
                    // セーブデータからRuntimeModuleDataを生成
                    RuntimeModuleData runtimeModule = new RuntimeModuleData(state);
                    // ここでNameプロパティをマスターデータから設定
                    runtimeModule.Name = masterModuleData.Name;
                    _runtimeModules.Add(runtimeModule.Id, runtimeModule);
                }
                else
                {
                    // セーブデータに存在するが、マスターデータには存在しないモジュールの場合
                    // （例: ゲーム更新で削除されたモジュールなど）
                    Debug.LogWarning($"Saved data for player module ID {state.id} found but corresponding master module data not found. Skipping.");
                }
            }
            Debug.Log($"Applied save data to {_runtimeModules.Count} player runtime modules.");
        }
    }
}