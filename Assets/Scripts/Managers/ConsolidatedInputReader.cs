using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Fusion;

public class ConsolidatedInputManager : MonoBehaviour
{
    public static ConsolidatedInputManager Instance { get; private set; }

    // Network input events
    public event Action<bool> OnMouseButton0;
    public event Action<bool> OnMouseButton1;
    public event Action<Vector3> OnTargetPosition;
    public event Action<bool> OnSpawnUnit;
    public event Action<bool> OnSwitchMaterial;
    public event Action<bool> OnJump;
    public event Action<int, bool> OnSelectUnit;
    public event Action<ActionType> OnActionTypeChanged;

    // Camera input events
    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnEdgeScroll;
    public event Action<Vector2> OnSwipeMove;
    public event Action<Vector2> OnSwipeRotate;
    public event Action<Vector2> OnClickMove;
    public event Action<float> OnRotate;
    public event Action<float> OnZoom;

    // Input state
    private Vector2 moveInput;
    private Vector2 edgeScrollInput;
    private Vector2 swipeMoveInput;
    private Vector2 swipeRotateInput;
    private float rotateInput;
    private float zoomInput;

    [SerializeField] private float edgeScrollThreshold = 10f;
    [SerializeField] private float swipeThreshold = 5f;

    private Vector2 lastMousePosition;
    private bool isSwipeMoving;
    private bool isSwipeRotating;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        HandleNetworkInputs();
        HandleCameraInputs();
    }

    private void HandleNetworkInputs()
    {
        // Mouse buttons
        if (Mouse.current.leftButton.wasPressedThisFrame) OnMouseButton0?.Invoke(true);
        if (Mouse.current.leftButton.wasReleasedThisFrame) OnMouseButton0?.Invoke(false);

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            OnMouseButton1?.Invoke(true);
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            OnTargetPosition?.Invoke(mouseWorldPos);
        }
        if (Mouse.current.rightButton.wasReleasedThisFrame) OnMouseButton1?.Invoke(false);

        // Other inputs
        if (Keyboard.current.uKey.wasPressedThisFrame)
        {
            OnSpawnUnit?.Invoke(true);
            Vector3 spawnPos = GetMouseWorldPosition();
            OnTargetPosition?.Invoke(spawnPos);
        }

        if (Keyboard.current.cKey.wasPressedThisFrame) OnSwitchMaterial?.Invoke(true);

        if (Keyboard.current.spaceKey.wasPressedThisFrame) OnJump?.Invoke(true);
        if (Keyboard.current.spaceKey.wasReleasedThisFrame) OnJump?.Invoke(false);

        // Unit selection and action type changes would typically be handled by your game logic,
        // but you could add methods here to trigger these events when appropriate
    }

    private void HandleCameraInputs()
    {
        // Move
        moveInput = new Vector2(
            Keyboard.current.dKey.isPressed ? 1 : Keyboard.current.aKey.isPressed ? -1 : 0,
            Keyboard.current.wKey.isPressed ? 1 : Keyboard.current.sKey.isPressed ? -1 : 0
        );
        if (moveInput != Vector2.zero) OnMove?.Invoke(moveInput);

        // Edge scrolling
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        edgeScrollInput = Vector2.zero;
        if (mousePosition.x <= edgeScrollThreshold) edgeScrollInput.x = -1;
        else if (mousePosition.x >= Screen.width - edgeScrollThreshold) edgeScrollInput.x = 1;
        if (mousePosition.y <= edgeScrollThreshold) edgeScrollInput.y = -1;
        else if (mousePosition.y >= Screen.height - edgeScrollThreshold) edgeScrollInput.y = 1;
        if (edgeScrollInput != Vector2.zero) OnEdgeScroll?.Invoke(edgeScrollInput);

        // Swipe move and rotate
        if (Mouse.current.middleButton.isPressed)
        {
            Vector2 mouseDelta = mousePosition - lastMousePosition;
            if (mouseDelta.magnitude > swipeThreshold)
            {
                if (!isSwipeRotating && Keyboard.current.leftAltKey.isPressed)
                {
                    isSwipeRotating = true;
                    isSwipeMoving = false;
                }
                else if (!isSwipeMoving)
                {
                    isSwipeMoving = true;
                    isSwipeRotating = false;
                }

                if (isSwipeMoving)
                {
                    swipeMoveInput = mouseDelta;
                    OnSwipeMove?.Invoke(swipeMoveInput);
                }
                else if (isSwipeRotating)
                {
                    swipeRotateInput = mouseDelta;
                    OnSwipeRotate?.Invoke(swipeRotateInput);
                }
            }
        }
        else
        {
            isSwipeMoving = false;
            isSwipeRotating = false;
        }

        // Click move
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            OnClickMove?.Invoke(mousePosition);
        }

        // Rotate
        rotateInput = 0f;
        if (Keyboard.current.qKey.isPressed) rotateInput -= 1f;
        if (Keyboard.current.eKey.isPressed) rotateInput += 1f;
        if (rotateInput != 0f) OnRotate?.Invoke(rotateInput);

        // Zoom
        zoomInput = Mouse.current.scroll.ReadValue().y;
        if (zoomInput != 0f) OnZoom?.Invoke(zoomInput);

        lastMousePosition = mousePosition;
    }

    public Vector3 GetMouseWorldPosition() 
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = Camera.main.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    // Methods to trigger unit selection and action type changes
    public void TriggerUnitSelection(int unitId, bool isSelected)
    {
        OnSelectUnit?.Invoke(unitId, isSelected);
    }

    public void TriggerActionTypeChange(ActionType actionType)
    {
        OnActionTypeChanged?.Invoke(actionType);
    }
}
