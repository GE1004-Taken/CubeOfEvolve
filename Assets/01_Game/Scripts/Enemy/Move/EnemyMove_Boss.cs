using R3;
using UnityEngine;

/// <summary>
/// É{ÉX
/// </summary>
public class EnemyMove_Boss : EnemyMoveBase
{
    // ---------------------------- SerializeField
    [SerializeField] private float _minDistance;

    // ---------------------------- OverrideMethod
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
        // éÄÇÃîªíË
        _status.Hp
            .Where(value => value <= 0)
            .Subscribe(value =>
            {
                GameManager.Instance.ChangeGameState(Assets.IGC2025.Scripts.GameManagers.GameState.GAMECLEAR);
            })
            .AddTo(this);
    }
}
