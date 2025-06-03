using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/DataBase/Item")]
public class ItemDataBase : ScriptableObject
{
    public List<ItemDataItem> ItemDataList = new();

    public ItemDataItem FindItemByName(ItemData data)
    {
        return ItemDataList.FirstOrDefault(item => item.Data == data);
    }
}

[Serializable]
public class ItemDataItem
{
    public ItemData Data;
    public int Count;
}