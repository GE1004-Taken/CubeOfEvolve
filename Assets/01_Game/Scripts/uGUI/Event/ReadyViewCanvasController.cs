using Assets.AT;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace AT.uGUI
{
    public class ReadyViewCanvasController : MonoBehaviour
    {
        private Canvas _canvas;
        private TextMeshProUGUI _startText;
        private CameraCtrlManager _cameraCtrlManager;

        [SerializeField] private float preCountdownDelay = 1f;
        [SerializeField] private float countInterval = 1f;
        [SerializeField] private int countdownFrom = 3;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _startText = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            _cameraCtrlManager = CameraCtrlManager.Instance;
        }

        public async UniTask PlayReadySequenceAsync(System.Action onReadyComplete)
        {
            if (_canvas == null || _startText == null)
            {
                Debug.LogError("[ReadyViewCanvasController] Canvas または Text が取得できていません");
                return;
            }

            _cameraCtrlManager = CameraCtrlManager.Instance;

            _canvas.enabled = true;
            _startText.text = "Ready...";
            Time.timeScale = 0f;

            // カメラ移動
            _cameraCtrlManager.ChangeCamera("Player Camera");
            await UniTask.WaitForSeconds(_cameraCtrlManager.CameraBlendTime, ignoreTimeScale: true);

            // カウントダウン前の待機
            await UniTask.WaitForSeconds(preCountdownDelay, ignoreTimeScale: true);

            for (int i = countdownFrom; i > 0; i--)
            {
                _startText.text = i.ToString();
                AnimateTextPunch(_startText);

                // カウントダウン効果音（ここに記述してください）
                GameSoundManager.Instance.PlaySE("Sys_Click_1", "System");

                await UniTask.WaitForSeconds(countInterval, ignoreTimeScale: true);
            }

            _startText.text = "START!";
            AnimateTextScaleUp(_startText);

            // スタート効果音（ここに記述してください）
            GameSoundManager.Instance.PlaySE("Sys_Start", "System");

            await UniTask.WaitForSeconds(0.6f, ignoreTimeScale: true);

            _canvas.enabled = false;
            Time.timeScale = 1f;
            onReadyComplete?.Invoke();
        }

        private void AnimateTextPunch(TextMeshProUGUI text)
        {
            text.rectTransform.DOPunchScale(Vector3.one * 0.3f, 0.4f, 8, 0.8f)
                .SetUpdate(true); // Timescale無視
        }

        private void AnimateTextScaleUp(TextMeshProUGUI text)
        {
            text.rectTransform.localScale = Vector3.one * 0.6f;
            text.rectTransform.DOScale(1f, 0.5f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
    }
}
