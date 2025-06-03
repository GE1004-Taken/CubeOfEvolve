using UnityEngine;

public abstract class ItemBase : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField] private ItemData _itemData;

    // ---------------------------- Property
    public ItemData Data => _itemData;

    // ---------------------------- UnityMassage
    private void Start()
    {
        Initialize();
    }

    // ---------------------------- virtual
    /// <summary>
    /// 初期化
    /// </summary>
    public virtual void Initialize()
    {
    }

    /// <summary>
    /// アイテムを使う処理
    /// </summary>
    public abstract void UseItem(PlayerCore playerCore);
}
