using System;
using System.Collections.Generic;
using Fusion;
using Grid;
using UI;
using UnityEngine;
using Units;

namespace Actions
{
    [RequireComponent(typeof(NetworkBehaviour))]
    public class UnitActionSystem : NetworkBehaviour
    {
        private ActionType _localSelectedAction = ActionType.Move;
        public static UnitActionSystem Instance { get; private set; }
        public event EventHandler OnSelectedActionChanged;
        
        private BaseAction _selectedAction;
        private Dictionary<PlayerRef, Unit> selectedUnitDict = new();
        
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
                SetLocalSelectedUnit(selectedUnits[0]);
            }
            else
            {
                ClearSelectedUnit();
            }
        }
        private void SetLocalSelectedUnit(Unit unit)
        {
            BaseAction defaultAction = unit.GetMoveAction();
            _selectedAction = defaultAction; 
            OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetLocalSelectedAction(ActionType newAction)
        {
            _localSelectedAction = newAction;
        }
        
        public ActionType GetLocalSelectedAction() => _localSelectedAction;

        public void SetSelectedUnitForPlayer(PlayerRef player, Unit unit)
        {
            selectedUnitDict[player] = unit;
        }

        public Unit GetSelectedUnitForPlayer(PlayerRef player)
        {
            selectedUnitDict.TryGetValue(player, out var found);
            return found;
        }

        private void ClearSelectedUnit()
        {
            _selectedAction = null;
            OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetSelectedAction(BaseAction baseAction)
        {
            _selectedAction = baseAction;
            OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
        }

        public BaseAction GetSelectedAction() => _selectedAction;

        private void HandleSelectedAction(PlayerRef playerRef, ActionType actionType, GridPosition gridPosition)
        {
            Unit selectedUnit = GetSelectedUnitForPlayer(playerRef);
            if (selectedUnit == null)
            {
                Debug.LogWarning($"Player {playerRef} tried to do {actionType}, but has no selected unit");
                return;
            }
            switch (actionType)
            {
                case ActionType.Move:
                    selectedUnit.GetMoveAction().TakeAction(gridPosition, () => Debug.Log("Move complete"));
                    break;

                case ActionType.Spin:
                    selectedUnit.GetSpinAction().TakeAction(gridPosition, () => Debug.Log("Spin complete"));
                    break;

                case ActionType.Shoot:
                    selectedUnit.GetShootAction().TakeAction(gridPosition, () => Debug.Log("Shoot complete"));
                    break;

                default:
                    Debug.LogWarning($"Unknown action type: {actionType}");
                    break;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) return;

            foreach (var playerRef in Runner.ActivePlayers)
            {
                NetworkInputData? maybeData = Runner.GetInputForPlayer<NetworkInputData>(playerRef);
                if (maybeData.HasValue)
                {
                    NetworkInputData data = maybeData.Value;

                    if (data.buttons.IsSet(NetworkInputData.SELECT_UNIT))
                    {
                        NetworkObject changeObj = Runner.FindObject(data.selectedUnitId);
                        if (changeObj != null && changeObj.TryGetComponent<Unit>(out var changedUnit))
                        {
                            if (data.isSelected)
                            {
                                SetSelectedUnitForPlayer(playerRef, changedUnit);
                            }
                            else
                            {
                                SetSelectedUnitForPlayer(playerRef, null);
                            }
                        }
                    }
                    if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
                    {
                        GridPosition clickedGridPosition = new GridPosition(data.targetGridX, data.targetGridZ);

                        HandleSelectedAction(playerRef, data.actionType, clickedGridPosition);
                    }
                }
            }
        }
    }
}