using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputMap : MonoBehaviour
{
    public Action OnRoll;
    public Action OnJump;
    public Action OnHoldShootPerformed;
    public Action OnHoldShootCanceled;
    public Action OnSwitchGun;
    public Action OnReloadGun;

    private Player_InputMap inputMap;

    private void Awake()
    {
        inputMap = new Player_InputMap();
        inputMap.Enable();
    }

    private void OnDisable()
    {
        inputMap.Disable();
    }

    private void Start()
    {
        inputMap.Player.Roll.performed += (InputAction.CallbackContext context) => OnRoll?.Invoke();
        inputMap.Player.Jump.performed += (InputAction.CallbackContext context) => OnJump?.Invoke();
        inputMap.Player.Shoot.performed += (InputAction.CallbackContext context) => OnHoldShootPerformed?.Invoke();
        inputMap.Player.Shoot.canceled += (InputAction.CallbackContext context) => OnHoldShootCanceled?.Invoke();
        inputMap.Player.SwitchGun.performed += (InputAction.CallbackContext context) => OnSwitchGun?.Invoke();
        inputMap.Player.Reload.performed += (InputAction.CallbackContext context) => OnReloadGun?.Invoke();
    }

    public Vector2 GetRawInputNormalized()
    {
        Vector2 inputVector = inputMap.Player.Move.ReadValue<Vector2>();
        return inputVector.normalized;
    }
}
