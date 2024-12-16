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

                if (data.buttons.IsSet(NetworkInputData.SPAWNUNIT) && delay.ExpiredOrNotRunning(Runner))
                {
                    delay = TickTimer.CreateFromSeconds(Runner, spawnDelay);
                    if (Object.HasStateAuthority)
                    {
                        SpawnUnitForPlayer(data.spawnPosition, Object.InputAuthority);
                    }
                }
            }
        }
        private void SpawnUnitForPlayer(Vector3 spawnPosition, PlayerRef owner)
        {
            NetworkObject unitObject = Runner.Spawn(_prefabUnit, spawnPosition, Quaternion.identity, owner);

            if (unitObject.TryGetComponent(out Unit unit))
            {
                unitObject.AssignInputAuthority(owner);
                unit.OwnerPlayerRef = owner;
                Debug.Log($"Unit Spawned: OwnerPlayerRef = {unit.OwnerPlayerRef}, InputAuthority = {unitObject.InputAuthority}");
            }
        }


    }

}