using System.Collections;
using System.Collections.Generic;
using Actions;
using Fusion;
using Units;
using UnityEngine;

namespace UI
{
    public class UnitActionSystemUI : MonoBehaviour
    {
        [SerializeField] private Transform actionButtonPrefab;
        [SerializeField] private Transform actionButtonContainerTransform;

        private List<GameObject> _actionButtons = new();
        private NetworkRunner _runner;

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => UnitSelectionManager.Instance != null);
            UnitSelectionManager.Instance.OnSelectedUnitsChanged += UnitSelectionManager_OnSelectedUnitsChanged;
            _runner = FindObjectOfType<NetworkRunner>();
            CreateUnitActionButtons();
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
            CreateUnitActionButtons();
        }

        private void CreateUnitActionButtons()
        {
            if (UnitSelectionManager.Instance is null || actionButtonPrefab is null ||
                actionButtonContainerTransform is null)
            {
                Debug.LogError("Missing references in UnitActionSystemUI");
                return;
            }

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
                    _actionButtons.Add(actionButton);

                    bool canPerformThisAction = !firstSelectedUnit.IsBusy;
                    actionButtonUI.SetInteractable(canPerformThisAction);
                    _actionButtons.Add(actionButton);
                }
            }
        }
    }
}
