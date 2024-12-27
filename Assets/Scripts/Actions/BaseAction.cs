using Fusion;
using Units;
using System;


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

        public void StartAction(Action onActionComplete = null)
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

        
    }
}