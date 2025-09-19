using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// ロゴ演出を再生しつつ、次シーンを非表示で先読み（allowSceneActivation=false）しておき、
/// 演出完了またはユーザのスキップ入力を契機に「アクティベートだけ」を行うコントローラ。
/// これにより、切替え直後のヒッチ(一瞬のカクつき)を体感的に低減する。
/// </summary>
/// <remarks>
/// - DOTween を TimeScale 非依存(SetUpdate(true))で駆動し、演出がポーズ/初期化に影響されないようにする。<br/>
/// - 競合回避のため、演出タスクとスキップ待ちを WhenAny で競争させ、勝者確定後に残りをキャンセル。<br/>
/// - 切替えフレームは LastPostLateUpdate に寄せて、初期化スパイクが画面に乗りにくいタイミングでアクティベート。<br/>
/// - 先読みが無かった/間に合わなかった場合はフォールバックとして通常の LoadSceneAsync を使用。<br/>
/// </remarks>
/// <example>
/// 使い方:
/// 1) CanvasGroup(ロゴ等) を <see cref="logo"/> に割り当て、各フェード秒数を設定。<br/>
/// 2) <see cref="nextSceneName"/> にビルド設定済みシーン名を指定。<br/>
/// 3) スキップ可能にしたい場合は <see cref="allowSkip"/> を true。<br/>
/// </example>
public class OpeningController : MonoBehaviour
{
    // ----- SerializedField
    [Header("参照")]
    [SerializeField] private CanvasGroup logo; // ロゴ等のフェード対象（任意。null ならフェード処理はスキップ）

    [Header("タイミング(秒)")]
    [Min(0f)][SerializeField] private float logoFadeIn = 1.2f;  // フェードイン時間
    [Min(0f)][SerializeField] private float logoHold   = 1.0f;  // 表示維持時間
    [Min(0f)][SerializeField] private float logoFadeOut= 0.8f;  // フェードアウト時間

    [Header("次シーン")]
    [SerializeField] private string nextSceneName = "";          // 切替え先シーン名（Build Settings 必須）

    [Header("オプション")]
    [SerializeField, Tooltip("スキップ可能？")] private bool allowSkip = true;

    // ----- Field
    private CancellationTokenSource _lifeCts;     // このコンポーネントのライフタイムに紐づく CTS
    private bool _sceneSwitching;                 // 二重遷移の防止フラグ
    private AsyncOperation _preloadOp;            // 先読み用 AsyncOperation（allowSceneActivation=false）
    private Sequence _logoSeq;                    // ロゴ演出の DOTween シーケンス

    // ----- UnityMessage
    private void Awake()
    {
        // 起動時はロゴを透明に
        if (logo != null) logo.alpha = 0f;
        _lifeCts = new CancellationTokenSource();
    }

    private void Start()
    {
        // 先読み開始（アクティベートはしない）
        PreloadNextScene(_lifeCts.Token).Forget();

        // 演出完了 or スキップ入力のどちらかでシーン切替えへ進む
        RunAsync().Forget();
    }

    private void OnDestroy()
    {
        // ライフタイム終了時はキャンセル/破棄
        _lifeCts?.Cancel();
        _lifeCts?.Dispose();

        // 再生中のシーケンスがあれば明示的にKill
        if (_logoSeq != null && _logoSeq.IsActive()) _logoSeq.Kill();
    }

    // ----- PrivateMethod
    // ---------- Main flow

    /// <summary>
    /// ロゴ演出とスキップ入力待ちを競争させ、勝った方でシーンをアクティベートするメインフロー。
    /// </summary>
    private async UniTaskVoid RunAsync()
    {
        // 破棄トークンと寿命トークンを合成（どちらでも止まる）
        using var raceCts = CancellationTokenSource.CreateLinkedTokenSource(
            this.GetCancellationTokenOnDestroy(), _lifeCts.Token);
        var token = raceCts.Token;

        try
        {
            var seqTask  = PlayOpeningSequence(token);                 // 演出タスク
            var skipTask = allowSkip ? WaitSkipAsync(token)            // スキップ待ち（無効ならNever）
                                     : UniTask.Never(token);

            // どちらかが終わればOK。残りはキャンセルして競合を断つ
            await UniTask.WhenAny(seqTask, skipTask);
            raceCts.Cancel();

            // 切替え：先読みを活かしてアクティベート
            await ActivateNextSceneAsync(this.GetCancellationTokenOnDestroy());
        }
        catch (OperationCanceledException)
        {
            // 破棄時なども、可能なら遷移を試みる（多重防止済み）
            await ActivateNextSceneAsync(CancellationToken.None);
        }
        catch (Exception e)
        {
            Debug.LogError($"[OpeningController] 予期せぬ例外: {e}");
            await ActivateNextSceneAsync(CancellationToken.None);
        }
    }

    // ---------- Presentation

