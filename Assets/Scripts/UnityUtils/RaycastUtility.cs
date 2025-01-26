using UnityEngine;

public static class RaycastUtility 
{
    public static bool TryRaycastFromCamera(Vector3 screenPos, out RaycastHit rayHit, float maxDistance = Mathf.Infinity)
    {
        rayHit = default;

        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("No Camera.main found for raycasting!");
            return false;
        }
        Ray ray = cam.ScreenPointToRay(screenPos);
        return Physics.Raycast(ray, out rayHit, maxDistance);
    }

    public static Vector3 GetMouseWorldPosition(LayerMask layerMask = default, Vector3 fallbackPosition = default)
    {
        if (TryRaycastFromCamera(Input.mousePosition, out RaycastHit hit))
        {
            return hit.point;
        }
        return fallbackPosition;
    }
}