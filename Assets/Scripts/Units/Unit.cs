using Actions;
using Fusion;
using Grid;
using UnityEngine;

namespace Units
{
    public class Unit : NetworkBehaviour
    {
        [SerializeField] private Transform aimTransform;
        
        [Networked] private NetworkBool IsSelected { get; set; }
        [Networked] public bool IsBusy { get; set; }
        [Networked] public PlayerRef OwnerPlayerRef { get; set; }
        [Networked] private int _teamID { get; set; }
        
        private GridPosition _gridPosition;
        private BaseAction[] _baseActionsArray;
        private MoveAction _moveAction;
        private SpinAction _spinAction;
        private ShootAction _shootAction;

        private void Awake()
        {
            _baseActionsArray = GetComponents<BaseAction>();
            _moveAction = GetComponent<MoveAction>();
            _spinAction = GetComponent<SpinAction>();
            _shootAction = GetComponent<ShootAction>();
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
        
        public Vector3 GetAimPosition()
        {
            return aimTransform.position;
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
        public void SetTeamID(int newTeamID)
        {
            if (HasStateAuthority)
            {
                _teamID = newTeamID;
            }
        }
        public int GetTeamID()
        {
            return _teamID;
        }

        public void Damage()
        {
            Debug.Log(transform + "Damage");
        }
        

        public bool GetIsSelected() => IsSelected;
        public MoveAction GetMoveAction() => _moveAction;
        public SpinAction GetSpinAction() => _spinAction;

        public ShootAction GetShootAction() => _shootAction;
        public GridPosition GetGridPosition() => _gridPosition;
        public BaseAction[] GetBaseActionArray() => _baseActionsArray;
        
        public Vector3 GetWorldPosition() => transform.position;

    }
}