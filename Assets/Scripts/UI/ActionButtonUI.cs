using Actions;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
            UnitActionSystem.Instance?.SetSelectedAction(_baseAction);

            if (baseAction is SpinAction)
            {
                UnitActionSystem.Instance?.RequestSpin();
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
