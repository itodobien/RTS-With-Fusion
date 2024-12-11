using Fusion;
using UnityEngine;

namespace Unit_Activities
{
    public class Unit : NetworkBehaviour
    {
        [Networked] private Vector3 TargetPosition { get; set; }
        private NetworkCharacterController _unitCharacterController;
        
        [SerializeField] private float stopDistance = 0.1f;
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float rotateSpeed = 10f;
        
        private void Awake()
        {
            _unitCharacterController = GetComponent<NetworkCharacterController>();
        }

        public override void Spawned()
        {
            TargetPosition = transform.position;
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority)
            {
                Vector3 toTarget = TargetPosition - transform.position;
                toTarget.y = 0; // Ignore vertical distance

                if (toTarget.magnitude > stopDistance)
                {
                    Vector3 moveDirection = toTarget.normalized;
                    _unitCharacterController.Move(moveDirection * moveSpeed * Runner.DeltaTime);

                    // Smooth rotation
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Runner.DeltaTime);

                    if (playerAnimator != null)
                    {
                        playerAnimator.SetBool("IsWalking", true);
                    }
                }
                else
                {
                    if (playerAnimator != null)
                    {
                        playerAnimator.SetBool("IsWalking", false);
                    }
                }
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_SetTargetPosition(Vector3 newTargetPosition)
        {
            TargetPosition = newTargetPosition;
        }
    }
}
