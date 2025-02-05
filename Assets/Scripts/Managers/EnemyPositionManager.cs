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