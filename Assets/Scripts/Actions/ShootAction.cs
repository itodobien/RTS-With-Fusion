using System.Collections.Generic;
using Actions;
using Grid;
using Units;
using UnityEngine;

public class ShootAction : BaseAction
{
    [SerializeField] private int maxShootDistance = 7;
    public override string GetActionName() => "Shoot";
   
    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = _unit.GetGridPosition();

        for (int x = -maxShootDistance; x <= maxShootDistance; x++)
        {
            for (int z = -maxShootDistance; z <= maxShootDistance; z++)
            {
                GridPosition offSetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offSetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) continue;
                if (!LevelGrid.Instance.HasUnitAtGridPosition(testGridPosition)) continue;
                
                List<Unit> targetUnits = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);
                
                foreach (Unit targetUnit in targetUnits)
                {
                    if (targetUnit.IsEnemy(_unit))
                    {
                        Debug.Log($"[ShootAction] {name} (Team: {_unit.GetTeamID()}) " +
                                  $"detected enemy {targetUnit.name} (Team: {targetUnit.GetTeamID()}) " +
                                  $"at grid {testGridPosition}. " +
                                  $"HasStateAuthority={Object.HasStateAuthority}, " +
                                  $"HasInputAuthority={Object.HasInputAuthority}");
                        
                        validGridPositionList.Add(testGridPosition);
                        break; // only add the cell once regardless of how many enemies are in the cell.
                    }
                }
            }
        }
        return validGridPositionList;
    }
}
