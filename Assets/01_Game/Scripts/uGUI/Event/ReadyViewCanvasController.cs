using Assets.AT;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem; // ★ 新InputSystemのみを使用

namespace AT.uGUI
{
    /// <summary>
    /// 「Ready...」→ カウントダウン(3..1) → 「GO!」のUI演出。
    /// - 背景: RectTransformのYスケールで 0→1（Ready前）/ 1→0（Go後）
    /// - 入力(キー/マウス/パッド)が来たらカウントダウンだけスキップ（Ready/背景演出は通す）
    /// - 非スケール時間で進行（useUnscaledTime）
    /// - すべてキャンセル安全／Tweenは破棄時Kill
    /// </summary>
    [DisallowMultipleComponent]
    public class ReadyViewCanvasController : MonoBehaviour
    {
        // -----SerializedField
        [Header("参照")]
        [SerializeField] private Canvas canvasRef;
        [SerializeField] private TextMeshProUGUI startText;

        [Header("背景（RectTransform）")]
        [Tooltip("Readyの前にY=0→1で開き、Goの後にY=1→0で閉じる背景")]
        [SerializeField] private RectTransform backgroundRect;

        [Header("タイミング")]
        [Min(0f)] [SerializeField] private float preCountdownDelay = 1f;
        [Min(1)]  [SerializeField] private int countdownFrom = 3;

        [Header("背景スケール時間")]
        [Min(0f)] [SerializeField] private float bgScaleIn  = 0.25f;
        [Min(0f)] [SerializeField] private float bgScaleOut = 0.25f;

        [Header("時間軸")]
        [Tooltip("true: 非スケール時間で進行（Time.timeScaleの影響を受けない）")]
        [SerializeField] private bool useUnscaledTime = true;

        [Header("サウンド")]
        [SerializeField] private string countdownSeKey = "Sys_Click_1";
        [SerializeField] private string goSeKey       = "Sys_Start";
        [SerializeField] private string seGroup       = "System";

        [Header("文言")]
        [SerializeField] private string readyLabel = "Ready...";
        [SerializeField] private string goLabel    = "GO!";

        // -----UnityMessage

