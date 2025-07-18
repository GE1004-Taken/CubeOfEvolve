using Assets.AT;
using R3;
using R3.Triggers;
using UnityEngine;

public class Bullet_Bomb : BulletBase
{
    // ---------------------------- Field
    [SerializeField] private float _range;
    [SerializeField] private LayerSearch _layerSearch;

    [SerializeField] private GameObject _hitEffect;

    // ---------------------------- Field
    private Vector3 _velocity;

    // ---------------------------- UnityMessage
    private void Start()
    {
        _layerSearch.Initialize(_range, _targetLayerMask);

        GameManager.Instance.CurrentGameState
            .Subscribe(value =>
            {
                var rb = GetComponent<Rigidbody>();

                if (value != Assets.IGC2025.Scripts.GameManagers.GameState.BATTLE)
                {
                    _velocity = rb.linearVelocity;

                    rb.isKinematic = true;
                }
                else
                {
                    rb.isKinematic = false;

                    rb.linearVelocity = _velocity;
                }
            })
            .AddTo(this);

        // è’ìÀèàóù
        this.OnTriggerEnterAsObservable()
            .Subscribe(other =>
            {
                GameObject rootObj = other.transform.root.gameObject;

                if ((_targetLayerMask.value & (1 << rootObj.layer)) != 0)
                {
                    Explosion();
                }

                if (other.CompareTag("Ground"))
                {
                    Explosion();
                }
            })
            .AddTo(this);
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// îöî≠
    /// </summary>
    private void Explosion()
    {
        // LayerSearch Ç…ÇÊÇÈåüçıåãâ ÇégÇ§
        foreach (var obj in _layerSearch.NearestTargetList)
        {
            GameObject rootObj = obj.transform.root.gameObject;

            if ((_targetLayerMask.value & (1 << rootObj.layer)) != 0 &&
                rootObj.TryGetComponent<IDamageble>(out var damageble))
            {
                damageble.TakeDamage(_attack);
            }
        }

        GameSoundManager.Instance.PlaySFX(_hitSEName, transform, _hitSEName);
        var effect = Instantiate(_hitEffect, transform.position, Quaternion.identity);
        effect.AddComponent<StopEffect>();
        Destroy(gameObject);
    }


    // ---------------------------- PublicMethod
    public void Initialize(
        LayerMask layerMask,
        float attack)
    {
        _targetLayerMask = layerMask;
        _attack = attack;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, _range);
    }
}
