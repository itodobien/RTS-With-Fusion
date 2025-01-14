using UnityEngine;

namespace RtsCamera
{
    [RequireComponent(typeof(RtsCameraController))]
    public class RtsCameraControllerOldInputSystem : MonoBehaviour
    {
        private RtsCameraController cameraController;

        #region Movement
        //KEYBOARD
        [SerializeField]
        private KeyCode MoveForwardKey = KeyCode.W;
        [SerializeField]
        private KeyCode MoveLeftKey = KeyCode.A;
        [SerializeField]
        private KeyCode MoveBackwardKey = KeyCode.S;
        [SerializeField]
        private KeyCode MoveRightKey = KeyCode.D;
        [SerializeField]
        private Vector2 moveDirection;

        //MOUSE AND SCREEN
        public bool EdgeScrollingEnabled;
        [SerializeField]
        private Vector2 edgeScrollingInputMoveDirection;

        //MOUSE SWIPE MOVE
        public bool SwipeMoveEnabled;
        [SerializeField]
        private KeyCode swipeMoveKey = KeyCode.Mouse0;
        [SerializeField]
        private float swipeMoveSpeed = 1.0f;
        private Vector3 previousMoveMousePosition;

        //MOVE ON CLICK
        public bool ClickMoveEnabled;
        [SerializeField]
        private KeyCode clickMoveKey = KeyCode.Mouse1;
        #endregion

        #region Zoom
        public bool ZoomEnabled = true;
        public bool ZoomReverse;
        #endregion 

        #region Rotation
        //KEYBOARD
        [SerializeField]
        private KeyCode rotateLeftKey = KeyCode.Q;
        [SerializeField]
        private KeyCode rotateRightKey = KeyCode.E;

        //MOUSE SWIPE ROTATION
        public bool SwipeRotateEnabled;
        [SerializeField]
        private KeyCode swipeRotateKey = KeyCode.Mouse2;
        [SerializeField]
        private float swipeRotateSpeed = 1.0f;
        private Vector3 previousRotateMousePosition;
        #endregion 

        private void Awake()
        {
            cameraController = GetComponent<RtsCameraController>();
        }

        private void LateUpdate()
        {
            if (SwipeMoveEnabled)
                MoveByMouse();

            if (SwipeRotateEnabled)
                RotateByMouse();

            MoveInput();

            if (ClickMoveEnabled)
                MoveTo();

            if (EdgeScrollingEnabled)
            {
                edgeScrollingInputMoveDirection = cameraController.ConvertScreenInputToDirection(Input.mousePosition);
                cameraController.EdgeScreenMove(edgeScrollingInputMoveDirection);
            }

            if (ZoomEnabled)
            {
                if (ZoomReverse)
                {
                    cameraController.Zoom(-ZoomInput());
                }
                else
                {
                    cameraController.Zoom(ZoomInput());
                }
            }

            cameraController.Rotate(RotateInput());
        }

        private void MoveInput()
        {
            if (Input.GetKey(MoveRightKey) && Input.GetKey(MoveForwardKey))
            {
                moveDirection.x = moveDirection.y = 0.75f;
            }
            else if (Input.GetKey(MoveRightKey) && Input.GetKey(MoveBackwardKey))
            {
                moveDirection.x = 0.75f;
                moveDirection.y = -0.75f;
            }
            else if (Input.GetKey(MoveLeftKey) && Input.GetKey(MoveForwardKey))
            {
                moveDirection.x = -0.75f;
                moveDirection.y = 0.75f;
            }
            else if (Input.GetKey(MoveLeftKey) && Input.GetKey(MoveBackwardKey))
            {
                moveDirection.x = moveDirection.y = -0.75f;
            }
            else if (Input.GetKey(MoveForwardKey))
            {
                moveDirection.x = 0.0f;
                moveDirection.y = 1.0f;
            }
            else if (Input.GetKey(MoveLeftKey))
            {
                moveDirection.x = -1.0f;
                moveDirection.y = 0.0f;
            }
            else if (Input.GetKey(MoveBackwardKey))
            {
                moveDirection.x = 0.0f;
                moveDirection.y = -1.0f;
            }
            else if (Input.GetKey(MoveRightKey))
            {
                moveDirection.x = 1.0f;
                moveDirection.y = 0.0f;
            }
            else
            {
                moveDirection = Vector2.zero;
            }

            cameraController.Move(moveDirection);
        }

        private void MoveByMouse()
        {
            if (Input.GetKey(swipeMoveKey))
            {
                if (previousMoveMousePosition != Vector3.zero)
                {
                    cameraController.Move(-(Input.mousePosition - previousMoveMousePosition) * swipeMoveSpeed / Time.deltaTime);
                }

                previousMoveMousePosition = Input.mousePosition;
            }

            if (Input.GetKeyUp(swipeMoveKey))
            {
                previousMoveMousePosition = Vector3.zero;
            }
        }

        private void MoveTo()
        {
            if (Input.GetKeyDown(clickMoveKey))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, float.MaxValue))
                {
                    cameraController.MoveTo(hit.point);
                }
            }
        }

        private void RotateByMouse()
        {
            if (Input.GetKey(swipeRotateKey))
            {
                if (previousRotateMousePosition != Vector3.zero)
                {
                    cameraController.Rotate((Input.mousePosition - previousRotateMousePosition).x * swipeRotateSpeed / Time.deltaTime);
                }

                previousRotateMousePosition = Input.mousePosition;
            }

            if (Input.GetKeyUp(swipeRotateKey))
            {
                previousRotateMousePosition = Vector3.zero;
            }
        }

        private float ZoomInput()
        {
            return Input.mouseScrollDelta.y;
        }

        private float RotateInput()
        {
            if (Input.GetKey(rotateLeftKey))
                return -1.0f;

            if (Input.GetKey(rotateRightKey))
                return 1.0f;

            return 0.0f;
        }
    }
}