using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Assets.IGC2025.Scripts.GameManagers;
using Assets.IGC2025.Scripts.View;
using Assets.AT;


public class GameEndController : MonoBehaviour
{
    // ----- SerializedField
    [Header("演出時間")]
    [SerializeField] private float _postExplosionWait = 2.0f;                        // 3) 撃破演出後の待機

    [Header("演出ターゲット")]
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private ViewResultCanvas _viewResultCanvas;

    //[Header("Cinemachine (Unity6)")]
    //[SerializeField] private CinemachineCamera _cinemachineCamera;                  // シーンのメインCinemachineカメラ
    //[SerializeField] private Transform _cameraFollowDummy;                           // カメラが追従するダミー（空のGameObject推奨）

    // [Header("ゲームセット演出パラメータ")]
    // [SerializeField] private Vector3 _offsetLocal = new Vector3(0f, 1f, -1f);       // 1) プレイヤー基準のオフセット
    // [SerializeField] private float _approachDuration = 0.6f;                         // カメラ初期移動の慣れ時間
    // [SerializeField] private float _orbitDuration = 3.0f;                            // 2) 右回りターンの最大継続時間
    // [SerializeField] private float _orbitAngularSpeedDegPerSec = 40f;                // 右回り角速度（度/秒）
    // [SerializeField] private bool  _endOnAnyKeyOrClick = true;                       // 入力でターン終了を許可

    [Header("ダメージエフェクト")]
    [SerializeField] private Volume _damageVolume;
    [SerializeField] private float _damageEffectSec = 0.1f;
    [SerializeField] private int _damageEffectCount = 3;

    [Header("背景暗転")]
    [SerializeField] private Graphic _backPanel;          // 例：Image, RawImage など
    [Min(0f)][SerializeField] private float _backFadeDuration = 0.35f;
    [Range(0f, 1f)][SerializeField] private float _backTargetAlpha = 1.0f; // どれくらい暗くするか(透過)
    [SerializeField] private bool _useUnscaledTime = true; // フェードや待機を非スケール時間で動かすか

    [Header("爆発パラメータ")]
    [Min(1)][SerializeField] private int _explosionCount = 5;            // 発生回数
    [SerializeField] private Vector2 _explosionRadiusRange = new(1f, 3f);     // 半径[min,max]
    [SerializeField] private float _explosionHeight = 0f;                   // プレイヤー基準の高さ差分
    [Min(0f)][SerializeField] private float _betweenExplosionsDelay = 0.05f; // 連続発生の間隔秒

    [Header("敗北演出（任意）")]
    [SerializeField] private ParticleSystem _explosionVfxPrefab;                     // 3) 爆発など
    [SerializeField] private Vector3 _explosionOffsetLocal = Vector3.zero;           // 爆発の相対位置

    // ----- Field
    private CinemachineCamera _camera;

    // ゲーム終了時に外部から呼ぶ入口（勝敗は state で受けるが演出は共通）
    public async UniTask PlayGameEndSequence(GameState state)
    {
        // --- 少しだけ同期（他の演出や物理停止待ち） ---
        await UniTask.Yield();

        // Cinemachineモジュールの取得（一度だけキャッシュ）
        //if (!EnsureCinemachineModules()) return;

        // 1) カメラ初期移動へ
        //await MoveCameraToPlayerOffsetAsync(_playerTransform, _offsetLocal, _approachDuration);

        // 2) 右回りターン（3秒 or 入力で終了）
        //await OrbitRightAsync(_playerTransform, _cameraFollowDummy,
        //                      _orbitAngularSpeedDegPerSec, _orbitDuration, _endOnAnyKeyOrClick);

        if (state == GameState.GAMEOVER)
        {
            // 3) 撃破演出（爆発など） → 数秒待機
            await PlayExplosionAsync(_playerTransform);
        }

        // 4) 完了 → リザルト表示
        _viewResultCanvas?.ShowCanvas(state);
    }

    // -----PraivateMethods

