using System.Collections.Generic;
using UnityEngine;

namespace App.BaseSystem.DataStores.ScriptableObjects
{
    public class BaseDataBase<T> : ScriptableObject where T : BaseData
    {
        // ------------------------------ SerializeField
        [SerializeField] private List<T> _itemList = new List<T>();

        // ------------------------------ Property
        public List<T> ItemList => _itemList;
    }
}