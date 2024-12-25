using System.Collections.Generic;
using Fusion;
using Grid;
using Units;
using UnityEngine;

namespace Actions
{
    public class MoveAction : NetworkBehaviour
    {
        [Header("Move Action Settings")]
        [SerializeField]private float stopDistance = 0.1f;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float rotateSpeed = 10f;
        [SerializeField] private int maxMoveDistance = 4;

        [SerializeField] private Animator playerAnimator;

        [Networked] private Vector3 TargetPosition { get; set; }
        [Networked] public PlayerRef OwnerPlayerRef { get; set; }
        [Networked] public bool IsMoving { get; set; }
    
        private static readonly int IsWalking = Animator.StringToHash("IsWalking");

        
        private Unit _unit;

        private void Awake()
        {
            _unit = GetComponent<Unit>();
        }

        public override void Spawned()
        {
            TargetPosition = transform.position;
            OwnerPlayerRef = Object.InputAuthority;
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData data))
            {
                if (data.buttons.IsSet(NetworkInputData.SELECT_UNIT) && data.selectedUnitId == Object.Id)
                {
                    _unit.SetNetworkSelected(data.isSelected);
                }
            }

            if (_unit.GetIsSelected() && data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
            {
                GridPosition clickedGridPosition = new GridPosition(data.targetGridX, data.targetGridZ);
                if (IsValidActionGridPosition(clickedGridPosition))
                {
                    Vector3 worldPosition = LevelGrid.Instance.GetWorldPostion(clickedGridPosition);
                    TargetPosition = worldPosition;
                    IsMoving = true;
                }
                else
                {
                    Debug.Log("Invalid Move Target");
                }
            }
            HandleMovement();
        }

        private void HandleMovement()
        {
            if (IsMoving)
            {
                Vector3 toTarget = TargetPosition - transform.position;
                toTarget.y = 0f;
                float distance = toTarget.magnitude;

                if (distance > stopDistance)
                {
                    Vector3 moveDirection = toTarget.normalized;
                    transform.position += moveDirection * moveSpeed * Runner.DeltaTime;

                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation, 
                        targetRotation, 
                        rotateSpeed * Runner.DeltaTime);

                    if (playerAnimator != null)
                    {
                        playerAnimator.SetBool(IsWalking, true);
                    }
                }
                else
                {
                    IsMoving = false;
                    
                }
            }
            else
            {
                if (playerAnimator != null)
                {
                    playerAnimator.SetBool(IsWalking, IsMoving);
                }
            }
        }

        public bool IsValidActionGridPosition(GridPosition gridPosition)
        {
            List<GridPosition> validGridPositionList = GetValidActionGridPositionList();
            return validGridPositionList.Contains(gridPosition);
        }

        public List<GridPosition> GetValidActionGridPositionList()
        {
            List<GridPosition> validGridPositionList = new List<GridPosition>();
            GridPosition unitGridPosition = _unit.GetGridPosition();

            for (int x = -maxMoveDistance; x <= maxMoveDistance; x++)
            {
                for (int z = -maxMoveDistance; z <= maxMoveDistance; z++)
                {
                    GridPosition offSetGridPosition = new GridPosition(x, z);
                    GridPosition testGridPosition = unitGridPosition + offSetGridPosition;

                    if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) continue;
                    if (unitGridPosition == testGridPosition) continue;
                    if (LevelGrid.Instance.HasUnitAtGridPosition(testGridPosition)) continue;
                
                    validGridPositionList.Add(testGridPosition);
                }
            }
            return validGridPositionList;
        }
    }
}
