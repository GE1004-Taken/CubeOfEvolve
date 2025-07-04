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
        [Header("Models")]
        [SerializeField] private PlayerCore _playerCore;

        [Header("UI")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI levelUpText;
        [SerializeField] private Image glowImage;
        [SerializeField] private ParticleSystem levelUpParticle;

        //[Header("Audio")]
        //[SerializeField] private AudioSource audioSource;
        //[SerializeField] private AudioClip levelUpSE;

        private Tween _glowRotateTween;
        private CancellationTokenSource _cts;

        private void Start()
        {
            _playerCore.Level
                .Pairwise()
                .Where(pair => pair.Previous < pair.Current)
                .Subscribe(_ => PlayAsync().Forget());
        }

        public void Trigger()
        {
            PlayAsync().Forget();
        }


        public async UniTask PlayAsync()
        {
            // 既存の再生を中断
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            // パーティクル演出（即時）
            levelUpParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            levelUpParticle?.Play();

            // Tween中断
            _glowRotateTween?.Kill();

            // パネル初期化
            panel.SetActive(false);
            panel.SetActive(true);

            levelUpText.color = new Color(1, 1, 1, 0);
            levelUpText.transform.localScale = Vector3.zero;
            glowImage.transform.rotation = Quaternion.identity;

            // 音再生
            GameSoundManager.Instance.PlaySE("Sys_LvUp", "System");

            // 回転開始（ループ）
            _glowRotateTween = glowImage.transform
                .DORotate(new Vector3(0, 0, 360), 2f, RotateMode.FastBeyond360)
                .SetLoops(-1)
                .SetEase(Ease.Linear);

            // テキストアニメーション（フェード＋スケール）
            var fadeTween = levelUpText.DOFade(1f, 0.5f);
            var scaleTween = levelUpText.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);

            await UniTask.WhenAll(
                fadeTween.ToUniTask(cancellationToken: token),
                scaleTween.ToUniTask(cancellationToken: token)
            );

            // 一定時間表示
            await UniTask.Delay(1500, cancellationToken: token);

            // フェードアウト
            await levelUpText.DOFade(0f, 0.5f).ToUniTask(cancellationToken: token);

            // 後処理
            _glowRotateTween?.Kill();
            panel.SetActive(false);
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _glowRotateTween?.Kill();
        }
    }
}
