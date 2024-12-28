using System.Collections;
using System.Collections.Generic;
using Actions;
using Fusion;
using JetBrains.Annotations;
using Units;
using UnityEngine;

namespace UI
{
    public class UnitActionSystemUI : MonoBehaviour
    {
        [SerializeField] private Transform actionButtonPrefab;
        [SerializeField] private Transform actionButtonContainerTransform;
        
        private List<ActionButtonUI> _actionButtonUIList = new();

        private readonly List<GameObject> _actionButtons = new();
        [UsedImplicitly] private NetworkRunner _runner;

        private void Awake()
        {
            _actionButtonUIList = new List<ActionButtonUI>();
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => UnitSelectionManager.Instance != null && UnitActionSystem.Instance != null);
            UnitSelectionManager.Instance.OnSelectedUnitsChanged += UnitSelectionManager_OnSelectedUnitsChanged;
            UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged;
            _runner = FindObjectOfType<NetworkRunner>();
            CreateUnitActionButtons();
        }

        private void OnDestroy()
        {
            if (UnitSelectionManager.Instance != null)
            {
                UnitSelectionManager.Instance.OnSelectedUnitsChanged -= UnitSelectionManager_OnSelectedUnitsChanged;
            }
            
            if (UnitActionSystem.Instance != null)
            {
                UnitActionSystem.Instance.OnSelectedActionChanged -= UnitActionSystem_OnSelectedActionChanged;
            }
        }

        private void UnitSelectionManager_OnSelectedUnitsChanged(object sender, System.EventArgs e)
        {
            CreateUnitActionButtons();
            UpdateSelectedVisuals();
        }
        
        private void UnitActionSystem_OnSelectedActionChanged(object sender, System.EventArgs e)
        {
            UpdateSelectedVisuals();
        }

        private void CreateUnitActionButtons()
        {
            if (UnitSelectionManager.Instance is null || actionButtonPrefab is null || actionButtonContainerTransform is null)
            {
                Debug.LogError("Missing references in UnitActionSystemUI");
                return;
            }
            
            _actionButtonUIList.Clear();

            foreach (GameObject button in _actionButtons)
            {
                Destroy(button); 
            }
            _actionButtons.Clear();

            var selectedUnits = UnitSelectionManager.Instance.GetSelectedUnits();
            if (selectedUnits.Count > 0)
            {
                Unit firstSelectedUnit = selectedUnits[0];

                foreach (BaseAction baseAction in firstSelectedUnit.GetBaseActionArray())
                {
                    GameObject actionButton = Instantiate(actionButtonPrefab.gameObject, actionButtonContainerTransform);
                    ActionButtonUI actionButtonUI = actionButton.GetComponent<ActionButtonUI>();
                    actionButtonUI.SetBaseAction(baseAction);
                    
                    bool canPerformThisAction = !firstSelectedUnit.IsBusy;
                    actionButtonUI.SetInteractable(canPerformThisAction);
                    _actionButtons.Add(actionButton);
                    _actionButtonUIList.Add(actionButtonUI);
                    
                }
            }
        }
        private void UpdateSelectedVisuals()
        {
            foreach (ActionButtonUI actionButtonUI in _actionButtonUIList)
            {
                actionButtonUI.UpdateSelectedVisual();
            }
        }
    }
}