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
                if (Vector3.Distance(transform.position, TargetPosition) > stopDistance)
                {
                    Vector3 moveDirection = (TargetPosition - transform.position).normalized;
                    _unitCharacterController.Move(moveDirection * (Runner.DeltaTime * moveSpeed));
                    transform.forward = Vector3.Lerp(transform.forward, moveDirection, Runner.DeltaTime * rotateSpeed);
                
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