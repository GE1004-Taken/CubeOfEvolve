using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField] private float _speed;


    // ---------------------------- Field
    /// <summary>
    /// 行動パターン
    /// </summary>
    private enum ActionPattern
    {
        IDLE,           // 待機
        MOVE,           // 移動
        WAIT,           // 行動を一旦停止
        ATTACK,         // 停止して攻撃
        MOVEANDATTACK,  // 移動しながら攻撃
        AVOID,          // 回避
    }

    private ActionPattern _currentAct;  // 現在の行動

    [SerializeField] private GameObject _targetObj;      // 攻撃対象

    private Rigidbody _rb;              // Rigidbody保存

    // ---------------------------- UnityMessage
    private void FixedUpdate()
    {
        switch (_currentAct)
        {
            case ActionPattern.IDLE:
                break;

            case ActionPattern.MOVE:
                // 移動処理
                Move();
                break;

            case ActionPattern.WAIT:
                // 待機処理
                Wait();
                break;

            case ActionPattern.ATTACK:
                break;

            case ActionPattern.MOVEANDATTACK:
                break;

            case ActionPattern.AVOID:
                break;
        }
    }

    private void OnEnable()
    {
        _rb = GetComponent<Rigidbody>();
        _currentAct = ActionPattern.MOVE;
    }


    // ---------------------------- PublicMethod


    // ---------------------------- PrivateMethod
    /// <summary>
    /// 移動処理
    /// </summary>
    private void Move()
    {
        // 追尾対象がいない時
        if (_targetObj == null)
        {
            _currentAct = ActionPattern.WAIT;
        }

        // 敵から対象へのベクトルを取得
        Vector3 moveForward = _targetObj.transform.position - transform.position;

        // 移動方向にスピードを掛ける
        _rb.linearVelocity = moveForward.normalized * _speed * Time.deltaTime + new Vector3(0, _rb.linearVelocity.y, 0);

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

    private void Wait()
    {

    }

}
