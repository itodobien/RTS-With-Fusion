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
            // Use the `PlayerRef` of the RPC source (the client that called it)
            SpawnUnitForPlayer(spawnPosition, Object.InputAuthority);
        }

        private void SpawnUnitForPlayer(Vector3 spawnPosition, PlayerRef owner)
        {
            // Spawns the unit on the network
            NetworkObject unitObject = Runner.Spawn(_prefabUnit, spawnPosition, Quaternion.identity, owner);
    
            if (unitObject.TryGetComponent(out Unit unit))
            {
                // Assign input authority to the correct player (client or host)
                unitObject.AssignInputAuthority(owner);

                // Assign proper target position
                if (owner == Runner.LocalPlayer)
                {
                    // For local player-right now
                    unit.SetTargetPositionLocal(spawnPosition);
                }
                else
                {
                    // For remote player through an RPC
                    unit.RPC_SetTargetPosition(spawnPosition);
                }

                // Debug to confirm correct ownership
                Debug.Log($"Unit Spawned: OwnerPlayerRef = {unit.OwnerPlayerRef}, InputAuthority = {unitObject.InputAuthority}");
            }
        }
    }
}