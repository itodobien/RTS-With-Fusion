using Fusion;
using UnityEngine;

namespace Unit_Activities
{
    public class UnitActionSystem : NetworkBehaviour
    {
        public static UnitActionSystem Instance { get; private set; }

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
        // FixedUpdateNetwork remains as the network tick method
public override void FixedUpdateNetwork()
{
    // Exit if this player doesn't own this UnitActionSystem instance
    if (!Object.HasInputAuthority) 
    {
        return;
    }

    // Check for player inputs
    if (GetInput(out NetworkInputData data))
    {
        // Handle right-click for unit movement actions
        if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1)) 
        {
            Vector3 targetPosition = data.targetPosition;

            // Get all currently selected units
            var selectedUnits = UnitSelectionManager.Instance?.GetSelectedUnits();

            Debug.Log($"UnitActionSystem: Target position: {targetPosition}, Selected units: {selectedUnits?.Count ?? 0}");

            if (selectedUnits != null && selectedUnits.Count > 0)
            {
                // Process movement requests for each selected unit
                foreach (Unit unit in selectedUnits)
                {
                    if (unit != null && unit.Object != null) // Valid Unit network object check
                    {
                        if (unit.Object.HasInputAuthority) 
                        {
                            // Issue the move command to the server (RPC_TargetPosition defined in Unit.cs)
                            unit.RPC_SetTargetPosition(targetPosition);
                            Debug.Log($"UnitActionSystem: Sending move command to unit: {unit.name}");
                        }
                        else
                        {
                            // Warn about units without input authority
                            Debug.LogWarning($"UnitActionSystem: Cannot move unit {unit.name}. Local player does not have input authority.");
                        }
                    }
                    else
                    {
                        Debug.LogError("UnitActionSystem: Unit or its NetworkObject is null.");
                    }
                }
            }
        }
    }
}

    }
}