using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using static App.BaseSystem.DataStores.ScriptableObjects.Modules.ModuleData;


[CreateAssetMenu(menuName = "ScriptableObjects/DataBase/WeaponDataBase")]
public class WeaponDataBase : ScriptableObject
{
    public List<ShopData> WeaponDataList;
}

[Serializable]
public class ShopData
{
    public MODULE_TYPE _moduleType;
    public string ViewName = "ï\é¶ñº";
    public string Description = "ê‡ñæï∂";
    public Sprite MainSprite;
    public Sprite BlockSprite;
    public int BasePrice = 100;
    public WeaponData Data;
    public WeaponBase Weapon;

    public ReadOnlyReactiveProperty<int> Level => _level;
    [SerializeField] private SerializableReactiveProperty<int> _level;
    public ReadOnlyReactiveProperty<int> Price => _price;
    [SerializeField] private SerializableReactiveProperty<int> _price;
}
