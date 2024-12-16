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
            if (!Object.HasInputAuthority) return;

            if (GetInput(out NetworkInputData data))
            {
                if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
                {
                    Vector3 targetPosition = data.targetPosition;
                    var selectedUnits = UnitSelectionManager.Instance?.GetSelectedUnits();

                    if (selectedUnits != null && selectedUnits.Count > 0)
                    {
                        foreach (Unit unit in selectedUnits)
                        {
                            if (unit != null && unit.Object != null && unit.Object.HasInputAuthority)
                            {
                                unit.SetTargetPositionLocal(targetPosition);
                            }
                        }
                    }
                }
            }
        }
    }
}