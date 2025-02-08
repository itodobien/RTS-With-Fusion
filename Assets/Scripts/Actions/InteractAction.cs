using System;
using System.Collections.Generic;
using Grid;
using UnityEngine;

namespace Actions
{
    public class InteractAction : BaseAction
    {
        
        public event EventHandler OnInteract;
        
        private int interactRange = 1;
        public override string GetActionName() => "Interact";
    

        public override List<GridPosition> GetValidActionGridPositionList()
        {
            GridPosition unitGridPosition = Unit.GetGridPosition();
            List<GridPosition> validGridPositionList = new List<GridPosition>();
    
            foreach (var testPosition in ActionUtils.GetGridPositionsInRange(unitGridPosition, interactRange))
            {
                Door door = LevelGrid.Instance.GetDoorAtGridPosition(testPosition);
                if (door == null) continue;
                
                validGridPositionList.Add(testPosition);
            }
            return validGridPositionList;
        }

        public override void TakeAction(GridPosition gridPosition, Action onActionComplete = null)
        {
            if (!Object.HasStateAuthority)
            {
                onActionComplete?.Invoke();
                return;
            }
            StartAction(onActionComplete);
            OnInteract?.Invoke(this, EventArgs.Empty);
            
            Door door = LevelGrid.Instance.GetDoorAtGridPosition(gridPosition);
            if (door != null)
            {
                door.RPC_RequestInteract(); 
            }
            Debug.Log("Interacting (Client Request)");
            ActionComplete();
        }
    }
}
