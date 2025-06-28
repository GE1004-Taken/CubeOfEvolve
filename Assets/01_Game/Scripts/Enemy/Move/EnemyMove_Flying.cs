using UnityEngine;

/// <summary>
/// ”ò‚Ô“G
/// </summary>
public class EnemyMove_Flying : EnemyMoveBase
{
    // ---------------------------- SerializeField
    [SerializeField] private float _minDistance;
    [SerializeField] private float _minFlyingHeight;
    [SerializeField] private float _maxFlyingHeight;
    [SerializeField] private float _flyingPower;

    [SerializeField] private LayerMask _layerMask;

    // ---------------------------- OverrideMethod
    public override void Move()
    {
        if (Vector3.Distance(_targetObj.transform.position, transform.position) >= _minDistance)
        {
            LinearMovement();
        }

        Flying();
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// ”ò‚Ôˆ—
    /// </summary>
    private void Flying()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, _layerMask))
        {
            float dis = hit.transform.position.y - transform.position.y;

            // w’è‚Ì‚“xˆÈ‰º‚Ì
            if (Mathf.Abs(dis) < _minFlyingHeight)
            {
                _rb.AddForce(Vector3.up * _flyingPower, ForceMode.Impulse);
            }
            else if (Mathf.Abs(dis) > _maxFlyingHeight)
            {
                _rb.linearVelocity = new(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            }
        }
    }
}
