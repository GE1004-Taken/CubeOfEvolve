using R3;
using R3.Triggers;
using UnityEngine;

public class Bullet_Missile : BulletBase
{
    // ---------------------------- SerializeField
    [SerializeField] private float _rangeAbortSuffer = 15f;

    [SerializeField] private GameObject _hitEffect;

    // ---------------------------- Field
    private Vector3 _velocity;  // •ûŒü
    private Vector3 _position;
    private Transform _target;  // –Ú“I’n
    private float _period;      // ˆÚ“®ŠúŠÔ

    private Vector3 _dis;

    // ---------------------------- UnityMessage
    private void Start()
    {
        // Ž©‘RÁ–ÅŽžŠÔ
        Destroy(gameObject, _destroySecond);

        _position = transform.position;

        // ˆÚ“®
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                // ‹——£
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

        // Õ“Ëˆ—
        this.OnTriggerEnterAsObservable()
            .Subscribe(other =>
            {
                string layerName = LayerMask.LayerToName(other.transform.root.gameObject.layer);
                if (other.transform.root.TryGetComponent<IDamageble>(out var damageble)
                    && layerName == _targetLayerName)
                {
                    Instantiate(_hitEffect, transform.position, Quaternion.identity);

                    damageble.TakeDamage(_attack);
                    Destroy(gameObject);
                }

                if (other.CompareTag("Ground"))
                {
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
