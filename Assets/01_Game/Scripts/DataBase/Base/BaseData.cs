using UnityEngine;

namespace App.BaseSystem.DataStores.ScriptableObjects
{
    public class BaseData : ScriptableObject
    {
        // ------------------------------ SerializeField
        [SerializeField] private string _name;
        [SerializeField] private int _id;

        // ------------------------------ Property
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public int Id => _id;
    }
}