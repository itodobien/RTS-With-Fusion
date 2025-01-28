using UnityEngine;
using TMPro;
using Units;
using Actions;

namespace UI
{
    public class GrenadeCountUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI grenadeCountText;
        [SerializeField] private GameObject grenadeIcon;

        private int _currentGrenadeCount;

        private void Start()
        {
            UnitSelectionManager.Instance.OnSelectedUnitsChanged += UpdateGrenadeCount;
            UnitActionSystem.Instance.OnSelectedActionChanged += UpdateGrenadeCount;
            UpdateGrenadeCount(null, System.EventArgs.Empty);
        }

        private void OnDestroy()
        {
            if (UnitSelectionManager.Instance != null)
            {
                UnitSelectionManager.Instance.OnSelectedUnitsChanged -= UpdateGrenadeCount;
            }
            if (UnitActionSystem.Instance != null)
            {
                UnitActionSystem.Instance.OnSelectedActionChanged -= UpdateGrenadeCount;
            }
        }

        private void UpdateGrenadeCount(object sender, System.EventArgs e)
        {
            var selectedUnits = UnitSelectionManager.Instance.GetSelectedUnits();
            if (selectedUnits.Count > 0)
            {
                Unit selectedUnit = selectedUnits[0];
                GrenadeAction grenadeAction = selectedUnit.GetAction<GrenadeAction>();
                if (grenadeAction != null)
                {
                    int newGrenadeCount = grenadeAction.GetGrenadeAmount();
                    SetGrenadeCount(newGrenadeCount);
                    SubscribeToGrenadeAction(grenadeAction);
                }
            }
        }

        private void SetGrenadeCount(int newCount)
        {
            if (newCount != _currentGrenadeCount)
            {
                _currentGrenadeCount = newCount;
                grenadeCountText.text = newCount.ToString();
            }
        }

        private void SubscribeToGrenadeAction(GrenadeAction grenadeAction)
        {
            grenadeAction.OnGrenadeAmountChanged -= HandleGrenadeAmountChanged;
            grenadeAction.OnGrenadeAmountChanged += HandleGrenadeAmountChanged;
        }

        private void HandleGrenadeAmountChanged(object sender, System.EventArgs e)
        {
            if (sender is GrenadeAction grenadeAction)
            {
                SetGrenadeCount(grenadeAction.GetGrenadeAmount());
            }
        }
    }
}
