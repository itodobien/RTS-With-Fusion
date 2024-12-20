using UnityEngine;

public static class SelectionModeHelper
{
    public static SelectionMode GetSelectionModeFromInput()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            return SelectionMode.Additive;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            return SelectionMode.Subtractive;
        }
        return SelectionMode.Default;
    }
    
    
}

