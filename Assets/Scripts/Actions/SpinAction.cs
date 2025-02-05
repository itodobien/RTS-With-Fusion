using System;
using System.Collections.Generic;
using Fusion;
using Grid;
using UnityEngine;

namespace Actions
{
    public class SpinAction : BaseAction
    {
        [Networked] private bool IsSpinning{ get; set;}
        
        [SerializeField] private float spinRotateSpeed = 1f;
        private readonly float _spinTime = 1f;
        private float _spinTimer;
        
        public override string GetActionName() => "Spin";
        
        public override List<GridPosition> GetValidActionGridPositionList()
        {
            GridPosition unitGridPosition = Unit.GetGridPosition();

            return new List<GridPosition>
            {
                unitGridPosition
            };
        }

        public override void TakeAction(GridPosition gridPosition, Action onActionComplete = null)
        {
            if (!Object.HasStateAuthority)
            {
                onActionComplete?.Invoke();
                return;
            }
            
            if (Unit.IsBusy || IsSpinning)
            {
                onActionComplete?.Invoke();
            }
            
            StartAction(onActionComplete);
            
            IsSpinning = true;
            _spinTimer = _spinTime;
        }

        public override void FixedUpdateNetwork()
        {
            if (Unit == null || !Unit.Object || !Unit.Object.IsInSimulation)
            {
                ActionComplete();
                return;
            }
            
            if (IsSpinning)
            {
                _spinTimer -= Runner.DeltaTime;
                transform.Rotate(Vector3.up, 360f * spinRotateSpeed * Runner.DeltaTime);
                if (_spinTimer <= 0)
                {
                    IsSpinning = false;
                    ActionComplete();
                }
            }
        }
    }
}