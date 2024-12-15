using Unit_Activities;
using UnityEngine;

namespace Fusion
{
    public class Player : NetworkBehaviour
    {
        private NetworkCharacterController _characterController;
        [SerializeField] private NetworkPrefabRef _prefabUnit;
        [SerializeField] private float spawnDelay = 0.5f;
    
        [Networked] private TickTimer delay { get; set; }
        
        private void Awake()
        {
            _characterController = GetComponent<NetworkCharacterController>();
        }
        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData data))
            {
                data.direction.Normalize();
                _characterController.Move(5 * data.direction * Runner.DeltaTime);

                if (data.spawnUnit && delay.ExpiredOrNotRunning(Runner))
                {
                    delay = TickTimer.CreateFromSeconds(Runner, spawnDelay);
                    Vector3 spawnPosition = MouseWorldPosition.GetMouseWorldPosition();
        
                    if (Object.HasStateAuthority)
                    {
                        if (Object.HasInputAuthority)
                        {
                            SpawnUnitForPlayer(spawnPosition, Object.InputAuthority);
                        }
                    }
                    else
                    {
                        RPC_RequestUnitSpawn(spawnPosition);
                    }
                }
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_RequestUnitSpawn(Vector3 spawnPosition)
        {
            SpawnUnitForPlayer(spawnPosition, Object.InputAuthority);
        }

        private void SpawnUnitForPlayer(Vector3 spawnPosition, PlayerRef owner)
        {
            NetworkObject unitObject = Runner.Spawn(_prefabUnit, spawnPosition, Quaternion.identity, owner);
    
            if (unitObject.TryGetComponent(out Unit unit))
            {
                unitObject.AssignInputAuthority(owner);

                if (owner == Runner.LocalPlayer)
                {
                    unit.SetTargetPositionLocal(spawnPosition);
                }
                else
                {
                    unit.RPC_SetTargetPosition(spawnPosition);
                }
                Debug.Log($"Unit Spawned: OwnerPlayerRef = {unit.OwnerPlayerRef}, InputAuthority = {unitObject.InputAuthority}");
            }
        }


    }

}