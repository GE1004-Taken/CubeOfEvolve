using Assets.IGC2025.Scripts.GameManagers;
using R3;
using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PlayerCore : MonoBehaviour, IDamageble
{
    // ---------- RP
    [SerializeField, Tooltip("HP")] private SerializableReactiveProperty<float> _hp;
    [SerializeField, Tooltip("最大HP")] private SerializableReactiveProperty<float> _maxHp;
    [SerializeField, Tooltip("無敵時間")] private float _invincibleTime = 1.0f;
    [SerializeField, Tooltip("移動速度")] private SerializableReactiveProperty<float> _moveSpeed;
    [SerializeField, Tooltip("回転速度")] private SerializableReactiveProperty<float> _rotateSpeed;
    [SerializeField, Tooltip("レベル")] private SerializableReactiveProperty<int> _level;
    [SerializeField, Tooltip("経験値")] private SerializableReactiveProperty<float> _exp;
    [SerializeField, Tooltip("必要経験値")] private SerializableReactiveProperty<float> _requireExp;
    [SerializeField, Tooltip("必要経験値の増加量(累乗)")] private float _AddRequireExpAmount;
    [SerializeField, Tooltip("キューブ数")] private SerializableReactiveProperty<int> _cubeCount;
    [SerializeField, Tooltip("最大キューブ数")] private SerializableReactiveProperty<int> _maxCubeCount;
    [SerializeField, Tooltip("お金")] private SerializableReactiveProperty<int> _money;
    [SerializeField, Tooltip("テストスキル")] private SerializableReactiveProperty<BaseSkill> _skill;

    public ReadOnlyReactiveProperty<float> Hp => _hp;
    public ReadOnlyReactiveProperty<float> MaxHp => _maxHp;
    public ReadOnlyReactiveProperty<float> MoveSpeed => _moveSpeed;
    public ReadOnlyReactiveProperty<float> RotateSpeed => _rotateSpeed;
    public ReadOnlyReactiveProperty<int> Level => _level;
    public ReadOnlyReactiveProperty<float> Exp => _exp;
    public ReadOnlyReactiveProperty<float> RequireExp => _requireExp;
    public ReadOnlyReactiveProperty<int> CubeCount => _cubeCount;
    public ReadOnlyReactiveProperty<int> MaxCubeCount => _maxCubeCount;
    public ReadOnlyReactiveProperty<int> Money => _money;
    public ReadOnlyReactiveProperty<BaseSkill> Skill => _skill;

    // ---------- Field
    private int _prevCubeCount;

    // 無敵フラグ
    private bool _isInvincibled;

    // ---------- UnityMessage
    private void Start()
    {
        // 経験値処理
        _exp
            .Where(x => x >= _requireExp.Value)
            .Subscribe(x =>
            {
                // リセットし、余剰経験値を加算
                _exp.Value = x - _requireExp.Value;

                // レベルを上げる
                _level.Value++;
            })
            .AddTo(this);

        // レベル処理(仮)
        _level
            .Skip(1)
            .Subscribe(x =>
            {
                _maxCubeCount.Value += 3;
                _requireExp.Value += _AddRequireExpAmount;
                _AddRequireExpAmount *= 2;
            })
            .AddTo(this);

        // 前回のキューブの値を初期化
        _prevCubeCount = _cubeCount.Value;

        // キューブ数の処理
        _cubeCount
            .Subscribe(x =>
            {
                // 増加処理
                if (x > _prevCubeCount)
                {
                    _maxHp.Value += 10;
                    _hp.Value += 10;
                }
                // 減少処理
                else if (x < _prevCubeCount)
                {
                    _maxHp.Value -= 10;
                    _hp.Value -= 10;
                }

                _prevCubeCount = x;
            })
            .AddTo(this);

        // HP関連処理
        _hp
            .Where(x => x <= 0)
            .Take(1)
            .Subscribe(x =>
            {
                GameManager.Instance.ChangeGameState(GameState.GAMEOVER);
            })
            .AddTo(this);
    }

    // ---------- Interface
    public void TakeDamage(float damage)
    {
        // 無敵時間中は処理しない
        if (_isInvincibled) return;

        // HPを減らす
        _hp.Value -= damage;

        // 無敵フラグオン
        _isInvincibled = true;

        // 被弾したら一定時間無敵
        StartCoroutine(StartInvincible());
    }

    // ---------- Method
    public void ReceiveMoney(int amount)
    {
        _money.Value += amount;
    }
    public void PayMoney(int amount)
    {
        _money.Value -= amount;
    }
    public void AddCube()
    {
        _cubeCount.Value++;
    }
    public void RemoveCube()
    {
        _cubeCount.Value--;
    }
    public void ReceiveExp(int amount)
    {
        _exp.Value += amount;
    }
    public void RecoveryHp(int amount)
    {
        // HPの最大値を超えないように加算
        _hp.Value = Mathf.Min(_hp.Value + amount, _maxHp.Value);
    }

    /// <summary>
    /// 一定時間後に無敵を解除する
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartInvincible()
    {
        yield return new WaitForSeconds(_invincibleTime);

        _isInvincibled = false;
    }
}
