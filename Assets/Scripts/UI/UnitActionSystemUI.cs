using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unit_Activities;
using UnityEngine;

public class UnitActionSystemUI : MonoBehaviour
{
    [SerializeField] private Transform actionButtonPrefab;
    [SerializeField] private Transform actionButtonContainerTransform;

    private List<GameObject> actionButtons = new ();
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
        if (UnitSelectionManager.Instance == null || actionButtonPrefab == null || actionButtonContainerTransform == null)
        {
            Debug.LogError("Missing references in UnitActionSystemUI");
            return;
        }

        foreach (GameObject button in actionButtons)
        {
            Destroy(button);
        }
        actionButtons.Clear();

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
