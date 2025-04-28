using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

public class ControllerChecker : MonoBehaviour
{
    public GameObject xboxObject;
    public GameObject playstationObject;

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
        CheckControllers();
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added || change == InputDeviceChange.Removed)
        {
            CheckControllers();
        }
    }

    private void CheckControllers()
    {
        bool isXbox = Gamepad.current is XInputController;
        bool isPlayStation = Gamepad.current is DualShockGamepad;

        xboxObject.SetActive(isXbox);
        playstationObject.SetActive(isPlayStation);
    }
}