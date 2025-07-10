using Assets.AT;
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
        // é©ëRè¡ñ≈éûä‘
        Destroy(gameObject, _destroySecond);

        // à⁄ìÆ
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                if (_direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(_direction);
                }

                transform.Translate(_direction * _moveSpeed * Time.deltaTime, Space.World);
            })
            .AddTo(this);


        // è’ìÀèàóù
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

                HitMethod();

                // ìñÇΩÇ¡ÇΩéûÇÃã§í èàóù
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
        float moveSpeed,
        Vector3 direction)
    {
        _targetLayerName = targetTag;
        _attack = attack;
        _moveSpeed = moveSpeed;
        _direction = direction;
    }
}
