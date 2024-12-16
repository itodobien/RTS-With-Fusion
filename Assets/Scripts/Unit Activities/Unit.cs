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
        
        [Networked] private NetworkBool IsSelected { get; set; }
        
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
                if (GetInput(out NetworkInputData data))
                {
                    Debug.Log($"Unit {Object.Id} received input. HasMoveCommand: {data.hasUnitMoveCommand}, IsSelected: {IsSelected}, Owner: {OwnerPlayerRef}, InputAuthority: {Object.InputAuthority}");
                
                    if (data.hasUnitMoveCommand && IsSelected && data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
                    {
                        TargetPosition = data.unitMoveTargetPosition;
                        Debug.Log($"Unit {Object.Id} moving to {TargetPosition}. Owner: {OwnerPlayerRef}, InputAuthority: {Object.InputAuthority}");
                    }
                }
                HandleMovement();
            }
        }

        private void HandleMovement()
        {
            Vector3 toTarget = TargetPosition - transform.position;
            toTarget.y = 0;

            float distance = toTarget.magnitude;
            Debug.Log($"Unit {Object.Id} handling movement. Current position: {transform.position}, Target position: {TargetPosition}");

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
        
        public void SetSelected(bool selected)
        {
            IsSelected = selected;
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