    /*
    /// <summary>
    /// Cinemachine の必須モジュールを取得できるか確認（Unity6のモジュール方式）
    /// </summary>
    private bool EnsureCinemachineModules()
    {
        var _cameraCtrlManager = CameraCtrlManager.Instance;
        _camera = _cameraCtrlManager.GetCamera(_cameraCtrlManager.GetCurrentActiveCameraKey());

        if (_camera == null || _cameraFollowDummy == null || _playerTransform == null)
        {
            Debug.LogWarning("[GameEnd] 参照が設定されていません（CinemachineCamera, FollowDummy, Player）");
            return false;
        }

        return true;
    }*/

    /*
    /// <summary>
    /// 1) プレイヤー基準の相対オフセット位置へ “カメラ用ダミー” を移動し、カメラがそこへ寄る。
    /// </summary>
    private async UniTask MoveCameraToPlayerOffsetAsync(Transform player, Vector3 offsetLocal, float duration)
    {
        // ダミーをプレイヤーの相対位置に配置（プレイヤーの回転も考慮）
        Vector3 worldTargetPos = player.TransformPoint(offsetLocal);
        _cameraFollowDummy.position = worldTargetPos;

        // Cinemachine のターゲット設定：位置はダミー、注視はプレイヤー
        _camera.Follow = _cameraFollowDummy;
        _camera.LookAt = _cameraFollowDummy;

        // “寄る感” を出したい場合は、いったん少し離れた所からスムーズ移動
        // 例：開始補助として現在位置→worldTargetPos へTween（TimeScale無視）
        if (duration > 0.01f)
        {
            // ダミーの現在位置から少しだけ内挿して “寄り感" を演出
            // すでに同位置ならTweenしない
            if (Vector3.Distance(_cameraFollowDummy.position, worldTargetPos) > 0.01f)
            {
                Tween t = _cameraFollowDummy.DOMove(worldTargetPos, duration)
                    .SetEase(Ease.OutCubic)
                    .SetUpdate(true); // Time.timeScale の影響を受けない
                await t.ToUniTask();
            }
        }
        // 注視はRotationComposer任せ（自動でplayerを見る）
    }*/

    /*
    /// <summary>
    /// 2) ターンテーブルのように “右回り” でプレイヤーを周回。最長duration秒、任意入力で中断可。
    /// </summary>
    private async UniTask OrbitRightAsync(Transform player, Transform followDummy,
                                          float angularSpeedDegPerSec, float duration, bool endOnInput)
    {
        float elapsed = 0f;
        // 常に TimeScale無視で進めたいので、unscaledDeltaTime を使う
        while (elapsed < duration)
        {
            // 入力で終了（クリック/何かキー）
            if (endOnInput && (Input.GetMouseButtonDown(0) || Input.anyKeyDown))
                break;

            // プレイヤー中心に右回り（Y+ 方向へ回転）
            // “右回り” = 時計回り想定：ワールドY軸でプレイヤー位置を中心に周回
            float angleThisFrame = angularSpeedDegPerSec * Time.unscaledDeltaTime;
            followDummy.RotateAround(player.position, Vector3.up, angleThisFrame);

            // ダミーの高さはプレイヤー基準に保ちたいなら、必要に応じて固定
            followDummy.position = new Vector3(followDummy.position.x, player.position.y + _offsetLocal.y, followDummy.position.z);

            elapsed += Time.unscaledDeltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update); // 1フレーム待ち（スケール非依存）
        }
    }*/

    /// <summary>
    /// 3) 爆発などの被撃破演出 → 数秒待機
    /// </summary>


