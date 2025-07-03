using Assets.AT;
using R3;
using R3.Triggers;
using UnityEngine;

public class Bullet_Missile : BulletBase
{
    // ---------------------------- SerializeField
    [SerializeField] private float _rangeAbortSuffer = 15f;

    [SerializeField] private GameObject _hitEffect;

    // ---------------------------- Field
    private Vector3 _velocity;  // 方向
    private Vector3 _position;
    private Transform _target;  // 目的地
    private float _period;      // 移動期間

    private Vector3 _dis;

    // ---------------------------- UnityMessage
    private void Start()
    {
        // 自然消滅時間
        Destroy(gameObject, _destroySecond);

        _position = transform.position;

        // 移動
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                // 進行方向へ回転
                if (_velocity != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(_velocity);
                }

                // 距離
                if (_target != null)
                {
                    _dis = _target.position - transform.position;
                }

                var accelerator = Vector3.zero;
                accelerator += (_dis - _velocity * _period) * 2f / (_period * _period);

                if (accelerator.magnitude > _rangeAbortSuffer)
                {
                    accelerator = accelerator.normalized * _rangeAbortSuffer;
                }

                _period -= Time.deltaTime;

                _velocity += accelerator * Time.deltaTime;
                _position += _velocity * Time.deltaTime;
                transform.position = _position;
            })
            .AddTo(this);

        // 衝突処理
        this.OnTriggerEnterAsObservable()
            .Subscribe(other =>
            {
                string layerName = LayerMask.LayerToName(other.transform.root.gameObject.layer);
                if (other.transform.root.TryGetComponent<IDamageble>(out var damageble)
                    && layerName == _targetLayerName)
                {
                    damageble.TakeDamage(_attack);

                    HitMethod();
                }

                if (other.CompareTag("Ground"))
                {
                    HitMethod();
                }

                // 当たった時の共通処理
                void HitMethod()
                {
                    GameSoundManager.Instance.PlaySFX(_hitSEName, transform, _hitSEName);
                    Instantiate(_hitEffect, transform.position, Quaternion.identity);

                    Destroy(gameObject);
                }
            })
            .AddTo(this);
    }

    // ---------------------------- PublicMethod
    public void Initialize(
        string targetTag,
        float attack,
        Vector3 velocity,
        Transform target,
        float period)
    {
        _targetLayerName = targetTag;
        _attack = attack;
        _velocity = velocity;
        _target = target;
        _period = period;
    }
}
