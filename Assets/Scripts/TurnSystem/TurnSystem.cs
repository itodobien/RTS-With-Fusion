using System;
using UnityEngine;

namespace TurnSystem
{
    public class TurnSystem : MonoBehaviour
    {
        public static TurnSystem Instance {get; private set;}
        
        public event EventHandler OnTurnChanged;
    
        private int _turnNumber;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("More than one TurnSystem in scene");
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        public void NextTurn()
        {
            _turnNumber++;
            
            OnTurnChanged?.Invoke(this, EventArgs.Empty);
        }

        public int GetTurnNumber() => _turnNumber;
    
    }
}
