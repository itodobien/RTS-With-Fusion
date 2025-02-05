using UnityEngine;
using UnityUtils;

namespace Units
{
    public class MouseWorldPosition : MonoBehaviour
    {
        /*[SerializeField] private LayerMask mousePlaneLayerMask;*/

        private void Update()
        {
            transform.position = GetMouseWorldPosition();
        }
        public static Vector3 GetMouseWorldPosition()
        {
            return RaycastUtility.GetMouseWorldPosition();
        }
    }
}