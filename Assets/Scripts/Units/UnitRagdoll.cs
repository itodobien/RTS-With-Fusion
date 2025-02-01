using System.Collections;
using UnityEngine;

namespace Units
{
    public class UnitRagdoll : MonoBehaviour
    {
        [SerializeField] private Transform ragdollRootBone;
        [SerializeField] private float explosionForce = 300f;
        [SerializeField] private float explosionRange = 10f;
        [SerializeField] private float fadeDuration = 3f;
        [SerializeField] private float timeBeforeFade = 5f;


        public void Setup(Transform originalRootBone)
        {
            MatchAllChildTransforms(originalRootBone, ragdollRootBone);
            ApplyForceToRagdoll(ragdollRootBone, explosionForce, transform.position, explosionRange);
            StartCoroutine(FadeAndDestroy());

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
                if (child.TryGetComponent(out Rigidbody childRigidbody))
                {
                    childRigidbody.AddExplosionForce(explosionForce, explosionPosition, explosionRange);
                }
                ApplyForceToRagdoll(child,explosionForce, explosionPosition, explosionRange);
            }
        }
        private IEnumerator FadeAndDestroy()
        {
            // Wait for a specified time before starting the fade
            yield return new WaitForSeconds(timeBeforeFade);

            // Get all renderers in the ragdoll
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            float elapsedTime = 0f;

            // Store the original colors
            Color[][] originalColors = new Color[renderers.Length][];
            for (int i = 0; i < renderers.Length; i++)
            {
                originalColors[i] = new Color[renderers[i].materials.Length];
                for (int j = 0; j < renderers[i].materials.Length; j++)
                {
                    originalColors[i][j] = renderers[i].materials[j].color;
                }
            }

            while (elapsedTime < fadeDuration)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

                for (int i = 0; i < renderers.Length; i++)
                {
                    for (int j = 0; j < renderers[i].materials.Length; j++)
                    {
                        Color color = originalColors[i][j];
                        color.a = alpha;
                        renderers[i].materials[j].color = color;
                    }
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            for (int i = 0; i < renderers.Length; i++)
            {
                for (int j = 0; j < renderers[i].materials.Length; j++)
                {
                    Color color = originalColors[i][j];
                    color.a = 0f;
                    renderers[i].materials[j].color = color;
                }
            }
            Destroy(gameObject);
        }
    }
}
