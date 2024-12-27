using Actions;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ActionButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private Button button;


    public void SetBaseAction(BaseAction _baseAction)
    {
        button.onClick.AddListener(() =>
        {
            if (_baseAction is SpinAction spinAction)
            {
                spinAction.SpinUnit();
                Debug.Log("Spinning");
            }
            else
            {
                UnitActionSystem.Instance?.SetSelectedAction(_baseAction);
            }
        });
        textMeshProUGUI.text = _baseAction.GetActionName().ToUpper();
    }
    public void SetInteractable(bool isInteractable)
    {
        button.interactable = isInteractable;
    }
}
