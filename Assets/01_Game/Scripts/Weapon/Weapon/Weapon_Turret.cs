using Assets.AT;
using UnityEngine;

public class Weapon_Turret : WeaponBase
{
    [Header("モデル")]
    [SerializeField] private GameObject _model;

    [Header("弾")]
    [SerializeField] private Transform _bulletSpawnPos;
    [SerializeField] private Bullet_Linear _bulletPrefab;

    protected override void Attack()
    {
        var target = _layerSearch.NearestTargetObj.transform;

        // 砲台の回転はY軸のみ（高さを無視して水平方向に向ける）
        Vector3 flatTargetPos = new Vector3(target.position.x, transform.position.y, target.position.z);
        Vector3 turretDir = (flatTargetPos - transform.position).normalized;

        if (turretDir != Vector3.zero)
        {
            _model.transform.rotation = Quaternion.LookRotation(turretDir);
        }

        // 弾の方向は正確な3D方向（高さも含む）
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
