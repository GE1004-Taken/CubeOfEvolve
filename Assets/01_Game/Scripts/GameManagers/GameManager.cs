using Assets.AT;
using Assets.IGC2025.Scripts.GameManagers;
using AT.uGUI;
using Cysharp.Threading.Tasks;
using R3;
using System.Threading;
using UnityEngine;

public enum GameFlowCommand
{
    NONE,
    RETRY,
    RETURN_TO_TITLE,
    RESET_ALL
}

[RequireComponent(typeof(TimeManager))]
[RequireComponent(typeof(SceneLoader))]
public class GameManager : MonoBehaviour
{
    // ---------- Singleton
    public static GameManager Instance { get; private set; }

    // ---------- RP
    private readonly ReactiveProperty<GameState> _currentGameState = new(GameState.INITIALIZE);
    public ReadOnlyReactiveProperty<GameState> CurrentGameState => _currentGameState;

    // ---------- Field
    private GameState _prevGameState;
    private static GameFlowCommand _pendingCommand = GameFlowCommand.NONE;

    private TimeManager _timeManager;
    private SceneLoader _sceneLoader;
    private CameraCtrlManager _cameraCtrlManager;
    private CanvasCtrlManager _canvasCtrlManager;
    private GameSoundManager _gameSoundManager;
    private GuideManager _guideManager;

    // ---------- Property
    public TimeManager TimeManager => _timeManager;
    public SceneLoader SceneLoader => _sceneLoader;
    public GameState PrevGameState => _prevGameState;

