using App.GameSystem.Modules;
using UnityEngine;
using UnityEngine.UI;

public class OptionBase : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Tooltip("データ")] private StatusEffectData[] _data;
    [SerializeField, Tooltip("ID")] private int _id = -1;

    // ---------------------------- Field
    protected float _attack;
    protected float _currentInterval;

    // ---------------------------- AbstractMethod
    /// <summary>
    /// 装備されたときの処理
    /// </summary>
    public void WhenEquipped()
    {
        foreach (var item in _data)
        {
            RuntimeModuleManager.Instance.AddOption(item);
        }
    }

    /// <summary>
    /// 外されたときの処理
    /// </summary>
    public void ProcessingWhenRemoved()
    {
        foreach (var item in _data)
        {
            RuntimeModuleManager.Instance.RemoveOption(item);
        }
    }
}
