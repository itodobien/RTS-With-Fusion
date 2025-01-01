using System;
using System.Collections.Generic;
using Grid;
using Units;
using UnityEngine;

namespace Actions
{
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

                    int attackerTeam = _unit.GetTeamID();
                
                    List<Unit> targetUnits = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);
                
                    bool hasEnemy = false;

                    foreach (Unit targetUnit in targetUnits)
                    {
                        if (targetUnit.GetTeamID() != attackerTeam)
                        {
                            hasEnemy = true;
                            break;
                        }
                    }
                    if (!hasEnemy) continue;
                
                    validGridPositionList.Add(testGridPosition);
                }
            }
            return validGridPositionList;
        }

        public override void TakeAction(GridPosition gridPosition, Action onActionComplete = null)
        {
            throw new NotImplementedException();
        }
    }
}