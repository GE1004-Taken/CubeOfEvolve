using R3;
using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObjects/Data/WeaponDataBase")]
public class WeaponDataBase : ScriptableObject
{
    public List<WeaponData> weaponDataList;
}

[Serializable]
public class WeaponData
{
    public WeaponBase Weapon;

    public string Name;
    public int ID;
    public ReadOnlyReactiveProperty<int> Level => _level;
    [SerializeField] private SerializableReactiveProperty<int> _level;
    public ReadOnlyReactiveProperty<int> Price => _price;
    [SerializeField] private SerializableReactiveProperty<int> _price;

    public float Attack;
}
