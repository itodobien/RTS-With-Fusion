using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static InputSystem_Actions;

public interface IInputReader
{
    Vector2 Direction { get; }
    void EnablePlayerActions();
}

[CreateAssetMenu(menuName = "Input/Input Reader", fileName = "New Input Reader")]
public class InputReader : ScriptableObject, IInputReader, IPlayerActions
{
    public event UnityAction<Vector2> Move = delegate { };
    public event UnityAction<bool> Jump = delegate { };
    public event UnityAction<Vector2> EdgeScroll = delegate { };
    public event UnityAction<Vector2> SwipeMove = delegate { };
    public event UnityAction<Vector2> SwipeRotate = delegate { };
    public event UnityAction<Vector2> ClickMove = delegate { };
    public event UnityAction<Vector2> RotateCamera = delegate { };
    public event UnityAction<Vector2> CameraZoom = delegate { };

    public InputSystem_Actions inputActions;

    public Vector2 Direction => inputActions.Player.Move.ReadValue<Vector2>();
    public bool IsJumpKeyPressed => inputActions.Player.Jump.IsPressed();

    public void EnablePlayerActions()
    {
        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
            inputActions.Player.SetCallbacks(this);
        }

        inputActions.Enable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Move.Invoke(context.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Jump.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                Jump.Invoke(false);
                break;
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }


    public void OnPrevious(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnEdgeScroll(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnSwipeMove(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnClickMove(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnRotateCamera(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnSwipeRotate(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnCameraZoom(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }
}