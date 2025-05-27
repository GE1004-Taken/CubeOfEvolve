// App.GameSystem.Modules/RuntimeModuleData.cs
using R3; // R3を使用
using System; // Serializableを使用する場合
using UnityEngine; // ScriptableObjectなど、Unityの型を使用する場合
using App.BaseSystem.DataStores.ScriptableObjects.Modules; // ModuleDataを参照するため

namespace App.GameSystem.Modules
{
    [Serializable] // Unity Inspectorで表示したい場合など
    public class RuntimeModuleData
    {
        public int Id { get; private set; }

        // 現在のレベルをReactivePropertyで公開
        [SerializeField]
        private ReactiveProperty<int> _currentLevel;
        public ReadOnlyReactiveProperty<int> Level => _currentLevel;
        public int CurrentLevelValue => _currentLevel.Value; // 直接値を取得するためのプロパティ

        // 現在の数量をReactivePropertyで公開
        [SerializeField]
        private ReactiveProperty<int> _quantity;
        public ReadOnlyReactiveProperty<int> Quantity => _quantity;
        public int CurrentQuantityValue => _quantity.Value; // 直接値を取得するためのプロパティ

        // コンストラクタ (MasterDataから初期化)
        public RuntimeModuleData(ModuleData masterData)
        {
            Id = masterData.Id;
            _currentLevel = new ReactiveProperty<int>(0); // 初期レベルは0
            _quantity = new ReactiveProperty<int>(0); // 初期数量は0
        }

        // レベルを更新する内部メソッド
        public void SetLevel(int newLevel)
        {
            if (newLevel < 0) newLevel = 0;
            _currentLevel.Value = newLevel;
        }

        // 数量を更新する内部メソッド
        public void SetQuantity(int newQuantity)
        {
            if (newQuantity < 0) newQuantity = 0;
            _quantity.Value = newQuantity;
        }

        // Convenience methods for changing level/quantity (通常はRuntimeModuleManager経由で呼ばれる)
        public void LevelUp() => SetLevel(_currentLevel.Value + 1);
        public void ChangeQuantity(int amount) => SetQuantity(_quantity.Value + amount);
    }
}