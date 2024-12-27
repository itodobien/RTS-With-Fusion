using Fusion;
using Units;
using UnityEngine;

namespace Actions
{
    public class SpinAction : BaseAction
    {
        [SerializeField] private MoveAction moveAction;
        [Networked] private bool IsSpinning{ get; set;}
        
        [SerializeField] private float spinRotateSpeed = 1f;
        private float _spinTime = 1f;
        private float _spinTimer;
        
        public override string GetActionName() => "Spin";
        
        protected override void Awake()
        {
            base.Awake();
            moveAction = GetComponent<MoveAction>();
        }

        public override void FixedUpdateNetwork()
        {
            if (_unit.IsBusy && !IsSpinning) return;
            if (GetInput(out NetworkInputData data))
            {
                if (data.buttons.IsSet(NetworkInputData.SPIN) && _unit.GetIsSelected() && Object.HasStateAuthority && !IsSpinning)
                {
                    StartAction();
                    IsSpinning = true;
                    _spinTimer = _spinTime;
                }
            }

            if (Object.HasStateAuthority && IsSpinning)
            {
                SpinUnit();
            }
        }
        
        public void SpinUnit()
        {
            _spinTimer -= Runner.DeltaTime;
            transform.Rotate(Vector3.up, 360f * spinRotateSpeed * Runner.DeltaTime);
                
            if (_spinTimer <= 0)
            {
                IsSpinning = false;
                ActionComplete();  // sets IsBusy = false
            }
        }
    }
}