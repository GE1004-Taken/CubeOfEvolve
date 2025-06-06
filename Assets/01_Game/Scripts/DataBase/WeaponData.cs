using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Data/WeaponData")]
public class WeaponData : ScriptableObject
{
    public float Attack;

    public float BulletSpeed;
    public float Interval;
    public float SearchRange;
}
