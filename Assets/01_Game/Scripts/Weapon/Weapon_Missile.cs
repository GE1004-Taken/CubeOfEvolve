using UnityEngine;

public class Weapon_Missile : BaseWeapon
{
    [SerializeField] private Bullet_Missile _bulletPrefab;
    [SerializeField] private int _spawnCount;

    protected override void Attack()
    {
        for (int i = 0; i < _spawnCount; i++)
        {
            var bullet = Instantiate(
            _bulletPrefab,
            transform.position,
            Quaternion.identity);

            int randomDir = Random.Range(0, 2) == 0 ? 1 : -1;

            bullet.Initialize(
                _atk,
                _attackSpeed,
                transform.right * Random.Range(1, 5) * 5 * randomDir + new Vector3(0, Random.Range(1, 4) * 5, 0),
                _nearestEnemyTransform,
                2);
        }
    }
}
