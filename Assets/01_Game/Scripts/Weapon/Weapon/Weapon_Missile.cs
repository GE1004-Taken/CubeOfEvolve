using UnityEngine;

public class Weapon_Missile : WeaponBase
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

            // ‰¡•ûŒü ~ ¶‰Eƒ‰ƒ“ƒ_ƒ€ ~ ƒ‰ƒ“ƒ_ƒ€‹——£ { ƒ‰ƒ“ƒ_ƒ€‚‚³
            var velocity
                = transform.right * randomDir * Random.Range(1, 5) * 5 + new Vector3(0, Random.Range(1, 4) * 5, 0);

            bullet.Initialize(
                _targetTag,
                _attack,
                velocity,
                _layerSearch.NearestEnemyObj.transform,
                2);
        }
    }
}
