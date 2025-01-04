using Fusion;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    
    [SerializeField] private float speed = 40f;
    [SerializeField] private float stopDistance = 0.1f;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private ParticleSystem bulletImpactPrefab;
    
    private Vector3 _direction;
    private Vector3 _targetPosition;

    public void ShootAtTarget(Vector3 shootDirection, Vector3 targetPosition)
    {
        _direction = shootDirection;
        _targetPosition = targetPosition;
    }
    
    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            float distanceBeforeShooting = Vector3.Distance(transform.position, _targetPosition);
            
            transform.position += _direction * speed * Runner.DeltaTime;
            
            float distanceAfterShooting = Vector3.Distance(transform.position, _targetPosition);
            if (distanceBeforeShooting < distanceAfterShooting)
            {
                transform.position = _targetPosition;
                trailRenderer.transform.parent = null;
                bulletImpactPrefab.transform.parent = null;
                bulletImpactPrefab.Play();
                
                
                Runner.Despawn(Object);
            }
        }
    }
}