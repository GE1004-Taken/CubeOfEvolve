using Assets.IGC2025.Scripts.GameManagers;
using R3;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMover : BasePlayerComponent
{
    // ---------- Field
    private Rigidbody _rb;

    protected override void OnInitialize()
    {
        // 色々取得処理
        _rb = GetComponent<Rigidbody>();
        var gameManager = GameManager.Instance;

        // 移動処理
        InputEventProvider.Move
            .Where(_ => gameManager.CurrentGameState.CurrentValue == GameState.BATTLE)
            .Subscribe(x =>
            {
                // カメラのxzの単位ベクトル取得
                var camaraForward =
                Vector3.Scale(
                    Camera.main.transform.forward,
                    new Vector3(1f, 0f, 1f)).normalized;

                // 移動方向取得
                var moveDirection = camaraForward * x.y + Camera.main.transform.right * x.x;

                _rb.linearVelocity =
                moveDirection * Core.MoveSpeed.CurrentValue +
                new Vector3(0f, _rb.linearVelocity.y, 0f);

                // 移動していたら回転
                if (moveDirection != Vector3.zero)
                {
                    var from = transform.rotation;
                    var to = Quaternion.LookRotation(moveDirection, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(
                        from,
                        to,
                        Core.RotateSpeed.CurrentValue * Time.deltaTime);
                }
            })
            .AddTo(this);

        // ポーズの開閉処理
        InputEventProvider.Pause
            .Where(x => x)
            .Subscribe(x =>
            {
                // ゲーム中のみポーズを開けるように
                if(gameManager.CurrentGameState.CurrentValue == GameState.BATTLE
                || gameManager.CurrentGameState.CurrentValue == GameState.BUILD)
                {
                    gameManager.ChangeGameState(GameState.PAUSE);
                }
                // ポーズ中のみ処理
                else if(gameManager.CurrentGameState.CurrentValue == GameState.PAUSE)
                {
                    // ポーズする前のゲームステートに戻す
                    if (gameManager.PrevGameState == GameState.BATTLE)
                    {
                        gameManager.ChangeGameState(GameState.BATTLE);
                    }
                    else if(gameManager.PrevGameState == GameState.BUILD)
                    {
                        gameManager.ChangeGameState(GameState.BUILD);
                    }
                }
            })
            .AddTo(this);
    }
}
