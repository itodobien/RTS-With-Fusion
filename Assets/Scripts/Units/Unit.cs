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
        [SerializeField] private float deathForce = 10f;
        [SerializeField] private float delayTimer = .2f;
        
        [Networked] public bool IsBusy { get; set; }
        [Networked] public PlayerRef OwnerPlayerRef { get; set; }
        [Networked] private int TeamID { get; set; }
        [Networked] private int PrefabIndex { get; set; }
        [Networked] private bool IsDead { get; set; }
        
        private GridPosition _gridPosition;
        private BaseAction[] _baseActionsArray;
        private MoveAction _moveAction;
        private SpinAction _spinAction;
        private ShootAction _shootAction;
        private HealthSystem _healthSystem;

        private void Awake()
        {
            _baseActionsArray = GetComponents<BaseAction>();
            _moveAction = GetComponent<MoveAction>();
            _spinAction = GetComponent<SpinAction>();
            _shootAction = GetComponent<ShootAction>();
            _healthSystem = GetComponent<HealthSystem>();
        }

        private void Start()
        {
            _gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
            LevelGrid.Instance.AddUnitAtGridPosition(_gridPosition, this);

            _healthSystem.onDeath += HealthSystem_OnDead;
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
            if (HasStateAuthority)
            {
                TeamID = newTeamID;
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
            if (HasStateAuthority)
            {
                Debug.Log($"Handling death for unit {gameObject.name}");
                
                RPC_ForceDeselectUnit(Object.Id, OwnerPlayerRef);
                UnitSelectionManager.Instance.ForceDeselectUnit(this);
                UnitSelectionManager.Instance.CleanupDestroyedUnits();
                LevelGrid.Instance.RemoveUnitAtGridPosition(_gridPosition, this);

                var unitData = BasicSpawner.Instance.unitDatabase.unitDataList[PrefabIndex];
                GameObject ragdoll = Instantiate(unitData.ragdollPrefab, transform.position, transform.rotation);
                
                Rigidbody[] rigidbodies = ragdoll.GetComponentsInChildren<Rigidbody>();
                foreach (Rigidbody rb in rigidbodies)
                {
                    rb.AddForce(Vector3.down * deathForce, ForceMode.Impulse);
                }
                StartCoroutine(DestroyAfterDelay(delayTimer));
            }
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
            else
            {
                UnitSelectionManager.Instance.ForceDeselectUnitById(deadUnitId);
            }

            var currentlySelected = UnitActionSystem.Instance.GetSelectedUnitForPlayer(ownerPlayerRef);
            if (currentlySelected != null && currentlySelected.Object != null)
            {
                if (currentlySelected.Object.Id == deadUnitId)
                {
                    UnitActionSystem.Instance.SetSelectedUnitForPlayer(ownerPlayerRef, null);
                }
            }
            Debug.Log($"[RPC_ForceDeselectUnit] Removing Unit: {deadUnitId}");

        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SpawnLocalRagdoll(Vector3 position, Quaternion rotation, int prefabIndex)
        {
            var data = BasicSpawner.Instance.unitDatabase.unitDataList[prefabIndex];
            var ragdollPrefab = data.ragdollPrefab;
            Instantiate(ragdollPrefab, position, rotation);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (_gridPosition != null)
            {
                LevelGrid.Instance.RemoveUnitAtGridPosition(_gridPosition, this);
            }
            base.Despawned(runner, hasState);
        }
        
        
        
        public MoveAction GetMoveAction() => _moveAction;
        public SpinAction GetSpinAction() => _spinAction;

        public ShootAction GetShootAction() => _shootAction;
        public GridPosition GetGridPosition() => _gridPosition;
        public BaseAction[] GetBaseActionArray() => _baseActionsArray;
        
        public Vector3 GetWorldPosition() => transform.position;
    }
}