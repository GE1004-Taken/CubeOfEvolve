using UnityEngine;

public class EnemyMove_Ground : EnemyMoveBase
{
    [SerializeField] private float _minDistance;

    public override void Move()
    {
        if (Vector3.Distance(_targetObj.transform.position, transform.position) >= _minDistance)
        {
            LinearMovement();
        }
    }
}
