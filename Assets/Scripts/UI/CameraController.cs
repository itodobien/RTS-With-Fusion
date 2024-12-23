using UnityEngine;

namespace UI
{
    public class CameraController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 10f;     // Speed for arrow-key movement
        public float panSpeed = 50f;      // Speed for middle-mouse panning

        [Header("Zoom Settings")]
        public float zoomSpeed = 20f;     // How quickly to zoom using the scroll wheel
        public float minZoom = 10f;       // Closest zoom distance
        public float maxZoom = 100f;      // Farthest zoom distance
    
        [Header("Zoom Interpolation Settings")]
        public float zoomLerpSpeed = 10f;  // Speed to smooth the zoom transition

        private float _targetZoom;   

        private float _currentZoom;

        void Start()
        {
            _currentZoom = transform.position.y;
            _targetZoom = _currentZoom;
        }

        void Update()
        {
        
            Vector3 inputMoveDir = Vector3.zero;

            if (Input.GetKey(KeyCode.UpArrow))
                inputMoveDir += Vector3.forward;
            if (Input.GetKey(KeyCode.DownArrow))
                inputMoveDir += Vector3.back;
            if (Input.GetKey(KeyCode.LeftArrow))
                inputMoveDir += Vector3.left;
            if (Input.GetKey(KeyCode.RightArrow))
                inputMoveDir += Vector3.right;
        
            Vector3 moveVector = transform.forward * inputMoveDir.z + transform.right * inputMoveDir.x;

            transform.position += moveVector * (moveSpeed * Time.deltaTime);

            // 2) Zoom with scroll wheel
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scrollInput) > 0.01f)
            {
                // Update target zoom value
                _targetZoom -= scrollInput * zoomSpeed;
                _targetZoom = Mathf.Clamp(_targetZoom, minZoom, maxZoom);
            }
        
            _currentZoom = Mathf.Lerp(_currentZoom, _targetZoom, zoomLerpSpeed * Time.deltaTime);
            Vector3 newPosition = transform.position;
            newPosition.y = _currentZoom;
            transform.position = newPosition;

            // 3) Middle mouse click + drag to pan
            if (Input.GetMouseButton(2))
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
            

                Vector3 panMovement = new Vector3(-mouseX, -mouseY, 0) * (panSpeed * Time.deltaTime);
                transform.Translate(panMovement, Space.Self);
            }
        }
    }
}
