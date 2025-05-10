using DG.Tweening;
using R3;
using System.IO;
using UnityEngine;

public abstract class BaseSkill : MonoBehaviour
{
    // ---------- RP
    [SerializeField] protected SerializableReactiveProperty<float> _coolTime;
    public ReadOnlyReactiveProperty<float> CoolTime => _coolTime;

    protected ReactiveProperty<float> _remainingCooiTime = new ReactiveProperty<float>();
    public ReadOnlyReactiveProperty<float> RemainingCoolTime => _remainingCooiTime;

    // ---------- Event
    public abstract void ActiveSkill();

    // ---------- PrivateMethod
    protected void CooldownSkill()
    {
        DOVirtual.Float(
            _coolTime.Value,
            0f,
            _coolTime.Value,
            value => _remainingCooiTime.Value = value)
            .SetLink(gameObject);
    }
}
