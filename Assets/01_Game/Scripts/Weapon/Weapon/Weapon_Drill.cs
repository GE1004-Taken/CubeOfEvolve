using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Drill : WeaponBase
{
    // ---------------------------- Field

    // ---------------------------- OverrideMethod
    protected override void Attack()
    {
        
        foreach (var enemy in _layerSearch.NearestEnemyList)
        {
            if (enemy.transform.root.TryGetComponent<IDamageble>(out var damageble)
            && enemy.CompareTag(_targetTag))
            {

                damageble.TakeDamage(_currentAttack);
            }
        }
    }

    protected override void Initialize()
    {
        base.Initialize();
    }
}
