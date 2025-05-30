using R3;
using R3.Triggers;
using UnityEngine;

public class Bullet_Linear : BulletBase
{
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
            .Subscribe(collider =>
            {
                if (collider.transform.parent == null) return;

                if (collider.transform.parent.TryGetComponent<IDamageble>(out var damageble)
                    && collider.CompareTag(_targetTag))
                {
                    damageble.TakeDamage(_attack);
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
