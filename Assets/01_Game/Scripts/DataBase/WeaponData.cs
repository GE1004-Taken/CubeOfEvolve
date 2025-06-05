using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Data/WeaponData")]
public class WeaponData : ScriptableObject
{
    public WeaponBase Weapon;

    public int ID;

    public float Attack;

    public float BulletSpeed;
    public float Interval;
    public float SearchRange;

}
