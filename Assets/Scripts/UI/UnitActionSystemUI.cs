using Unit_Activities;
using UnityEngine;
using Unit = Unit_Activities.Unit;


public class UnitActionSystemUI : MonoBehaviour
{

    [SerializeField] private Transform actionButtonPrefab;
    [SerializeField] private Transform actionButtonContainerTransform;
    private void Start()
    {
        CreateUnitActionButtons();
    }

    private void CreateUnitActionButtons()
    {
        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();

        foreach (BaseAction baseAction in selectedUnit.GetBaseActionArray())
        {
            Instantiate(actionButtonPrefab, actionButtonContainerTransform);
        }
    }
}
