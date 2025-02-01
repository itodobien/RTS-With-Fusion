using Fusion;
using Grid;
using Pathfinding;
using Units;
using UnityEngine;

namespace DestructibleObjects
{
    [RequireComponent(typeof(NetworkBehaviour))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(GraphUpdateScene))]
    public class DestructibleObject : NetworkBehaviour
    {
        [Networked] public NetworkBool IsDestroyed { get; set; }
        
        [SerializeField] private int maxHealth = 10;
        [Networked] private int CurrentHealth { get; set; }

        [SerializeField] private GameObject fracturedPrefab;
        
        private GridPosition _gridPosition;
        private GraphUpdateScene _graphUpdateScene;

        public override void Spawned()
        {
            _graphUpdateScene = GetComponent<GraphUpdateScene>();
            ValidateGraphUpdateScene();

            if (!IsDestroyed)
            {
                InitializeObject();
            }
            else
            {
                DestroyImmediate(gameObject);
                Debug.Log("Object destroyed in Spawned Method else statement");
            }
        }

        public void Damage(int damageAmount)
        {
            if (!HasStateAuthority || IsDestroyed) return;
            
            CurrentHealth -= damageAmount;
            if (CurrentHealth <= 0)
            {
                IsDestroyed = true;
                RPC_DestroyObject();
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_DestroyObject()
        {
            RemoveFromGrid();
            DisableComponents();
            RescanWalkableArea();
            RPC_NotifyUnitsToRecalculatePaths();

            if (DestructionManager.Instance != null)
            {
                Vector3 destroyPosition = transform.position;
                Quaternion destroyRotation = transform.rotation;
                Debug.Log($"Destroying at {destroyPosition}");
                DestructionManager.Instance.RPC_SpawnFracturedEffect(destroyPosition, destroyRotation);
            }
            else
            {
                Debug.LogError("DestructionManager instance is not available.");
            }

            Runner.Despawn(Object);
        }


        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_NotifyUnitsToRecalculatePaths()
        {
            ForceUnitsRecalculatePath();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }
        
        private void ValidateGraphUpdateScene()
        {
            if (_graphUpdateScene == null)
            {
                Debug.LogError("GraphUpdateScene component missing on DestructibleObject!");
            }
        }
        
        private void InitializeObject()
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
        }
        
        private void RemoveFromGrid()
        {
            if (LevelGrid.Instance?.GetGridSystem() == null) return;

            var gridObject = LevelGrid.Instance.GetGridSystem().GetGridObject(_gridPosition);
            gridObject?.RemoveDestructibleObject(this);
        }
        
        private void DisableComponents()
        {
            var collider = GetComponent<Collider>();
            if (collider != null) collider.enabled = false;

            if (_graphUpdateScene != null) _graphUpdateScene.enabled = false;
        }
        
        private void RescanWalkableArea()
        {
            if (AstarPath.active == null) return;
            if (_graphUpdateScene != null)
            {
                _graphUpdateScene.Apply();
            }
            else
            {
                Bounds bounds = new Bounds(transform.position, Vector3.one * 5f);
                var guo = new GraphUpdateObject(bounds);
                AstarPath.active.UpdateGraphs(guo);
            }
        }

        private void ForceUnitsRecalculatePath()
        {
            var allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
            foreach (var unit in allUnits)
            {
                if (unit?.Object != null && unit.Object.IsInSimulation)
                {
                    unit.ForceRecalculatePath();
                }
            }
        }
    }
}
