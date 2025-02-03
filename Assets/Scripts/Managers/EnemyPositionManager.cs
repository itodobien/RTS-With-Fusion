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

        [Networked]
        private NetworkString<_128> SerializedEnemyPositions { get; set; }

        private List<GridPosition> _enemyPositions = new List<GridPosition>();

        private string _lastSerialized;

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

        public override void Spawned()
        {
            if (Object.HasStateAuthority)
            {
                UpdateEnemyPositions(0);
            }

            _lastSerialized = SerializedEnemyPositions.Value;
            DeserializeEnemyPositions();
        }

        public override void FixedUpdateNetwork()
        {
            var currentValue = SerializedEnemyPositions.Value;
            if (_lastSerialized != currentValue)
            {
                _lastSerialized = currentValue;
                DeserializeEnemyPositions();
            }
        }

        public void UpdateEnemyPositions(int localPlayerTeamID)
        {
            if (!Object.HasStateAuthority) return;

            _enemyPositions.Clear();
            var allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
            foreach (var unit in allUnits)
            {
                if (unit.GetTeamID() != localPlayerTeamID)
                {
                    _enemyPositions.Add(unit.GetGridPosition());
                }
            }

            SerializeEnemyPositions();
        }

        private void SerializeEnemyPositions()
        {
            var posStrings = new List<string>();
            foreach (GridPosition gridPos in _enemyPositions)
            {
                posStrings.Add($"{gridPos.x},{gridPos.z}");
            }
            SerializedEnemyPositions = string.Join("|", posStrings);
        }

        private void DeserializeEnemyPositions()
        {
            string serialized = SerializedEnemyPositions.Value;
            _enemyPositions.Clear();

            if (string.IsNullOrEmpty(serialized))
                return;

            string[] positions = serialized.Split('|');
            foreach (string pos in positions)
            {
                string[] coords = pos.Split(',');
                if (coords.Length == 2 &&
                    int.TryParse(coords[0], out int x) &&
                    int.TryParse(coords[1], out int z))
                {
                    _enemyPositions.Add(new GridPosition(x, z));
                }
            }
        }

        public List<GridPosition> GetEnemyPositions()
        {
            if (!Object.HasStateAuthority)
            {
                DeserializeEnemyPositions();
            }
            return _enemyPositions;
        }
    }
}
