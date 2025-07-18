using Assets.AT;
using UnityEngine;

public class Weapon_Missile : WeaponBase
{
    // ---------------------------- SerializeField
    [SerializeField] private Bullet_Missile _bulletPrefab;
    [SerializeField] private int _spawnCount;

    // ---------------------------- OverrideMethod
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
                _targetLayerMask,
                _currentAttack,
                velocity,
                _layerSearch.NearestTargetObj.transform,
                2);
        }

        GameSoundManager.Instance.PlaySFX(_fireSEName, transform, _fireSEName);
    }
}
