using System.Collections;
using System.Collections.Generic;
using Unit_Activities;
using UnityEngine;

public class UnitActionSystemUI : MonoBehaviour
{
    [SerializeField] private Transform actionButtonPrefab;
    [SerializeField] private Transform actionButtonContainerTransform;

    private List<GameObject> actionButtons = new ();

    private IEnumerator Start()
    {
        // Wait until UnitSelectionManager.Instance is available
        yield return new WaitUntil(() => UnitSelectionManager.Instance != null);

        // Subscribe to the OnSelectedUnitsChanged event
        UnitSelectionManager.Instance.OnSelectedUnitsChanged += UnitSelectionManager_OnSelectedUnitsChanged;

        // Initialize action buttons
        CreateUnitActionButtons();
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to prevent memory leaks
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
        // Ensure references are valid
        if (UnitSelectionManager.Instance == null || actionButtonPrefab == null || actionButtonContainerTransform == null)
        {
            Debug.LogError("Missing references in UnitActionSystemUI");
            return;
        }

        // Clear existing buttons
        foreach (GameObject button in actionButtons)
        {
            Destroy(button);
        }
        actionButtons.Clear();

        // Get selected units
        var selectedUnits = UnitSelectionManager.Instance.GetSelectedUnits();
        if (selectedUnits.Count > 0)
        {
            Unit firstSelectedUnit = selectedUnits[0];

            foreach (BaseAction baseAction in firstSelectedUnit.GetBaseActionArray())
            {
                GameObject actionButton = Instantiate(actionButtonPrefab.gameObject, actionButtonContainerTransform);
                actionButtons.Add(actionButton);
            }
        }
    }
}
