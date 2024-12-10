using Fusion;
using UnityEngine;

namespace Unit_Activities
{
    public class Unit : NetworkBehaviour
    {
        [Networked] private Vector3 TargetPosition { get; set; }
        [Networked] private NetworkBool IsMoving { get; set; }
    
        [SerializeField] private float stopDistance = 0.1f;
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float rotateSpeed = 10f;

        public override void FixedUpdateNetwork()
        {
            if (IsMoving)
            {
                MoveUnit();
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_SetTargetPosition(Vector3 newPosition)
        {
            TargetPosition = newPosition;
            IsMoving = true;
        }

        public void MoveUnit()
        {
            if (Vector3.Distance(transform.position, TargetPosition) > stopDistance)
            {
                Vector3 moveDirection = (TargetPosition - transform.position).normalized;
                transform.position += moveDirection * (Runner.DeltaTime * moveSpeed);
                transform.forward = Vector3.Lerp(transform.forward, moveDirection, Runner.DeltaTime * rotateSpeed);
            
                if (playerAnimator != null)
                {
                    playerAnimator.SetBool("IsWalking", true);
                }
            }
            else
            {
                IsMoving = false;
                if (playerAnimator != null)
                {
                    playerAnimator.SetBool("IsWalking", false);
                }
            }
        }
    }
}