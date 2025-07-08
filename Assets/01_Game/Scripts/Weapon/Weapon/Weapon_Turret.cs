using Assets.AT;
using UnityEngine;

public class Weapon_Turret : WeaponBase
{
    [Header("’e")]
    [SerializeField] private Transform _bulletSpawnPos;
    [SerializeField] private Bullet_Linear _bulletPrefab;

    protected override void Attack()
    {
        var target = _layerSearch.NearestTargetObj.transform;

        // –C‘ä‚Ì‰ñ“]‚ÍY²‚Ì‚İi‚‚³‚ğ–³‹‚µ‚Ä…•½•ûŒü‚ÉŒü‚¯‚éj
        //Vector3 flatTargetPos = new Vector3(target.position.x, transform.position.y, target.position.z);
        //Vector3 turretDir = (flatTargetPos - transform.position).normalized;

        //if (turretDir != Vector3.zero)
        //{
        //    transform.localRotation = Quaternion.LookRotation(turretDir);
        //}

        // ’e‚Ì•ûŒü‚Í³Šm‚È3D•ûŒüi‚‚³‚àŠÜ‚Şj
        Vector3 shootDir = (target.position - _bulletSpawnPos.position).normalized;
        Quaternion shootRotation = Quaternion.LookRotation(shootDir);

        var bullet = Instantiate(
            _bulletPrefab,
            _bulletSpawnPos.position,
            shootRotation);

        bullet.Initialize(
            _targetTag,
            _currentAttack,
            _data.BulletSpeed,
            shootDir);

        GameSoundManager.Instance.PlaySFX(_fireSEName, transform, _fireSEName);
    }
}
