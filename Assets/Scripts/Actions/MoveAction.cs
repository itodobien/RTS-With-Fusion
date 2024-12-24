using Actions;
using Fusion;
using Units;
using UnityEngine;

public class MoveAction : NetworkBehaviour
{
    [Header("Move Action Settings")]
    [SerializeField]private float stopDistance = 0.1f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotateSpeed = 10f;

    [SerializeField] private Animator playerAnimator;

    [Networked] private Vector3 TargetPosition { get; set; }
    [Networked] public PlayerRef OwnerPlayerRef { get; set; }
    [Networked] private bool IsMoving { get; set; }
    
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");

    private NetworkCharacterController _unitCharacterController;
    private ChangeDetector _changeDetector;
    private Unit _unit;

    private void Awake()
    {
        _unitCharacterController = GetComponent<NetworkCharacterController>();
        _unit = GetComponent<Unit>();
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
                _unit.SetNetworkSelected(data.isSelected);
            }
        }

        HandleMovement();
    }

    private void HandleMovement()
    {
        if (_unit != null && _unit.GetIsSelected())
        {
            if (GetInput(out NetworkInputData data) && data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
            {
                TargetPosition = data.targetPosition;
                IsMoving = true;
            }
        }

        if (IsMoving)
        {
            Vector3 toTarget = TargetPosition - transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;

            if (distance > stopDistance)
            {
                Vector3 moveDirection = toTarget.normalized;
                _unitCharacterController.Move(moveDirection * moveSpeed * Runner.DeltaTime);

                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation =
                    Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Runner.DeltaTime);

                if (playerAnimator != null)
                {
                    playerAnimator.SetBool(IsWalking, true);
                }
            }
            else
            {
                IsMoving = false;
                if (playerAnimator != null)
                {
                    playerAnimator.SetBool(IsWalking, false);
                }
            }
        }
        else
        {
            if (playerAnimator != null)
            {
                playerAnimator.SetBool(IsWalking, false);
            }
        }
    }
}