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

    // ---------------------------- UnityMessage
    private void Start()
    {
        _layerSearch.Initialize(_range, _targetLayerName);

        // è’ìÀèàóù
        this.OnTriggerEnterAsObservable()
            .Subscribe(other =>
            {
                string layerName = LayerMask.LayerToName(other.transform.root.gameObject.layer);
                if (other.transform.root.TryGetComponent<IDamageble>(out var damageble)
                    && layerName == _targetLayerName)
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
            if (obj.TryGetComponent<IDamageble>(out var damageble))
            {
                damageble.TakeDamage(_attack);
            }
        }

        GameSoundManager.Instance.PlaySFX(_hitSEName, transform, _hitSEName);
        Instantiate(_hitEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }


    // ---------------------------- PublicMethod
    public void Initialize(
        string targetTag,
        float attack)
    {
        _targetLayerName = targetTag;
        _attack = attack;
    }
}
