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
        private readonly Dictionary<PlayerRef, Unit> _selectedUnitDict = new();

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
            BaseAction defaultAction = unit.GetAction<MoveAction>();
            _selectedAction = defaultAction;
            OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
            SetLocalSelectedAction(ActionType.Move);
        }

        public void SetLocalSelectedAction(ActionType newAction)
        {
            _localSelectedAction = newAction;
        }

        public ActionType GetLocalSelectedAction() => _localSelectedAction;

        public void SetSelectedUnitForPlayer(PlayerRef player, Unit unit)
        {
            _selectedUnitDict[player] = unit;
        }

        public Unit GetSelectedUnitForPlayer(PlayerRef player)
        {
            _selectedUnitDict.TryGetValue(player, out var found);
            return found;
        }

        private void ClearSelectedUnit()
        {
            _selectedAction = null;
            OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetSelectedAction(BaseAction baseAction)
        {
            if (_selectedAction is DanceAction oldDanceAction && oldDanceAction.GetIsDancing())
            {
                oldDanceAction.StopDancing();
            }
            
            _selectedAction = baseAction;
            OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
        }

        public BaseAction GetSelectedAction() => _selectedAction;

        private void HandleSelectedAction(PlayerRef playerRef, ActionType actionType, GridPosition gridPosition)
        {
            Unit selectedUnit = GetSelectedUnitForPlayer(playerRef);
            if (selectedUnit == null) return;

            if (actionType != ActionType.Dance)
            {
                DanceAction danceAction = selectedUnit.GetAction<DanceAction>();
                if (danceAction.GetIsDancing())
                {
                    danceAction.StopDancing();
                }
            }

            switch (actionType)
            {
                case ActionType.Move:
                    selectedUnit.GetAction<MoveAction>().TakeAction(gridPosition, () => Debug.Log("Move complete"));
                    break;

                case ActionType.Spin:
                    selectedUnit.GetAction<SpinAction>().TakeAction(gridPosition, () => Debug.Log("Spin complete"));
                    break;

                case ActionType.Shoot:
                    var shootAction = selectedUnit.GetAction<ShootAction>();
                    var validPositions = shootAction.GetValidActionGridPositionList();
                    if (!validPositions.Contains(gridPosition)) return;
                    shootAction.TakeAction(gridPosition, () => Debug.Log("Shoot complete"));
                    break;
                
                case ActionType.Dance:
                    selectedUnit.GetAction<DanceAction>().TakeAction(gridPosition, () => Debug.Log("Dance complete"));
                    break;
                
                case ActionType.Grenade:
                    selectedUnit.GetAction<GrenadeAction>().TakeAction(gridPosition, () => Debug.Log("Grenade complete"));
                    break;
                
                case ActionType.Knife:
                    selectedUnit.GetAction<KnifeAction>().TakeAction(gridPosition, () => Debug.Log("Knife complete"));
                    break;
                
                case ActionType.Interact:
                    selectedUnit.GetAction<InteractAction>().TakeAction(gridPosition, () => Debug.Log("Interact complete"));
                    break;
                
                default:
                    Debug.LogWarning($"Unknown action type: {actionType}");
                    break;
            }
        }


        public override void FixedUpdateNetwork()
        {
            foreach (var playerRef in Runner.ActivePlayers)
            {
                NetworkInputData? maybeData = Runner.GetInputForPlayer<NetworkInputData>(playerRef);
                if (maybeData.HasValue)
                {
                    NetworkInputData data = maybeData.Value;

                    if (data.Buttons.IsSet(NetworkInputData.SelectUnit))
                    {
                        NetworkObject changeObj = Runner.FindObject(data.SelectedUnitId);
                        if (changeObj != null && changeObj.TryGetComponent<Unit>(out var changedUnit))
                        {
                            SetSelectedUnitForPlayer(playerRef, data.IsSelected ? changedUnit : null);
                        }
                    }

                    if (data.Buttons.IsSet(NetworkInputData.Mousebutton1))
                    {
                        GridPosition clickedGridPosition = new GridPosition(data.TargetGridX, data.TargetGridZ);

                        HandleSelectedAction(playerRef, data.ActionType, clickedGridPosition);
                    }
                }
            }
        }
    }
}