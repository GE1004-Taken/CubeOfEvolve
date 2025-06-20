using R3;
using UnityEngine;
using Assets.IGC2025.Scripts.GameManagers;
using Assets.AT;
using System.Collections;
using Unity.Cinemachine;
using AT.uGUI;

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
        // シングルトン
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 初期化
        _timeManager = GetComponent<TimeManager>();
        _sceneLoader = GetComponent<SceneLoader>();
        _prevGameState = _currentGameState.Value;

    }

    private void Start()
    {
        // アクセス取得
        _cameraCtrlManager = CameraCtrlManager.Instance;
        _canvasCtrlManager = CanvasCtrlManager.Instance;

        // ゲームステート変更時の処理
        _currentGameState
            .Skip(1)
            .Subscribe(x =>
            {
                Debug.Log($"【GameManager】 ゲームステートが変更されました {_prevGameState} -> {x}");

                switch (x)
                {
                    case GameState.TITLE:
                        break;

                    case GameState.INITIALIZE:
                        break;

                    case GameState.READY:
                        StartCoroutine(ReadyGame());
                        ResetGame();
                        break;

                    case GameState.BATTLE:
                        StartGame();
                        break;

                    case GameState.BUILD:
                        StopGame();
                        break;

                    case GameState.PAUSE:
                        StopGame();
                        break;

                    case GameState.TUTORIAL:
                        StopGame();
                        break;

                    case GameState.GAMEOVER:
                        StopGame();
                        break;

                    case GameState.GAMECLEAR:
                        StopGame();
                        break;
                }
            })
            .AddTo(this);
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
    private IEnumerator ReadyGame()
    {
        // カメラ移動
        CameraCtrlManager.Instance.ChangeCamera("Player Camera");
        // 演出もここ
        yield return _cameraCtrlManager.CameraBlendTime;
        ChangeGameState(GameState.BATTLE);
        _canvasCtrlManager.ShowOnlyCanvas("GameView");
    }

    /// <summary>
    /// ゲームを再開
    /// </summary>
    private void StartGame()
    {
        _timeManager.StartTimer();
        Time.timeScale = 1;
    }

    /// <summary>
    /// ゲームを一時停止
    /// </summary>
    private void StopGame()
    {
        _timeManager.StopTimer();
        Time.timeScale = 0;
    }

    /// <summary>
    /// ゲームをリセット
    /// </summary>
    private void ResetGame()
    {
        _timeManager.ResetTimer();
        Time.timeScale = 1;
    }
}
