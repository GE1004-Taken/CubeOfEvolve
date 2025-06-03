using App.BaseSystem.DataStores.ScriptableObjects;
using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObjects/Data/ItemData")]
public class ItemData : BaseData
{
    // ----- Enum
    /// <summary>
    /// モジュールの種類を定義する列挙型です。
    /// </summary>
    public enum ItemType
    {
        [InspectorName("なし")]
        None = 0,
        [InspectorName("使うもの")]
        Use,
        [InspectorName("ステータス")]
        Status,
    }
    public ItemBase Item;
    public ItemType Type;
}
