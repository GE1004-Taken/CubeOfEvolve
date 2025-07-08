using App.GameSystem.Modules;
using UnityEngine;

public class Item_Blueprint : ItemBase
{
    public override void UseItem(PlayerCore playerCore)
    {
        RuntimeModuleManager.Instance.TriggerDropUI();
    }
}