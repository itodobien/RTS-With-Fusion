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
        public override void FixedUpdateNetwork()
        {
            if (!Object.HasInputAuthority)
            {
                Debug.Log($"[UnitActionSystem] No input authority. Skipping input processing.");
                return;
            }

            Debug.Log($"[UnitActionSystem] HasStateAuthority: {HasStateAuthority}, InputAuthority: {Object.InputAuthority}");

            if (GetInput(out NetworkInputData data))
            {
                Debug.Log($"[UnitActionSystem] Input received: MOUSEBUTTON1 = {data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1)}, targetPosition = {data.targetPosition}");

                if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
                {
                    Vector3 targetPosition = data.targetPosition;
                    var selectedUnits = UnitSelectionManager.Instance?.GetSelectedUnits();

                    if (selectedUnits != null && selectedUnits.Count > 0)
                    {
                        Debug.Log($"[UnitActionSystem] Right-click detected. targetPosition: {targetPosition}, selectedUnits: {selectedUnits.Count}");

                        foreach (Unit unit in selectedUnits)
                        {
                            if (unit != null && unit.Object != null && unit.Object.HasStateAuthority)
                            {
                                unit.RPC_SetTargetPosition(targetPosition);
                            }
                            else
                            {
                                Debug.LogWarning($"[UnitActionSystem] Unable to set target position for unit. HasStateAuthority: {unit?.Object?.HasStateAuthority}");
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("[UnitActionSystem] Right-click detected, but no units are selected.");
                    }
                }
            }
            else
            {
                Debug.Log("[UnitActionSystem] No input received this frame.");
            }
        }

    }
}