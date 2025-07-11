using Assets.AT;
using Assets.IGC2025.Scripts.GameManagers;
using AT.uGUI;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

[RequireComponent(typeof(TimeManager))]
[RequireComponent(typeof(SceneLoader))]
public class GameManager : MonoBehaviour
{
    // ---------- Singleton
    public static GameManager Instance;

    // ---------- SerializeField
    private TimeManager _timeManager;
    private SceneLoader _sceneLoader;

    // ---------- Field
    private CameraCtrlManager _cameraCtrlManager;
    private CanvasCtrlManager _canvasCtrlManager;

    private GameState _prevGameState;
    public static bool IsRetry { get; private set; } = false;


    // ---------- RP
    private ReactiveProperty<GameState> _currentGameState = new();
    public ReadOnlyReactiveProperty<GameState> CurrentGameState => _currentGameState;

    // ---------- Property
    public TimeManager TimeManager => _timeManager;
    public SceneLoader SceneLoader => _sceneLoader;
    public GameState PrevGameState => _prevGameState;

    // ---------- UnityMessage
    private void Awake()
    {
        // すでに別のインスタンスが存在する場合、それを破棄
        if (Instance != null && Instance != this)
        {
            Debug.Log("[GameManager] 古いインスタンスを破棄し、最新のインスタンスに差し替えます", this);
            Destroy(Instance.gameObject);
        }

        // このインスタンスを最新として登録
        Instance = this;
        Debug.Log("[GameManager] 新しいインスタンスが設定されました", this);

        // 初期化
        _timeManager = GetComponent<TimeManager>();
        _sceneLoader = GetComponent<SceneLoader>();
        _currentGameState.Value = GameState.TITLE;
        _prevGameState = _currentGameState.Value;

    }

    private void Start()
    {
        // ゲームステート変更時の処理
        _currentGameState
            .Skip(1)
            .Subscribe(x =>
            {
                Debug.Log($"【GameManager】 ゲームステートが変更されました {_prevGameState} -> {x}");
                _canvasCtrlManager = CanvasCtrlManager.Instance;

                switch (x)
                {
                    case GameState.TITLE:
                        _canvasCtrlManager.ShowOnlyCanvas("TitleView");
                        break;

                    case GameState.INITIALIZE:

                        break;

                    case GameState.READY:
                        ShowStartThenReady().Forget(); // ← 非同期の処理を別に
                        break;

                    case GameState.BATTLE:
                        // カメラ移動
                        CameraCtrlManager.Instance.ChangeCamera("Player Camera");
                        StartGame();
                        break;

                    case GameState.BUILD:
                        CameraCtrlManager.Instance.ChangeCamera("Build Camera");
                        StopGame();
                        break;

                    case GameState.SHOP:
                        StopGame();
                        break;

                    case GameState.PAUSE:
                        StopGame();
                        break;

                    case GameState.TUTORIAL:
                        StopGame();
                        break;

                    case GameState.GAMEOVER:
                        GameSoundManager.Instance.StopBGMWithFade(.5f);
                        GameSoundManager.Instance.PlaySE("Gameover", "SE");
                        StopGame();
                        break;

                    case GameState.GAMECLEAR:
                        GameSoundManager.Instance.PlayBGM("Clear1", "BGM");

                        StopGame();
                        break;
                }
            })
            .AddTo(this);
    }

    private void OnEnable()
    {
        ResetGame();
        if (IsRetry)
        {
            IsRetry = false;
            ChangeGameState(GameState.READY);
        }
        else
        {
            ChangeGameState(GameState.TITLE);
        }
    }

    // ---------- Event
    /// <summary>
    /// ゲームステートを変更
    /// </summary>
    /// <param name="state"></param>
    public void ChangeGameState(GameState state)
    {
        // 前のステートを更新
        _prevGameState = _currentGameState.Value;

        // 現在のステートを更新
        _currentGameState.Value = state;
    }

    /// <summary>
    /// ゲームステート変更(インスペクター用)
    /// </summary>
    /// <param name="stateNum"></param>
    [EnumAction(typeof(GameState))]
    public void ChangeGameState(int stateNum)
    {
        var state = (GameState)stateNum;

        if (_currentGameState.Value == GameState.READY) return;

        // 前のステートを更新
        _prevGameState = _currentGameState.Value;

        // 現在のステートを更新
        _currentGameState.Value = state;
    }

    // ---------- PrivateMethod
    /// <summary>
    /// ゲーム開始前の処理
    /// </summary>
    /// <returns></returns>
    private async UniTask ReadyGameAsync()
    {
        _canvasCtrlManager = CanvasCtrlManager.Instance;

        var readyCtrl = _canvasCtrlManager
            .GetCanvas("ReadyView")
            .GetComponent<ReadyViewCanvasController>();

        await readyCtrl.PlayReadySequenceAsync(() =>
        {
            ChangeGameState(GameState.BATTLE);
            _canvasCtrlManager.ShowOnlyCanvas("GameView");
        });
    }


    /// <summary>
    /// ゲームを開始
    /// </summary>
    private void StartGame()
    {
        Time.timeScale = 1;
    }

    /// <summary>
    /// ゲームを再開
    /// </summary>
    private void ContinueGame()
    {
        Time.timeScale = 1;
    }

    /// <summary>
    /// ゲームを一時停止
    /// </summary>
    private void StopGame()
    {
        Time.timeScale = 0f;
    }

    /// <summary>
    /// ゲームをリセット
    /// </summary>
    private void ResetGame()
    {
        _timeManager.ResetTimer();
        Time.timeScale = 1;
    }

    // ---------- PublicMethod

    public void RequestRetry()
    {
        IsRetry = true;
        Instance.SceneLoader.ReloadScene();
    }

    // ---------- PrivateMethod
    private async UniTask ShowStartThenReady()
    {
        GuideManager guideManager = GuideManager.Instance;
        await guideManager.ShowGuideAndWaitAsync("Start");
        await guideManager.DoBuildModeAndWaitAsync();
        await ReadyGameAsync();
    }
}
