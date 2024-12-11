using Fusion;
using UnityEngine;

namespace Unit_Activities
{
    public class UnitActionSystem : NetworkBehaviour
    {
        [SerializeField] private Unit selectedUnit;
        [SerializeField] private LayerMask unitLayerMask;

        private void Update()
        {
            if (TryHandleUnitSelection()) return;

            if (Input.GetMouseButtonDown(1) && selectedUnit != null) // Right mouse button
            {
                Vector3 targetPosition = MouseWorldPosition.GetMouseWorldPosition();
                selectedUnit.RPC_SetTargetPosition(targetPosition);
            }
        }

        public bool TryHandleUnitSelection()
        {
            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, unitLayerMask))
                {
                    if (raycastHit.transform.TryGetComponent(out Unit unit))
                    {
                        selectedUnit = unit;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}