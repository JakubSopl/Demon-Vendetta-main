using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField]
    private float health;
    [SerializeField]
    private float maxHealth; // Maximum health for reference or future use

    // UI Element for displaying health
    public TextMeshProUGUI healthText; // Assign in the inspector

    private void Start()
    {
        UpdateHealthUI(); // Initial UI update
    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        health = Mathf.Clamp(health, 0, maxHealth); // Ensures health stays within bounds

        UpdateHealthUI(); // Update UI whenever health changes

        if (health <= 0f)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        // Update the health text to display the current health
        if (healthText != null)
        {
            healthText.text = "Health: " + health.ToString();
        }
    }

    private void Die()
    {
        // Handle player death here (e.g., show game over screen, respawn, etc.)
        Debug.Log("Player Died!");
        // Optionally disable the player gameObject or components to simulate death
        gameObject.SetActive(false);
    }
}
