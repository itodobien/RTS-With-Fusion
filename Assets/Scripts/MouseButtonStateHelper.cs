using UnityEngine;

public static class MouseButtonStateHelper
{
    public static MouseButtonState GetMouseButtonState(int button = 0)
    {
        if (Input.GetMouseButtonDown(0))
        {
            return MouseButtonState.ButtonDown;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            return MouseButtonState.ButtonUp;
        }
        else if (Input.GetMouseButton(0))
        {
            return MouseButtonState.ButtonHeld;
        }
        return default;
    }
}
