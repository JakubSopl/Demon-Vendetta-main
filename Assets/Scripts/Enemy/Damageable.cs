using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // Adjusted to return bool
    public bool ApplyDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
            return true; // Indicate the enemy has died
        }
        return false; // Indicate the enemy is still alive
    }

    void Die()
    {
        Destroy(gameObject); // Destroy the enemy object
    }
}

