using System;
using System.Collections;
using Fusion;
using Grid;
using Units;
using UnityEngine;
using UnityEngine.VFX;
using MoreMountains.Feedbacks;

namespace Projectiles
{
    public class GrenadeProjectile : NetworkBehaviour
    {
        public event EventHandler OnGrenadeExplode;
        
        [SerializeField] private float speed = 20f;
        [SerializeField] private float stopDistance = 0.1f;
        [SerializeField] private VisualEffect explosionVisualEffect;
        
        private bool _hasExploded;
        private Vector3 _direction;
        private Vector3 _targetPosition;
        

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) return;
            if (_hasExploded) return;

            float distanceBefore = Vector3.Distance(transform.position, _targetPosition);
            transform.position += _direction * speed * Runner.DeltaTime;
            float distanceAfter = Vector3.Distance(transform.position, _targetPosition);

            if (distanceBefore < distanceAfter || distanceAfter <= stopDistance)
            {
                transform.position = _targetPosition;
                ExplodeVisuals();
            }
        }
        private void ExplodeVisuals()
        {
            _hasExploded = true;
            
            OnGrenadeExplode?.Invoke(this, EventArgs.Empty);
            
            RPC_PlayExplosionEffect(transform.position);
            StartCoroutine(DespawnAfterDelay(0.3f));
            
        }
        public void ThrowGrenade(Vector3 throwDirection, Vector3 targetPosition)
        {
            _direction = throwDirection;
            _targetPosition = targetPosition;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayExplosionEffect(Vector3 impactPosition)
        {
            if (explosionVisualEffect != null)
            {
                var explosionInstance = Instantiate(explosionVisualEffect, impactPosition, Quaternion.identity);
                explosionInstance.Play();
            }
        }
        private IEnumerator DespawnAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Runner.Despawn(Object);
        }
    }
}
