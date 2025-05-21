public class Weapon_Drill : BaseWeapon
{
    protected override void Attack()
    {
        foreach (var enemy in _inRangeEnemies)
        {
            if (enemy
                .TryGetComponent<IDamageble>(out var damageble))
            {
                damageble.TakeDamage(_atk);
            }
        }
    }
}
