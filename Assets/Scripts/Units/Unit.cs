using Actions;
using Fusion;
using Grid;
using UnityEngine;

namespace Units
{
    public class Unit : NetworkBehaviour
    {
        [Networked] public NetworkBool IsSelected { get; set; }
        
        private GridPosition _gridPosition;
        private BaseAction[] _baseActionsArray;
        private MoveAction _moveAction;
        private SpinAction _spinAction;
        
        public override void Spawned()
        {
            //
        }

        private void Awake()
        {
            _baseActionsArray = GetComponents<BaseAction>();
            _moveAction = GetComponent<MoveAction>();
            _spinAction = GetComponent<SpinAction>();
        }

        private void Start()
        {
            _gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
            LevelGrid.Instance.AddUnitAtGridPosition(_gridPosition, this);
        }

        private void Update()
        {
            GridPosition newGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
            if (newGridPosition != _gridPosition)
            {
                LevelGrid.Instance.UnitMovedGridPosition(this, _gridPosition, newGridPosition);
                _gridPosition = newGridPosition;
            }
        }

        public void SetNetworkSelected(bool selected)
        {
            if (HasStateAuthority)
            {
                IsSelected = selected;
            }
            else
            {
                Debug.LogWarning($"Attempted to set IsSelected on Unit {Object.Id} without state authority");
            }
        }

        public bool GetIsSelected()
        {
            return IsSelected;
        }

        public MoveAction GetMoveAction()
        {
            return _moveAction;
        }

        public SpinAction GetSpinAction()
        {
            return _spinAction;
        }
        
        public GridPosition GetGridPosition()
        {
            return _gridPosition;
        }
        
        public BaseAction[] GetBaseActionArray()
        {
            return _baseActionsArray;
        }
    }
}
