using UnityEngine;

public abstract class EnemyMoveBase : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField] protected EnemyStatus _status;
    [SerializeField] private LayerMask _obstacleMask; // 壁や柱など障害物レイヤーのみを指定

    // ---------------------------- Field
    protected Rigidbody _rb;
    protected GameObject _targetObj;

    // 回避用フィールド
    private Vector3 _moveDir;
    private float _nextCheckTime;
    private float _holdDirectionTime;

    // ---------------------------- UnityMessage
    private void Start()
    {
        _status = GetComponent<EnemyStatus>();
        _rb = GetComponent<Rigidbody>();
        _targetObj = PlayerMonitoring.Instance.PlayerObj;

        Initialize();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.CurrentGameState.CurrentValue != Assets.IGC2025.Scripts.GameManagers.GameState.BATTLE)
        {
            _rb.linearVelocity = Vector3.zero;
            return;
        }

        switch (_status.CurrentAct.CurrentValue)
        {
            case EnemyStatus.ActionPattern.IDLE:
                break;
            case EnemyStatus.ActionPattern.MOVE:
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

    // ---------------------------- ProtectedMethod
    /// <summary>
    /// 直線移動（回避なし）
    /// </summary>
    protected void LinearMovement()
    {
        if (_targetObj == null) return;

        Vector3 moveForward = _targetObj.transform.position - transform.position;
        moveForward.y = 0;

        _rb.linearVelocity = _status.Speed * Time.deltaTime * moveForward.normalized
                           + new Vector3(0, _rb.linearVelocity.y, 0);

        if (moveForward != Vector3.zero)
        {
            Vector3 direction = _targetObj.transform.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        }
    }

    /// <summary>
    /// 直線移動（回避あり）
    /// </summary>
    /// <param name="checkInterval">方向を再計算する間隔</param>
    protected void LinearMovementWithAvoidance(float checkInterval = 0.2f)
    {
        if (_targetObj == null) return;

        // 一定時間ごとに進行方向を再計算
        if (Time.time >= _nextCheckTime || _holdDirectionTime <= 0)
        {
            _nextCheckTime = Time.time + checkInterval;
            _holdDirectionTime = 0.2f; // 方向維持時間
            _moveDir = FindDirection();
        }

        _holdDirectionTime -= Time.deltaTime;

        // 移動
        _rb.linearVelocity = _moveDir * _status.Speed * Time.deltaTime
                           + new Vector3(0, _rb.linearVelocity.y, 0);

        // 回転
        if (_moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_moveDir);
            transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        }
    }

    private bool _isSlidingWall = false;

    /// <summary>
    /// 障害物がある場合は、角度調整を優先し、
    /// どうしても抜けられない場合に壁沿いスライドで回避する
    /// </summary>
    private Vector3 FindDirection()
    {
        Vector3 dirToTarget = (_targetObj.transform.position - transform.position).normalized;
        dirToTarget.y = 0;

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        float sphereRadius = 0.5f;

        // プレイヤーとの距離に応じて検知距離を可変にする
        float targetDist = Vector3.Distance(transform.position, _targetObj.transform.position);
        float detectDistance = Mathf.Min(10f, targetDist);

        // 1. まず直進チェック
        Debug.DrawRay(origin, dirToTarget * detectDistance, Color.green); // 直進方向
        if (!Physics.SphereCast(origin, sphereRadius, dirToTarget, out RaycastHit hit, detectDistance, _obstacleMask))
        {
            _isSlidingWall = false;
            return dirToTarget; // 障害物がなければ直進
        }

        // 3. 壁沿いスライドを試す（ただし両方とも空いてたら角度探索に回す）
        Vector3 slideRight = Vector3.Cross(hit.normal, Vector3.up).normalized;
        Vector3 slideLeft = -slideRight;
        float slideCheckDistance = Mathf.Min(20f, targetDist * 0.8f);
        bool canSlideRight = !Physics.SphereCast(origin, sphereRadius, slideRight, out _, slideCheckDistance, _obstacleMask);
        bool canSlideLeft = !Physics.SphereCast(origin, sphereRadius, slideLeft, out _, slideCheckDistance, _obstacleMask);

        Debug.DrawRay(origin, slideRight * slideCheckDistance, Color.cyan); // 壁沿い右
        Debug.DrawRay(origin, slideLeft * slideCheckDistance, Color.cyan);  // 壁沿い左

        if (canSlideRight ^ canSlideLeft) // 片方だけ空いている場合はスライド確定
        {
            _isSlidingWall = true;

            // スライド方向
            Vector3 slideDir = canSlideRight ? slideRight : slideLeft;

            // プレイヤー方向とのブレンド
            Vector3 blendedDir = (slideDir * 0.7f + dirToTarget * 0.3f).normalized;

            //Debug.DrawRay(origin, blendedDir * slideCheckDistance, Color.magenta); // スライド＋プレイヤー方向

            return blendedDir;
        }
        else if (canSlideRight && canSlideLeft)
        {
            // ★ 両方空いてたらここでは決めず → 角度探索へ進む
        }

        // 2. 左右15°ずつ最大90°まで探索（角度回避）
        for (int angle = 30; angle <= 90; angle += 30)
        {
            foreach (int sign in new int[] { 1, -1 }) // 右回り、左回り
            {
                Vector3 testDir = Quaternion.Euler(0, sign * angle, 0) * dirToTarget;
                Debug.DrawRay(origin, testDir * detectDistance, Color.yellow); // 探索方向
                if (!Physics.SphereCast(origin, sphereRadius, testDir, out _, detectDistance, _obstacleMask))
                    return testDir.normalized;
            }
        }

        // 4. 完全に塞がれていたら微小ランダム移動して再探索
        Vector3 microMove = Quaternion.Euler(0, Random.Range(-45, 45), 0) * dirToTarget;
        Debug.DrawRay(origin, microMove * detectDistance, Color.red); // ランダム方向
        return microMove.normalized;
    }

    // ---------------------------- VirtualMethod
    public virtual void Initialize() { }

    // ---------------------------- AbstractMethod
    public abstract void Move();
}
