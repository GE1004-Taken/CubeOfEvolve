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
    [SerializeField, Tooltip("武器の基本データ")]
    protected ModuleData _data;

    [Header("音")]
    [SerializeField, Tooltip("発射時のSE名")]
    protected string _fireSEName;

    [Header("追尾")]
    [SerializeField, Tooltip("追尾速度")]
    private float _rotationSpeed = 90f;

    [SerializeField, Tooltip("横軸を追尾するか")]
    protected bool _isHorizontalAxisTracking = true;
    [SerializeField, Tooltip("武器モデル")]
    protected GameObject _horizontalModel;

    [SerializeField, Tooltip("縦軸を追尾するか")]
    protected bool _isVerticalAxisTracking = true;
    [SerializeField, Tooltip("武器モデル")]
    protected GameObject _verticalModel;

    [Header("索敵")]
    [SerializeField, Tooltip("索敵用コンポーネント")]
    protected LayerSearch _layerSearch;
    [SerializeField, Tooltip("攻撃対象レイヤーマスク")]
    protected LayerMask _targetLayerMask;

    [Header("敵の場合")]
    [SerializeField, Tooltip("敵が装備した場合の攻撃力倍率")]
    private float _enemyRate = 1;

    // ---------------------------- Field
    protected float _attackStatusEffects;   // ステータス効果による追加攻撃力
    protected float _currentAttack;         // 実際の攻撃力
    protected float _currentInterval;       // 攻撃間隔の経過時間

    // ---------------------------- Property
    /// <summary>
    /// 武器のモジュールID
    /// </summary>
    public int Id => _data.Id;

    // ---------------------------- UnityMessage
    private void Start()
    {
        // 初期化
        Initialize();

        // レベルアップやステータス効果を監視
        ObserveLevel();
        ObserveStatusEffects();

        // 毎フレーム監視
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                // ゲーム状態がバトル中でなければ処理しない
                if (GameManager.Instance.CurrentGameState.CurrentValue
                    != Assets.IGC2025.Scripts.GameManagers.GameState.BATTLE)
                {
                    return;
                }

                ProcessingFaceEnemyOrientation();

                // 攻撃間隔の経過時間を加算
                if (_currentInterval < _data.ModuleState.Interval)
                {
                    _currentInterval += Time.deltaTime;
                }
                else if (!_isHorizontalAxisTracking && !_isVerticalAxisTracking)
                {
                    // 攻撃処理（派生クラスで実装）
                    Attack();

                    _currentInterval = 0f;
                }
                // 攻撃可能状態になり、ターゲットが存在する場合
                else if (_layerSearch.NearestTargetObj != null)
                {
                    // 攻撃処理（派生クラスで実装）
                    Attack();

                    // インターバルをリセット
                    _currentInterval = 0f;
                }
            })
            .AddTo(this);

        // 初期攻撃力を計算
        UpdateAttackStatus();
    }

    // ---------------------------- Initialization
    /// <summary>
    /// 初期化処理
    /// </summary>
    protected virtual void Initialize()
    {
        _layerSearch.Initialize(_data.ModuleState.SearchRange, _targetLayerMask);
    }

    /// <summary>
    /// レベルの監視処理
    /// </summary>
    private void ObserveLevel()
    {
        if (transform.root.CompareTag("Cube"))
        {
            // Cubeの場合はランタイムのモジュールデータを監視
            RuntimeModuleManager.Instance.GetRuntimeModuleData(_data.Id).Level
                .Subscribe(level =>
                {
                    UpdateAttackStatus();
                })
                .AddTo(this);
        }
        else if (transform.root.CompareTag("Enemy"))
        {
            // 敵の場合は倍率をかけた固定値を使用
            _currentAttack = _data.ModuleState.Attack * _enemyRate;
        }
    }

    /// <summary>
    /// ステータス効果の監視
    /// </summary>
    private void ObserveStatusEffects()
    {
        if (!transform.root.CompareTag("Cube")) return;

        // 効果追加・削除の監視ストリーム
        var addStream = RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList
            .ObserveAdd(destroyCancellationToken)
            .Select(_ => Unit.Default);

        var removeStream = RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList
            .ObserveRemove(destroyCancellationToken)
            .Select(_ => Unit.Default);

        // どちらかが発生したら攻撃力を更新
        addStream.Merge(removeStream)
            .Subscribe(_ =>
            {
                UpdateAttackStatus();
            })
            .AddTo(this);
    }

    /// <summary>
    /// 攻撃力を計算・更新  
    /// レベル成長とステータス効果を考慮して計算する
    /// </summary>
    private void UpdateAttackStatus()
    {
        if (!transform.root.CompareTag("Cube")) return;

        // ステータス効果による追加攻撃力をリセット
        _attackStatusEffects = 0;

        // 現在の全ステータス効果を加算
        foreach (var effect in RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList)
        {
            _attackStatusEffects += effect.Attack;
        }

        // レベルを取得
        var level = RuntimeModuleManager.Instance.GetRuntimeModuleData(_data.Id).Level.CurrentValue;

        // 基本攻撃力をレベルに応じて計算（最大50%の成長）
        _currentAttack = StateValueCalculator.CalcStateValue(
            baseValue: _data.ModuleState.Attack,
            currentLevel: level,
            maxLevel: 5,
            maxRate: 0.5f
        );

        // ステータス効果を反映（%換算）
        _currentAttack *= 1f + (_attackStatusEffects / 100);
    }

    /// <summary>
    /// ターゲットの方向にモデルを回転させる（水平・垂直別制御）
    /// </summary>
    private void ProcessingFaceEnemyOrientation()
    {
        if (!_isHorizontalAxisTracking && !_isVerticalAxisTracking) return;
        if (_layerSearch.NearestTargetObj == null) return;

        var target = _layerSearch.NearestTargetObj.transform;

        // 水平方向
        if (_isHorizontalAxisTracking && _horizontalModel != null)
        {
            Vector3 lookDir = target.position - _horizontalModel.transform.position;
            lookDir.y = 0f; // 高さは無視して水平ベクトルだけ

            if (lookDir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
                _horizontalModel.transform.rotation = Quaternion.RotateTowards(
                    _horizontalModel.transform.rotation,
                    targetRot,
                    _rotationSpeed * Time.deltaTime
                );
            }
        }

        // 垂直方向
        if (_isVerticalAxisTracking && _verticalModel != null)
        {
            Vector3 lookDir = target.position - _verticalModel.transform.position;

            if (lookDir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);

                // 現在の回転
                Quaternion current = _verticalModel.transform.rotation;

                // Pitchだけ目標値に更新し、YawとRollは維持
                Quaternion newRot = Quaternion.Euler(
                    targetRot.eulerAngles.x, // Pitch
                    current.eulerAngles.y,   // Yaw維持
                    current.eulerAngles.z    // Roll維持
                );

                _verticalModel.transform.rotation = Quaternion.RotateTowards(
                    current,
                    newRot,
                    _rotationSpeed * Time.deltaTime
                );
            }
        }
    }


    // ---------------------------- Abstract Method
    /// <summary>
    /// 攻撃処理
    /// </summary>
    protected abstract void Attack();
}
