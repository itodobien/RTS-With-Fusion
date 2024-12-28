using Units;
using UnityEngine;

namespace Fusion
{
    public class Player : NetworkBehaviour
    {
        private static readonly int IsWalking = Animator.StringToHash("IsWalking");
        private NetworkCharacterController _characterController;
        private Vector3 _forward;
        
        [SerializeField] private int moveSpeed = 4;

        [SerializeField] private NetworkPrefabRef[] unitPrefabs;
        [SerializeField] private float spawnDelay = 0.5f;
        [SerializeField] private Animator playerAnimator;
        [Networked] private TickTimer Delay { get; set; }
        [Networked] private int UnitPrefabIndex { get; set; }

        private void Awake()
        {
            _characterController = GetComponent<NetworkCharacterController>();
            _forward = transform.forward;
        }

        public void SetUnitPrefabIndex(int index)
        {
            UnitPrefabIndex = index;
        }

        public void SetUnitPrefabs(NetworkPrefabRef[] prefabs)
        {
            unitPrefabs = prefabs;
        }
        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData data))
            {
                data.direction.Normalize();
                _characterController.Move(moveSpeed * data.direction * Runner.DeltaTime);

                if (data.direction.sqrMagnitude > 0)
                {
                    _forward = data.direction;
                    playerAnimator.SetBool(IsWalking, true);
                }
                else
                {
                    playerAnimator.SetBool(IsWalking, false);
                }

                if (HasStateAuthority && Delay.ExpiredOrNotRunning(Runner))
                {
                    if (data.buttons.IsSet(NetworkInputData.SPAWNUNIT)) 
                    {
                        Delay = TickTimer.CreateFromSeconds(Runner, spawnDelay);

                        Vector3 spawnPos = data.spawnPosition != Vector3.zero
                            ? data.spawnPosition
                            : transform.position + _forward;
                        
                        NetworkPrefabRef chosenUnitPrefab = unitPrefabs[UnitPrefabIndex];

                        Runner.Spawn(
                            chosenUnitPrefab,
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