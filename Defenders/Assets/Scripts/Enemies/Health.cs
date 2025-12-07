using System;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public UnityEvent<float, float> onHealthChange;
    public UnityEvent onDeath;

    private void Start()
    {
        currentHealth = maxHealth;
        onHealthChange?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        onHealthChange?.Invoke(currentHealth, maxHealth);
        if (currentHealth == 0)
            onDeath?.Invoke();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        onHealthChange?.Invoke(currentHealth, maxHealth);
    }
}
