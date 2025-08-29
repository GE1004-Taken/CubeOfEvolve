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

    private ReactiveProperty<bool> _isPushedCreateButton = new ReactiveProperty<bool>();
    public ReadOnlyReactiveProperty<bool> Create => _isPushedCreateButton;

    private ReactiveProperty<bool> _isPushedPauseButton = new ReactiveProperty<bool>();
    public ReadOnlyReactiveProperty<bool> Pause => _isPushedPauseButton;

    private ReactiveProperty<float> _zoomInput = new ReactiveProperty<float>();
    public ReadOnlyReactiveProperty<float> Zoom => _zoomInput;

    private ReactiveProperty<bool> _isPushedMoveCameraButton = new ReactiveProperty<bool>();
    public ReadOnlyReactiveProperty<bool> MoveCamera => _isPushedMoveCameraButton;

    private ReactiveProperty<bool> _isShop = new();
    public ReadOnlyReactiveProperty<bool> Shop => _isShop;

    private ReactiveProperty<bool> _isBuild = new();
    public ReadOnlyReactiveProperty<bool> Build => _isBuild;

    public ReactiveProperty<bool> _isRemove = new();

    public ReadOnlyReactiveProperty<bool> Remove => _isRemove;
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

    public void OnCreate(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _isPushedCreateButton.Value = true;
        }
        else if (context.canceled)
        {
            _isPushedCreateButton.Value = false;
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

    public void OnZoom(InputAction.CallbackContext context)
    {
        _zoomInput.Value = context.ReadValue<float>();
    }

    public void OnMoveCamera(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            _isPushedMoveCameraButton.Value = true;
        }
        else
        {
            _isPushedMoveCameraButton.Value = false;
        }
    }

    public void OnShop(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _isShop.Value = true;
        }
        else if(context.canceled)
        {
            _isShop.Value = false;
        }
    }

    public void OnBuild(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _isBuild.Value = true;
        }
        else if (context.canceled)
        {
            _isBuild.Value = false;
        }
    }

    public void OnRemove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _isRemove.Value = true;
        }
        else if (context.canceled)
        {
            _isRemove.Value = false;
        }
    }

    // ---------- UnityMessage
    private void Start()
    {
        this.UpdateAsObservable()
            .Select(x => _moveContextReadValue)
            .Subscribe(x => _moveInput.OnNext(x));

        _moveInput.AddTo(this);
        _isPushedSkillButton.AddTo(this);
        _isPushedCreateButton.AddTo(this);
        _isPushedPauseButton.AddTo(this);
        _zoomInput.AddTo(this);
    }
}
