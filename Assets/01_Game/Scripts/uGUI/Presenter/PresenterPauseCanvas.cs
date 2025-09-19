using Assets.IGC2025.Scripts.GameManagers;
using Assets.IGC2025.Scripts.View;
using AT.uGUI;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Assets.IGC2025.Scripts.Presenter
{
    public class PresenterPauseCanvas : MonoBehaviour, IPresenter
    {
        [Header("Models")]
        [SerializeField] private GameManager gameManager;

        [Header("Views")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private ViewPauseCanvas pauseView;

        public bool IsInitialized { get; private set; } = false;

        private CanvasCtrl _canvasCtrl;

        private void Start()
        {
            if (canvas != null) canvas.enabled = false; // 初期は閉
        }

        public void Initialize()
        {
            if (IsInitialized) return;

            if (gameManager == null) gameManager = GameManager.Instance;

            if (gameManager == null || canvas == null || pauseView == null)
            {
                Debug.LogWarning($"{nameof(PresenterPauseCanvas)}: 依存が不足のため初期化を中止します。", this);
                return;
            }

            _canvasCtrl = canvas.GetComponent<CanvasCtrl>();
            if (_canvasCtrl == null)
            {
                Debug.LogWarning($"{nameof(PresenterPauseCanvas)}: CanvasCtrl が見つかりません。", this);
                return;
            }

            // GameState 変化に合わせて Pause を開閉
            gameManager.CurrentGameState
                .Skip(1)
                .Subscribe(state =>
                {
                    if (state == GameState.PAUSE)
                    {
                        // 開く：Canvas を開→View 準備→Open アニメ
                        _canvasCtrl.OnOpenCanvas();
                        pauseView.PrepareInitialStatesForOpen();
                        UniTask.Void(async () =>
                        {
                            await pauseView.PlayOpenAsync();
                        });
                    }
                    else
                    {
                        // 閉じる：View Close アニメ→Canvas を閉
                        UniTask.Void(async () =>
                        {
                            await pauseView.PlayCloseAsync();
                            _canvasCtrl.OnCloseCanvas();
                        });
                    }
                })
                .AddTo(this);

            IsInitialized = true;
        }
    }
}
