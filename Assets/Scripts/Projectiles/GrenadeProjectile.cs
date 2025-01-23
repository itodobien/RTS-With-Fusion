using System;
using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.VFX;

namespace Projectiles
{
    public class GrenadeProjectile : NetworkBehaviour
    {
        public event EventHandler OnGrenadeExplode;

        [SerializeField] private VisualEffect explosionVisualEffect;
        [SerializeField] private float arcHeight = 2f;
        [SerializeField] private float flightDuration = 1.2f;

        private bool _hasExploded;
        private float _elapsedTime;
        private Vector3 _startPosition;
        private Vector3 _endPosition;

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) return;
            if (_hasExploded) return;

            _elapsedTime += Runner.DeltaTime;
            float t = Mathf.Clamp01(_elapsedTime / flightDuration);

            Vector3 horizontalPosition = Vector3.Lerp(_startPosition, _endPosition, t);
            float heightOffset = Mathf.Sin(Mathf.PI * t) * arcHeight;
            Vector3 curvedPosition = new Vector3(horizontalPosition.x, horizontalPosition.y + heightOffset,
                horizontalPosition.z);
            transform.position = curvedPosition;

            if (t >= 1f)
            {
                ExplodeVisuals();
            }
        }

        private void ExplodeVisuals()
        {
            if (_hasExploded) return;
            _hasExploded = true;
            Debug.Log("ExplodeVisuals called");

            OnGrenadeExplode?.Invoke(this, EventArgs.Empty);

            RPC_PlayExplosionEffect(transform.position);
            StartCoroutine(DespawnAfterDelay(0.3f));
        }

        public void ThrowGrenade(Vector3 throwDirection, Vector3 targetPosition)
        {
            _startPosition = transform.position;
            _endPosition = targetPosition;
            _elapsedTime = 0f;
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