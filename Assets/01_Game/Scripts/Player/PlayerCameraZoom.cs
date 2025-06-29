using R3;
using R3.Triggers;
using Unity.Cinemachine;
using UnityEngine;
using Assets.IGC2025.Scripts.GameManagers;

public class PlayerCameraZoom : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField, Tooltip("プレイヤーカメラ")] private CinemachineCamera _playerCamera;
    [SerializeField, Tooltip("ビルドカメラ")] private CinemachineCamera _buildCamera;
    [SerializeField, Tooltip("最大のFOV")] private float _maxFov = 90f;
    [SerializeField, Tooltip("最小のFOV")] private float _minFov = 10f;
    [SerializeField, Tooltip("一回のスクロールでズームする量")] private float _zoomAmount = 10f;
    [SerializeField, Tooltip("一回のズームにかかる時間")] private float _zoomTime = 0.1f;

    // ---------- Field
    // 現在使用しているカメラ
    private CinemachineCamera _currentCamera = null;
    // 現在のFOV
    private float _currentFOV = 0f;
    // 目標のFOV
    private float _targetFOV = 0f;
    // SmoothDamp用の変数
    private float _currentVelocity = 0f;

    // ---------- UnityMessage
    protected override void OnInitialize()
    {
        // ゲームステートによってカメラを切り替える
        GameManager.Instance.CurrentGameState
            .Where(x => x == GameState.BATTLE || x == GameState.BUILD)
            .Subscribe(x =>
            {
                // 切り替える処理
                if(x == GameState.BATTLE)
                {
                    _currentCamera = _playerCamera;
                }
                else if(x == GameState.BUILD)
                {
                    _currentCamera = _buildCamera;
                }

                // 切り替えたカメラによって初期化する
                _currentFOV = _currentCamera.Lens.FieldOfView;
                _targetFOV = _currentFOV;
            })
            .AddTo(this);

        // マウスホイールによるズーム処理
        InputEventProvider.Zoom
            .Where(x => GameManager.Instance.CurrentGameState.CurrentValue == GameState.BATTLE
            || GameManager.Instance.CurrentGameState.CurrentValue == GameState.BUILD)
            .Subscribe(x =>
            {
                // 前スクロール時
                if(x > 0)
                {
                    // ズームイン
                    if (_targetFOV + -x * _zoomAmount >= _minFov)
                    {
                        _targetFOV += -x * _zoomAmount;
                    }
                    // 最小値を下回らないように
                    else
                    {
                        _targetFOV = _minFov;
                    }
                }
                // 後ろスクロール時
                else if(x < 0)
                {
                    // ズームアウト
                    if (_targetFOV + -x * _zoomAmount <= _maxFov)
                    {
                        _targetFOV += -x * _zoomAmount;
                    }
                    // 最大値を超えないように
                    else
                    {
                        _targetFOV = _maxFov;
                    }
                }
            })
            .AddTo(this);

        // 実際のズーム処理
        this.UpdateAsObservable()
            .Where(x => GameManager.Instance.CurrentGameState.CurrentValue == GameState.BATTLE
            || GameManager.Instance.CurrentGameState.CurrentValue == GameState.BUILD)
            .Where(_ => _currentFOV != _targetFOV)
            .Subscribe(x =>
            {
                // 滑らかにFOVを変える
                _currentFOV = Mathf.SmoothDamp(
                    _currentCamera.Lens.FieldOfView,
                    _targetFOV,
                    ref _currentVelocity,
                    _zoomTime,
                    Mathf.Infinity,
                    Time.unscaledDeltaTime);

                // 変わったFOVをセットする
                _currentCamera.Lens.FieldOfView = _currentFOV;
            });
    }
}
