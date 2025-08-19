using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using R3;
using Assets.IGC2025.Scripts.GameManagers;

public class PlayerCameraInputActiver : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField] private CinemachineInputAxisController _playerCameraInput;

    // ---------- UnityMessage
    protected override void OnInitialize()
    {
        var currentGameState = GameManager.Instance.CurrentGameState;

        // ステートが変わるごとにカメラが操作できるかを変える
        currentGameState
            .Subscribe(x =>
            {
                if (x == GameState.BATTLE)
                {
                    _playerCameraInput.enabled = true;
                }
                else
                {
                    _playerCameraInput.enabled = false;
                }
            })
            .AddTo(this);

        // ビルド時特定のボタンを押しているときに限りカメラ操作可能
        InputEventProvider.MoveCamera
            .Where(_ => currentGameState.CurrentValue == GameState.BUILD
            || currentGameState.CurrentValue == GameState.TUTORIAL)
            .Subscribe(x =>
            {
                // 押したらカメラ操作可能
                if(x)
                {
                    _playerCameraInput.enabled = true;
                }
                // 離したらカメラ操作不可
                else
                {
                    _playerCameraInput.enabled = false;
                }
            })
            .AddTo(this);

    }
}
