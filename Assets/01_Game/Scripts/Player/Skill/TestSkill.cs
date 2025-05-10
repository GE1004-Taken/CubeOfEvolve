using R3;
using UnityEngine;

public class TestSkill : BaseSkill
{
    public override void ActiveSkill()
    {
        if (RemainingCoolTime.CurrentValue > 0) return;

        Debug.Log("ƒXƒLƒ‹”­“®");

        base.CooldownSkill();
    }
}
