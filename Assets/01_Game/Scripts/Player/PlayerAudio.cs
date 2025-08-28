using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;
using Assets.AT;

public class PlayerAudio : BasePlayerComponent
{
    protected override void OnInitialize()
    {
        var builder = GetComponent<PlayerBuilder>();

        var soundManager = GameSoundManager.Instance;

        builder.OnCreate
            .Subscribe(_ =>
            {
                soundManager.PlaySE("Sys_Put", "SE");
            })
            .AddTo(this);

        builder.OnRemove
            .Subscribe(_ =>
            {
                soundManager.PlaySE("Sys_Remove", "SE");
            })
            .AddTo(this);
    }
}
