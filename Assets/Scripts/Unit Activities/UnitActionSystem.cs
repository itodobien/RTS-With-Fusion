/*using Fusion;
using UnityEngine;

namespace Unit_Activities
{
    public class UnitActionSystem : NetworkBehaviour
    {
        public static UnitActionSystem Instance { get; private set; }
        public override void Spawned()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("Multiple UAS instances detected. Destroying the new one.");
                Runner.Despawn(Object);
            }
            Instance = this;
        }
        public override void FixedUpdateNetwork()
        {
            if (Runner.IsServer)
            {
                if (GetInput(out NetworkInputData data))
                {
                    Debug.Log($"UnitActionSystem processing input. SELECT_UNIT: {data.buttons.IsSet(NetworkInputData.SELECT_UNIT)}, SelectedUnitId: {data.selectedUnitId}");
                    
                    if (data.buttons.IsSet(NetworkInputData.SELECT_UNIT))
                    {
                        NetworkObject unitObject = Runner.FindObject(data.selectedUnitId);
                        if (unitObject != null && unitObject.TryGetComponent(out Unit unit))
                        {
                            unit.SetSelected(data.isSelected);
                            Debug.Log($"Unit {unit.Object.Id} selection state changed to: {data.isSelected}");
                        }
                    }
                    else if (data.selectedUnitId != default)
                    {
                        NetworkObject unitObject = Runner.FindObject(data.selectedUnitId);
                        if (unitObject != null && unitObject.TryGetComponent(out Unit unit))
                        {
                            unit.SetSelected(false);
                            Debug.Log($"Unit {unit.Object.Id} deselected");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("No input data received in UnitActionSystem");
                }
            }
            else
            {
                Debug.LogWarning("UnitActionSystem does not have state authority");
            }
        }
    }
}*/