using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using UnityEngine;

public class OptionBase : MonoBehaviour, IModuleID
{
    // ---------------------------- SerializeField
    [Header("データ")]
    [SerializeField, Tooltip("データ")] protected ModuleData _data;
    [SerializeField, Tooltip("バフデータ")] private StatusEffectData[] _statusEffectData;

    // ---------------------------- Field
    protected float _attack;
    protected float _currentInterval;

    // ---------------------------- Property
    /// <summary>
    /// ID
    /// </summary>
    public int Id => _data.Id;

    // ---------------------------- AbstractMethod
    /// <summary>
    /// 装備されたときの処理
    /// </summary>
    public void WhenEquipped()
    {
        foreach (var item in _statusEffectData)
        {
            RuntimeModuleManager.Instance.AddOption(item);
        }
    }

    /// <summary>
    /// 外されたときの処理
    /// </summary>
    public void ProcessingWhenRemoved()
    {
        foreach (var item in _statusEffectData)
        {
            RuntimeModuleManager.Instance.RemoveOption(item);
        }
    }
}
