using UnityEngine;
using TMPro; // Make sure to include this namespace for TextMeshPro elements

public class PlayerHealth : MonoBehaviour
{
    [SerializeField]
    private float health;
    [SerializeField]
    private float maxHealth; // Maximum health for reference or future use

    // UI Element for displaying health
    public TextMeshProUGUI healthText; // Assign in the inspector

    // LayerMask to specify which layer is considered deadly
    public LayerMask deadlyLayerMask;

    private void Start()
    {
        UpdateHealthUI(); // Initial UI update
    }

    private void Update()
    {
        CheckForDeadlySurface();
    }

    private void CheckForDeadlySurface()
    {
        RaycastHit hit;
        // Cast a ray straight down from the player's position
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f, deadlyLayerMask)) // Adjust the distance as needed
        {
            // If the ray hits a surface on the deadly layer, trigger death
            Die();
        }
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