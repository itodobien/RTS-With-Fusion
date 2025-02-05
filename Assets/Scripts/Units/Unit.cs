using System.Collections;
using Actions;
using Fusion;
using Grid;
using UI;
using UnityEngine;

namespace Units
{
    public class Unit : NetworkBehaviour
    {
        [SerializeField] private Transform aimTransform;
        [SerializeField] private float deathForce = 1f;
        [SerializeField] private float delayTimer = .2f;
        [SerializeField] private Transform originalRootBone;

        [Networked] public bool IsBusy { get; private set; }
        [Networked] public PlayerRef OwnerPlayerRef { get; set; }
        [Networked] private int TeamID { get; set; }
        [Networked] private int PrefabIndex { get; set; }
        [Networked] private bool IsDead { get; set; }

        private GridPosition _gridPosition;
        private BaseAction[] _baseActionsArray;
        private HealthSystem _healthSystem;


        private void Awake()
        {
            _baseActionsArray = GetComponents<BaseAction>();
            _healthSystem = GetComponent<HealthSystem>();
        }

        public override void Spawned()
        {
            _gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
            LevelGrid.Instance.AddUnitAtGridPosition(_gridPosition, this);
            _healthSystem.OnDeath += HealthSystem_OnDead;
        }

        private void Update()
        {
            GridPosition newGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
            if (newGridPosition != _gridPosition)
            {
                LevelGrid.Instance.UnitMovedGridPosition(this, _gridPosition, newGridPosition);
                _gridPosition = newGridPosition;
            }
        }


        public void SetPrefabIndex(int index)
        {
            if (HasStateAuthority)
            {
                PrefabIndex = index;
            }
        }

        public int GetPrefabIndex()
        {
            return PrefabIndex;
        }

        public Vector3 GetAimPosition()
        {
            return aimTransform.position;
        }

        public void SetIsBusy(bool isBusy)
        {
            if (HasStateAuthority)
            {
                IsBusy = isBusy;
            }
        }

        public void SetTeamID(int newTeamID)
        {
            if (Object.HasStateAuthority)
            {
                TeamID = newTeamID;
                Debug.Log($"[Unit] Setting TeamID={newTeamID} for {Object.Id}");
            }
        }

        public int GetTeamID()
        {
            return TeamID;
        }

        public void Damage(int damageAmount)
        {
            _healthSystem.TakeDamage(damageAmount);
        }

        private void HealthSystem_OnDead(object sender, System.EventArgs e)
        {
            if (HasStateAuthority && !IsDead)
            {
                IsDead = true;
                RPC_HandleUnitDeath();
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_HandleUnitDeath()
        {
            RPC_ForceDeselectUnit(Object.Id, OwnerPlayerRef);
            UnitSelectionManager.Instance.ForceDeselectUnit(this);
            UnitSelectionManager.Instance.CleanupDestroyedUnits();
            LevelGrid.Instance.RemoveUnitAtGridPosition(_gridPosition, this);
            RPC_SpawnLocalRagdoll(transform.position, transform.rotation, PrefabIndex);

            StartCoroutine(DestroyAfterDelay(delayTimer));
        }

        private IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (HasStateAuthority)
            {
                Runner.Despawn(Object);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ForceDeselectUnit(NetworkId deadUnitId, PlayerRef ownerPlayerRef)
        {
            var foundUnit = Runner.FindObject(deadUnitId)?.GetComponent<Unit>();
            if (foundUnit != null)
            {
                UnitSelectionManager.Instance.ForceDeselectUnit(foundUnit);
            }

            var currentlySelected = UnitActionSystem.Instance.GetSelectedUnitForPlayer(ownerPlayerRef);
            if (currentlySelected != null && currentlySelected.Object != null && 
                currentlySelected.Object.Id == deadUnitId)
            {
                UnitActionSystem.Instance.SetSelectedUnitForPlayer(ownerPlayerRef, null);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SpawnLocalRagdoll(Vector3 position, Quaternion rotation, int prefabIndex)
        {
            var unitData = BasicSpawner.Instance.unitDatabase.unitDataList[prefabIndex]; // Using parameter
            GameObject ragdoll = Instantiate(unitData.ragdollPrefab, position, rotation); // Using parameters
            UnitRagdoll unitRagdoll = ragdoll.GetComponent<UnitRagdoll>();

            if (unitRagdoll != null)
            {
                unitRagdoll.Setup(originalRootBone);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (_gridPosition.IsValid())
            {
                LevelGrid.Instance.RemoveUnitAtGridPosition(_gridPosition, this);
            }

            base.Despawned(runner, hasState);
        }

        public T GetAction<T>() where T : BaseAction
        {
            foreach (BaseAction baseAction in _baseActionsArray)
            {
                if (baseAction is T action)
                {
                    return action;
                }
            }
            return null;
        }

        public void ForceRecalculatePath()
        {
            if (TryGetComponent<MoveAction>(out var moveAction))
            {
                moveAction.ResetPath();
                Debug.Log("Forcing unit to recalculate path in Unit.cs");
            }
        }

        public GridPosition GetGridPosition() => _gridPosition;
        public BaseAction[] GetBaseActionArray() => _baseActionsArray;
        public Vector3 GetWorldPosition() => transform.position;
    }
}