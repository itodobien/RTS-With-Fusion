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
            if (HasStateAuthority)
            {
                if (GetInput(out NetworkInputData data))
                {
                    if (data.buttons.IsSet(NetworkInputData.SELECT_UNIT))
                    {
                        NetworkObject unitObject = Runner.FindObject(data.selectedUnitId);
                        if (unitObject != null && unitObject.TryGetComponent(out Unit unit))
                        {
                            unit.SetSelected(true);
                        }
                    }
                    else if (data.selectedUnitId != default)
                    {
                        NetworkObject unitObject = Runner.FindObject(data.selectedUnitId);
                        if (unitObject != null && unitObject.TryGetComponent(out Unit unit))
                        {
                            unit.SetSelected(false);
                        }
                    }
                }
            }
        }
    }
}