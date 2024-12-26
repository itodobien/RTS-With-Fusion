using Fusion;
using Units;
using UnityEngine;

namespace Actions
{
    public class SpinAction : BaseAction
    {
        [SerializeField] private MoveAction moveAction;
        [Networked] private bool IsSpinning{ get; set;}
        [SerializeField] private float spinRotateSpeed = 5f;
    

        protected override void Awake()
        {
            base.Awake();
            moveAction = GetComponent<MoveAction>();
        }

        public override string GetActionName()
        {
            return "Spin";
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData data))
            {
                if (data.buttons.IsSet(NetworkInputData.SPIN) && _unit.GetIsSelected())
                {
                    if (Object.HasStateAuthority && !moveAction.IsMoving)
                    {
                        IsSpinning = !IsSpinning;
                    }
                }
                else
                {
                    IsSpinning = false;
                }
            }
            if (Object.HasStateAuthority && IsSpinning && !moveAction.IsMoving)
            {
                transform.Rotate(Vector3.up, 360 * Time.deltaTime * spinRotateSpeed);
            }
        }
    }
}