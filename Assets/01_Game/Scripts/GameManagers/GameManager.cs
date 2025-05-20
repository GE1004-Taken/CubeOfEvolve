using R3;
using UnityEngine;
using Assets.IGC2025.Scripts.GameManagers;

[RequireComponent(typeof(TimeManager))]
public class GameManager : MonoBehaviour
{
    // ---------- Singleton
    public static GameManager Instance;

    // ---------- SerializeField
    [SerializeField] private TimeManager _timeManager;

    // ---------- RP
    private ReactiveProperty<GameState> _currentGameState = new();
    public ReadOnlyReactiveProperty<GameState> CurrentGameState => _currentGameState;

    // ---------- Property
    public TimeManager TimeManager => _timeManager;

    // ---------- UnityMessage
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _currentGameState
            .Subscribe(x =>
            {
                switch(x)
                {
                    case GameState.TITLE:
                        break;

                    case GameState.INITIALIZE:
                        ResetGame();
                        break;

                    case GameState.READY:
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
    public void ChangeGameState(GameState state)
    {
        _currentGameState.Value = state;
    }

    // ---------- PrivateMethod
    /// <summary>
    /// ゲームを再開
    /// </summary>
    private void StartGame()
    {
        _timeManager.StartTimer();
    }

    /// <summary>
    /// ゲームを一時停止
    /// </summary>
    private void StopGame()
    {
        _timeManager.StopTimer();
    }

    /// <summary>
    /// ゲームをリセット
    /// </summary>
    private void ResetGame()
    {
        _timeManager.ResetTimer();
    }
}
