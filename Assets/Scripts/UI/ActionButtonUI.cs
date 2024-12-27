using Actions;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ActionButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private Button button;
    
    private BaseAction _baseAction;

    public void SetBaseAction(BaseAction _baseAction)
    {
        this._baseAction = _baseAction;
        textMeshProUGUI.text = _baseAction.GetActionName().ToUpper();
    }
    
    public void SetInteractable(bool isInteractable)
    {
        button.interactable = isInteractable;
    }
}
