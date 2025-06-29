public class Weapon_Drill : WeaponBase
{
    // ---------------------------- OverrideMethod
    protected override void Attack()
    {
        // LayerSearch ‚É‚æ‚éŒŸõŒ‹‰Ê‚ğg‚¤
        foreach (var obj in _layerSearch.NearestTargetList)
        {
            if (obj.TryGetComponent<IDamageble>(out var damageble))
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
