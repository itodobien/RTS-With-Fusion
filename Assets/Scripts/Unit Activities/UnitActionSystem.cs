using System;
using Fusion;
using UnityEngine;

namespace Unit_Activities
{
    public class UnitActionSystem : NetworkBehaviour
    {
        
        public static UnitActionSystem Instance { get; private set; }
        public event EventHandler OnSelectedUnitChanged;
        
        [SerializeField] private Unit selectedUnit;
        [SerializeField] private LayerMask unitLayerMask;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("There is more than one UnitActionSystem in the scene " + transform + " and " + Instance);
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (TryHandleUnitSelection()) return;

            if (Input.GetMouseButtonDown(1) && selectedUnit != null) 
            {
                Vector3 targetPosition = MouseWorldPosition.GetMouseWorldPosition();
                selectedUnit.RPC_SetTargetPosition(targetPosition);
            }
        }

        public bool TryHandleUnitSelection()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, unitLayerMask))
                {
                    if (raycastHit.transform.TryGetComponent(out Unit unit))
                    {
                        SetSelectedUnit(unit);
                        return true;
                    }
                }
                selectedUnit = null;
            }
            return false;
        }

        private void SetSelectedUnit(Unit unit)
        {
            selectedUnit = unit;
            OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);
        }
        public Unit GetSelectedUnit()
        {
            return selectedUnit;
        }
    }
}