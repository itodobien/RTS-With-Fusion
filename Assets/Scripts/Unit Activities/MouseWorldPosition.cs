using UnityEngine;

namespace Unit_Activities
{
    public class MouseWorldPosition : MonoBehaviour
    {
        [SerializeField] private LayerMask mousePlaneLayerMask;
        private static MouseWorldPosition _instance;
        private void Awake()
        {
            _instance = this;
        }

        void Update()
        {
            transform.position = GetMouseWorldPosition();
        }
        public static Vector3 GetMouseWorldPosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, _instance.mousePlaneLayerMask);
            return raycastHit.point;
        }
    }
}