using UnityEngine;

namespace Enums
{
    public static class HandleMouseButtonState
    {
        public enum MouseButtonState
        {
            ButtonDown,
            ButtonUp,
            ButtonHeld
        }
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
}
