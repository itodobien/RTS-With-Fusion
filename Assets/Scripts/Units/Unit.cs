using Actions;
using Fusion;
using Grid;

namespace Units
{
    public class Unit : NetworkBehaviour
    {
        [Networked] private NetworkBool IsSelected { get; set; }
        [Networked] public bool IsBusy { get; set; }
        
        private GridPosition _gridPosition;
        private BaseAction[] _baseActionsArray;
        private MoveAction _moveAction;
        private SpinAction _spinAction;

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
        }

        public void SetIsBusy(bool isBusy)
        {
            if (HasStateAuthority)
            {
                IsBusy = isBusy;
            }
        }

        public bool GetIsSelected() => IsSelected;
        public MoveAction GetMoveAction() => _moveAction;
        public SpinAction GetSpinAction() => _spinAction;
        public GridPosition GetGridPosition() => _gridPosition;
        public BaseAction[] GetBaseActionArray() => _baseActionsArray;
        
    }
}