    /// <summary>
    /// ロゴ: フェードイン → 表示維持 → フェードアウト を再生。TimeScale 非依存。
    /// </summary>
    /// <param name="token">キャンセル用トークン</param>
    private async UniTask PlayOpeningSequence(CancellationToken token)
    {
        if (logo == null) return; // ロゴ未指定なら何もしない

        logo.alpha = 0f;

        // DOTween シーケンス構築（ゲーム終了時に自動Killされるようにリンク）
        _logoSeq = DOTween.Sequence()
            .Append(logo.DOFade(1f, logoFadeIn).SetEase(Ease.InOutSine))
            .AppendInterval(logoHold)
            .Append(logo.DOFade(0f, logoFadeOut).SetEase(Ease.InOutSine))
            .SetUpdate(true) // TimeScale 無視（ポーズ等でも進行）
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy);

        // 外部キャンセルでシーケンスをKill
        using (token.Register(() => { if (_logoSeq.IsActive()) _logoSeq.Kill(); }))
        {
            await _logoSeq.ToUniTask(cancellationToken: token);
        }
    }

    /// <summary>
    /// スキップ入力を待つ（Input System）。いずれかの入力が押されたフレームで true。
    /// </summary>
    /// <param name="token">キャンセル用トークン</param>
    private async UniTask WaitSkipAsync(CancellationToken token)
    {
        await UniTask.WaitUntil(CheckSkipPressed, cancellationToken: token);
    }

    /// <summary>
    /// スキップ条件: キーボード任意キー、マウス左右、ゲームパッド Start/South、タッチ。
    /// </summary>
    private bool CheckSkipPressed()
    {
        // Keyboard
        if (Keyboard.current?.anyKey.wasPressedThisFrame == true) return true;

        // Mouse
        if (Mouse.current?.leftButton.wasPressedThisFrame == true ||
            Mouse.current?.rightButton.wasPressedThisFrame == true) return true;

        // Gamepad (Start / South)
        if (Gamepad.current?.startButton.wasPressedThisFrame == true ||
            Gamepad.current?.buttonSouth.wasPressedThisFrame == true) return true;

        // Touch (ざっくり押下検知)
        if (Touchscreen.current?.primaryTouch.press.wasPressedThisFrame == true) return true;

        return false;
    }

    // ---------- Loading

    /// <summary>
    /// 次シーンを裏で先読み（<see cref="AsyncOperation.allowSceneActivation"/> = false）。
    /// 進捗 0.9f 到達でロード完了とみなし、アクティベーション待ちに入る。
    /// </summary>
    /// <param name="token">キャンセル用トークン</param>
    private async UniTask PreloadNextScene(CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(nextSceneName)) return;

        // 既にキックしていれば再利用
        if (_preloadOp == null)
        {
            _preloadOp = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
            _preloadOp.allowSceneActivation = false; // ここでは表示しない
        }

        // 0.9 で読み込み完了（残りはアクティベーション＝表示）
        await UniTask.WaitUntil(() => _preloadOp.progress >= 0.9f, cancellationToken: token);
    }

    /// <summary>
    /// 一度だけシーンを有効化。先読みがあればそれをアクティベート、無ければ通常ロードにフォールバック。
    /// 切替えフレームは LastPostLateUpdate に寄せ、さらに1フレーム流して Awake/Start の初期化を消化する。
    /// </summary>
    /// <param name="token">キャンセル用トークン</param>
    private async UniTask ActivateNextSceneAsync(CancellationToken token)
    {
        if (_sceneSwitching) return; // 二重遷移ガード

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogError("[OpeningController] 次シーンが空欄");
            return;
        }

        _sceneSwitching = true;

        // 見切れ防止に軽く黒へ（ロゴが無ければスキップ）
        if (logo != null)
        {
            try
            {
                await logo.DOFade(0f, 0.2f).SetUpdate(true).ToUniTask(cancellationToken: token);
            }
            catch
            {
                // 破棄時などは握りつぶし
            }
        }

        if (_preloadOp != null)
        {
            // 念のため完了を保証（演出が早く終わったケース）
            if (_preloadOp.progress < 0.9f)
                await UniTask.WaitUntil(() => _preloadOp.progress >= 0.9f, cancellationToken: token);

            // ヒッチを目に乗せないため、フレーム末尾でアクティベート
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            _preloadOp.allowSceneActivation = true;

            // 完了待ち & さらに1フレーム流して Awake/Start の初期化を消化
            await UniTask.WaitUntil(() => _preloadOp.isDone, cancellationToken: token);
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        }
        else
        {
            // フォールバック：先読みが無い場合は普通にロード
            await SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single)
                              .ToUniTask(cancellationToken: token);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// エディタ時の入力検証。必須プロパティ未設定などの早期警告。
    /// </summary>
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(nextSceneName))
            Debug.LogWarning($"{nameof(OpeningController)}: 次シーンが空欄");
    }
#endif
}
