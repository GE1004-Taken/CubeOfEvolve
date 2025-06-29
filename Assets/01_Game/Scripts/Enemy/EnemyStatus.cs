using R3;
using UnityEngine;

public class EnemyStatus : MonoBehaviour, IDamageble
{
    // ---------------------------- SerializeField
    [Header("ステータス")]
    [SerializeField, Tooltip("移動速度")] private float _speed;
    [SerializeField, Tooltip("体力")] private float _maxHp;

    [Header("ドロップ")]
    [SerializeField, Tooltip("ドロップ")] private ItemDrop _itemDrop;

    [Header("エフェクト")]
    [SerializeField, Tooltip("死んだ時")] private GameObject _effect;

    // ---------------------------- Field
    public enum ActionPattern                              // 行動パターン
    {
        IDLE,           // 待機
        MOVE,           // 移動
        WAIT,           // 行動を一旦停止
        ATTACK,         // 停止して攻撃
        MOVEANDATTACK,  // 移動しながら攻撃
        AVOID,          // 回避
    }

    private ReactiveProperty<ActionPattern> _currentAct = new();    // 現在の行動

    private ReactiveProperty<float> _hp = new();            // 体力


    // ---------------------------- Property
    public ReadOnlyReactiveProperty<ActionPattern> CurrentAct => _currentAct;
    public float MaxHp => _maxHp;
    public ReadOnlyReactiveProperty<float> Hp => _hp;
    public float Speed => _speed;


    // ---------------------------- UnityMessage
    private void Awake()
    {
        _currentAct.Value = ActionPattern.WAIT;

        _hp.Value = _maxHp;

        // 死の判定
        _hp.Where(value => value <= 0)
            .Subscribe(value =>
            {
                _itemDrop.DropItemProcess();

                Instantiate(_effect, transform.position, Quaternion.identity);

                Destroy(gameObject);
            })
            .AddTo(this);
    }

    // ---------------------------- PublicMethod
    /// <summary>
    /// エネミーが生成される処理
    /// </summary>
    public void EnemySpawn()
    {
        _currentAct.Value = ActionPattern.MOVE;
    }


    // ---------------------------- Interface
    public void TakeDamage(float damage)
    {
        _hp.Value -= damage;
    }
}
