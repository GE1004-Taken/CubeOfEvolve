using Assets.AT;
using R3;
using R3.Triggers;
using UnityEngine;

public class Bullet_Beam : BulletBase
{
    // ---------------------------- SerializeField
    [SerializeField] private GameObject _hitEffect;

    // ---------------------------- Field
    private float _distance;        // 距離
    private float _radius;          // 半径
    private Vector3 _direction;     // 向き
    private float _attackInterval;        // 攻撃間隔
    private float _currentInterval; // 攻撃間隔の経過時間

    // ---------------------------- UnityMessage
    private void Start()
    {
        // 毎フレーム監視
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                // ゲーム状態がバトル中でなければ処理しない
                if (GameManager.Instance.CurrentGameState.CurrentValue
                    != Assets.IGC2025.Scripts.GameManagers.GameState.BATTLE)
                {
                    return;
                }

                // 自然消滅処理
                DestroySecondCount();

                if (_currentInterval < _attackInterval)
                {
                    _currentInterval += Time.deltaTime;
                    return;
                }
                else
                {
                    // インターバルをリセット
                    _currentInterval = 0f;
                }

                Vector3 beamDir = transform.forward; // 弾が向いている方向
                Ray ray = new Ray(transform.position, beamDir);

                Debug.DrawRay(transform.position, beamDir * _distance, Color.red);

                if (Physics.SphereCast(ray, _radius, out RaycastHit hit, _distance, _targetLayerMask))
                {
                    GameObject rootObj = hit.collider.transform.root.gameObject;

                    if ((_targetLayerMask.value & (1 << rootObj.layer)) != 0 &&
                        rootObj.TryGetComponent<IDamageble>(out var damageble))
                    {
                        damageble.TakeDamage(_attack);
                    }

                    // ヒットエフェクト生成
                    GameSoundManager.Instance.PlaySFX(_hitSEName, transform, _hitSEName);
                    var effect = Instantiate(_hitEffect, hit.point, Quaternion.identity);
                    effect.AddComponent<StopEffect>();
                }
            })
            .AddTo(this);
    }

    // ---------------------------- PublicMethod
    public void Initialize(
        LayerMask layerMask,
        float attack,
        float distance,
        float radius,
        Vector3 direction,
        float destroySecond,
        float interval)
    {
        _targetLayerMask = layerMask;
        _attack = attack;
        _distance = distance;
        _radius = radius;
        _direction = direction;
        _destroySecond = destroySecond;
        _attackInterval = interval;
    }
}
