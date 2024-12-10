using Unit_Activities;
using UnityEngine;

namespace Fusion
{
    public class Player : NetworkBehaviour
    {
        private NetworkCharacterController _characterController;
        [SerializeField] private Unit _prefabUnit;
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
                _characterController.Move(5*data.direction*Runner.DeltaTime);
            }

            if (data.spawnUnit && delay.ExpiredOrNotRunning(Runner))
            {
                delay = TickTimer.CreateFromSeconds(Runner, spawnDelay);
                Runner.Spawn(_prefabUnit, MouseWorldPosition.GetMouseWorldPosition());
            }
        }
    }
}