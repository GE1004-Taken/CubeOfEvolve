using R3;
using UnityEngine;

[RequireComponent(typeof(PlayerCore))]
public class PlayerSkill : BasePlayerComponent
{
    protected override void OnInitialize()
    {
        Input.Skill
            .Skip(1)
            .Subscribe(_ =>
            {
                Core.Skill.CurrentValue.ActiveSkill();
            });
    }
}
