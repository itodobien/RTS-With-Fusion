using System;
using UnityEngine;

namespace Units
{
    public class HealthSystem : MonoBehaviour
    {
        public event EventHandler OnDeath;
        private int _health = 100;
    
        public void TakeDamage(int damageAmount)
        {
            _health -= damageAmount;

            if (_health < 0)
            {
                _health = 0;
            }

            if (_health == 0)
            {
                Die();
            }
        }

        private void Die()
        {
            OnDeath?.Invoke(this, EventArgs.Empty);
        }
    }
}
