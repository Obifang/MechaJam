using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;
using static UnityEngine.InputSystem.InputAction;

public class Manager_Input : MonoBehaviour
{
    protected PlayerInputActions PlayerInputActions;
    public static Manager_Input Instance { get; private set; }

    public PlayerInput _playerInput;

    private InputAction _fire;
    private InputAction _changeState;
    private InputAction _move;
    private InputAction _jump;
    private InputAction _dash;
    private InputAction _look;
    private InputAction _lockonCamera;
    private InputAction _mediumState;
    private InputAction _pause;
    private InputAction _shiftToHighState;
    private InputAction _shiftToLowState;
    private InputAction _shiftToMediumState;
    private InputAction _rotateRight;
    private InputAction _rotateLeft;

    public delegate void RebindDelegate();
    public event RebindDelegate RebindingComplete;
    public event RebindDelegate RebindingCancelled;
    public event RebindDelegate AllRebindinsReset;

    public InputAction Fire { get => _fire; }
    public InputAction ChangeState { get => _changeState; }
    public InputAction Move { get => _move; }
    public InputAction Jump { get => _jump; }
    public InputAction Dash { get => _dash; }
    public InputAction MediumState { get => _mediumState; }
    public InputAction Look { get => _look; }
    public InputAction Lockon { get => _lockonCamera; }
    public InputAction Pause { get => _pause; }
    public InputAction ShiftToHighState { get => _shiftToHighState;}
    public InputAction ShiftToLowState { get => _shiftToLowState; }
    public InputAction ShiftToMediumState { get => _shiftToMediumState; }
    public InputAction RotateRight { get => _rotateRight; }
    public InputAction RotateLeft { get => _rotateLeft; }

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        } else {
            Instance = this;
            PlayerInputActions = new PlayerInputActions();
            UpdateKeybinds();
            LoadUserRebinds();
            DontDestroyOnLoad(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Rebinding

    public void SaveXInversion(bool value)
    {
        if (value) {
            PlayerPrefs.SetInt("invertX", 1);
        } else {
            PlayerPrefs.SetInt("invertX", 0);
        }
        
    }
    public void SaveYInversion(bool value)
    {
        if (value) {
            PlayerPrefs.SetInt("invertY", 1);
        } else {
            PlayerPrefs.SetInt("invertY", 0);
        }
    }

    public (int, int) LoadUserInversion()
    {
        return (PlayerPrefs.GetInt("invertX"), PlayerPrefs.GetInt("invertY"));
    }

    public void UpdateScaleFactorVector2(string action, int bindingindex, float value)
    {
        //PlayerInputActions.Player.Disable();
        PlayerInputActions.FindAction(action).ApplyParameterOverride("scaleVector2:x", value, bindingindex);
        PlayerInputActions.FindAction(action).ApplyParameterOverride("scaleVector2:y", value, bindingindex);
        //PlayerInputActions.Player.Enable();
    }

    public void RebindKeyKeyboard(string action)
    {
        PlayerInputActions.Player.Disable();
        var rebind = PlayerInputActions.FindAction(action);
        
        rebind.PerformInteractiveRebinding(0).
            WithCancelingThrough("<Keyboard>/escape").
            OnCancel(callback => {
                Debug.Log(callback);
                callback.Dispose();
                PlayerInputActions.Player.Enable();
                if (RebindingCancelled != null) {
                    RebindingCancelled.Invoke();
                }
            }).
            OnComplete(callback => {
                Debug.Log(callback);
                callback.Dispose();
                PlayerInputActions.Player.Enable();
                SaveUserRebinds();
                if (RebindingComplete != null) {
                    RebindingComplete.Invoke();
                }
            })
            .Start();
    }

    public void RebindKeyController(string action)
    {
        PlayerInputActions.Player.Disable();
        var rebind = PlayerInputActions.FindAction(action);

        rebind.PerformInteractiveRebinding(1).
            WithControlsExcluding("Mouse").
            WithCancelingThrough("<Keyboard>/escape").
            WithCancelingThrough("<Gamepad>/start").
            OnCancel(callback => {
                Debug.Log(callback);
                callback.Dispose();
                PlayerInputActions.Player.Enable();
                if (RebindingCancelled != null) {
                    RebindingCancelled.Invoke();
                }
            }).
            OnComplete(callback => {
                Debug.Log(callback);
                callback.Dispose();
                PlayerInputActions.Player.Enable();
                SaveUserRebinds();
                if (RebindingComplete != null) {
                    RebindingComplete.Invoke();
                }
            })
            .Start();
    }

    public void ResetBinding(string action)
    {
        PlayerInputActions.Player.Disable();
        var rebind = PlayerInputActions.FindAction(action);
        rebind.RemoveAllBindingOverrides();
        PlayerInputActions.Player.Enable();
        SaveUserRebinds();
    }

    public void ResetBindings()
    {
        PlayerInputActions.Player.Disable();
        PlayerInputActions.RemoveAllBindingOverrides();
        PlayerInputActions.Player.Enable();
        SaveUserRebinds();
        if (AllRebindinsReset != null) {
            AllRebindinsReset.Invoke();
        }
    }

    public void SaveUserRebinds()
    {
        var rebinds = PlayerInputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
    }

    public void LoadUserRebinds()
    {
        var rebinds = PlayerPrefs.GetString("rebinds");
        PlayerInputActions.LoadBindingOverridesFromJson(rebinds);
    }

    #endregion

    public void AddPerformedCallback(string actionName, Action<CallbackContext> action)
    {
        _playerInput.actions[actionName].performed += action;
    }

    public void AddCanceledCallback(string actionName, Action<CallbackContext> action)
    {
        _playerInput.actions[actionName].canceled += action;
    }
    public void RemovePerformedCallback(string actionName, Action<CallbackContext> action)
    {
        _playerInput.actions[actionName].performed -= action;
    }

    public void RemoveCanceledCallback(string actionName, Action<CallbackContext> action)
    {
        _playerInput.actions[actionName].canceled -= action;
    }

    public void UpdateKeybinds()
    {
        //string rebinds = PlayerPrefs.GetString("Player");
        //LoadUserRebinds();
        if (_playerInput == null) {
            _playerInput = GetComponent<PlayerInput>();
        }

        _playerInput.currentActionMap.Enable();
    }

    private void OnEnable()
    {
        UpdateKeybinds();

        _fire = PlayerInputActions.Player.Fire;
        _changeState = PlayerInputActions.Player.ChangeState;
        _fire.Enable();
        _changeState.Enable();
        _move = PlayerInputActions.Player.Move;
        _jump = PlayerInputActions.Player.Jump;
        _dash = PlayerInputActions.Player.Dash;
        _mediumState = PlayerInputActions.Player.MediumState;
        _look = PlayerInputActions.Player.Look;
        _lockonCamera = PlayerInputActions.Player.LockonCamera;
        _pause = PlayerInputActions.UI.Pause;
        _shiftToHighState = PlayerInputActions.Player.ShiftToHighState;
        _shiftToLowState = PlayerInputActions.Player.ShiftToLowState;
        _shiftToMediumState = PlayerInputActions.Player.ShiftToMediumState;
        _rotateRight = PlayerInputActions.Player.RotateRight;
        _rotateLeft = PlayerInputActions.Player.RotateLeft;
        _lockonCamera.Enable();
        _look.Enable();
        _mediumState.Enable();
        _move.Enable();
        _jump.Enable();
        _dash.Enable();
        _pause.Enable();
        _shiftToHighState.Enable();
        _shiftToLowState.Enable();
        _shiftToMediumState.Enable();
        _rotateRight.Enable();
        _rotateLeft.Enable();
    }

    private void OnDisable()
    {
        if (_playerInput == null) {
            _playerInput = GetComponent<PlayerInput>();
        }

        _mediumState.Disable();
        _fire.Disable();
        _changeState.Disable();
        _move.Disable();
        _jump.Disable();
        _dash.Disable();
        _look.Disable();
        _lockonCamera.Disable();
        _pause.Disable();
        _shiftToHighState.Disable();
        _shiftToLowState.Disable();
        _shiftToMediumState.Disable();
        _rotateRight.Disable();
        _rotateLeft.Disable();
    }
}
