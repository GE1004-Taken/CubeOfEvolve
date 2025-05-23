using R3;
using R3.Triggers;
using UnityEngine;

public class Bullet_Missile : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Tooltip("攻撃対象のタグ")] private string _targetTag;

    // ---------------------------- Field
    private float _atk;
    private float _attackSpeed;
    private float _destroySecond = 10f;

    private Vector3 _velocity;  // 方向
    private Vector3 _position;
    private Transform _target;  // 目的地
    private float _period;      // 移動期間

    // ---------------------------- UnityMessage
    public string TargetTag => _targetTag;

    // ---------------------------- UnityMessage
    private void Start()
    {
        // 自然消滅時間
        Destroy(gameObject, _destroySecond);

        _position = transform.position;

        // 移動
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                // 距離
                var dis = _target.position - transform.position;

                var accelerator = Vector3.zero;
                accelerator += (dis - _velocity * _period) * 2f / (_period * _period);

                if (accelerator.magnitude > 50f)
                {
                    accelerator = accelerator.normalized * 50f;
                }

                _period -= Time.deltaTime;

                _velocity += accelerator * Time.deltaTime;
                _position += _velocity * Time.deltaTime;
                transform.position = _position;
            })
            .AddTo(this);

        // 衝突処理
        this.OnTriggerEnterAsObservable()
            .Subscribe(other =>
            {
                if (other.transform.root.TryGetComponent<IDamageble>(out var damageble)
                && other.CompareTag(_targetTag))
                {
                    damageble.TakeDamage(_atk);
                    Destroy(gameObject);
                }

                if (other.CompareTag("Ground"))
                {
                    Destroy(gameObject);
                }
            })
            .AddTo(this);
    }

    // ---------------------------- PublicMethod
    public void Initialize(
        float atk,
        float attackSpeed,
        Vector3 attackDir,
        Transform target,
        float period)
    {
        _atk = atk;
        _attackSpeed = attackSpeed;
        _velocity = attackDir;
        _target = target;
        _period = period;
    }
}
