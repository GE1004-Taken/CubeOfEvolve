using System;
using System.Threading;
using Cysharp.Threading.Tasks;

using DG.Tweening;
using R3; // R3 (Reactive) を使用
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // 新InputSystem
#endif

public class OpeningController : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private CanvasGroup logo;

    [Header("タイミング(秒)")]
    [SerializeField] private float logoFadeIn = 1.2f;
    [SerializeField] private float logoHold = 1.0f;
    [SerializeField] private float logoFadeOut = 0.8f;

    [Header("次シーン")]
    [SerializeField] private string nextSceneName = "";

    [Header("オプション")]
    [SerializeField] private bool allowSkip = true;

    private CancellationTokenSource _lifeCts;
    private bool _sceneLoading = false;

    private void Awake()
    {
        if (logo != null) logo.alpha = 0f;
        _lifeCts = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        _lifeCts?.Cancel();
        _lifeCts?.Dispose();
    }

    private async void Start()
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy(), _lifeCts.Token);
        var token = linkedCts.Token;
        IDisposable skipSub = null;
        try
        {
            skipSub = SetupSkipObserver(linkedCts);
            await PlayOpeningSequence(token);
        }
        catch (OperationCanceledException)
        {
            // スキップ時は即シーン遷移
            LoadNextSceneOnce();
        }
        catch (Exception e)
        {
            Debug.LogError($"[OpeningController] Unexpected error: {e}");
            LoadNextSceneOnce();
        }
        finally
        {
            skipSub?.Dispose();
        }
    }

    private IDisposable SetupSkipObserver(CancellationTokenSource linkedCts)
    {
        if (!allowSkip) return null;
        return Observable.EveryUpdate()
            .Where(_ => CheckSkipPressed())
            .Take(1)
            .Subscribe(_ => linkedCts.Cancel());
    }

    private async UniTask PlayOpeningSequence(CancellationToken token)
    {
        // ロゴ フェードイン
        if (logo != null)
            await logo.DOFade(1f, logoFadeIn).SetUpdate(true).AsyncWaitForCompletion();

        // ロゴ ホールド
        await UniTask.Delay((int)(logoHold * 1000), DelayType.UnscaledDeltaTime, cancellationToken: token);

        // ロゴ フェードアウト
        if (logo != null)
            await logo.DOFade(0f, logoFadeOut).SetUpdate(true).AsyncWaitForCompletion();

        // シーン遷移
        LoadNextSceneOnce();
    }

    private void LoadNextSceneOnce()
    {
        if (_sceneLoading) return;
        _sceneLoading = true;
        SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
    }

    private bool CheckSkipPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current?.anyKey.wasPressedThisFrame == true) return true;
        if (Mouse.current?.leftButton.wasPressedThisFrame == true || Mouse.current?.rightButton.wasPressedThisFrame == true) return true;
        if (Gamepad.current?.startButton.wasPressedThisFrame == true || Gamepad.current?.buttonSouth.wasPressedThisFrame == true) return true;
        return false;
#else
        return Input.anyKeyDown;
#endif
    }
}
