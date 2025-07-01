using UnityEngine;

public class Item_RecoveryHp : ItemBase
{
    [SerializeField] private int _value;

    public override void UseItem(PlayerCore playerCore)
    {
        Debug.Log("‰ñ•œIII");
        playerCore.RecoveryHp(_value);
    }
}