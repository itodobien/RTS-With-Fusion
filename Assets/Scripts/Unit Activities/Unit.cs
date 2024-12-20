using Fusion;
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
        private ChangeDetector _changeDetector;
        
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
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData data))
            {
                if (data.buttons.IsSet(NetworkInputData.SELECT_UNIT) && data.selectedUnitId == Object.Id)
                {
                    SetSelected(data.isSelected);
                }
            }
            HandleMovement(data);
        }
        
        public void SetSelected(bool selected) // this is not doing anything of meaning. 
        {
            if (HasStateAuthority)
            {
                IsSelected = selected;
            }
            else
            {
                Debug.LogWarning($"Attempted to set IsSelected on Unit {Object.Id} without state authority");
            }
        }
        public override void Render()
        {
            base.Render();
            foreach (var change in _changeDetector.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(IsSelected):
                        UpdateSelectionVisual(IsSelected);
                        break;
                }
            }
        }

        private void UpdateSelectionVisual(bool isSelected)
        {
            if (TryGetComponent<MeshRenderer>(out var meshRenderer))
            {
                meshRenderer.enabled = isSelected;
            }
        }
        private void HandleMovement(NetworkInputData data)
        {
            if (IsSelected && data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
            {
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
                    IsMoving = false;
                    if (playerAnimator != null)
                    {
                        playerAnimator.SetBool("IsWalking", false);
                    }
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