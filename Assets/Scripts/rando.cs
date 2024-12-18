/*
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

namespace Unit_Activities
{
    public class testing : NetworkBehaviour
    {
        private BaseAction[] baseActionsArray;
        [Networked] private Vector3 TargetPosition { get; set; }
        [Networked] public PlayerRef OwnerPlayerRef { get; set; }
        [Networked] public NetworkBool IsSelected { get; set; }
        
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
            Debug.Log($"Unit spawned: OwnerPlayerRef = {OwnerPlayerRef}, Object.InputAuthority = {Object.InputAuthority}, Local Player = {Runner.LocalPlayer}");
            Debug.Log($"Unit spawned with the following: " +
                      $"World Position: {transform.position}, " +
                      $"InputAuthority: {Object.InputAuthority}, " +
                      $"OwnerPlayerRef: {OwnerPlayerRef}, " +
                      $"HasStateAuthority: {HasStateAuthority}, " +
                      $"Local Player: {Runner.LocalPlayer}");
            
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority)
            {
                if (GetInput(out NetworkInputData data))
                {
                    Debug.Log($"Input received by Unit {Object.Id}: TargetPosition = {data.targetPosition}");
    
                    if (IsSelected && data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
                    {
                        TargetPosition = data.targetPosition;
                        Debug.Log($"Unit {Object.Id} setting target position to: {TargetPosition}");
                    }
                }
                HandleMovement();
            }
        }
        
        public void SetSelected(bool selected)
        {
            if (HasStateAuthority)
            {
                IsSelected = selected;
                Debug.Log($"Unit {Object.Id} selection changed. IsSelected: {IsSelected}, Owner: {OwnerPlayerRef}, HasStateAuthority: {Object.HasStateAuthority}");
            }
        }
        private void HandleMovement()
        {

            if (GetInput(out NetworkInputData data))
            {
                TargetPosition = data.targetPosition;
                Vector3 toTarget = TargetPosition - transform.position;
                toTarget.y = 0;
                float distance = toTarget.magnitude;

                if (distance > stopDistance)
                {
                    Debug.Log($"[Unit {Object.Id}] at {transform.position} moving to {TargetPosition}. current mouse position is {Input.mousePosition}");
                
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
                    Debug.Log($"[Unit {Object.Id}] Reached TargetPosition or within stopDistance.");
                    if (playerAnimator != null)
                    {
                        playerAnimator.SetBool("IsWalking", false);
                    }
                }
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
*/
