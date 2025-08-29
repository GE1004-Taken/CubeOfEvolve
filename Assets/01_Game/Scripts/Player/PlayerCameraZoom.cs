using R3;
using R3.Triggers;
using Unity.Cinemachine;
using UnityEngine;
using Assets.IGC2025.Scripts.GameManagers;

public class PlayerCameraZoom : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField, Tooltip("プレイヤーカメラ")] private CinemachineOrbitalFollow _playerCamera;
    [SerializeField, Tooltip("最大の半径")] private float _maxRadius = 20f;
    [SerializeField, Tooltip("最小の半径")] private float _minRadius = 1f;
    [SerializeField, Tooltip("一回のスクロールでズームする量")] private float _zoomAmount = 10f;
    [SerializeField, Tooltip("一回のズームにかかる時間")] private float _zoomTime = 0.1f;

    // ---------- Field
    // 現在の半径
    private float _currentRadius = 0f;
    // 目標の半径
    private float _targetRadius = 0f;
    // SmoothDamp用の変数
    private float _currentVelocity = 0f;

    // ---------- UnityMessage
    protected override void OnInitialize()
    {
        // 現在のゲームステートのRPを取得
        var currentGameState = GameManager.Instance.CurrentGameState;

        // 初期の半径を取得
        _currentRadius = _playerCamera.Radius;
        _targetRadius = _currentRadius;

        // マウスホイールによるズーム処理
        InputEventProvider.Zoom
            .Where(x => currentGameState.CurrentValue == GameState.BATTLE
            || currentGameState.CurrentValue == GameState.BUILD
            || currentGameState.CurrentValue == GameState.TUTORIAL)
            .Subscribe(x =>
            {
                // 前スクロール時
                if(x > 0)
                {
                    // ズームイン
                    if (_targetRadius + -x * _zoomAmount >= _minRadius)
                    {
                        _targetRadius += -x * _zoomAmount;
                    }
                    // 最小値を下回らないように
                    else
                    {
                        _targetRadius = _minRadius;
                    }
                }
                // 後ろスクロール時
                else if(x < 0)
                {
                    // ズームアウト
                    if (_targetRadius + -x * _zoomAmount <= _maxRadius)
                    {
                        _targetRadius += -x * _zoomAmount;
                    }
                    // 最大値を超えないように
                    else
                    {
                        _targetRadius = _maxRadius;
                    }
                }
            })
            .AddTo(this);

        // 実際のズーム処理
        this.UpdateAsObservable()
            .Where(x => currentGameState.CurrentValue == GameState.BATTLE
            || currentGameState.CurrentValue == GameState.BUILD
            || currentGameState.CurrentValue == GameState.TUTORIAL)
            .Where(_ => _currentRadius != _targetRadius)
            .Subscribe(x =>
            {
                // 滑らかにFOVを変える
                _currentRadius = Mathf.SmoothDamp(
                    _playerCamera.Radius,
                    _targetRadius,
                    ref _currentVelocity,
                    _zoomTime,
                    Mathf.Infinity,
                    Time.unscaledDeltaTime);

                // 変わったFOVをセットする
                _playerCamera.Radius = _currentRadius;
            });
    }
}
