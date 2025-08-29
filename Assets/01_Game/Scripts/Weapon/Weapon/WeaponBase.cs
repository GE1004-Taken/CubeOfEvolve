using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Game.Utils;
using ObservableCollections;
using R3;
using R3.Triggers;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour, IModuleID
{
    // ---------------------------- SerializeField
    [Header("データ")]
    [SerializeField, Tooltip("データ")] protected ModuleData _data;

    [Header("音")]
    [SerializeField, Tooltip("SE")] protected string _fireSEName;

    [Header("モデル")]
    [SerializeField, Tooltip("モデル")] private GameObject _model;

    [Header("索敵")]
    [SerializeField, Tooltip("対象検知用")] protected LayerSearch _layerSearch;
    [SerializeField, Tooltip("攻撃対象のレイヤー")] protected LayerMask _targetLayerMask;

    [Header("敵の場合")]
    [SerializeField, Tooltip("攻撃力倍率")] private float _enemyRate = 1;

    // ---------------------------- Field
    protected float _attackStatusEffects;
    protected float _currentAttack;
    protected float _currentInterval;

    // ---------------------------- Property
    /// <summary>
    /// ID
    /// </summary>
    public int Id => _data.Id;

    // ---------------------------- Unity Method
    private void Start()
    {
        Initialize();

        ObserveLevel();
        ObserveStatusEffects();

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                if (GameManager.Instance.CurrentGameState.CurrentValue != Assets.IGC2025.Scripts.GameManagers.GameState.BATTLE)
                {
                    return;
                }

                if (_currentInterval < _data.ModuleState.Interval)
                {
                    _currentInterval += Time.deltaTime;
                }
                else if (_layerSearch.NearestTargetObj != null)
                {
                    if (_model != null)
                    {
                        ProcessingFaceEnemyOrientation();
                    }
                    Attack();
                    _currentInterval = 0f;
                }
            })
            .AddTo(this);

        // 攻撃力更新
        UpdateAttackStatus();
    }

    // ---------------------------- Initialization
    /// <summary>
    /// 初期化
    /// </summary>
    protected virtual void Initialize()
    {
        _layerSearch.Initialize(_data.ModuleState.SearchRange, _targetLayerMask);
    }

    /// <summary>
    /// レベルを監視
    /// </summary>
    private void ObserveLevel()
    {
        if (transform.root.CompareTag("Cube"))
        {
            RuntimeModuleManager.Instance.GetRuntimeModuleData(_data.Id).Level
                .Subscribe(level =>
                {
                    UpdateAttackStatus();
                })
                .AddTo(this);
        }
        else if (transform.root.CompareTag("Enemy"))
        {
            _currentAttack = _data.ModuleState.Attack * _enemyRate;
        }
    }

    /// <summary>
    /// オプションを監視
    /// </summary>
    private void ObserveStatusEffects()
    {
        if (!transform.root.CompareTag("Cube")) return;

        var addStream = RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList
        .ObserveAdd(destroyCancellationToken)
        .Select(_ => Unit.Default);

        var removeStream = RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList
            .ObserveRemove(destroyCancellationToken)
            .Select(_ => Unit.Default);

        // どちらかのイベントが発生した時を監視
        addStream.Merge(removeStream)
            .Subscribe(_ =>
            {
                UpdateAttackStatus();
            })
            .AddTo(this);
    }

    /// <summary>
    /// 攻撃力更新
    /// </summary>
    private void UpdateAttackStatus()
    {
        if (!transform.root.CompareTag("Cube")) return;

        _attackStatusEffects = 0;

        foreach (var effect in RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList)
        {
            _attackStatusEffects += effect.Attack;
        }

        var level = RuntimeModuleManager.Instance.GetRuntimeModuleData(_data.Id).Level.CurrentValue;

        // レベル　攻撃力計算
        _currentAttack = StateValueCalculator.CalcStateValue(
                baseValue: _data.ModuleState.Attack,
                currentLevel: level,
                maxLevel: 5,
                maxRate: 0.5f // 最大+50%の成長
            );

        // オプション　攻撃力計算
        _currentAttack *= 1f + (_attackStatusEffects / 100);
    }

    /// <summary>
    /// 対象の方向を向く処理
    /// </summary>
    private void ProcessingFaceEnemyOrientation()
    {
        var target = _layerSearch.NearestTargetObj.transform;

        // 砲台の回転はY軸のみ（高さを無視して水平方向に向ける）
        Vector3 flatTargetPos = new Vector3(target.position.x, transform.position.y, target.position.z);
        Vector3 turretDir = (flatTargetPos - transform.position).normalized;

        if (turretDir != Vector3.zero)
        {
            _model.transform.rotation = Quaternion.LookRotation(turretDir);
        }
    }

    // ---------------------------- Abstract Method
    protected abstract void Attack();
}
