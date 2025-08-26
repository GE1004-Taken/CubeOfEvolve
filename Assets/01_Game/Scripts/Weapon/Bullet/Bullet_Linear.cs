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
        // Ž©‘RÁ–ÅŽžŠÔ
        Destroy(gameObject, _destroySecond);

        // ˆÚ“®
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                if (GameManager.Instance.CurrentGameState.CurrentValue != Assets.IGC2025.Scripts.GameManagers.GameState.BATTLE)
                {
                    return;
                }

                if (_direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(_direction);
                }

                transform.Translate(_direction * _moveSpeed * Time.deltaTime, Space.World);
            })
            .AddTo(this);


        // Õ“Ëˆ—
        this.OnTriggerEnterAsObservable()
            .Subscribe(other =>
            {
                GameObject rootObj = other.transform.root.gameObject;

                if ((_targetLayerMask.value & (1 << rootObj.layer)) != 0 &&
                    rootObj.TryGetComponent<IDamageble>(out var damageble))
                {
                    damageble.TakeDamage(_attack);
                    HitMethod();

                    return;
                }

                // ’n–Ê‚É“–‚½‚Á‚½Žž
                if (other.CompareTag("Ground"))
                {
                    HitMethod();
                }

                void HitMethod()
                {
                    GameSoundManager.Instance.PlaySFX(_hitSEName, transform, _hitSEName);
                    var effect = Instantiate(_hitEffect, transform.position, Quaternion.identity);
                    effect.AddComponent<StopEffect>();
                    Destroy(gameObject);
                }
            })
            .AddTo(this);
    }

    // ---------------------------- PublicMethod
    public void Initialize(
        LayerMask layerMask,
        float attack,
        float moveSpeed,
        Vector3 direction)
    {
        _targetLayerMask = layerMask;
        _attack = attack;
        _moveSpeed = moveSpeed;
        _direction = direction;
    }
}