        private void Awake()
        {
            if (canvasRef == null) TryGetComponent(out canvasRef);
            if (startText == null) startText = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        // -----PublicMethods

        /// <summary>
        /// Ready演出本体。完了後に onReadyComplete を呼ぶ。
        /// キャンセル時も Canvas 表示・背景状態を復帰する。
        /// </summary>
        public async UniTask PlayReadySequenceAsync(Action onReadyComplete, CancellationToken token)
        {
            if (canvasRef == null || startText == null)
            {
                Debug.LogError("[ReadyViewCanvasController] Canvas または Text が取得できていません");
                return;
            }

            bool originalCanvasEnabled = canvasRef.enabled;
            Vector3? originalBgScale = backgroundRect ? backgroundRect.localScale : (Vector3?)null;

            try
            {
                // このキャンバスだけ見せる（存在しない場合はログのみ）
                var canvasMgr = CanvasCtrlManager.Instance;
                if (canvasMgr != null) canvasMgr.ShowOnlyCanvas(canvasRef.name);
                canvasRef.enabled = true;

                // 背景: Ready 前に Y=0→1 で開く
                if (backgroundRect != null)
                {
                    backgroundRect.DOKill();
                    // ★ X/Z は保ちつつ、Y=0 から開始
                    var ls = backgroundRect.localScale; 
                    backgroundRect.localScale = new Vector3(ls.x == 0 ? 1f : ls.x, 0f, ls.z == 0 ? 1f : ls.z);
                    await ScaleY(backgroundRect, 1f, bgScaleIn, token);
                }

                // テキスト初期化（Ready）
                var rect = startText.rectTransform;
                rect.DOKill();
                rect.localScale = Vector3.one;
                startText.text = readyLabel;

                // カメラ移動
                var camCtrl = CameraCtrlManager.Instance;
                camCtrl.ChangeCamera("Player Camera");

                // カメラの移動時間
                await UniTask.WaitForSeconds(camCtrl.CameraBlendTime,
                    ignoreTimeScale: useUnscaledTime, cancellationToken: token);

                // Ready の間
                await UniTask.WaitForSeconds(preCountdownDelay,
                    ignoreTimeScale: useUnscaledTime, cancellationToken: token);

                // --- カウントダウン vs スキップ を競合（カウントだけ飛ばす） ---
                using (var raceCts = CancellationTokenSource.CreateLinkedTokenSource(token))
                {
                    int winner = await UniTask.WhenAny(
                        RunCountdownOnlyAsync(raceCts.Token), // 0
                        WaitSkipAsync(raceCts.Token)          // 1
                    );

                    // 負け側を必ず止める（勝敗に関係なく cancel）
                    raceCts.Cancel();

                    // ※ここで countdown/skip を await したり Forget したりしない
                }



                // 「GO!」
                await ShowGoAsync(token);

                // 背景: Go 後に Y=1→0 で閉じる
                if (backgroundRect != null)
                {
                    await ScaleY(backgroundRect, 0f, bgScaleOut, token);
                }

                // 畳む
                canvasRef.enabled = false;
                onReadyComplete?.Invoke();
            }
            catch (OperationCanceledException)
            {
                // 外部キャンセル時、オブジェクトが破棄されない場合に備えてTweenを明示停止
                if (startText != null) startText.rectTransform.DOKill();
                if (backgroundRect != null) backgroundRect.DOKill();
            }
            finally
            {
                // 復帰
                canvasRef.enabled = originalCanvasEnabled;
                if (backgroundRect != null && originalBgScale.HasValue)
                    backgroundRect.localScale = originalBgScale.Value;
            }
        }

        // -----PrivateMethods

        /// <summary>
        /// 演出：カウントダウン
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async UniTask RunCountdownOnlyAsync(CancellationToken token)
        {
            for (int i = countdownFrom; i > 0; i--)
            {
                startText.text = i.ToString();
                AnimateTextPunch(startText);

                var gs = GameSoundManager.Instance;
                if (gs != null) gs.PlaySE(countdownSeKey, seGroup);

                await UniTask.WaitForSeconds(1f,
                    ignoreTimeScale: useUnscaledTime, cancellationToken: token);
            }
        }

        /// <summary>
        /// 演出：ぜろ
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async UniTask ShowGoAsync(CancellationToken token)
        {
            startText.text = goLabel;
            AnimateTextScaleUp(startText);

            var gss = GameSoundManager.Instance;
            if (gss != null) gss.PlaySE(goSeKey, seGroup);

            await UniTask.WaitForSeconds(0.6f,
                ignoreTimeScale: useUnscaledTime, cancellationToken: token);
        }

        
        /// <summary>
        /// スキップ入力があるまで待機する処理。
        /// </summary>
        private async UniTask WaitSkipAsync(CancellationToken token)
        {
            await UniTask.WaitUntil(() => CheckSkipPressed(), cancellationToken: token);
        }

        /// <summary>
        /// スキップ判定処理
        /// </summary>
        private bool CheckSkipPressed()
        {
            // キーボード
            if (Keyboard.current?.anyKey.wasPressedThisFrame == true) return true;

            // マウス
            if (Mouse.current?.leftButton.wasPressedThisFrame == true ||
                Mouse.current?.rightButton.wasPressedThisFrame == true) return true;

            // ゲームパッド（Start or Southボタン）
            if (Gamepad.current?.startButton.wasPressedThisFrame == true ||
                Gamepad.current?.buttonSouth.wasPressedThisFrame == true) return true;

            return false;
        }

        /// <summary>
        /// 演出：背景のYスケール変化
        /// </summary>
        /// <param name="rt"></param>
        /// <param name="toY"></param>
        /// <param name="duration"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async UniTask ScaleY(RectTransform rt, float toY, float duration, CancellationToken token)
        {
            if (rt == null) return;

            rt.DOKill();
            var from = rt.localScale;
            var to   = new Vector3(from.x == 0 ? 1f : from.x, toY, from.z == 0 ? 1f : from.z);

            // DOScale はVector3。Yだけ変えるためにlocalScale全体を扱う。
            var tween = rt.DOScale(to, Mathf.Max(duration, 0.0001f))
                          .SetEase(Ease.InOutSine)
                          .SetUpdate(useUnscaledTime)
                          .SetLink(gameObject);

            using (token.Register(() => { if (tween.IsActive()) tween.Kill(); }))
                await tween.ToUniTask(cancellationToken: token);
        }

        /// <summary>
        /// アニメ：パンチ
        /// </summary>
        /// <param name="text"></param>
        private void AnimateTextPunch(TextMeshProUGUI text)
        {
            var rect = text.rectTransform;
            rect.DOKill();
            rect.localScale = Vector3.one;
            rect.DOPunchScale(Vector3.one * 0.3f, 0.4f, 8, 0.8f)
                .SetUpdate(useUnscaledTime)
                .SetLink(gameObject);
        }

        /// <summary>
        /// アニメ：スケールアップ
        /// </summary>
        /// <param name="text"></param>
        private void AnimateTextScaleUp(TextMeshProUGUI text)
        {
            var rect = text.rectTransform;
            rect.DOKill();
            rect.localScale = Vector3.one * 0.6f;
            rect.DOScale(1f, 0.5f)
                .SetEase(Ease.OutBack)
                .SetUpdate(useUnscaledTime)
                .SetLink(gameObject);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (countdownFrom < 1) countdownFrom = 1;
            if (canvasRef == null) TryGetComponent(out canvasRef);
            if (startText == null) startText = GetComponentInChildren<TextMeshProUGUI>(true);
        }
#endif
    }
}
