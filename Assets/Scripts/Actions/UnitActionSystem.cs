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
        
        private BaseAction selectedAction;
        private Unit selectedUnit;

        private void Awake()
        {
            Debug.Log($"UnitActionSystem Awake on {gameObject.name}");
            if (Instance != null && Instance != this)
            {
                Debug.LogError("Multiple UAS instances detected. Destroying the new one.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Debug.Log("UnitActionSystem: Instance has been set.");
        }

        private void Start()
        {
            SetSelectedUnit(selectedUnit);
            UnitSelectionManager.Instance.OnSelectedUnitsChanged += UnitSelectionManager_OnSelectedUnitsChanged;
        }

        private void OnDestroy()
        {
            if (UnitSelectionManager.Instance != null)
            {
                UnitSelectionManager.Instance.OnSelectedUnitsChanged -= UnitSelectionManager_OnSelectedUnitsChanged;
            }
        }
        
        private void UnitSelectionManager_OnSelectedUnitsChanged(object sender, System.EventArgs e)
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
            Debug.Log($"Selected Action: {selectedAction.GetActionName()}");
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