using Fusion;
using UnityEngine;

public class UnitActionSystem : NetworkBehaviour
{
    [Networked] private NetworkBool IsUnitSelected { get; set; }
    [Networked] private NetworkId SelectedUnitId { get; set; }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            if (!data.spawnUnit && data.mousePosition != Vector3.zero)
            {
                HandleUnitAction(data.mousePosition);
            }
        }
    }

    private void HandleUnitAction(Vector3 targetPosition)
    {
        if (IsUnitSelected)
        {
            // Use FindObject instead of TryGetNetworkedObject
            NetworkObject unitObject = Runner.FindObject(SelectedUnitId);
            if (unitObject != null)
            {
                Unit selectedUnit = unitObject.GetComponent<Unit>();
                selectedUnit.RPC_SetTargetPosition(targetPosition);
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SelectUnit(NetworkId unitId)
    {
        SelectedUnitId = unitId;
        IsUnitSelected = true;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_DeselectUnit()
    {
        IsUnitSelected = false;
    }
}