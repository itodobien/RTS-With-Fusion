using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public event EventHandler onDeath;
    private int health = 100;
    
    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;

        if (health < 0)
        {
            health = 0;
        }

        if (health == 0)
        {
            Die();
        }
    }

    private void Die()
    {
        onDeath?.Invoke(this, EventArgs.Empty);
    }
}
