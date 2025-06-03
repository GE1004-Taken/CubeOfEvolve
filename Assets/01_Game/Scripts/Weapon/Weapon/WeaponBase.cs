using R3;
using R3.Triggers;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Tooltip("データ")] protected WeaponData _data;

    [Header("索敵")]
    [SerializeField, Tooltip("索敵範囲")] protected float _scoutingRange;
    [SerializeField, Tooltip("対象検知用")] protected LayerSearch _layerSearch;

    [SerializeField, Tooltip("攻撃対象のタグ")] protected string _targetTag;

    // ---------------------------- Field
    protected float _attack;
    protected float _currentInterval;

    // ---------------------------- UnityMethod
    private void Start()
    {
        if (transform.root.CompareTag("Player"))
        {
            foreach (var i in WeaponLevelManager.Instance.PlayerWeaponLevels)
            {
                i.Subscribe(value =>
                {
                    _attack = _data.Attack * value;
                })
                    .AddTo(this);
            }
        }
        if (transform.root.CompareTag("Enemy"))
        {
            foreach (var level in WeaponLevelManager.Instance.EnemyWeaponLevels)
            {
                level.Subscribe(value =>
                {
                    _attack = _data.Attack * value;
                })
                    .AddTo(this);
            }
        }

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                // インターバル中なら
                if (_currentInterval < _data.Interval)
                {
                    _currentInterval += Time.deltaTime;
                }
                // インターバル終了かつ敵がいたら
                else if (_layerSearch.NearestEnemyObj != null)
                {
                    Attack();
                    _currentInterval = 0f;
                }
            })
            .AddTo(this);

        Initialize();
    }

    // ---------------------------- AbstractMethod
    protected virtual void Initialize()
    {

    }

    // ---------------------------- AbstractMethod
    protected abstract void Attack();
}
