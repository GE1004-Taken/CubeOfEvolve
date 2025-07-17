using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using System;
using System.Threading;
using UnityEngine;

/// <summary>
/// 直線移動する敵
/// </summary>
public class EnemyMove_Assault : EnemyMoveBase
{
    // ---------------------------- SerializeField
    [SerializeField] private float _moveDelaySecond;
    [SerializeField] private float _destroyDelaySecond;

    // ---------------------------- Field
    private Vector3 _moveForward;
    private bool _isAssault = false;
    private float _countSecond = 0;

    private CancellationToken _token;

    // ---------------------------- UnityMessage
    private void Awake()
    {
        _token = this.GetCancellationTokenOnDestroy();
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// 突撃処理
    /// </summary>
    private void Assault()
    {
        // 移動方向にスピードを掛ける
        _rb.linearVelocity = _status.Speed * Time.deltaTime * _moveForward.normalized + new Vector3(0, _rb.linearVelocity.y, 0);
    }

    // ---------------------------- OverrideMethod
    public override void Move()
    {
        if (_isAssault)
        {
            Assault();
        }
        else
        {
            // 敵から対象へのベクトルを取得
            _moveForward = _targetObj.transform.position - transform.position;

            // 高さは追わない
            _moveForward.y = 0;

            // キャラクターの向きを進行方向に向ける
            if (_moveForward != Vector3.zero)
            {
                // 方向ベクトルを取得
                Vector3 direction = _targetObj.transform.position - transform.position;
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

                // Y軸の回転のみ取得
                transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
            }
        }
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentGameState.CurrentValue != Assets.IGC2025.Scripts.GameManagers.GameState.BATTLE)
        {
            return;
        }

        _countSecond += Time.deltaTime;

        if (_countSecond >= _moveDelaySecond)
        {
            if (this != null && gameObject != null)
            {
                _isAssault = true;
            }
        }
        if (_countSecond >= _moveDelaySecond + _destroyDelaySecond)
        {
            if (this != null && gameObject != null)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public override void Initialize()
    {
        //// キャンセル処理を書くところ要相談
        //await UniTask.Delay(TimeSpan.FromSeconds(_moveDelaySecond), cancellationToken: _token, delayType: DelayType.DeltaTime)
        // .SuppressCancellationThrow();

        //if (this != null && gameObject != null)
        //{
        //    _isAssault = true;
        //}

        //// キャンセル処理を書くところ要相談
        //await UniTask.Delay(TimeSpan.FromSeconds(_destroyDelaySecond), cancellationToken: _token, delayType: DelayType.DeltaTime)
        // .SuppressCancellationThrow();

        //if (this != null && gameObject != null)
        //{
        //    Destroy(gameObject);
        //}
    }
}
