using UnityEngine;

namespace Unit_Activities
{
    public class MouseWorldPosition : MonoBehaviour
    {
        [SerializeField] private LayerMask mousePlaneLayerMask;

        private void Update()
        {
            transform.position = GetMouseWorldPosition();
        }
        public static Vector3 GetMouseWorldPosition()
        {
            Camera camera = Camera.main;
            
            if (camera == null)
            {
                Debug.LogWarning("No main camera found for MouseWorldPosition.");
                return Vector3.zero;
            }

            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                return hit.point;
            }
            else
            {
                Debug.LogWarning("MouseWorldPosition raycast failed. Returning (0,0,0).");
                return Vector3.zero;
            }
            
        }
    }
}