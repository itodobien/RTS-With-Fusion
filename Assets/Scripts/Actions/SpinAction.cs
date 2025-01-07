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
            GridPosition unitGridPosition = _unit.GetGridPosition();

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
            
            if (_unit.IsBusy || IsSpinning)
            {
                onActionComplete?.Invoke();
            }
            
            StartAction(onActionComplete);
            
            IsSpinning = true;
            _spinTimer = _spinTime;
        }

        public override void FixedUpdateNetwork()
        {
            if (_unit == null || !_unit.Object || !_unit.Object.IsInSimulation)
            {
                Debug.Log($"{GetActionName()} => Our acting unit is gone or out of simulation, force-completing action.");
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