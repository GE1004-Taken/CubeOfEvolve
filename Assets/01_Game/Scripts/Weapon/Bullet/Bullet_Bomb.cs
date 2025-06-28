using R3;
using R3.Triggers;
using UnityEngine;

public class Bullet_Bomb : BulletBase
{
    // ---------------------------- Field
    [SerializeField] private float _range;
    [SerializeField] private LayerSearch _layerSearch;

    // ---------------------------- Field

    // ---------------------------- UnityMessage
    private void Start()
    {
        _layerSearch.Initialize(_range, _targetTag);

        // Õ“Ëˆ—
        this.OnTriggerEnterAsObservable()
            .Subscribe(other =>
            {
                Debug.Log(_targetTag);

                if (other.CompareTag("Ground") || other.transform.root.CompareTag(_targetTag))
                {
                    Explosion();
                }
            })
            .AddTo(this);
    }
    private void Update()
    {
        Debug.Log($"”š”­”ÍˆÍ“à: {_layerSearch.NearestEnemyList.Count}");
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// ”š”­
    /// </summary>
    private void Explosion()
    {
        foreach (var item in _layerSearch.NearestEnemyList)
        {
            if (item == null) continue;

            if (item.transform.root.TryGetComponent<IDamageble>(out var damageble))
            {
                damageble.TakeDamage(_attack);
            }
        }

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
