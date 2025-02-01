using System.Collections;
using Fusion;
using Pathfinding;
using UnityEngine;

namespace DestructibleObjects
{
    public class FracturedExplosion : NetworkBehaviour
    {
        [SerializeField] private float explosionForce = 500f;
        [SerializeField] private float explosionRadius = 5f;
        [SerializeField] private float upwardModifier = 0.5f;
        [SerializeField] private Vector3 explosionOffset = Vector3.zero;
        [SerializeField] private float debrisLifetime = 5f;
        [SerializeField] private float graphUpdateDelay = 0.2f; // delay in seconds

        public override void Spawned()
        {
            ExplodeDebris();
            StartCoroutine(DelayedUpdateGraph());
        }
        public void InitializeExplosion()
        {
            ExplodeDebris();
            StartCoroutine(DelayedUpdateGraph());
        }

        private void ExplodeDebris()
        {
            Vector3 explosionPosition = transform.position + explosionOffset;
            int childCount = transform.childCount;
            Transform[] pieces = new Transform[childCount];
            for (int i = 0; i < childCount; i++)
            {
                pieces[i] = transform.GetChild(i);
            }
        
            foreach (Transform piece in pieces)
            {
                if (piece == null) continue;
            
                Rigidbody rb = piece.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    piece.parent = null;
                    rb.isKinematic = false;
                    rb.AddExplosionForce(explosionForce, explosionPosition, explosionRadius, upwardModifier, ForceMode.Impulse);
                    Destroy(piece.gameObject, debrisLifetime);
                }
            }
            Destroy(gameObject, debrisLifetime);
        }

        private IEnumerator DelayedUpdateGraph()
        {
            yield return new WaitForSeconds(graphUpdateDelay);
            UpdateGraph();
        }

        private void UpdateGraph()
        {
            if (AstarPath.active == null) return;

            GraphUpdateScene gus = GetComponent<GraphUpdateScene>();
            if (gus != null)
            {
                gus.Apply();
            }
            else
            {
                Bounds bounds = new Bounds(transform.position, Vector3.one * 5f); 
                GraphUpdateObject guo = new GraphUpdateObject(bounds);
                AstarPath.active.UpdateGraphs(guo);
            }
        }
    }
}
