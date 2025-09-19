using Assets.IGC2025.Scripts.GameManagers;
using Assets.IGC2025.Scripts.View;
using R3;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.Presenter
{
    public sealed class PresenterResultCanvas : MonoBehaviour, IPresenter
    {
        // -----SerializedField
        [Header("Models")]

        [Header("Views")]
        [SerializeField] private ViewResultCanvas _view;
        [SerializeField] private GameEndController _gameEndController;
        [SerializeField] private Button[] _endGameButton;
        [SerializeField] private TextMeshProUGUI _finishTimeTextUGUI;
        [SerializeField] private Canvas _canvas;

        // -----Field
        public bool IsInitialized { get; private set; } = false;

        private GameManager _gameManager;
        private TimeManager _timeManager;

        // -----UnityMessage
        private void Start()
        {
            // 初期
            if (_canvas != null) _canvas.enabled = false;
        }

        // -----PublicMethod

        public void Initialize()
        {
            if (IsInitialized) return;

            _gameManager = GameManager.Instance;
            if (_gameManager != null)
                _gameManager.TryGetComponent(out _timeManager);

            if (_gameManager == null || _gameEndController == null || _canvas == null || _finishTimeTextUGUI == null)
            {
                Debug.LogError($"{nameof(PresenterResultCanvas)}: 依存が不足しています。", this);
                return;
            }

            // ゲーム終了/クリアで結果を表示
            _gameManager.CurrentGameState
                .Where(x => x == GameState.GAMEOVER || x == GameState.GAMECLEAR)
                .Subscribe(x =>
                {
                    _canvas.enabled = true;
                    _gameEndController.PlayGameEndSequence(x).Forget();

                    var time = _timeManager != null ? _timeManager.CurrentTimeSecond.CurrentValue : 0f;
                    _finishTimeTextUGUI.text = $"{time:F1}カウント";
                })
                .AddTo(this);

            // ボタンのリスナー（二重登録防止のため毎回クリア）
            if (_gameManager.SceneLoader != null && _endGameButton != null && _endGameButton.Length > 0)
            {
                foreach (var btn in _endGameButton)
                {
                    if (btn == null) continue;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => _gameManager.SceneLoader.ReloadScene());
                }
            }

            IsInitialized = true;

        }

        #region ModelToView



        #endregion


        #region ViewToModel



        #endregion

    }
}