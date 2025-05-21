using UnityEngine;

public abstract class EnemyMoveBase : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField] protected EnemyStatus _status;

    // ---------------------------- Field
    protected Rigidbody _rb;                          // RigidBody保存
    protected GameObject _targetObj;                  // 攻撃対象

    // ---------------------------- UnityMessage
    private void Start()
    {
        _status = GetComponent<EnemyStatus>();
        _rb = GetComponent<Rigidbody>();
        _targetObj = PlayerMonitoring.Instance.PlayerObj;

        InitializeAsync();
    }

    private void FixedUpdate()
    {
        switch (_status.CurrentAct.CurrentValue)
        {
            case EnemyStatus.ActionPattern.IDLE:
                break;

            case EnemyStatus.ActionPattern.MOVE:
                // 移動処理
                Move();
                break;

            case EnemyStatus.ActionPattern.WAIT:
                break;

            case EnemyStatus.ActionPattern.ATTACK:
                break;

            case EnemyStatus.ActionPattern.MOVEANDATTACK:
                break;

            case EnemyStatus.ActionPattern.AVOID:
                break;
        }
    }


    // ---------------------------- PrivateMethod
    /// <summary>
    /// 直線移動処理
    /// </summary>
    protected void LinearMovement()
    {
        if (_targetObj == null) return;

        // 敵から対象へのベクトルを取得
        Vector3 moveForward = _targetObj.transform.position - transform.position;

        // 高さは追わない
        moveForward.y = 0;

        // 移動方向にスピードを掛ける
        _rb.linearVelocity = _status.Speed * Time.deltaTime * moveForward.normalized + new Vector3(0, _rb.linearVelocity.y, 0);

        // キャラクターの向きを進行方向に向ける
        if (moveForward != Vector3.zero)
        {
            // 方向ベクトルを取得
            Vector3 direction = _targetObj.transform.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

            // Y軸の回転のみ取得
            transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        }
    }


    // ---------------------------- VirtualMethod
    /// <summary>
    /// 初期化
    /// </summary>
    public virtual void InitializeAsync()
    {

    }

    // ---------------------------- AbstractMethod
    /// <summary>
    /// 移動処理
    /// </summary>
    public abstract void Move();
}
