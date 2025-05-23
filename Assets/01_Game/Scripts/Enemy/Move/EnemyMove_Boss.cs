using UnityEngine;

public class EnemyMove_Boss : EnemyMoveBase
{
    [SerializeField] private float _minDistance;

    public override void Move()
    {
        var rangeWithPlayer = Vector3.Distance(transform.position, _targetObj.transform.position);

        if (rangeWithPlayer >= _minDistance)
        {
            LinearMovement();
        }
    }
}
