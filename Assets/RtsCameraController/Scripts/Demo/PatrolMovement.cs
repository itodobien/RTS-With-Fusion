using UnityEngine;

namespace RtsCamera
{
    public class PatrolMovement : MonoBehaviour
    {
        [SerializeField]
        private float movementSpeed = 1.0f;
        [SerializeField]
        private float rotateSpeed = 1.0f;

        [SerializeField]
        private Transform[] waypoints;

        private int currentWaypointIndex = 0;

        private void Update()
        {
            transform.position = Vector3.Lerp(transform.position, waypoints[currentWaypointIndex].position, movementSpeed * Time.deltaTime);

            Vector3 dir = waypoints[currentWaypointIndex].position - transform.position;
            float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0.0f, angle, 0.0f), rotateSpeed * Time.deltaTime);

            if (Vector3.SqrMagnitude(transform.position - waypoints[currentWaypointIndex].position) < 1000.0f)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
        }
    }
}