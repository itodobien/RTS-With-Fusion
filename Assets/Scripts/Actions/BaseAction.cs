using Fusion;
using Units;
using System;
using System.Collections.Generic;
using Grid;


namespace Actions
{
    public abstract class BaseAction : NetworkBehaviour
    {
        protected Unit _unit;
        private Action _onActionComplete;

        public abstract string GetActionName();

        protected virtual void Awake()
        {
            _unit = GetComponent<Unit>();
        }

        protected void StartAction(Action onActionComplete = null)
        {
            if (_unit == null) return;

            _unit.SetIsBusy(true);
            _onActionComplete = onActionComplete;
        }

        protected void ActionComplete()
        {
            if (_unit == null) return;

            _unit.SetIsBusy(false);
            _onActionComplete?.Invoke();
        }

        public abstract List<GridPosition> GetValidActionGridPositionList();
        public abstract void TakeAction(GridPosition gridPosition, Action onActionComplete = null);
    }
}