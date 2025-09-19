using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.Presenter
{
    public class PresenterTitleCanvas : MonoBehaviour
    {
        // -----SerializeField
        [Header("CheckCanvas")]
        [SerializeField] private CheckCanvasCtrl _checkCanvasCtrl;

        [SerializeField] private Button _resetButton;
        [SerializeField] private CheckDialogConfig _resetEvent;

        [SerializeField] private Button _quitButton;
        [SerializeField] private CheckDialogConfig _quitEvent;

        // -----UnityMessage
        private void Start()
        {
            if (!Initialize()) enabled = false;
        }

        // -----private
        private bool Initialize()
        {
            var isSuccess = false;

            // 依存チェック
            if (_checkCanvasCtrl == null || _resetEvent == null || _quitEvent == null) ;
            else isSuccess = true;

            if (_resetButton == null) ;
            else
            {
                _resetButton.onClick.AddListener(() => _checkCanvasCtrl.ShowCheckCanvas(_resetEvent, _resetButton));
                isSuccess = true;
            }

            if (_quitButton == null) ;
            else
            {
                _quitButton?.onClick.AddListener(() => _checkCanvasCtrl.ShowCheckCanvas(_quitEvent, _quitButton));
                isSuccess = true;
            }

            return isSuccess;
        }

    }
}