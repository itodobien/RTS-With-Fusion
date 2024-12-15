using Fusion;
using UnityEngine;

namespace Unit_Activities
{
    public class Unit : NetworkBehaviour
    {
        private BaseAction[] baseActionsArray;
        
        [Networked] private Vector3 TargetPosition { get; set; }
        [Networked] public PlayerRef OwnerPlayerRef { get; set; }
        
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
            OwnerPlayerRef = Object.InputAuthority;
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority)
            {
                HandleMovement();
            }
        }

        private void HandleMovement()
        {
            Vector3 toTarget = TargetPosition - transform.position;
            toTarget.y = 0;

            float distance = toTarget.magnitude;
            Debug.Log($"[Unit {name}] Distance to Target: {distance} StopDistance: {stopDistance}, TargetPosition: {TargetPosition}, CurrentPosition: {transform.position}");

            if (distance > stopDistance)
            {
                Vector3 moveDirection = toTarget.normalized;
                Debug.Log($"[Unit {name}] Move Direction: {moveDirection}");

                _unitCharacterController.Move(moveDirection * moveSpeed * Runner.DeltaTime);

                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Runner.DeltaTime);

                if (playerAnimator != null)
                {
                    Debug.Log($"[Unit {name}] Setting IsWalking = true");
                    playerAnimator.SetBool("IsWalking", true);
                }
            }
            else
            {
                if (playerAnimator != null)
                {
                    Debug.Log($"[Unit {name}] Setting IsWalking = false");
                    playerAnimator.SetBool("IsWalking", false);
                }
            }
        }
        
        public void SetSelected(bool isSelected)
        {
            this.isSelected = isSelected;
        }
        
        public bool IsSelected => isSelected;

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_SetTargetPosition(Vector3 newTargetPosition, RpcInfo info = default)
        {
            Debug.Log($"RPC_SetTargetPosition called by {info.Source}. New Target Position: {newTargetPosition}");

            // Allow processing only if called by the correct party (client InputAuthority or host StateAuthority)
            if (info.IsInvokeLocal || Object.HasStateAuthority)
            {
                TargetPosition = newTargetPosition;

                // Log confirmation for debugging
                Debug.Log($"Set Target Position updated for unit: {name}. Processed by: {(Object.HasStateAuthority ? "Server (StateAuthority)" : "Client (InputAuthority)")}");
            }
            else
            {
                // Invalid command attempts are logged
                Debug.LogWarning($"RPC_SetTargetPosition: Rejected due to improper authority. Caller: {info.Source}");
            }
        }

        public void SetTargetPositionLocal(Vector3 newTargetPosition)
        {
            if (HasStateAuthority)
            {
                TargetPosition = newTargetPosition;
            }
        }

        public BaseAction[] GetBaseActionArray()
        {
            return baseActionsArray;
        }
    }
}