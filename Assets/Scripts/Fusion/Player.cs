using Unit_Activities;
using UnityEngine;

namespace Fusion
{
    public class Player : NetworkBehaviour
    {
        private NetworkCharacterController _characterController;
        private Vector3 _forward;

        [SerializeField] private NetworkPrefabRef _prefabUnit;
        [SerializeField] private float spawnDelay = 0.5f;
        [SerializeField] private Animator playerAnimator;
        [Networked] private TickTimer Delay { get; set; }

        private void Awake()
        {
            _characterController = GetComponent<NetworkCharacterController>();
            _forward = transform.forward;
        }
        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData data))
            {
                data.direction.Normalize();
                _characterController.Move(5 * data.direction * Runner.DeltaTime);

                if (data.direction.sqrMagnitude > 0)
                {
                    _forward = data.direction;
                    playerAnimator.SetBool("IsWalking", true);
                }
                else
                {
                    playerAnimator.SetBool("IsWalking", false);
                }

                if (HasStateAuthority && Delay.ExpiredOrNotRunning(Runner))
                {
                    if (data.buttons.IsSet(NetworkInputData.SPAWNUNIT)) 
                    {
                        Delay = TickTimer.CreateFromSeconds(Runner, spawnDelay);

                        if (data.spawnPosition == Vector3.zero)
                        {
                            Debug.LogWarning("Spawn position is Vector3.zero, using fallback position.");
                        }

                        Vector3 spawnPos = data.spawnPosition != Vector3.zero
                            ? data.spawnPosition
                            : transform.position + _forward;

                        Runner.Spawn(
                            _prefabUnit,
                            spawnPos,
                            Quaternion.LookRotation(_forward),
                            Object.InputAuthority,
                            (runner, o) =>
                            {
                                o.GetComponent<Unit>();
                            }
                        );
                    }
                }
            }
        }
    }
}