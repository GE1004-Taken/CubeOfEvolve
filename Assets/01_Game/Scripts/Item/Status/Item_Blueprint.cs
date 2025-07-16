using App.GameSystem.Modules;
using UnityEngine;

public class Item_Blueprint : ItemBase
{
    public override void UseItem(PlayerCore playerCore)
    {
        RuntimeModuleManager.Instance.TriggerDropUI();

        // ガイド表示（設計図アイテムを初入手時）
        if (GuideManager.Instance.GuideEnabled.CurrentValue && !GuideManager.Instance.HasShown("Blueprint"))
        {
            GuideManager.Instance.TryShowGuide("Blueprint");
        }
    }
}