using Assets.AT;
using UnityEngine;

public class Weapon_Turret : WeaponBase
{
    // ---------------------------- SerializeField
    [Header("íe")]
    [SerializeField] private Transform _bulletSpawnPos;
    [SerializeField] private Bullet_Linear _bulletPrefab;

    // ---------------------------- OverrideMethod
    protected override void Attack()
    {
        var target = _layerSearch.NearestTargetObj.transform;

        // íeÇÃï˚å¸ÇÕê≥ämÇ»3Dï˚å¸ÅiçÇÇ≥Ç‡ä‹ÇﬁÅj
        Vector3 shootDir = (target.position - _bulletSpawnPos.position).normalized;
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
