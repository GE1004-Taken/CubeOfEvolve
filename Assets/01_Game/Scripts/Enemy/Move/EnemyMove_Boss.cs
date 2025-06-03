using R3;
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

    public override void Initialize()
    {
        // Ž€‚Ì”»’è
        _status.Hp
            .Where(value => value <= 0)
            .Subscribe(value =>
            {
                GameManager.Instance.ChangeGameState(Assets.IGC2025.Scripts.GameManagers.GameState.GAMECLEAR);
            })
            .AddTo(this);
    }
}
