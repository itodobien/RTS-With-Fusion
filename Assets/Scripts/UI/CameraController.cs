using UnityEngine;
using Unity.Cinemachine;

namespace UI
{
    public class CameraController : MonoBehaviour // attached to Empty game Object in scene
    {
        [Header("Movement Settings")]
        public float moveSpeed = 10f;     
        public float rotationsSpeed = 100;
        public float zoomSpeed = 10f;
        
        [SerializeField] private CinemachineCamera cmCamera;


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

            Vector3 rotationVector = Vector3.zero;

            if (Input.GetKey(KeyCode.Q))
            {
                rotationVector.y = +1f;
            }
            if (Input.GetKey(KeyCode.E))
            {
                rotationVector.y = -1f;
            }
            transform.eulerAngles += rotationVector * (rotationsSpeed * Time.deltaTime);
            
            var zoom = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * Time.deltaTime;
            var offset = cmCamera.GetComponent<CinemachineFollow>().FollowOffset;
            offset.y -= zoom;
            offset.z += zoom;
            offset.y = Mathf.Clamp(offset.y, 3, 11);
            offset.z = Mathf.Clamp(offset.z, -14, -6);
            cmCamera.GetComponent<CinemachineFollow>().FollowOffset = offset;

        }
    }
}
