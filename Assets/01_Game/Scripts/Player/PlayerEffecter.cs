using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using R3;
using DG.Tweening;
using System.Linq;
using Assets.IGC2025.Scripts.GameManagers;

public class PlayerEffecter : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField, Tooltip("ダメージエフェクト用のボリューム")]
    private Volume _damageVolume;
    [SerializeField, Tooltip("ダメージエフェクト表示時間")]
    private float _damageEffectSec;

    // ---------- Method
    protected override void OnInitialize()
    {
        // ダメージエフェクト用のVignetteを取得
        _damageVolume.profile.TryGet(out Vignette damageVignette);

        // nullチェック
        if (damageVignette != null)
        {
            Debug.LogError("Vignetteが無いよ");
        }

        // 被ダメージエフェクト処理
        Core.Hp
            .Chunk(2,1)
            .Where(x => x.Last() < x.First())
            .Where(_ =>
            GameManager.
            Instance.
            CurrentGameState.
            CurrentValue == GameState.BATTLE)
            .Subscribe(_ =>
            {
                // 画面端がダメージエフェクト表示時間分赤くなる
                DOVirtual.Float(
                    0.0f,
                    1.0f,
                    _damageEffectSec,
                    value => damageVignette.smoothness.value = value
                    )
                .SetLoops(2, LoopType.Yoyo)
                .SetLink(this.gameObject);
            })
            .AddTo(this);
    }
}
