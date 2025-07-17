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
            GameObject rootObj = obj.transform.root.gameObject;

            if ((_targetLayerMask.value & (1 << rootObj.layer)) != 0 &&
                rootObj.TryGetComponent<IDamageble>(out var damageble))
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
