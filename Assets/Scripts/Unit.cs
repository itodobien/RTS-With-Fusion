using UnityEngine;

public class Unit : MonoBehaviour
{
    private Vector3 targetPosition;
    private float stopDistance = 0.1f;
    [SerializeField] private float moveSpeed = 4f;
    
    private void Update()
    {
        if (Vector3.Distance(transform.position, targetPosition) > stopDistance)
        {
            MoveUnit(targetPosition);
            Vector3 moveDirection = (targetPosition - transform.position).normalized;
            transform.position += moveDirection * (Time.deltaTime * moveSpeed);
        }

        if (Input.GetMouseButtonDown(0))
        {
            MoveUnit(MouseWorldPosition.GetMouseWorldPosition());
        }
    }

    private void MoveUnit(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }
}