    // ---------- UnityMessage
    private void Awake()
    {
        l_Singleton(); // シングルトン

        // 初期値代入
        _prevGameState = _currentGameState.Value;

        // 監視宣言
        _currentGameState
            .Skip(1)
            .Subscribe(async state =>
            {
                var token = this.GetCancellationTokenOnDestroy();
                var task = OnGameStateChanged(state, token);
                if (await task.SuppressCancellationThrow()) return;
            })
            .AddTo(this);

        void l_Singleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(Instance.gameObject);
                return;
            }
            Instance = this;
        }
    }

    private async void Start()
    {
        var InitTask = InitializeGameAsync(destroyCancellationToken);
        if (await InitTask.SuppressCancellationThrow()) { return; }
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async UniTask InitializeGameAsync(CancellationToken ct)
    {
        Debug.Log("[GameManager] 初期化処理中...");
        await UniTask.DelayFrame(1, cancellationToken: ct);

        _timeManager = GetComponent<TimeManager>();
        _sceneLoader = GetComponent<SceneLoader>();
        _cameraCtrlManager = CameraCtrlManager.Instance;
        _canvasCtrlManager = CanvasCtrlManager.Instance;
        _gameSoundManager = GameSoundManager.Instance;
        _guideManager = GuideManager.Instance;

        ChangeGameState(GameState.TITLE);
        Debug.Log("[GameManager] 初期化完了！");
    }

    /// <summary>
    /// ゲームステート変更時イベント処理
    /// </summary>
    /// <param name="newState"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private async UniTask OnGameStateChanged(GameState newState, CancellationToken token)
    {
        Debug.Log($"[GameManager] State Changed: {_prevGameState} → {newState}");

        switch (newState)
        {
            case GameState.TITLE:
                await SetupTitleAsync(token);
                break;
            case GameState.READY:
                await SetupReadyAsync(token);
                break;
            case GameState.BUILD:
                await SetupBuildAsync(token);
                break;
            case GameState.SHOP:
                await SetupShopAsync(token);
                break;
            case GameState.BATTLE:
                await SetupBattleAsync(token);
                break;
            case GameState.RESULT:
                await SetupResultAsync(token);
                break;

            case GameState.TUTORIAL:
                await SetupTutorialAsync(token);
                break;

            default:
                break;
        }

        await HandleGameFlowCommand(token);
    }

    /// <summary>
    /// ゲームのリセット処理関数
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private async UniTask HandleGameFlowCommand(CancellationToken token)
    {
        if (_pendingCommand == GameFlowCommand.NONE) return;

        var command = _pendingCommand;
        _pendingCommand = GameFlowCommand.NONE;

        switch (command)
        {
            case GameFlowCommand.RETRY:
                ResetSessionData();
                ChangeGameState(GameState.READY);
                break;
            case GameFlowCommand.RETURN_TO_TITLE:
                ResetSessionData();
                ChangeGameState(GameState.TITLE);
                break;
            case GameFlowCommand.RESET_ALL:
                await ResetAllAsync(token);
                break;
        }
    }


    /// <summary>
    /// ゲームステート変更
    /// </summary>
    /// <param name="stateNum"></param>
    public void ChangeGameState(GameState newState)
    {
        // 前のステートを更新
        _prevGameState = _currentGameState.Value;

        // 現在のステートを更新
        _currentGameState.Value = newState;
    }

    /// <summary>
    /// ゲームステート変更(インスペクター用)
    /// </summary>
    /// <param name="stateNum"></param>
    [EnumAction(typeof(GameState))]
    public void ChangeGameState(int stateNum)
    {
        var state = (GameState)stateNum;

        //if (_currentGameState.Value == GameState.READY) return;

        // 前のステートを更新
        _prevGameState = _currentGameState.Value;

        // 現在のステートを更新
        _currentGameState.Value = state;
    }


    #region Reset
    public void RequestRetry() => _pendingCommand = GameFlowCommand.RETRY;
    public void RequestReturnToTitle() => _pendingCommand = GameFlowCommand.RETURN_TO_TITLE;
    public void RequestResetAll() => _pendingCommand = GameFlowCommand.RESET_ALL;


    private void ResetSessionData()
    {
        _timeManager = GetComponent<TimeManager>();

        _timeManager.ResetTimer();
        // ステータス・スコア・一時オブジェクト初期化など
        Debug.Log("[GameManager] プレイセッションを初期化");
    }

    private async UniTask ResetAllAsync(CancellationToken token)
    {
        Debug.Log("[GameManager] 完全リセットを実行中...");
        // TODO: セーブデータ初期化処理をここに記述（SaveManager.ClearAll() など）
        await UniTask.Delay(100, cancellationToken: token);
        await InitializeGameAsync(token);
    }

    #endregion

    // ステートが変わった際に行う処理群
    #region Setup 

    /// <summary>
    /// タイトル
    /// </summary>
    /// <param name="token">中断時の安全保障</param>
    /// <returns></returns>
    private async UniTask SetupTitleAsync(CancellationToken token)
    {
        _canvasCtrlManager = CanvasCtrlManager.Instance;
        _cameraCtrlManager = CameraCtrlManager.Instance;
        _canvasCtrlManager.ShowOnlyCanvas("TitleView");
        _cameraCtrlManager.ChangeCamera("TitleDemoCamera");
        await UniTask.CompletedTask;
    }

    private async UniTask SetupReadyAsync(CancellationToken token)
    {
        var readyCtrl = CanvasCtrlManager.Instance
            .GetCanvas("ReadyView")
            .GetComponent<ReadyViewCanvasController>();

        await readyCtrl.PlayReadySequenceAsync(() =>
        {
            ChangeGameState(GameState.BATTLE);
        }, token);

        await UniTask.CompletedTask;
    }

    private async UniTask SetupBuildAsync(CancellationToken token)
    {
        _canvasCtrlManager.GetCanvas("BuildView")?.OnOpenCanvas();
        _cameraCtrlManager.ChangeCamera("Build Camera");
        await UniTask.CompletedTask;
    }

    private async UniTask SetupShopAsync(CancellationToken token)
    {
        _canvasCtrlManager.GetCanvas("ShopView")?.OnOpenCanvas();
        await UniTask.CompletedTask;
    }

    private async UniTask SetupBattleAsync(CancellationToken token)
    {
        _canvasCtrlManager.GetCanvas("GameView")?.OnOpenCanvas();
        _cameraCtrlManager.ChangeCamera("Player Camera");
        await UniTask.CompletedTask;
    }

    private async UniTask SetupResultAsync(CancellationToken token)
    {
        await UniTask.CompletedTask;
    }

    private async UniTask SetupTutorialAsync(CancellationToken token)
    {
        _guideManager = GuideManager.Instance;

        await _guideManager.ShowGuideAndWaitAsync("Start", token);
        await _guideManager.ShowGuideAndWaitAsync("Start (1)", token);
        await _guideManager.DoBuildModeAndWaitAsync(token);

        ChangeGameState(GameState.READY);

        await UniTask.CompletedTask;
    }

    #endregion
}