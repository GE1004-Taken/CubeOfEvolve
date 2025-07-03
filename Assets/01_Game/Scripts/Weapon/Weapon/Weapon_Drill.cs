using Assets.AT;
using UnityEngine;

public class Weapon_Drill : WeaponBase
{
    // ---------------------------- Field
    [SerializeField] private GameObject _hitEffect;

    // ---------------------------- OverrideMethod
    protected override void Attack()
    {
        // LayerSearch ‚É‚æ‚éŒŸõŒ‹‰Ê‚ğg‚¤
        foreach (var obj in _layerSearch.NearestTargetList)
        {
            string layerName = LayerMask.LayerToName(obj.layer);
            if (obj.transform.root.TryGetComponent<IDamageble>(out var damageble)
                && layerName == _targetTag)
            {
                damageble.TakeDamage(_currentAttack);

                Instantiate(_hitEffect, transform.position, Quaternion.identity);
            }
        }
    }

    protected override void Initialize()
    {
        base.Initialize();
    }
}
