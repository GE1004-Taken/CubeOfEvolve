using System;
using UnityEngine;

[Serializable]
public class ModuleParameter
{
    // -----SerializeField
    [SerializeField] private float _attack;
    [SerializeField] private float _bulletSpeed; 
    [SerializeField] private float _interval; 
    [SerializeField] private float _searchRange;

    // -----Property
    public float Attack => _attack;
    public float BulletSpeed => _bulletSpeed;
    public float Interval => _interval;
    public float SearchRange => _searchRange;

}
