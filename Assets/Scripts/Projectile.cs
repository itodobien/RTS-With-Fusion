using Fusion;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    
    [SerializeField] private float speed = 20f;
    private Vector3 _direction;

    public void ShootAtTarget(Vector3 shootDirection)
    {
        _direction = shootDirection;
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            transform.position += _direction * speed * Runner.DeltaTime;
        }
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}