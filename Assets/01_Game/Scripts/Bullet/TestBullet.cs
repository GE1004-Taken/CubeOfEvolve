using R3;
using R3.Triggers;
using UnityEngine;

public class TestBullet : MonoBehaviour
{
    // ---------- Field
    private float _atk;
    private float _attackSpeed;
    private Vector3 _attackDir;
    private float _destroySecond = 20f;

    private void Start()
    {
        // 自然消滅時間
        Destroy(this, _destroySecond);

        // 移動
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                transform.Translate(_attackDir * _attackSpeed * Time.deltaTime);
            });

        // 衝突処理
        this.OnTriggerEnterAsObservable()
            .Subscribe(collider =>
            {
                if(collider.TryGetComponent<IDamageble>(out var damageble))
                {
                    Debug.Log("敵にダメージを与えた");
                    damageble.TakeDamage(_atk);
                    Destroy(gameObject);
                }
            })
            .AddTo(this);
    }

    // ---------- Method
    public void Initialize(
        float atk,
        float attackSpeed,
        Vector3 attackDir)
    {
        _atk = atk;
        _attackSpeed = attackSpeed;
        _attackDir = attackDir;
    }
}
