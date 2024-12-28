using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TurnSystem
{
    public class TurnSystemUI : MonoBehaviour
    {
        [SerializeField] private Button _endTurnButton;
        [SerializeField] private TextMeshProUGUI _turnNumberText;

        private void Start()
        {
            _endTurnButton.onClick.AddListener(() =>
            {
                TurnSystem.Instance?.NextTurn();
            });
            TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
        
            UpdateTurnText();
        }

        private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
        {
            UpdateTurnText();
        }

        private void UpdateTurnText()
        {
            _turnNumberText.text = "TURN " + TurnSystem.Instance.GetTurnNumber();
        }
    }
}
