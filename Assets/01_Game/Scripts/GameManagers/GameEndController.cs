using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using Assets.IGC2025.Scripts.GameManagers;
using Assets.IGC2025.Scripts.View;
using Assets.AT;

public class GameEndController : MonoBehaviour
{
    // ----- SerializedField
    [Header("演出ターゲット")]
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private ViewResultCanvas _viewResultCanvas;

    //[Header("Cinemachine (Unity6)")]
    //[SerializeField] private CinemachineCamera _cinemachineCamera;                  // シーンのメインCinemachineカメラ
    //[SerializeField] private Transform _cameraFollowDummy;                           // カメラが追従するダミー（空のGameObject推奨）

     [Header("ゲームセット演出パラメータ")]
    // [SerializeField] private Vector3 _offsetLocal = new Vector3(0f, 1f, -1f);       // 1) プレイヤー基準のオフセット
    // [SerializeField] private float _approachDuration = 0.6f;                         // カメラ初期移動の慣れ時間
    // [SerializeField] private float _orbitDuration = 3.0f;                            // 2) 右回りターンの最大継続時間
    // [SerializeField] private float _orbitAngularSpeedDegPerSec = 40f;                // 右回り角速度（度/秒）
    // [SerializeField] private bool  _endOnAnyKeyOrClick = true;                       // 入力でターン終了を許可
     [SerializeField] private float _postExplosionWait = 2.0f;                        // 3) 撃破演出後の待機

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

        // 3) 撃破演出（爆発など） → 数秒待機
        await PlayExplosionAsync(_playerTransform);

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
        if (_explosionVfxPrefab != null)
        {
            // プレイヤー基準の相対位置に爆発を出す（回転も考慮）
            Vector3 vfxPos = player.TransformPoint(_explosionOffsetLocal);
            var vfx = Instantiate(_explosionVfxPrefab, vfxPos, Quaternion.identity);
            vfx.Play();
        }

        // 被撃破モーション再生などがあればここで呼ぶ
        // e.g., playerAnimator.SetTrigger("Defeat");

        // 余韻待機（スケール非依存）
        if (_postExplosionWait > 0f)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(_postExplosionWait), ignoreTimeScale: true);
        }
    }
}
