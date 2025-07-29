// AT
// ポーズ画面での処理を実行する。

using Assets.IGC2025.Scripts.GameManagers;
using AT.uGUI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.Presenter
{
    public class PresenterPauseCanvas : MonoBehaviour
    {
        // -----
        // -----SerializeField
        [Header("Models")]
        [SerializeField] private GameManager gameManager;

        [Header("Views")]
        [SerializeField] private Canvas canvas;

        // -----UnityMessage
        private void Start()
        {
            if (!Initialize()) enabled = false;
        }

        // -----private
        private bool Initialize()
        {
            // 参照確認
            if (gameManager == null)
            {
                Debug.LogWarning($"PresenterPauseCanvas: 参照切れのため代入");
                gameManager = GameManager.Instance;
            }

            // 依存チェック
            if (gameManager == null || canvas == null)
            {
                Debug.LogWarning($"PresenterPauseCanvas: 依存が不足のため処理中止");
                return false;
            }

            var canvasCtrl = canvas.GetComponent<CanvasCtrl>();

            gameManager.CurrentGameState
                .Skip(1)
                .Subscribe(
                x =>
                {
                    if (x == GameState.PAUSE)
                        canvasCtrl.OnOpenCanvas();
                    else
                        canvasCtrl.OnCloseCanvas();
                })
                .AddTo(this);

            return true;
        }

    }
}