    private async UniTask PlayExplosionAsync(Transform player)
    {
        if (player == null || _explosionVfxPrefab == null) return;

        // オブジェクト破棄時に全体を止めるためのトークン
        var token = this.GetCancellationTokenOnDestroy();

        // (1)(2)(3) を同時に走らせる
        //CameraCtrlManager.Instance.ChangeCamera("Player Camera Death");
        var bgTask = FadeBackPanelToBlackAsync(token);          // (1) 背景暗転
        var dmgTask = PlayDamagePostEffectAsync(token);          // (2) ダメージエフェクト
        var explTask = RunExplosionsAsync(player, token);         // (3) ランダム多発爆発

        await UniTask.WhenAll(bgTask, dmgTask, explTask);
    }

    private async UniTask FadeBackPanelToBlackAsync(CancellationToken token)
    {
        if (_backPanel == null) return;

        _backPanel.DOKill(); // 競合Tween除去

        // 目標色：黒 + 指定のAlpha（RGBは0,0,0）
        var target = new Color(0f, 0f, 0f, _backTargetAlpha);

        var tween = _backPanel.DOColor(target, Mathf.Max(_backFadeDuration, 0.0001f))
                            .SetEase(Ease.InOutSine)
                            .SetUpdate(_useUnscaledTime)
                            .SetLink(gameObject); // 破棄でKill

        // killOnCancel が無い環境向け：キャンセル時は明示Kill
        using (token.Register(() => { if (tween.IsActive()) tween.Kill(); }))
        {
            await tween.ToUniTask(cancellationToken: token);
        }
    }

    private async UniTask PlayDamagePostEffectAsync(CancellationToken token)
    {
        // ここで画面の被ダメ演出などを起動
        // ダメージエフェクト用のVignetteを取得
        _damageVolume.profile.TryGet(out Vignette damageVignette);

        // nullチェック
        if (damageVignette == null)
        {
            Debug.LogError("Vignetteが無いよ");
        }

        // 画面端がダメージエフェクト表示時間分赤くなる
        DOVirtual.Float(0.0f,
            1.0f,
            _damageEffectSec,
            value => damageVignette.smoothness.value = value
            )
        .SetLoops(_damageEffectCount, LoopType.Yoyo)
        .SetLink(this.gameObject);

        // 継続時間を待ちたいなら:
        // await UniTask.WaitForSeconds(_damageEffectDuration, ignoreTimeScale: _useUnscaledTime, cancellationToken: token);

        await UniTask.CompletedTask;
    }

    private async UniTask RunExplosionsAsync(Transform player, CancellationToken token)
    {
        // パラメータ正規化（あなたの元コードを流用）
        int count = Mathf.Max(1, _explosionCount);
        float rMin = Mathf.Max(0f, Mathf.Min(_explosionRadiusRange.x, _explosionRadiusRange.y));
        float rMax = Mathf.Max(_explosionRadiusRange.x, _explosionRadiusRange.y);
        if (Mathf.Approximately(rMax, 0f)) rMax = 0.1f;

        for (int i = 0; i < count; i++)
        {
            // 面積一様な半径サンプル
            float u = UnityEngine.Random.value;
            float r = Mathf.Sqrt(Mathf.Lerp(rMin * rMin, rMax * rMax, u));
            float ang = UnityEngine.Random.value * Mathf.PI * 2f;
            float x = Mathf.Cos(ang) * r;
            float z = Mathf.Sin(ang) * r;

            Vector3 local = _explosionOffsetLocal + new Vector3(x, _explosionHeight, z);
            Vector3 worldPos = player.TransformPoint(local);

            // エフェクト再生
            var vfx = Instantiate(_explosionVfxPrefab, worldPos, Quaternion.identity);
            vfx.Play();
            // サウンド再生
            GameSoundManager.Instance.PlaySE("Ene_Death_1");

            if (_betweenExplosionsDelay > 0f && i < count - 1)
            {
                await UniTask.WaitForSeconds(_betweenExplosionsDelay, ignoreTimeScale: _useUnscaledTime, cancellationToken: token);
            }
        }

        if (_postExplosionWait > 0f)
        {
            await UniTask.WaitForSeconds(_postExplosionWait, ignoreTimeScale: _useUnscaledTime, cancellationToken: token);
        }
    }

}
