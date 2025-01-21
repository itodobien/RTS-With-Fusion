using System.Collections;
using Fusion;
using UnityEngine;

namespace Projectiles
{
    public class AutomaticRifleProjectile : NetworkBehaviour
    {
    
        [SerializeField] private float speed = 40f;
        [SerializeField] private float stopDistance = 0.1f;
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private ParticleSystem bulletImpactPrefab;
        private bool _hasImpacted;
        private Vector3 _direction;
        private Vector3 _targetPosition;

        public void ShootAtTarget(Vector3 shootDirection, Vector3 targetPosition)
        {
            _direction = shootDirection;
            _targetPosition = targetPosition;
        }
    
        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) return;
            if (_hasImpacted) return;
        
            if (Object.HasStateAuthority)
            {
                float distanceBeforeShooting = Vector3.Distance(transform.position, _targetPosition);
                transform.position += _direction * speed * Runner.DeltaTime;
                float distanceAfterShooting = Vector3.Distance(transform.position, _targetPosition);
            
                if (distanceBeforeShooting < distanceAfterShooting || Vector3.Distance(transform.position, _targetPosition) < 0.1f)
                {
                    _hasImpacted = true;
                    transform.position = _targetPosition;

                    if (trailRenderer != null)
                    {
                        trailRenderer.transform.SetParent(null, true);
                        RPC_PlayImpactEffect(transform.position);
                    }
                    StartCoroutine(DespawnAfterDelay(0.2f));
                }
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayImpactEffect(Vector3 impactPosition)
        {
            var impactInstance = Instantiate(bulletImpactPrefab, impactPosition, Quaternion.identity);
            impactInstance.Play();
        }
        private IEnumerator DespawnAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Runner.Despawn(Object);
        }
    }
}