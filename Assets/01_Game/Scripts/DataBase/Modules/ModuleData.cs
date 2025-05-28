using System.Collections.Generic;
using UnityEngine;

namespace App.BaseSystem.DataStores.ScriptableObjects.Modules
{
    /// <summary>
    /// モジュールデータを定義するScriptableObjectクラス。
    /// 各モジュールの種類、表示名、説明、基本価格、レベル、数量などを保持します。
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObjects/Data/Module")]
    public class ModuleData : BaseData
    {
        // ----- Enum
        /// <summary>
        /// モジュールの種類を定義する列挙型です。
        /// </summary>
        public enum MODULE_TYPE
        {
            [InspectorName("なし")]
            None = 0, // モジュールの種類が設定されていない状態を示します。
            [InspectorName("武器")]
            Weapons,  // 武器タイプのモジュールを示します。
            [InspectorName("オプション")]
            Options,  // オプションタイプのモジュールを示します。
        }

        /// <summary>
        /// MODULE_TYPE列挙型とそれに対応する日本語文字列のマッピング。
        /// </summary>
        private static readonly Dictionary<MODULE_TYPE, string> _moduleTypeMapping = new Dictionary<MODULE_TYPE, string>()
        {
            {MODULE_TYPE.None, "なし"},     // モジュールタイプ「なし」の表示名
            {MODULE_TYPE.Weapons, "ウェポン"}, // モジュールタイプ「武器」の表示名
            {MODULE_TYPE.Options, "オプション"}, // モジュールタイプ「オプション」の表示名
        };

        // ----- SerializeField
        // 不動
        [SerializeField] private MODULE_TYPE _moduleType; // モジュールの種類を設定します。
        [SerializeField] private string _viewName = "表示名"; // モジュールの表示名を設定します。
        [SerializeField] private string _description = "説明文"; // モジュールの詳細な説明文を設定します。
        [SerializeField] private int _basePrice = 100; // モジュールの基本価格を設定します。
        //[SerializeField] private BaseWeapon _weaponData; // 武器等のデータ。攻撃力など変数と、ダメージ処理の記述が入ってる。

        // 可変
        [SerializeField] private int _level = 0; // モジュールの現在のレベルを設定します。
        [SerializeField] private int _quantity = 0; // モジュールの現在の数量を設定します。

        // ----- Property
        /// <summary>
        /// モジュールタイプとその表示名の読み取り専用辞書を取得します。
        /// </summary>
        public static IReadOnlyDictionary<MODULE_TYPE, string> ModuleTypeMapping => _moduleTypeMapping;

        /// <summary>
        /// このモジュールの種類を取得します。
        /// </summary>
        public MODULE_TYPE ModuleType => _moduleType;
        /// <summary>
        /// このモジュールの表示名を取得します。
        /// </summary>
        public string ViewName => _viewName;
        /// <summary>
        /// このモジュールの説明文を取得します。
        /// </summary>
        public string Description => _description;
        /// <summary>
        /// このモジュールの基本価格を取得します。
        /// </summary>
        public int BasePrice => _basePrice;

        /// <summary>
        /// このモジュールの現在のレベルを取得します。
        /// </summary>
        public int Level => _level;
        /// <summary>
        /// このモジュールの現在の数量を取得します。
        /// </summary>
        public int Quantity => _quantity;
        //public BaseWeapon WeaponData => _weaponData; // 武器データに関するプロパティ。コメントアウトされています。
    }
}