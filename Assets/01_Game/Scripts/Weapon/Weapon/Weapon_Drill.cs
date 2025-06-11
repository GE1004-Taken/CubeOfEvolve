using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Drill : WeaponBase
{
    // ---------------------------- Field
    private readonly List<GameObject> _enemyList = new();

    // ---------------------------- OverrideMethod
    protected override void Attack()
    {
        foreach (var enemy in _enemyList)
        {
            if (enemy
                .TryGetComponent<IDamageble>(out var damageble))
            {
                damageble.TakeDamage(_currentAttack);
            }
        }
    }

    protected override void Initialize()
    {
        base.Initialize();

        this.OnTriggerEnterAsObservable()
            .Where(other => other.CompareTag(_targetTag))
            .Subscribe(other =>
            {
                _enemyList.Add(other.gameObject);
            })
            .AddTo(this);

        this.OnTriggerExitAsObservable()
            .Where(other => other.CompareTag(_targetTag))
            .Subscribe(other =>
            {
                _enemyList.Remove(_enemyList[_enemyList.IndexOf(other.gameObject)]);
            })
            .AddTo(this);

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                // デストロイされた要素をRemoveする
                if (_enemyList.Count > 0)
                {
                    _enemyList.RemoveAll(x => x == null);
                }
            })
            .AddTo(this);
    }
}
