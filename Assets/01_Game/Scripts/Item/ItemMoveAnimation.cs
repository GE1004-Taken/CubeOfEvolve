using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using System;
using UnityEngine;

public class ItemMoveAnimation : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Tooltip("マテリアル")] private Renderer _material;
    [SerializeField, Tooltip("色")] private Color _color;

    [SerializeField, Tooltip("吸われるまでの待機時間")] private float _delaySecond;
    [SerializeField, Tooltip("移動速度")] private float _moveSpeed;
    [SerializeField, Tooltip("Collider")] private Collider _collider;
    [SerializeField, Tooltip("RigidBody")] private Rigidbody _rb;

    // ---------------------------- UnityMessage
    private void Awake()
    {
        // マテリアルを取得（注意：sharedMaterial は全体共有、material はインスタンス）
        Material mat = _material.material;

        // Emission有効化
        mat.EnableKeyword("_EMISSION");

        // Emissionの色を変更
        mat.SetColor("_EmissionColor", _color);
    }

    private async void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ground")) return;

        if (_rb != null)
        {
            _rb.useGravity = false;
            _rb.linearVelocity = Vector3.zero;
        }

        // キャンセル処理を書くところ要相談
        await UniTask.Delay(TimeSpan.FromSeconds(_delaySecond), cancellationToken: destroyCancellationToken, delayType: DelayType.DeltaTime)
         .SuppressCancellationThrow();


        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                if (GameManager.Instance.CurrentGameState.CurrentValue != Assets.IGC2025.Scripts.GameManagers.GameState.BATTLE)
                {
                    _rb.linearVelocity = Vector3.zero;
                    return;
                }

                SuctionProcess();
            })
            .AddTo(this);

        this.OnTriggerEnterAsObservable()
            .Subscribe(other =>
            {
                if (other.CompareTag("Player"))
                {
                    Destroy(gameObject);
                }
            })
            .AddTo(this);
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// プレイヤーに吸い込まれる処理
    /// </summary>
    private void SuctionProcess()
    {
        var targetPos = PlayerMonitoring.Instance.PlayerObj.transform.position;

        // ベクトルを取得
        Vector3 moveForward = targetPos - transform.position;

        // 移動方向にスピードを掛ける
        _rb.linearVelocity = _moveSpeed * Time.deltaTime * moveForward.normalized;
    }
}
