using UnityEngine;

namespace Units
{
    public class UnitRagdoll : MonoBehaviour
    {
        [SerializeField] private Transform ragdollRootBone;
        [SerializeField] private float explosionForce = 300f;
        [SerializeField] private float explosionRange = 10f;

        public void Setup(Transform originalRootBone)
        {
            MatchAllChildTransforms(originalRootBone, ragdollRootBone);
            ApplyForceToRagdoll(ragdollRootBone, explosionForce, transform.position, explosionRange);
        }

        private void MatchAllChildTransforms(Transform root, Transform clone)
        {
            foreach (Transform child in root)
            {
                Transform cloneChild = clone.Find(child.name);
                if (cloneChild != null)
                {
                    cloneChild.position = child.position;
                    cloneChild.rotation = child.rotation;
                
                    MatchAllChildTransforms(child, cloneChild);
                }
            }
        }

        private void ApplyForceToRagdoll(Transform root, float explosionForce, Vector3 explosionPosition, float explosionRange)
        {
            foreach (Transform child in root)
            {
                if (child.TryGetComponent<Rigidbody>(out Rigidbody childRigidbody))
                {
                    childRigidbody.AddExplosionForce(explosionForce, explosionPosition, explosionRange);
                }
                ApplyForceToRagdoll(child,explosionForce, explosionPosition, explosionRange);
            }
        }
    }
}
