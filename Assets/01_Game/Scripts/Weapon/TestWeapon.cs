using UnityEngine;

public class TestWeapon : BaseWeapon
{
    [SerializeField] private TestBullet _bulletPrefab;

    protected override void Attack()
    {
        var dir = (nearestEnemyTransform.position - transform.position).normalized;

        var bullet = Instantiate(
            _bulletPrefab,
            transform.position,
            Quaternion.identity);

        bullet.Initialize(
            atk,
            attackSpeed,
            dir);
    }
}
