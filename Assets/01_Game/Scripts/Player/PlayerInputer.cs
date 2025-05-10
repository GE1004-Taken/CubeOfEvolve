using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputer : MonoBehaviour, IInputEventProvider
{
    // ---------- RP
    private ReactiveProperty<Vector2> _moveInput = new ReactiveProperty<Vector2>();
    public ReadOnlyReactiveProperty<Vector2> Move => _moveInput;

    private ReactiveProperty<bool> _isPushedSkillButton = new ReactiveProperty<bool>();
    public ReadOnlyReactiveProperty<bool> Skill => _isPushedSkillButton;

    private ReactiveProperty<bool> _isPushedPauseButton = new ReactiveProperty<bool>();
    public ReadOnlyReactiveProperty<bool> Pause => _isPushedPauseButton;

    // ---------- Field
    private Vector2 _moveContextReadValue;

    // ---------- InputSystem
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveContextReadValue = context.ReadValue<Vector2>();
    }

    public void OnSkill(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            _isPushedSkillButton.Value = true;
        }
        else if(context.canceled)
        {
            _isPushedSkillButton.Value = false;
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            _isPushedPauseButton.Value = true;
        }
        else if(context.canceled)
        {
            _isPushedPauseButton.Value = false;
        }
    }

    // ---------- UnityMessage
    private void Start()
    {
        this.UpdateAsObservable()
            .Select(x => _moveContextReadValue)
            .Subscribe(x => _moveInput.OnNext(x));
    }
}
