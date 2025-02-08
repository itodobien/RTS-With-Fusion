using System;
using System.Collections.Generic;
using Grid;
using Integrations.Interfaces;
using UnityEngine;

namespace Actions
{
    public class InteractAction : BaseAction
    {
        
        public event EventHandler OnInteract;

        private const int InteractRange = 1;
        public override string GetActionName() => "Interact";
    

        public override List<GridPosition> GetValidActionGridPositionList()
        {
            GridPosition unitGridPosition = Unit.GetGridPosition();
            List<GridPosition> validGridPositionList = new List<GridPosition>();
    
            foreach (var testPosition in ActionUtils.GetGridPositionsInRange(unitGridPosition, InteractRange))
            {
                IInteractable interactable = LevelGrid.Instance.GetInteractableAtGridPosition(testPosition);
                if (interactable == null) continue;
                
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
            
            IInteractable interactable = LevelGrid.Instance.GetInteractableAtGridPosition(gridPosition);
            if (interactable != null)
            {
                interactable.Interact(() => 
                {
                    Debug.Log("Interaction Complete");
                    ActionComplete();
                });
            }
            else
            {
                Debug.Log("No interactable object found");
                ActionComplete();
            }
        }
    }
}
