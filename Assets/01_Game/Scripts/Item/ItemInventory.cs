using UnityEngine;

public class ItemInventory : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField] private ItemDataBase _itemDataBase;

    // ---------------------------- SerializeField
    private PlayerCore _playerCore;

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
        itemData.Item.UseItem(_playerCore);
    }
}
