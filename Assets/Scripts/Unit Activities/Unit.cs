using Fusion;
using Unity.VisualScripting;
using UnityEngine;

namespace Unit_Activities
{
    public class Unit : NetworkBehaviour
    {
        private BaseAction[] baseActionsArray;
        [Networked] private Vector3 TargetPosition { get; set; }
        [Networked] public PlayerRef OwnerPlayerRef { get; set; }
        [Networked] public NetworkBool IsSelected { get; set; }
        
        [Networked] private bool IsMoving { get; set; }
        
        private NetworkCharacterController _unitCharacterController;
        
        [SerializeField] private float stopDistance = 0.1f;
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float rotateSpeed = 10f;
       
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
            HandleMovement();
        }
        
        public void SetSelected(bool selected)
        {
            if (HasStateAuthority)
            {
                IsSelected = selected;
            }
        }
        private void HandleMovement()
        {
            if (GetInput(out NetworkInputData data))
            {
                if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
                {
                    Debug.Log($"Unit {Object.Id} setting target position to: {data.targetPosition}");
                    TargetPosition = data.targetPosition;
                    IsMoving = true;
                }

                if (IsMoving)
                {
                    Vector3 toTarget = TargetPosition - transform.position;
                    toTarget.y = 0;
                    float distance = toTarget.magnitude;

                    if (distance > stopDistance)
                    {
                        Vector3 moveDirection = toTarget.normalized;
                        _unitCharacterController.Move(moveDirection * moveSpeed * Runner.DeltaTime);

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
            
        }

        public void SetTargetPositionLocal(Vector3 newTargetPosition)
        {
            if (HasStateAuthority && IsSelected)
            {
                TargetPosition = newTargetPosition;
                IsMoving = true;
            }
        }

        public BaseAction[] GetBaseActionArray()
        {
            return baseActionsArray;
        }
    }
}
