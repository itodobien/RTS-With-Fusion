using UnityEngine;

namespace Grid
{
    public class GridSystemVisualSingle : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;

        public void Show()
        {
            meshRenderer.enabled = true;
        }
    
        public void Hide()
        {
            meshRenderer.enabled = false;
        }
        public void SetColor(Color color)
        {
            meshRenderer.material.color = color;
        }
    }
}
