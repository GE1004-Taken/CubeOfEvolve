using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class OpeningController : MonoBehaviour
{
    // ----- SerializedField
    [Header("参照")]
    [SerializeField] private CanvasGroup logo;

    [Header("タイミング(秒)")]
    [Min(0f)][SerializeField] private float logoFadeIn = 1.2f;
    [Min(0f)][SerializeField] private float logoHold = 1.0f;
    [Min(0f)][SerializeField] private float logoFadeOut = 0.8f;

    [Header("次シーン")]
    [SerializeField] private string nextSceneName = "";

    [Header("オプション")]
    [SerializeField,Tooltip("スキップ可能？")] private bool allowSkip = true;

    // ----- Field
    private CancellationTokenSource _lifeCts;
    private bool _sceneLoading;

    // ----- UnityMessage

    private void Awake()
    {
        // 初期化：ロゴを透明にしておく
        if (logo != null) logo.alpha = 0f;

        // ライフサイクル用キャンセルトークン生成
        _lifeCts = new CancellationTokenSource();
    }

    private void Start()
    {
        // 非同期処理開始（async voidの代わりにUniTaskVoid＋Forgetで実行）
        RunAsync().Forget();
    }

    private void OnDestroy()
    {
        // トークンをキャンセル＆破棄
        _lifeCts?.Cancel();
        _lifeCts?.Dispose();
    }

    // ----- PrivateMethods

    /// <summary>
    /// メインの非同期ルーチン。
    /// - ロゴ演出タスクとスキップ待機タスクを並列で走らせ、
    ///   どちらかが先に終わった時点でシーン遷移を実行する。
    /// </summary>
    private async UniTaskVoid RunAsync()
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            this.GetCancellationTokenOnDestroy(), _lifeCts.Token);
        var token = linkedCts.Token;

        try
        {
            // 並行タスク：演出／スキップ待機
            var seqTask = PlayOpeningSequence(token);
            var skipTask = allowSkip ? WaitSkipAsync(token) : UniTask.Never(token);

            // どちらかが完了した時点で先へ進む
            await UniTask.WhenAny(seqTask, skipTask);

            LoadNextSceneOnce();
        }
        catch (OperationCanceledException)
        {
            // OnDestroy などでキャンセルされた場合もシーン遷移を試みる
            LoadNextSceneOnce();
        }
        catch (Exception e)
        {
            Debug.LogError($"[OpeningController] 不明のエラー発生！: {e}");
            LoadNextSceneOnce();
        }
    }

    /// <summary>
    /// ロゴ表示演出：
    /// フェードイン → 一定時間保持 → フェードアウト
    /// </summary>
    private async UniTask PlayOpeningSequence(CancellationToken token)
    {
        if (logo == null) return; // ロゴが無ければ即終了

        logo.alpha = 0f;

        var seq = DOTween.Sequence()
            .AppendInterval(logoHold)
            .Append(logo.DOFade(1f, logoFadeIn).SetEase(Ease.InOutSine))
            .AppendInterval(logoHold)
            .Append(logo.DOFade(0f, logoFadeOut).SetEase(Ease.InOutSine))
            .SetUpdate(true)        // 全体を非スケール更新
            .SetLink(gameObject);   // 破棄でKill

        using (token.Register(() => { if (seq.IsActive()) seq.Kill(); }))// AI
        {
            await seq.ToUniTask(cancellationToken: token);
        }
    }

    /// <summary>
    /// スキップ入力があるまで待機する処理。
    /// </summary>
    private async UniTask WaitSkipAsync(CancellationToken token)
    {
        await UniTask.WaitUntil(() => CheckSkipPressed(), cancellationToken: token);
    }

    /// <summary>
    /// 一度だけシーン遷移を行う。
    /// 二重呼び出しや未設定のシーン名は防止。
    /// </summary>
    private void LoadNextSceneOnce()
    {
        if (_sceneLoading) return;

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogError("[OpeningController] 次シーンが空欄");
            return;
        }

        _sceneLoading = true;
        SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// スキップ判定処理。
    /// 新InputSystem／旧InputSystem 両対応。
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

        // ここにTouchやその他ボタンの条件を追加可能
        return false;
    }

#if UNITY_EDITOR
    /// <summary>
    /// インスペクタでのバリデーション。
    /// シーン名が未設定なら警告を出す。
    /// </summary>
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(nextSceneName))
            Debug.LogWarning($"{nameof(OpeningController)}: 次シーンが空欄");
    }
#endif
}
