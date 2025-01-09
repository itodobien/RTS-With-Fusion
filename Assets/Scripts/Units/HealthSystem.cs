using System;
using UnityEngine;

namespace Units
{
    public class HealthSystem : MonoBehaviour
    {
        public event EventHandler OnDeath;
        public event EventHandler OnDamaged;
        [SerializeField] private float _health = 100f;
        [SerializeField] private float _healthMax = 100f;
    
        public void TakeDamage(int damageAmount)
        {
            _health -= damageAmount;

            if (_health < 0)
            {
                _health = 0;
            }
            OnDamaged?.Invoke(this, EventArgs.Empty);

            if (_health == 0)
            {
                Die();
            }
        }

        private void Die()
        {
            OnDeath?.Invoke(this, EventArgs.Empty);
        }
        
        public float GetHealthNormalized() => _health / _healthMax;
    }
}
