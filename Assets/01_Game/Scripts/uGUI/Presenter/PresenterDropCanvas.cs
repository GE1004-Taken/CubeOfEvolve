using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Assets.IGC2025.Scripts.View;
using AT.uGUI;
using R3;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.Presenter
{
    public class PresenterDropCanvas : MonoBehaviour
    {
        // ----- SerializedField
        [Header("Models")]
        [SerializeField] private RuntimeModuleManager _runtimeModuleManager; // ランタイムモジュールデータを管理するマネージャー。
        [SerializeField] private ModuleDataStore _moduleDataStore; // モジュールマスターデータを管理するデータストア。
        [Header("Views")]
        [SerializeField] private ViewDropCanvas _dropView; // ドロップUIを表示するViewコンポーネント。
        [Header("Views_Hovered")]
        [SerializeField] private TextMeshProUGUI _unitName;
        [SerializeField] private TextMeshProUGUI _infoText; // 説明文
        [SerializeField] private TextMeshProUGUI _level; // 
        [SerializeField] private Image _image; // 
        [SerializeField] private Image _icon; // 
        [SerializeField] private TextMeshProUGUI _atk; // 
        [SerializeField] private TextMeshProUGUI _rpd; // 
        [SerializeField] private TextMeshProUGUI _prc; // 

        // ----- Private Members (内部データ)
        private const int NUMBER_OF_OPTIONS = 3; // 提示するモジュールの数。
        private List<int> _candidateModuleIds = new List<int>();

        // ----- UnityMessage
        private void Start()
        {
            if (_dropView != null)
            {
                _dropView.OnModuleSelected
                    .Subscribe(x => HandleModuleSelected(x))
                    .AddTo(this); // R3 の AddTo(CompositeDisposable) を使用。
                _dropView.OnModuleHovered
                    .Subscribe(x => HandleModuleHovered(x))
                    .AddTo(this);
            }
        }
        private void Awake()
        {
            // 依存関係が未設定の場合、シーンから取得を試みる
            if (_runtimeModuleManager == null) _runtimeModuleManager = RuntimeModuleManager.Instance;

            // 必須の依存関係が揃っているかチェック
            if (_runtimeModuleManager == null || _moduleDataStore == null || _dropView == null)
            {
                Debug.LogError("Drop_Presenter: RuntimeModuleManager, ModuleDataStore, またはDrop_Viewが設定されていません。このコンポーネントを無効にします。", this);
                enabled = false;
            }
        }

        #region ModelToView

        /// <summary>
        /// ドロップ選択UIを表示する準備をし、Viewに表示を依頼します。
        /// </summary>
        public void PrepareAndShowDropUI()
        {
            if (_runtimeModuleManager == null || _moduleDataStore == null || _dropView == null)
            {
                Debug.LogError("依存関係が満たされていません！");
                return;
            }

            var gameState = GameManager.Instance.CurrentGameState.CurrentValue;
            var displayIds = _runtimeModuleManager.GetDisplayModuleIds(NUMBER_OF_OPTIONS, gameState);
            var candidatePool = _runtimeModuleManager.AllRuntimeModuleData
                                  .Where(m => m.CurrentLevelValue < 5).ToList();

            if (displayIds.Count == 0)
            {
                Debug.Log("全モジュールが最大レベル。選択肢なし。");
                var Player = FindFirstObjectByType(typeof(PlayerCore));
                Player.GetComponent<PlayerCore>().ReceiveMoney(500); // 500金追加
                return;
            }

            _dropView.DisplayModulesByIdOrRandom(displayIds, candidatePool, _moduleDataStore);
            _dropView.GetComponent<CanvasCtrl>().OnOpenCanvas();
        }

        #endregion


        #region ViewToModel

        /// <summary>
        /// ユーザーがモジュールを選択した際のイベントハンドラ。
        /// Viewからのイベント（R3で購読）によって呼び出されます。
        /// </summary>
        /// <param name="selectedModuleId">選択されたモジュールのID。</param>
        private void HandleModuleSelected(int selectedModuleId)
        {
            if (selectedModuleId == -1) // 何でもないものを選択した場合
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

            _dropView.gameObject.GetComponent<CanvasCtrl>().OnCloseCanvas();

            // 必要であれば、UIの更新など、ゲーム全体の状態に応じた後処理を呼び出す
            // 例: GameManager.Instance.OnPlayerModuleUpgraded();
        }

        /// <summary>
        /// モジュールにマウスオーバーした際のイベントハンドラ。
        /// 説明文を更新します。
        /// </summary>
        /// <param name="EnterModuleId"></param>
        private void HandleModuleHovered(int EnterModuleId)
        {
            var module = _moduleDataStore.FindWithId(EnterModuleId);
            var Rruntime = RuntimeModuleManager.Instance.GetRuntimeModuleData(EnterModuleId);

            _unitName.text = module.ViewName;
            _infoText.text = module.Description;
            _level.text = $"{Rruntime.CurrentLevelValue}";
            _image.sprite = module.MainSprite;
            _icon.sprite = module.BlockSprite;
            _atk.text = $"{module.ModuleState?.Attack ?? 0}";
            _rpd.text = $"{module.ModuleState?.Interval ?? 0}";
            _prc.text = $"{module.BasePrice}";
        }


        #endregion

    }
}