using R3;
using R3.Triggers;
using UnityEngine;

public class EnemyMove_Avoid : EnemyMoveBase
{
    // ---------------------------- SerializeField
    [SerializeField, Tooltip("‘ÎÛŒŸ’m—p")] private SphereCollider _sphereCollider;
    [SerializeField, Tooltip("‰ñ”ğ‹——£")] private float _avoidanceDistance;
    [SerializeField, Tooltip("‰ñ”ğŠÔŠu")] private float _interval;

    // ---------------------------- Field
    private float _currentInterval;

    // ---------------------------- PrivateMethod
    /// <summary>
    /// ‰ñ”ğˆ—
    /// </summary>
    private void Avoid()
    {
        int randomValue = Random.value < 0.5f ? -1 : 1;

        transform.position += transform.right * _avoidanceDistance * randomValue;
    }

    // ---------------------------- OverrideMethod
    /// <summary>
    /// ‰Šú‰»
    /// </summary>
    public override void Initialize()
    {
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                if (_currentInterval < _interval)
                {
                    _currentInterval += Time.deltaTime;
                }
            })
            .AddTo(this);

        this.OnTriggerEnterAsObservable()
            .Where(other => other.TryGetComponent<BulletBase>(out var component) && gameObject.CompareTag(component.TargetTag))
            .Subscribe(x =>
            {
                if (_currentInterval >= _interval)
                {
                    // ‰ñ”ğˆ—
                    Avoid();

                    _currentInterval = 0;
                }
            })
            .AddTo(this);
    }

    /// <summary>
    /// ˆÚ“®ˆ—
    /// </summary>
    public override void Move()
    {
        LinearMovement();
    }
}
