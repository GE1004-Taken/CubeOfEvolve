using Assets.IGC2025.Scripts.GameManagers;
using R3;
using UnityEngine;

namespace Assets.IGC2025.Scripts.View
{
    public class ViewResultCanvas : MonoBehaviour
    {
        // -----SerializedField

        [SerializeField] private Canvas _resultCanvas;
        [SerializeField] private Canvas _clearCanvas;
        [SerializeField] private Canvas _overCanvas;
        // -----Field

        // -----CompositeDisposable
        private CompositeDisposable _disposables = new CompositeDisposable();

        // ----- Events (PresenterがR3で購読する)

        // ----- UnityMessage

        private void OnDestroy()
        {
            _disposables.Dispose(); // オブジェクトが破棄される際に、すべての購読を解除
        }

        // -----PrivateMethods

        // -----PublicMethods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameState"></param>
        public void ShowCanvas(GameState gameState)
        {

            _resultCanvas.enabled = true;
            if (gameState == GameState.GAMEOVER)
            {
                _overCanvas.enabled = true;
            }
            else if (gameState == GameState.GAMECLEAR)
            {
                _clearCanvas.enabled = true;
            }
        }
    }
}