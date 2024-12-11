using System;
using UnityEngine;

namespace Unit_Activities
{
    public class UnitSelectedVisual : MonoBehaviour
    {
        [SerializeField] private Unit unit;
    
        private MeshRenderer _meshRenderer;

        private void Awake()
        {
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        private void Start()
        {
            UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged;
            UpdateVisual();
        }
        
        private void UnitActionSystem_OnSelectedUnitChanged(object sender, EventArgs e)
        {
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (UnitActionSystem.Instance.GetSelectedUnit() == unit)
            {
                _meshRenderer.enabled = true;
            }
            else
            {
                _meshRenderer.enabled = false;
            }
        }
    }
}