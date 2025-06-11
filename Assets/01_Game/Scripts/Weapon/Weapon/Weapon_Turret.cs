using UnityEngine;

public class Weapon_Turret : WeaponBase
{
    [SerializeField] private Bullet_Linear _bulletPrefab;

    protected override void Attack()
    {
        var dir = (_layerSearch.NearestEnemyObj.transform.position - transform.position).normalized;

        var bullet = Instantiate(
            _bulletPrefab,
            transform.position,
            Quaternion.identity);

        bullet.Initialize(
            _targetTag,
            _currentAttack,
            _data.BulletSpeed,
            dir);
    }
}
