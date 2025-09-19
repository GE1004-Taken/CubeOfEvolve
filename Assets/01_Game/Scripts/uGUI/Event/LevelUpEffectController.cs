using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.AT
{
    public class LevelUpEffectController : MonoBehaviour
    {
        // -----SerializeField
        [Header("Models")]
        [SerializeField] private PlayerCore _playerCore;

        [Header("UI")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI levelUpText;
        [SerializeField] private Image glowImage;
        [SerializeField] private ParticleSystem levelUpParticle;

        // -----Field
        private Tween _glowRotateTween;
        private CancellationTokenSource _cts;

        // -----unityMessage
        private void Start()
        {
            _playerCore.Level
                .Pairwise()
                .Where(pair => pair.Previous < pair.Current)
                .Subscribe(_ => PlayFancyLevelUpAsync().Forget());
        }

        public void Trigger()
        {
            PlayFancyLevelUpAsync().Forget();
        }

        // -----PublicMethod
        public async UniTask PlayFancyLevelUpAsync()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            RectTransform rect = panel.GetComponent<RectTransform>();
            float targetY = 450f;
            Vector2 originalPos = rect.anchoredPosition;

            // 初期化
            rect.anchoredPosition = new Vector2(0, targetY - 300f); // 下から
            panel.GetComponent<CanvasGroup>().alpha = 0f;
            panel.SetActive(true);

            levelUpText.text = "LEVEL UP!";
            levelUpText.alpha = 1f;
            DOTweenTMPAnimator animator = new DOTweenTMPAnimator(levelUpText);

            // パーティクル＆音
            levelUpParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            levelUpParticle?.Play();
            GameSoundManager.Instance.PlaySE("Sys_LvUp", "System");

            // パネル：下からフェードイン＋上昇
            await UniTask.WhenAll(
                rect.DOAnchorPosY(targetY, 0.5f).SetEase(Ease.OutCubic).ToUniTask(token),
                panel.GetComponent<CanvasGroup>().DOFade(1f, 0.5f).ToUniTask(token)
            );

            // 各文字ポップ
            for (int i = 0; i < animator.textInfo.characterCount; i++)
            {
                if (!animator.textInfo.characterInfo[i].isVisible) continue;

                animator.DOScaleChar(i, 1.3f, 0.3f)
                    .From(0f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(i * 0.05f);
            }

            await UniTask.Delay(2000, cancellationToken: token);

            // パネル：上へ退場しながらフェードアウト
            await UniTask.WhenAll(
                rect.DOAnchorPosY(targetY + 300f, 0.5f).SetEase(Ease.InCubic).ToUniTask(token),
                panel.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).ToUniTask(token)
            );

            panel.SetActive(false);
            rect.anchoredPosition = originalPos;
        }

        // -----PrivateMethod
        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _glowRotateTween?.Kill();
        }
    }
}
