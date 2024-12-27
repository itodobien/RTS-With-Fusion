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
            UnitActionSystem.Instance.SetSelectedAction(_baseAction);
        });
        textMeshProUGUI.text = _baseAction.GetActionName().ToUpper();
    }
    
    public void SetInteractable(bool isInteractable)
    {
        button.interactable = isInteractable;
    }
}
