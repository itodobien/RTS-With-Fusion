using System;
using System.Collections.Generic;
using Fusion;
using UI;
using UnityEngine;
using Unit = Units.Unit;

namespace Actions
{
    public class UnitActionSystem : NetworkBehaviour
    {
        public static UnitActionSystem Instance { get; private set; }
        public event EventHandler OnSelectedActionChanged;
        
        private BaseAction selectedAction;
        private Unit selectedUnit;

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
            selectedUnit = unit;
            BaseAction defaultAction = unit.GetMoveAction();
            SetSelectedAction(defaultAction);
        }

        private void ClearSelectedUnit()
        {
            selectedUnit = null;
            selectedAction = null;
        }

        public void SetSelectedAction(BaseAction baseAction)
        {
            selectedAction = baseAction;
            OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
        }

        public BaseAction GetSelectedAction()
        {
            return selectedAction;
        }

        private void HandleSelectedAction()
        {
            if (Input.GetMouseButtonDown(0))
            {
                switch (selectedAction)
                {
                    case MoveAction moveAction:
                        moveAction.MoveUnit();
                        break;
                    case SpinAction spinAction:
                        spinAction.SpinUnit();
                        break;
                }
            }
        }
        public override void FixedUpdateNetwork()
        {
            HandleSelectedAction();
        }
    }
}