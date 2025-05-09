using R3;
using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
    // ---------------------------- SerializeField
    [Header("ステータス")]
    [SerializeField, Tooltip("体力")] private float _maxHp;
    [SerializeField, Tooltip("移動速度")] private float _speed;


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

    private ReactiveProperty<ActionPattern> _currentAct;    // 現在の行動

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
    }

    private void OnTriggerEnter(Collider other)
    {
        // ダメージ処理
        if (other.CompareTag("PlayerAttack"))
        {
            _hp.Value--;
        }
    }


    // ---------------------------- PublicMethod
    /// <summary>
    /// エネミーのスタート処理
    /// </summary>
    public void EnemyStart()
    {
        _currentAct.Value = ActionPattern.MOVE;
    }
}
