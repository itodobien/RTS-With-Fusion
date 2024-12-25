using Fusion;
using Units;
using UnityEngine;

namespace Actions
{
    public class SpinAction : NetworkBehaviour
    {
        private Unit _unit;
        [SerializeField] private MoveAction moveAction;
        [Networked] private bool IsSpinning{ get; set;}
        [SerializeField] private float spinRotateSpeed = 5f;
    

        private void Awake()
        {
            _unit = GetComponent<Unit>();
            moveAction = GetComponent<MoveAction>();
        }
    
        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData data))
            {
                if (data.buttons.IsSet(NetworkInputData.SPIN) && _unit.GetIsSelected())
                {
                    if (!moveAction.IsMoving)
                        IsSpinning = true;
                }
                else
                {
                    IsSpinning = false;
                }
            }
            if (IsSpinning && !moveAction.IsMoving)
            {
                transform.Rotate(Vector3.up, 360 * Time.deltaTime * spinRotateSpeed);
            }
        }
    }
}