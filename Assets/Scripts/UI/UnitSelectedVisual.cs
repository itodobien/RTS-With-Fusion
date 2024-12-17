using Fusion;
using UnityEngine;

namespace Unit_Activities
{
    public class UnitSelectedVisual : MonoBehaviour
    {
        [SerializeField] private Unit unit;
    
        private MeshRenderer meshRenderer;
        private NetworkRunner _runner;

        private void Awake()
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
            _runner = FindObjectOfType<NetworkRunner>();
        }

        private void Start()
        {
            UnitSelectionManager.Instance.OnSelectedUnitsChanged += UnitSelectionManager_OnSelectedUnitsChanged;
            UpdateVisual();
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
            if (_runner != null)
            {
                meshRenderer.enabled = UnitSelectionManager.Instance.GetSelectedUnits(_runner.LocalPlayer).Contains(unit);
            }
            
        }
    }
}
