using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using R3;
using R3.Triggers;
using DG.Tweening;
using System.Linq;

public class PlayerEffecter : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField] private Volume _damageVolume;
    [SerializeField] private float _damageEffectSec;

    // ---------- Field
    private Vignette _vignette;

    // ---------- Method
    protected override void OnInitialize()
    {
        // Vignetteを取得
        _damageVolume.profile.TryGet(out _vignette);

        // nullチェック
        if (_vignette != null)
        {
            Debug.LogError("Vignetteが無いよ");
        }

        // 被ダメージエフェクト処理
        Core.Hp
            .Chunk(2,1)
            .Where(x => x.Last() < x.First())
            .Subscribe(_ =>
            {
                DOVirtual.Float(
                    0.0f,
                    1.0f,
                    _damageEffectSec,
                    value => _vignette.smoothness.value = value
                    )
                .SetLoops(2, LoopType.Yoyo)
                .SetLink(this.gameObject);
            })
            .AddTo(this);
    }
}
