using UnityEngine;
using Assets.AT;

public static class GameSoundManagerExtensions
{
    public static void PlaySE_ItemGet(this GameSoundManager manager)
    {
        string[] candidates = { "Sys_ItemGet_3", "Sys_ItemGet_4", "Sys_ItemGet_5", "Sys_ItemGet_6" };

        // ƒ‰ƒ“ƒ_ƒ€‘Io
        string selected = candidates[Random.Range(0, candidates.Length)];

        // Ä¶
        manager.PlaySE(selected, "Hit_Item");
    }
}
