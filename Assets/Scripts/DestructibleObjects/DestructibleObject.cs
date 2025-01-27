using Fusion;
using Grid;
using Pathfinding;
using Units;
using UnityEngine;

namespace DestructibleObjects
{
    public class DestructibleObject : NetworkBehaviour
    {
        [Networked] public NetworkBool IsDestroyed { get; set; }
        private GridPosition _gridPosition;
        private GraphUpdateScene _graphUpdateScene;

        [SerializeField] private int maxHealth = 100;
        [Networked] private int CurrentHealth { get; set; }

        public override void Spawned()
        {
            _graphUpdateScene = GetComponent<GraphUpdateScene>();
            
            if (_graphUpdateScene == null) {
                Debug.LogError("GraphUpdateScene component missing on DestructibleObject!");
            }
            if (!IsDestroyed)
            {
                CurrentHealth = maxHealth;
                _gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
                var gridObject = LevelGrid.Instance.GetGridSystem()?.GetGridObject(_gridPosition);
                if (gridObject != null)
                {
                    gridObject.AddDestructibleObject(this);
                    
                }
                else
                {
                    Debug.LogError($"Failed to add DestructibleObject to grid at position {_gridPosition}");
                }
                _graphUpdateScene = GetComponent<GraphUpdateScene>();
            }
            else
            {
                DestroyImmediate(gameObject);
                Debug.Log("Object destroyed in Spawned Method else statement");
            }
        }

        public void Damage(int damageAmount)
        {
            if (HasStateAuthority && !IsDestroyed)
            {
                CurrentHealth -= damageAmount;
                if (CurrentHealth <= 0)
                {
                    IsDestroyed = true;
                    RPC_DestroyObject();
                    Debug.Log("Object destroyed in Damage Method");
                }
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_DestroyObject()
        {
            if (LevelGrid.Instance != null && LevelGrid.Instance.GetGridSystem() != null)
            {
                var gridObject = LevelGrid.Instance.GetGridSystem().GetGridObject(_gridPosition);
                if (gridObject != null) gridObject.RemoveDestructibleObject(this);
            }

            var collider = GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
            if (_graphUpdateScene != null) _graphUpdateScene.enabled = false;

            RescanWalkableArea();
            RPC_NotifyUnitsToRecalculatePaths();
            Runner.Despawn(Object);
            Debug.Log("Object destroyed in RPC_DestroyObject");
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_NotifyUnitsToRecalculatePaths()
        {
            ForceUnitsRecalculatePath();
            Debug.Log("Notifying units to recalculate paths");
        }

        private void RescanWalkableArea()
        {
            AstarPath.active.Scan();
            Debug.Log("Rescanning walkable area");
        }

        private void ForceUnitsRecalculatePath()
        {
            Debug.Log("Rescanning walkable area in ForceUnitsRecalculatePath");
            var allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
            foreach (var unit in allUnits)
            {
                if (unit != null && unit.Object != null && unit.Object.IsInSimulation)
                {
                    unit.ForceRecalculatePath();
                    Debug.Log("Forcing unit to recalculate path");
                }
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        public bool CanBeDamaged() => !IsDestroyed && CurrentHealth > 0;
        

        public float GetHealthPercentage() => (float)CurrentHealth / maxHealth;
        
    }
}
