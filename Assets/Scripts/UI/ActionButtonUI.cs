using Actions;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ActionButtonUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textMeshProUGUI;
        [SerializeField] private Button button;
        [SerializeField] private GameObject selectedGameObject;

        private BaseAction _baseAction;

        public void SetBaseAction(BaseAction baseAction)
        {
            _baseAction = baseAction;
            button.onClick.AddListener(() =>
            {
                UnitSelectionManager.Instance?.ForceClearSelectionBox();
                UnitActionSystem.Instance.SetSelectedAction(_baseAction);

                if (_baseAction is MoveAction)
                {
                    UnitActionSystem.Instance.SetLocalSelectedAction(ActionType.Move);
                }
                else if (_baseAction is SpinAction)
                {
                    UnitActionSystem.Instance.SetLocalSelectedAction(ActionType.Spin);
                }
                else if (_baseAction is ShootAction)
                {
                    UnitActionSystem.Instance.SetLocalSelectedAction(ActionType.Shoot);
                }
                else if (_baseAction is DanceAction)
                {
                    UnitActionSystem.Instance.SetLocalSelectedAction(ActionType.Dance);
                }
                else if (_baseAction is GrenadeAction)
                {
                    UnitActionSystem.Instance.SetLocalSelectedAction(ActionType.Grenade);
                }
                else if (_baseAction is KnifeAction)
                {
                    UnitActionSystem.Instance.SetLocalSelectedAction(ActionType.Knife);
                }
                else
                {
                    UnitActionSystem.Instance.SetLocalSelectedAction(ActionType.None);
                }
            });
            textMeshProUGUI.text = _baseAction.GetActionName().ToUpper();
        }

        public void SetInteractable(bool isInteractable)
        {
            button.interactable = isInteractable;
        }

        public void UpdateSelectedVisual()
        {
            BaseAction selectedBaseAction = UnitActionSystem.Instance.GetSelectedAction();
            selectedGameObject.SetActive(selectedBaseAction == _baseAction);
        }
    }
}