using UnityEngine;

public class Item_RecoveryHp : ItemBase
{
    [SerializeField] private int _value;

    public override void UseItem(PlayerCore playerCore)
    {
        playerCore.RecoveryHp(_value);
    }
}