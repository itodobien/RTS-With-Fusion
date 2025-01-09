using UnityEngine;

namespace Units
{
    public class UnitRagdoll : MonoBehaviour
    {
        [SerializeField] private Transform ragdollRootBone;

        public void Setup(Transform originalRootBone)
        {
            MatchAllChildTransforms(originalRootBone, ragdollRootBone);
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
    }
}
