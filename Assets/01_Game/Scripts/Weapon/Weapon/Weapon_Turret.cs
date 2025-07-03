using Assets.AT;
using UnityEngine;

public class Weapon_Turret : WeaponBase
{
    [SerializeField] private Bullet_Linear _bulletPrefab;

    protected override void Attack()
    {
        var player = _layerSearch.NearestTargetObj.transform;
        var dir = (player.position - transform.position).normalized;

        // プレイヤー方向を向く回転を使う
        Quaternion finalRotation = Quaternion.LookRotation(dir);

        var bullet = Instantiate(
            _bulletPrefab,
            transform.position,
            finalRotation);

        bullet.Initialize(
            _targetTag,
            _currentAttack,
            _data.BulletSpeed,
            dir);

        GameSoundManager.Instance.PlaySFX(_fireSEName, transform, _fireSEName);
    }

}
