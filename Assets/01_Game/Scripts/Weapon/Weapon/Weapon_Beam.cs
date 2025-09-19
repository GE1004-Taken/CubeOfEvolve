using Assets.AT;
using UnityEngine;

public class Weapon_Beam : WeaponBase
{
    // ---------------------------- SerializeField
    [Header("’e")]
    [SerializeField] private Transform _bulletSpawnPos;
    [SerializeField] private Bullet_Beam _bulletPrefab;
    [SerializeField] private float _distance;
    [SerializeField] private float _radius;
    [SerializeField] private float _destroySecond;
    [SerializeField] private float _attackInterval;

    // ---------------------------- OverrideMethod
    protected override void Attack()
    {
        // ”­ŽË•ûŒü‚Í SpawnPos ‚Ì forward ‚ð‚»‚Ì‚Ü‚ÜŽg‚¤
        Vector3 shootDir = _bulletSpawnPos.forward;
        Quaternion shootRotation = Quaternion.LookRotation(shootDir);

        var bullet = Instantiate(
            _bulletPrefab);

        bullet.transform.SetParent(_bulletSpawnPos.transform, false);
        bullet.transform.localPosition = Vector3.zero;
        bullet.transform.localRotation = Quaternion.identity;

        bullet.Initialize(
            _targetLayerMask,
            _currentAttack,
            _distance,
            _radius,
            shootDir,
            _destroySecond,
            _attackInterval);

        GameSoundManager.Instance.PlaySFX(_fireSEName, transform, _fireSEName);
    }
}
