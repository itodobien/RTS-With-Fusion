using UnityEngine;
using UnityEngine.Serialization;

namespace Unit_Activities
{
    public class UnitSelectedVisual : MonoBehaviour
    {
        [SerializeField] private Unit unit;
    
        private MeshRenderer _meshRenderer;

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }
    }
}
