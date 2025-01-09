using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform _cameraTransform;
    [SerializeField] private bool _invert;
    
    private void Awake()
    {
        _cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (_invert)
        {
            Vector3 dirToCamera = _cameraTransform.position - transform.position.normalized;
            transform.LookAt(transform.position + (dirToCamera *-1));
        }
        transform.LookAt(_cameraTransform);
    }
}
