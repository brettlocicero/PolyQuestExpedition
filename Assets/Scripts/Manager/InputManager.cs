using UnityEngine;

public static class InputManager
{
    public static InputSystem_Actions Actions { get; private set; }

    static InputManager()
    {
        Actions = new InputSystem_Actions();
        Actions.Enable();
    }
}