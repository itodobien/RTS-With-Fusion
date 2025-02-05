using Actions;
using Units;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager Instance { get; set; }

        [SerializeField] private GameObject grenadeCountUIPrefab;
        private GameObject _grenadeCountUIInstance;

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
            if (_grenadeCountUIInstance == null)
            {
                _grenadeCountUIInstance = Instantiate(grenadeCountUIPrefab, transform);
            }
        }

        private void DestroyGrenadeCountUI()
        {
            if (_grenadeCountUIInstance != null)
            {
                Destroy(_grenadeCountUIInstance);
                _grenadeCountUIInstance = null;
            }
        }
    }
}