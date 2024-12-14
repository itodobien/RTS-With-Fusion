using Fusion;
using UnityEngine;

namespace Unit_Activities
{
    public class Unit : NetworkBehaviour
    {
        private BaseAction[] baseActionsArray;
        
        [Networked] private Vector3 TargetPosition { get; set; }
        private NetworkCharacterController _unitCharacterController;
        
        [SerializeField] private float stopDistance = 0.1f;
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float rotateSpeed = 10f;
        
        private bool isSelected;
        
        private void Awake()
        {
            _unitCharacterController = GetComponent<NetworkCharacterController>();
            baseActionsArray = GetComponents<BaseAction>();
        }

        public override void Spawned()
        {
            TargetPosition = transform.position;
        }

        public override void FixedUpdateNetwork()
        {
            // Only the player with input authority processes movement
            if (HasStateAuthority && Object.HasInputAuthority)
            {
                HandleMovement();
            }
        }


        private void HandleMovement()
        {
            Vector3 toTarget = TargetPosition - transform.position;
            toTarget.y = 0;

            float distance = toTarget.magnitude;
            Debug.Log($"[Unit] Distance to Target: {distance} StopDistance: {stopDistance}");

            if (distance > stopDistance)
            {
                Vector3 moveDirection = toTarget.normalized;
                Debug.Log($"[Unit] Move Direction: {moveDirection}");
        
                _unitCharacterController.Move(moveDirection * moveSpeed * Runner.DeltaTime);
        
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Runner.DeltaTime);

                if (playerAnimator != null)
                {
                    Debug.Log("[Unit] Setting IsWalking = true");
                    playerAnimator.SetBool("IsWalking", true);
                }
            }
            else
            {
                if (playerAnimator != null)
                {
                    Debug.Log("[Unit] Setting IsWalking = false");
                    playerAnimator.SetBool("IsWalking", false);
                }
            }
        }
        
        public void SetSelected(bool isSelected)
        {
            this.isSelected = isSelected;
        }
        
        public bool IsSelected => isSelected;

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_SetTargetPosition(Vector3 newTargetPosition)
        {
            Debug.Log($"[Unit] RPC_SetTargetPosition received on {name}, newTargetPosition: {newTargetPosition}");
            TargetPosition = newTargetPosition;
        }

        public BaseAction[] GetBaseActionArray()
        {
            return baseActionsArray;
        }
    }
}