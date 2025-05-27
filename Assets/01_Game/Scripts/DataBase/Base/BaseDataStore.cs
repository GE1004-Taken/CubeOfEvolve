using System.Collections.Generic;
using UnityEngine;

namespace App.BaseSystem.DataStores.ScriptableObjects
{
    public abstract class BaseDataStore<T, U> : MonoBehaviour where T : BaseDataBase<U> where U : BaseData
    {
        // ------------------------------ SerializeField
        [SerializeField] protected T _dataBase;

        // ------------------------------ Property
        public T DaraBase => _dataBase;

        // ------------------------------ PublicMethod
        public U FindWithName(string name)
        {
            if (string.IsNullOrEmpty(name)) { return default; }

            return _dataBase.ItemList.Find(e => e.name == name);
        }

        public U FindWithId(int id)
        {
            return _dataBase.ItemList.Find(e => e.Id == id);
        }

        IReadOnlyList<BaseData> AllMasterDatas => _dataBase.ItemList;
    }
}

