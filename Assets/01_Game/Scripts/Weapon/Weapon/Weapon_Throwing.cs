using Assets.AT;
using UnityEngine;

public class Weapon_Throwing : WeaponBase
{
    [Header("弾")]
    [SerializeField] private Transform _bulletSpawnPos;
    [SerializeField] private Bullet_Bomb _bullet;
    [SerializeField] private float _shootAngle;

    protected override void Attack()
    {
        // ボールを射出する
        ThrowingBall();

        GameSoundManager.Instance.PlaySFX(_fireSEName, transform, _fireSEName);
    }

    /// <summary>
    /// ボールを射出する
    /// </summary>
    private void ThrowingBall()
    {
        if (_bullet != null && _layerSearch.NearestTargetObj != null)
        {
            var ball = Instantiate(_bullet, _bulletSpawnPos.position, Quaternion.identity);

            Transform enemy = _layerSearch.NearestTargetObj.transform;
            Rigidbody enemyRb = _layerSearch.NearestTargetObj.GetComponent<Rigidbody>();

            Vector3 targetPosition = enemy.position;
            Vector3 enemyVelocity = enemyRb != null ? enemyRb.linearVelocity : Vector3.zero;

            // 仮の速度で飛行時間を予測
            float dummySpeed = 10f; // 適当な初速（後で調整）
            float distance = Vector3.Distance(_bulletSpawnPos.position, targetPosition);
            float flightTime = distance / dummySpeed;

            // 予測位置
            Vector3 predictedPosition = targetPosition + enemyVelocity * flightTime;

            // 実際の速度を再計算
            Vector3 velocity = CalculateVelocity(_bulletSpawnPos.position, predictedPosition, _shootAngle);

            Rigidbody rid = ball.GetComponent<Rigidbody>();
            rid.AddForce(velocity * rid.mass, ForceMode.Impulse);


            ball.Initialize(
                _targetTag,
                _currentAttack);
        }
    }


    /// <summary>
    /// 標的に命中する射出速度の計算
    /// </summary>
    /// <param name="startPos">射出開始座標</param>
    /// <param name="endPos">標的の座標</param>
    /// <returns>射出速度</returns>
    private Vector3 CalculateVelocity(Vector3 startPos, Vector3 endPos, float angle)
    {
        // 射出角をラジアンに変換
        float rad = angle * Mathf.PI / 180;

        // 水平方向の距離x
        float x = Vector2.Distance(new Vector2(startPos.x, startPos.z), new Vector2(endPos.x, endPos.z));

        // 垂直方向の距離y
        float y = startPos.y - endPos.y;

        // 斜方投射の公式を初速度について解く
        float speed = Mathf.Sqrt(-Physics.gravity.y * Mathf.Pow(x, 2) / (2 * Mathf.Pow(Mathf.Cos(rad), 2) * (x * Mathf.Tan(rad) + y)));

        if (float.IsNaN(speed))
        {
            // 条件を満たす初速を算出できなければVector3.zeroを返す
            return Vector3.zero;
        }
        else
        {
            return (new Vector3(endPos.x - startPos.x, x * Mathf.Tan(rad), endPos.z - startPos.z).normalized * speed);
        }
    }
}
