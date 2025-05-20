using UnityEngine;

[RequireComponent(typeof(PlayerCore))]
public abstract class BasePlayerComponent : MonoBehaviour
{
    private IInputEventProvider _inputEventProvider;

    // 各コンポーネントでよく使われるものコンポーネント
    protected IInputEventProvider InputEventProvider => _inputEventProvider;
    protected PlayerCore Core;

    // ---------- UnityMessage
    private void Start()
    {
        Core = GetComponent<PlayerCore>();
        _inputEventProvider = GetComponent<IInputEventProvider>();
        OnInitialize();
    }

    // ---------- Method
    protected abstract void OnInitialize();
}
