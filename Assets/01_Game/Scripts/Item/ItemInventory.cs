using UnityEngine;

public class ItemInventory : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField] private ItemDataBase _itemDataBase;

    // ---------------------------- UnityMassage
    private void OnTriggerEnter(Collider other)
    {
        if ("Item" == LayerMask.LayerToName(other.gameObject.layer)
                && other.TryGetComponent<ItemBase>(out var status))
        {
            GetItem(status.Data);

            Destroy(other.gameObject);
        }
    }

    // ---------------------------- PublicMethod
    public void GetItem(ItemData itemData)
    {
        switch (itemData.Type)
        {
            // 入手してストックしておけるもの
            case ItemData.ItemType.Use:
                _itemDataBase.FindItemByName(itemData).Count++;
                break;

            // 入手してすぐ効力を発揮するモノ
            case ItemData.ItemType.Status:
                itemData.Item.UseItem();
                break;

            default:
                break;
        }
    }
}
