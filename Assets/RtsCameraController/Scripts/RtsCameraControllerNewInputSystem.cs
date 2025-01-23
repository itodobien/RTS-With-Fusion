using UnityEngine;
using UnityEngine.InputSystem;

namespace RtsCamera
{
    [RequireComponent(typeof(RtsCameraController))]
    public class RtsCameraControllerNewInputSystem : MonoBehaviour
    {
        private RtsCameraController cameraController;

        [SerializeField] private bool edgeScrollingEnabled = true;
        [SerializeField] private bool swipeMoveEnabled = true;
        [SerializeField] private bool clickMoveEnabled = true;
        [SerializeField] private bool swipeRotateEnabled = true;
        [SerializeField] private bool zoomEnabled = true;
        [SerializeField] private bool zoomReverse = false;

        [SerializeField] private float swipeMoveSpeed = 1.0f;
        [SerializeField] private float swipeRotateSpeed = 1.0f;
        [SerializeField] private float inputDeadzone = 0.1f;

        private Vector2 moveDirection;
        private Vector2 edgeScrollingDirection;
        private float rotateDirection;
        private Vector2 previousMovePosition;
        private Vector2 previousRotatePosition;

        private void Awake()
        {
            cameraController = GetComponent<RtsCameraController>();
        }

        private void Update()
        {
            if (moveDirection.magnitude > inputDeadzone)
            {
                cameraController.Move(moveDirection);
            }
            else
            {
                moveDirection = Vector2.zero;
            }

            if (edgeScrollingEnabled && edgeScrollingDirection != Vector2.zero)
            {
                cameraController.EdgeScreenMove(edgeScrollingDirection);
            }

            if (rotateDirection != 0)
            {
                cameraController.Rotate(rotateDirection);
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 newMoveDirection = context.ReadValue<Vector2>();
            if (context.canceled)
            {
                moveDirection = Vector2.zero;
            }
            else
            {
                moveDirection = newMoveDirection;
            }
        }

        public void OnEdgeScroll(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Vector2 mousePosition = context.ReadValue<Vector2>();
                edgeScrollingDirection = cameraController.ConvertScreenInputToDirection(mousePosition);
            }
            else if (context.canceled)
            {
                edgeScrollingDirection = Vector2.zero;
            }
        }

        public void OnSwipeMove(InputAction.CallbackContext context)
        {
            if (swipeMoveEnabled)
            {
                Vector2 swipeDelta = context.ReadValue<Vector2>();
                if (context.performed)
                {
                    if (previousMovePosition != Vector2.zero)
                    {
                        cameraController.Move(-(swipeDelta - previousMovePosition) * swipeMoveSpeed / Time.deltaTime);
                    }

                    previousMovePosition = swipeDelta;
                }
                else if (context.canceled)
                {
                    previousMovePosition = Vector2.zero;
                }
            }
        }

        public void OnClickMove(InputAction.CallbackContext context)
        {
            if (clickMoveEnabled && context.performed)
            {
                Vector2 clickPosition = Mouse.current.position.ReadValue();
                var ray = Camera.main.ScreenPointToRay(clickPosition);
                if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue))
                {
                    cameraController.MoveTo(hit.point);
                }
            }
        }

        public void OnRotate(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                rotateDirection = context.ReadValue<float>();
            }
            else if (context.canceled)
            {
                rotateDirection = 0;
            }
        }

        public void OnSwipeRotate(InputAction.CallbackContext context)
        {
            if (swipeRotateEnabled)
            {
                Vector2 swipeRotateDelta = context.ReadValue<Vector2>();
                if (context.performed)
                {
                    if (previousRotatePosition != Vector2.zero)
                    {
                        cameraController.Rotate((swipeRotateDelta.x - previousRotatePosition.x) * swipeRotateSpeed /
                                                Time.deltaTime);
                    }

                    previousRotatePosition = swipeRotateDelta;
                }
                else if (context.canceled)
                {
                    previousRotatePosition = Vector2.zero;
                }
            }
        }

        public void OnZoom(InputAction.CallbackContext context)
        {
            if (zoomEnabled && context.performed)
            {
                float zoomInput = context.ReadValue<float>();
                cameraController.Zoom(zoomReverse ? -zoomInput : zoomInput);
            }
        }
    }
}