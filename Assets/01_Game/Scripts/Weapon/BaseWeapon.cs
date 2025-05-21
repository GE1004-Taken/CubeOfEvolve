using R3;
using R3.Triggers;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public abstract class BaseWeapon : MonoBehaviour
{
    // ---------- SerializeField
    [SerializeField, Tooltip("攻撃力")] protected float _atk;
    [SerializeField, Tooltip("攻撃速度")] protected float _attackSpeed;
    [SerializeField, Tooltip("攻撃範囲")] protected float _range;
    [SerializeField, Tooltip("攻撃間隔")] protected float _interval;
    [SerializeField, Tooltip("対象検知用")] protected SphereCollider _sphereCollider;

    [SerializeField, Tooltip("攻撃対象のタグ")] private string _targetTag;

    // ---------- Field
    protected float _currentInterval;
    protected List<Transform> _inRangeEnemies = new();
    protected Transform _nearestEnemyTransform;

    // ---------- UnityMethod
    private void Start()
    {
        _sphereCollider.radius = _range;

        this.OnTriggerEnterAsObservable()
            .Where(x => x.CompareTag(_targetTag))
            .Subscribe(x =>
            {
                _inRangeEnemies.Add(x.transform);
            })
            .AddTo(this);

        this.OnTriggerExitAsObservable()
            .Where(x => x.CompareTag(_targetTag))
            .Subscribe(x =>
            {
                _inRangeEnemies[_inRangeEnemies.IndexOf(x.transform)] = null;
            })
            .AddTo(this);


        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                var nearestEnemyDist = 0f;

                // 一番近い敵を取得
                foreach (var enemyTransform in _inRangeEnemies)
                {
                    if (enemyTransform == null) continue;

                    var dist = Vector3.Distance(
                        transform.position,
                        enemyTransform.position);

                    if (nearestEnemyDist == 0f || dist < nearestEnemyDist)
                    {
                        nearestEnemyDist = dist;
                        _nearestEnemyTransform = enemyTransform;
                    }
                }

                // 範囲外(null)になった要素を消す
                if (_inRangeEnemies.Count > 0)
                {
                    _inRangeEnemies.RemoveAll(x => x == null);
                }

                // インターバル中なら
                if (_currentInterval < _interval)
                {
                    _currentInterval += Time.deltaTime;
                }
                // インターバル終了かつ敵がいたら
                else
                {
                    if (_inRangeEnemies.Count <= 0) return;

                    Attack();
                    _currentInterval = 0f;
                }
            })
            .AddTo(this);
    }

    // ---------- AbstractMethod
    protected abstract void Attack();
}
