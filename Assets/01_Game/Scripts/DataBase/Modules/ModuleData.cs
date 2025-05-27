// 作成日：   250522
// 更新日：   250522
// 作成者： 安中 健人

using System.Collections.Generic;
using UnityEngine;

namespace App.BaseSystem.DataStores.ScriptableObjects.Modules
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Data/Module")]
    public class ModuleData : BaseData
    {
        // -----
        // -----Enum

        public enum MODULE_TYPE
        {
            [InspectorName("なし")]
            None = 0,
            [InspectorName("武器")]
            Weapons,
            [InspectorName("オプション")]
            Options,
        }
        private static readonly Dictionary<MODULE_TYPE, string> _moduleTypeMapping = new Dictionary<MODULE_TYPE, string>()
        {
            {MODULE_TYPE.None, "なし"},
            {MODULE_TYPE.Weapons, "ウェポン"},
            {MODULE_TYPE.Options, "オプション"},
        };

        // -----SerializeField

        // 不動
        [SerializeField] private MODULE_TYPE _moduleType;
        [SerializeField] private string _viewName = "表示名";
        [SerializeField] private string _description = "説明文";
        [SerializeField] private int _basePrice = 100;
        //[SerializeField] private BaseWeapon _weaponData; <- 武器等のデータ。攻撃力など変数と、ダメージ処理の記述が入ってる。

        // 可変
        [SerializeField] private int _level = 0;
        [SerializeField] private int _quantity = 0;

        

        // -----Property
        public static IReadOnlyDictionary<MODULE_TYPE,string> ModuleTypeMapping => _moduleTypeMapping;

        public MODULE_TYPE ModuleType => _moduleType;
        public string ViewName => _viewName;
        public string Description => _description;
        public int BasePrice => _basePrice;

        public int Level => _level;
        public int Quantity => _quantity;
        //public BaseWeapon WeaponData => _weaponData;
    }
}
