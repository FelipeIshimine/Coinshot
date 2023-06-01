using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour, ActionMap.IInteractionActions, ActionMap.IAimActions,ActionMap.IMovementActions
{
    [SerializeField] private PlayerCharacter player;
    
    [Range(-1,1)]public int manualDir;

    private Camera _cam;
    
    public enum ControllerMode { None, KeyboardAndMouse, Joystick }

    [SerializeField] private ControllerMode mode = ControllerMode.Joystick;

    private ActionMap _actionMap;
    
    private void Awake()
    {
        _actionMap = new ActionMap();
        _actionMap.Aim.SetCallbacks(this);
        _actionMap.Movement.SetCallbacks(this);
        _actionMap.Interaction.SetCallbacks(this);
        InputSystem.onDeviceChange += InputSystemOnDeviceChange;
        
    }

    private void OnDestroy()
    {
        InputSystem.onDeviceChange -= InputSystemOnDeviceChange;
    }

    public void SetController(ControllerMode controllerMode)
    {
        mode = controllerMode;
        switch (controllerMode)
        {
            case ControllerMode.None:
                break;
            case ControllerMode.KeyboardAndMouse:
                _actionMap.bindingMask = InputBinding.MaskByGroup("KeyboardAndMouse");
                break;
            case ControllerMode.Joystick:
                _actionMap.bindingMask = InputBinding.MaskByGroup("Joystick");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(controllerMode), controllerMode, null);
        }
        Debug.Log($"InputChanged {controllerMode}");
    }

    private void InputSystemOnDeviceChange(InputDevice arg1, InputDeviceChange arg2)
    {
        //Debug.Log($"{arg1} {arg2}");
        switch (arg2)
        {
            case InputDeviceChange.Added:
                break;
            case InputDeviceChange.Removed:
                break;
            case InputDeviceChange.Disconnected:
                break;
            case InputDeviceChange.Reconnected:
                break;
            case InputDeviceChange.Enabled:
                break;
            case InputDeviceChange.Disabled:
                break;
            case InputDeviceChange.UsageChanged:
                if (arg1 is Mouse or Keyboard)
                {
                    _actionMap.bindingMask = InputBinding.MaskByGroup("KeyboardAndMouse");
                    Debug.Log("Masked to Keyboard and mouse");
                }
                else if (arg1 is Gamepad)
                {
                    _actionMap.bindingMask = InputBinding.MaskByGroup("Joystick");
                    Debug.Log("Masked to Joystick");
                }

                break;
            case InputDeviceChange.ConfigurationChanged:
                break;
            case InputDeviceChange.SoftReset:
                break;
            case InputDeviceChange.HardReset:
                break;
            case InputDeviceChange.Destroyed:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(arg2), arg2, null);
        }
    }


    private void OnEnable()
    {
        _cam = Camera.main;
        _actionMap.Enable();
        SetController(mode);
    }

    private void OnDisable()
    {
        _actionMap.Disable();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            if(mode == ControllerMode.Joystick)
                SetController(ControllerMode.KeyboardAndMouse);
            else
                SetController(ControllerMode.Joystick);
        }
    }
  
    public void OnPush(InputAction.CallbackContext context)
    {
        if (context.performed)
            player.StartPush();
        else
            player.StopPush();
    }
    
    public void OnPull(InputAction.CallbackContext context)
    {
        if (context.performed)
            player.StartPull();
        else
            player.StopPull();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(!context.performed) return;
        player.Jump();
    }

    public void OnLookPosition(InputAction.CallbackContext context)
    {
        if (context.control.device is Mouse)
        {
            if(!context.performed) return;

            var mousePosition = context.ReadValue<Vector2>();
            var worldPost = _cam.orthographic?_cam.ScreenToWorldPoint(mousePosition):_cam.GetWorldPositionOnPlane(mousePosition,0);
            var diff = (worldPost - frog.tonguePivot.transform.position);
            var dir = diff.normalized * Mathf.Clamp01(diff.magnitude / frog.cameraTargetMaxDistance);
            frog.SetLookDirection(dir);
        }
        else
        {
            if(context.performed)
                frog.SetLookDirection(context.ReadValue<Vector2>());
            else
                frog.SetLookDirection(Vector2.zero);
        }
    }

    public void OnMoveDirection(InputAction.CallbackContext context)
    {
        if (context.performed)
            frog.SetMoveInput(context.ReadValue<Vector2>());
        else
            frog.SetMoveInput(Vector2.zero);
    }
}
