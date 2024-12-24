using Fusion;
using Units;
using UnityEngine;

namespace UI
{
    public class UnitSelectedVisual : MonoBehaviour
    {
        [SerializeField] private Unit unit;
    
        private MeshRenderer _meshRenderer;

        private void Awake()
        {
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
            
            if (_meshRenderer == null)
            {
                _meshRenderer = GetComponent<MeshRenderer>();   
            }
        }

        private void Start()
        {
            if (UnitSelectionManager.Instance != null)
            {
                UnitSelectionManager.Instance.OnSelectedUnitsChanged += UnitSelectionManager_OnSelectedUnitsChanged;
                UpdateVisual();
            }
        }

        private void OnDestroy()
        {
            if (UnitSelectionManager.Instance != null)
            {
                UnitSelectionManager.Instance.OnSelectedUnitsChanged -= UnitSelectionManager_OnSelectedUnitsChanged;
            }
        }
        private void UnitSelectionManager_OnSelectedUnitsChanged(object sender, System.EventArgs e)
        {
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (_meshRenderer == null || UnitSelectionManager.Instance == null) return;
            
            _meshRenderer.enabled = UnitSelectionManager.Instance.GetSelectedUnits().Contains(unit);
            
        }
    }
}