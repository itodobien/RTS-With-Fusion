using Units;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UnitWorldUI : MonoBehaviour
    {
        [SerializeField] private Unit unit; 
        [SerializeField] private Image healthBarImage;
        [SerializeField] private HealthSystem healthSystem;
        
        public void SetHealthBar(float normalizedHealth)
        {
            healthBarImage.fillAmount = normalizedHealth;
        }
    }
}
