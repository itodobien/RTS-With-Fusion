using System;
using UnityEngine;

public class MouseWorldPosition : MonoBehaviour
{
    [SerializeField] private LayerMask mousePlaneLayerMask;
    private static MouseWorldPosition instance;
    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        transform.position = MouseWorldPosition.GetMouseWorldPosition();
    }
    public static Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, instance.mousePlaneLayerMask);
        return raycastHit.point;
    }
}
