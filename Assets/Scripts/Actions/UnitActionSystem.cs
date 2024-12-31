using System;
using System.Collections.Generic;
using Fusion;
using Grid;
using UI;
using UnityEngine;
using Units;
using UnityEngine.EventSystems;


namespace Actions
{
    public class UnitActionSystem : NetworkBehaviour
    {
        public static UnitActionSystem Instance { get; private set; }
        public event EventHandler OnSelectedActionChanged;
        
        private BaseAction _selectedAction;

        private bool _spinRequested;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("Multiple UAS instances detected. Destroying the new one.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            UnitSelectionManager.Instance.OnSelectedUnitsChanged += UnitSelectionManager_OnSelectedUnitsChanged;
        }

        private void OnDestroy()
        {
            if (UnitSelectionManager.Instance != null)
            {
                UnitSelectionManager.Instance.OnSelectedUnitsChanged -= UnitSelectionManager_OnSelectedUnitsChanged;
            }
        }
        
        private void UnitSelectionManager_OnSelectedUnitsChanged(object sender, EventArgs e)
        {
            List<Unit> selectedUnits = UnitSelectionManager.Instance.GetSelectedUnits();
            if (selectedUnits.Count > 0)
            {
                SetSelectedUnit(selectedUnits[0]);
            }
            else
            {
                ClearSelectedUnit();
            }
        }

        private void SetSelectedUnit(Unit unit)
        {
            BaseAction defaultAction = unit.GetMoveAction();
            SetSelectedAction(defaultAction);
        }

        private void ClearSelectedUnit()
        {
            _selectedAction = null;
        }

        public void SetSelectedAction(BaseAction baseAction)
        {
            _selectedAction = baseAction;
            OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
        }

        public BaseAction GetSelectedAction()
        {
            return _selectedAction;
        }

        public void RequestSpin()
        {
            _spinRequested = true;
        }

        public bool GetSpinRequested()
        {
            bool wasSpinRequested = _spinRequested;
            _spinRequested = false;
            return wasSpinRequested;
        }
        

        private void HandleSelectedAction()
        {
            if (_selectedAction == null) return;
            
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
                {
                    Vector3 hitWorldPos = hitInfo.point;
                    GridPosition gridPosition = LevelGrid.Instance.GetGridPosition(hitWorldPos);

                    if (_selectedAction is MoveAction moveAction)
                    {
                        moveAction.TakeAction(gridPosition, () =>
                        {
                            Debug.Log("moving complete");
                        });
                    }
                    else if (_selectedAction is SpinAction spinAction)
                    {
                        spinAction.TakeAction(gridPosition, () => Debug.Log("spinning complete"));
                    }
                    else if (_selectedAction is ShootAction shootAction)
                    {
                        if (shootAction.GetValidActionGridPositionList().Contains(gridPosition))
                        {
                            shootAction.TakeAction(gridPosition, () =>
                            {
                                Debug.Log("shooting complete");
                            });
                        }
                        else
                        {
                            Debug.Log("invalid shoot target");
                        }
                    }
                    else
                    {
                        Debug.Log("Selected actions is not shoot agion, ignoring left-click");
                    }
                }
            }
        }
        public override void FixedUpdateNetwork()
        {
            HandleSelectedAction();
        }
    }
}