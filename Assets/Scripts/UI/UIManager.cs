using Actions;
using UnityEngine;
using UI;
using Units;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject grenadeCountUIPrefab;
    private GameObject grenadeCountUIInstance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UnitSelectionManager.Instance.OnSelectedUnitsChanged += HandleUnitSelectionChanged;
    }

    private void OnDestroy()
    {
        if (UnitSelectionManager.Instance != null)
        {
            UnitSelectionManager.Instance.OnSelectedUnitsChanged -= HandleUnitSelectionChanged;
        }
    }

    private void HandleUnitSelectionChanged(object sender, System.EventArgs e)
    {
        var selectedUnits = UnitSelectionManager.Instance.GetSelectedUnits();
        if (selectedUnits.Count > 0)
        {
            Unit selectedUnit = selectedUnits[0];
            if (selectedUnit.GetAction<GrenadeAction>() != null)
            {
                CreateGrenadeCountUI();
            }
            else
            {
                DestroyGrenadeCountUI();
            }
        }
        else
        {
            DestroyGrenadeCountUI();
        }
    }

    private void CreateGrenadeCountUI()
    {
        if (grenadeCountUIInstance == null)
        {
            grenadeCountUIInstance = Instantiate(grenadeCountUIPrefab, transform);
        }
    }

    private void DestroyGrenadeCountUI()
    {
        if (grenadeCountUIInstance != null)
        {
            Destroy(grenadeCountUIInstance);
            grenadeCountUIInstance = null;
        }
    }
}