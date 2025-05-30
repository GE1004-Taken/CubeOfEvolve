using UnityEngine;

public class Weapon_Turret : WeaponBase
{
    [SerializeField] private Bullet_Linear _bulletPrefab;

    protected override void Attack()
    {
        var dir = (_nearestEnemyTransform.position - transform.position).normalized;

        var bullet = Instantiate(
            _bulletPrefab,
            transform.position,
            Quaternion.identity);

        bullet.Initialize(
            _targetTag,
            _attack,
            _bulletSpeed,
            dir);
    }
}
