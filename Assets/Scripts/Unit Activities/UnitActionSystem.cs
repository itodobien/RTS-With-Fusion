using Fusion;
using Unity.VisualScripting;
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
            if (HasStateAuthority)
            {
                if (GetInput(out NetworkInputData data))
                {
                    if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
                    {
                        var selectedUnits = UnitSelectionManager.Instance?.GetSelectedUnits();

                        foreach (var unit in selectedUnits)
                        {
                            if (unit.OwnerPlayerRef == Runner.LocalPlayer)
                            {
                                unit.SetTargetPositionLocal(data.targetPosition);
                            }
                        }
                    }
                }
            }
        }
    }
}