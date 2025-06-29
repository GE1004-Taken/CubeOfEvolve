using R3;
using R3.Triggers;
using UnityEngine;

public class Bullet_Bomb : BulletBase
{
    // ---------------------------- Field
    [SerializeField] private float _range;
    [SerializeField] private LayerSearch _layerSearch;

    [SerializeField] private GameObject _effect;

    // ---------------------------- Field

    // ---------------------------- UnityMessage
    private void Start()
    {
        _layerSearch.Initialize(_range, _targetTag);

        // Õ“Ëˆ—
        this.OnTriggerEnterAsObservable()
            .Subscribe(other =>
            {
                if (other.CompareTag("Ground") || other.transform.root.CompareTag(_targetTag))
                {
                    Explosion();
                }
            })
            .AddTo(this);
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// ”š”­
    /// </summary>
    private void Explosion()
    {
        // LayerSearch ‚É‚æ‚éŒŸõŒ‹‰Ê‚ğg‚¤
        foreach (var obj in _layerSearch.NearestTargetList)
        {
            if (obj.TryGetComponent<IDamageble>(out var damageble))
            {
                damageble.TakeDamage(_attack);
            }
        }

        Instantiate(_effect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }


    // ---------------------------- PublicMethod
    public void Initialize(
        string targetTag,
        float attack)
    {
        _targetTag = targetTag;
        _attack = attack;
    }
}
