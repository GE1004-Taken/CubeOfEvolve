using Assets.AT;
using UnityEngine;

public class Weapon_Turret : WeaponBase
{
    // ---------------------------- SerializeField
    [Header("’e")]
    [SerializeField] private Transform _bulletSpawnPos;
    [SerializeField] private Bullet_Linear _bulletPrefab;

    // ---------------------------- OverrideMethod
    protected override void Attack()
    {
        // ”­ŽË•ûŒü‚Í SpawnPos ‚Ì forward ‚ð‚»‚Ì‚Ü‚ÜŽg‚¤
        Vector3 shootDir = _bulletSpawnPos.forward;
        Quaternion shootRotation = Quaternion.LookRotation(shootDir);

        var bullet = Instantiate(
            _bulletPrefab,
            _bulletSpawnPos.position,
            shootRotation);

        bullet.Initialize(
            _targetLayerMask,
            _currentAttack,
            _data.ModuleState.BulletSpeed,
            shootDir);

        GameSoundManager.Instance.PlaySFX(_fireSEName, transform, _fireSEName);
    }
}
