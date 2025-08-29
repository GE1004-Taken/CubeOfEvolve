using App.GameSystem.Modules;
using Assets.IGC2025.Scripts.GameManagers;
using AT.uGUI;
using ObservableCollections;
using R3;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMover : BasePlayerComponent
{
    // ---------- Field
    private Rigidbody _rb;
    private float _currentSpeed;

    protected override void OnInitialize()
    {
        // 色々取得処理
        _rb = GetComponent<Rigidbody>();
        var gameManager = GameManager.Instance;
        var ccm = CanvasCtrlManager.Instance;
        var builder = GetComponent<PlayerBuilder>();

        UpdateMoveSpeedStatus();

        // オプション監視
        ObserveStatusEffects();

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
                moveDirection * _currentSpeed +
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
                if (gameManager.CurrentGameState.CurrentValue == GameState.BATTLE
                || gameManager.CurrentGameState.CurrentValue == GameState.BUILD)
                {
                    gameManager.ChangeGameState(GameState.PAUSE);
                }
                // ポーズ中のみ処理
                else if (gameManager.CurrentGameState.CurrentValue == GameState.PAUSE)
                {
                    // ポーズする前のゲームステートに戻す
                    if (gameManager.PrevGameState == GameState.BATTLE)
                    {
                        gameManager.ChangeGameState(GameState.BATTLE);
                    }
                    else if (gameManager.PrevGameState == GameState.BUILD)
                    {
                        gameManager.ChangeGameState(GameState.BUILD);
                    }
                }
            })
            .AddTo(this);

        // ショップ画面の開閉処理
        InputEventProvider.Shop
            .Where(x => x)
            .Subscribe(_ =>
            {
                if (gameManager.CurrentGameState.CurrentValue == GameState.SHOP)
                {
                    gameManager.ChangeGameState(GameState.BATTLE);
                    ccm.GetCanvas("ShopView")?.OnCloseCanvas();
                }
                else if (gameManager.CurrentGameState.CurrentValue == GameState.BATTLE
                || gameManager.CurrentGameState.CurrentValue == GameState.BUILD)
                {
                    gameManager.ChangeGameState(GameState.SHOP);
                    ccm.GetCanvas("GameView")?.OnCloseCanvas();
                    ccm.GetCanvas("BuildView")?.OnCloseCanvas();
                }
            })
            .AddTo(this);

        // ビルド画面の開閉処理
        InputEventProvider.Build
            .Where(x => x)
            .Subscribe(_ =>
            {
                if (gameManager.CurrentGameState.CurrentValue == GameState.BUILD)
                {
                    gameManager.ChangeGameState(GameState.BATTLE);
                    ccm.GetCanvas("BuildView")?.OnCloseCanvas();
                }
                else if (gameManager.CurrentGameState.CurrentValue == GameState.BATTLE
                || gameManager.CurrentGameState.CurrentValue == GameState.SHOP)
                {
                    gameManager.ChangeGameState(GameState.BUILD);
                    ccm.GetCanvas("GameView")?.OnCloseCanvas();
                    ccm.GetCanvas("ShopView")?.OnCloseCanvas();
                }
            })
            .AddTo(this);

        // ビルド時のモード切替
        InputEventProvider.Remove
            .Where(x => x)
            .Where(_ => gameManager.CurrentGameState.CurrentValue == GameState.BUILD)
            .Subscribe(_ =>
            {
                builder.ChangeBuildMode();
            })
            .AddTo(this);
    }

    /// <summary>
    /// オプションを監視
    /// </summary>
    private void ObserveStatusEffects()
    {
        var addStream = RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList
        .ObserveAdd(destroyCancellationToken)
        .Select(_ => Unit.Default);

        var removeStream = RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList
            .ObserveRemove(destroyCancellationToken)
            .Select(_ => Unit.Default);

        // どちらかのイベントが発生した時を監視
        addStream.Merge(removeStream)
            .Subscribe(_ =>
            {
                UpdateMoveSpeedStatus();
            })
            .AddTo(this);
    }

    /// <summary>
    /// 移動速度更新
    /// </summary>
    private void UpdateMoveSpeedStatus()
    {
        var speedRate = 0f;

        foreach (var effect in RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList)
        {
            speedRate += effect.MoveSpeed;
        }

        _currentSpeed = Core.MoveSpeed.CurrentValue * (1f + speedRate / 100f);
    }
}
