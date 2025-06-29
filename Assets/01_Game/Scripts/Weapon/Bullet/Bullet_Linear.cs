using R3;
using R3.Triggers;
using UnityEngine;

public class Bullet_Linear : BulletBase
{
    // ---------------------------- SerializeField
    [SerializeField] private GameObject _hitEffect;

    // ---------------------------- Field
    private float _moveSpeed;
    private Vector3 _direction;

    // ---------------------------- UnityMessage
    private void Start()
    {
        // Ž©‘RÁ–ÅŽžŠÔ
        Destroy(gameObject, _destroySecond);

        // ˆÚ“®
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                transform.Translate(_direction * _moveSpeed * Time.deltaTime);
            })
            .AddTo(this);

        // Õ“Ëˆ—
        this.OnTriggerEnterAsObservable()
            .Subscribe(other =>
            {
                if (other.transform.root.TryGetComponent<IDamageble>(out var damageble)
                && other.CompareTag(_targetTag))
                {
                    Instantiate(_hitEffect, transform.position, Quaternion.identity);

                    damageble.TakeDamage(_attack);
                    Destroy(gameObject);
                }

                if (other.CompareTag("Ground"))
                {
                    Destroy(gameObject);
                }
            })
            .AddTo(this);
    }

    // ---------------------------- PublicMethod
    public void Initialize(
        string targetTag,
        float attack,
        float moveSpeed,
        Vector3 direction)
    {
        _targetTag = targetTag;
        _attack = attack;
        _moveSpeed = moveSpeed;
        _direction = direction;
    }
}
