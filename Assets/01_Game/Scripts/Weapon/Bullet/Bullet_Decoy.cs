using Assets.AT;
using R3;
using R3.Triggers;
using UnityEngine;

public class Bullet_Decoy : BulletBase, IDamageble
{
    // ---------------------------- SerializeField
    [SerializeField] float _maxHp;
    [SerializeField] private LayerSearch _layerSearch;
    [SerializeField] private GameObject _hitEffect;

    // ---------------------------- Field
    private ReactiveProperty<float> _currentHp = new();

    // ---------------------------- UnityMessage
    private void Awake()
    {
        _currentHp.Value = _maxHp;

        _currentHp
            .Where(value => value <= 0)
            .Subscribe(_ =>
            {
                Explosion();
            })
            .AddTo(this);

        // è’ìÀèàóù
        this.OnTriggerEnterAsObservable()
            .Subscribe(other =>
            {
                if (other.CompareTag("Ground"))
                {
                    Debug.Log("AAA");
                    GetComponent<Rigidbody>().isKinematic = true;
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

    // ---------------------------- Interface
    public void TakeDamage(float damage)
    {
        _currentHp.Value -= damage;
    }
}
