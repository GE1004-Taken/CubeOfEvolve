using R3;
using R3.Triggers;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Tooltip("データ")] protected WeaponDataBase _data;

    [SerializeField, Tooltip("ID")] private int _id;

    [SerializeField, Tooltip("弾速")] protected float _bulletSpeed;
    [SerializeField, Tooltip("攻撃間隔")] protected float _interval;

    [Header("索敵")]
    [SerializeField, Tooltip("索敵範囲")] protected float _scoutingRange;
    [SerializeField, Tooltip("対象検知用")] protected LayerSearch _layerSearch;

    [SerializeField, Tooltip("攻撃対象のタグ")] protected string _targetTag;

    // ---------------------------- Field
    protected float _attack;
    protected float _currentInterval;
    protected Transform _nearestEnemyTransform;

    // ---------------------------- UnityMethod
    private void Start()
    {
        _data.weaponDataList[_id].Level.
            Subscribe(value =>
            {
                _attack = _data.weaponDataList[_id].Attack * _data.weaponDataList[_id].Level.CurrentValue;
            }).
            AddTo(this);

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                // インターバル中なら
                if (_currentInterval < _interval)
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
