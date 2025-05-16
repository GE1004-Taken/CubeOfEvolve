using UnityEngine;

public abstract class EnemyMove : MonoBehaviour
{
    // ---------------------------- SerializeField
    [Header("ステータス")]
    [SerializeField] private EnemyStatus _status;

    [Header("Rigidbody")]
    [SerializeField] private Rigidbody _rb;                          // RigidBody保存

    // ---------------------------- Field
    protected GameObject _targetObj;                  // 攻撃対象


    // ---------------------------- UnityMessage
    private void Start()
    {
        _targetObj = EnemyManager.Instance.PlayerObj;
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
    public abstract void Move();
}
