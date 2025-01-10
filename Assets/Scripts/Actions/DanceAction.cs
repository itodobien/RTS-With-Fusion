using System;
using System.Collections.Generic;
using Fusion;
using Grid;
namespace Actions
{
    public class DanceAction : BaseAction
    {

        public event EventHandler OnStartDancing;
        public event EventHandler OnStopDancing;
    
        [Networked] private bool IsDancing { get; set;}   
        public override string GetActionName() => "Dance";
        public bool GetIsDancing() => IsDancing;
    
    
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
            
            if (_unit.IsBusy || IsDancing)
            {
                onActionComplete?.Invoke();
                return;
            }
            
            StartAction(onActionComplete);
            OnStartDancing?.Invoke(this, EventArgs.Empty);
            IsDancing = true;
        }
        public void StopDancing()
        {
            if (!IsDancing) return;
            RPC_StopDancing();
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_StopDancing()
        {
            IsDancing = false;
            OnStopDancing?.Invoke(this, EventArgs.Empty);
            ActionComplete();
        }
    }
}
