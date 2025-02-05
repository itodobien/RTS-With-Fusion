using System.Collections.Generic;
using Fusion;
using Grid;
using Units;
using UnityEngine;

namespace Managers
{
    public class EnemyPositionManager : NetworkBehaviour
    {
        public static EnemyPositionManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        /*public List<GridPosition> GetEnemyPositions()
        {
            var localPlayer = UnitSelectionManager.Instance.GetLocalPlayer();
            if (localPlayer == null) return new List<GridPosition>();

            int localTeamID = localPlayer.GetTeamID();
            List<GridPosition> enemyPositions = new List<GridPosition>();

            var allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
            foreach (Unit unit in allUnits)
            {
                if (unit != null && unit.Object != null && unit.Object.IsValid)
                {
                    int unitTeamID = unit.GetTeamID();
            
                    if (unitTeamID != localTeamID && unit.Object.IsValid)
                    {
                        enemyPositions.Add(unit.GetGridPosition());
                    }
                }
            }
            return enemyPositions;
        }*/
        
        public List<GridPosition> GetEnemyPositionsForTeam(int teamID)
        {
            List<GridPosition> enemyPositions = new List<GridPosition>();
            var allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
            foreach (Unit unit in allUnits)
            {
                if (unit != null && unit.Object != null && unit.Object.IsValid)
                {
                    if (unit.GetTeamID() != teamID)
                    {
                        enemyPositions.Add(unit.GetGridPosition());
                    }
                }
            }
            return enemyPositions;
        }

    }
}