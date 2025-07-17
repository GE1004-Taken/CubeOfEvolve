using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using R3;
using Assets.IGC2025.Scripts.GameManagers;

public class PlayerCameraInputActiver : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField] private CinemachineInputAxisController _buildCameraInput;

    // ---------- UnityMessage
    protected override void OnInitialize()
    {
        // ゲームステートがビルド時に毎回初期化
        GameManager.Instance.CurrentGameState
            .Where(x => x == GameState.BUILD
            || x == GameState.TUTORIAL)
            .Subscribe(_ =>
            {
                _buildCameraInput.enabled = false;
            })
            .AddTo(this);

        // ビルド時特定のボタンを押しているときに限りカメラ操作可能
        InputEventProvider.MoveCamera
            .Where(_ => GameManager.Instance.CurrentGameState.CurrentValue == GameState.BUILD
            || GameManager.Instance.CurrentGameState.CurrentValue == GameState.TUTORIAL)
            .Subscribe(x =>
            {
                // 押したらカメラ操作可能
                if(x)
                {
                    _buildCameraInput.enabled = true;
                }
                // 離したらカメラ操作不可
                else
                {
                    _buildCameraInput.enabled = false;
                }
            })
            .AddTo(this);

    }
}
