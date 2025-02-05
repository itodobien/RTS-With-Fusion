using Fusion;
using Units;
using System;
using System.Collections.Generic;
using Grid;


namespace Actions
{
    public abstract class BaseAction : NetworkBehaviour
    {
        protected Unit Unit;
        private Action _onActionComplete;

        public abstract string GetActionName();

        protected virtual void Awake()
        {
            Unit = GetComponent<Unit>();
        }

        protected void StartAction(Action onActionComplete = null)
        {
            if (Unit == null) return;

            Unit.SetIsBusy(true);
            _onActionComplete = onActionComplete;
        }

        protected void ActionComplete()
        {
            if (Unit == null) return;

            Unit.SetIsBusy(false);
            _onActionComplete?.Invoke();
        }

        public abstract List<GridPosition> GetValidActionGridPositionList();
        public abstract void TakeAction(GridPosition gridPosition, Action onActionComplete = null);
    }
}