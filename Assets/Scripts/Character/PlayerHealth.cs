using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField]
    private float health;
    [SerializeField]
    private float maxHealth;

    public TextMeshProUGUI healthText;
    public LayerMask deadlyLayerMask;

    private void Start()
    {
        UpdateHealthUI();
    }

    private void Update()
    {
        CheckForDeadlySurface();
        CheckForMedKit();
    }

    private void CheckForDeadlySurface()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f, deadlyLayerMask))
        {
            Die();
        }
    }

    private void CheckForMedKit()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Vector3 rayOrigin = Camera.main.transform.position;
        Vector3 rayDirection = Camera.main.transform.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hitInfo, 3.0f))
        {
            if (hitInfo.collider.CompareTag("MedKit"))
            {
                RestoreHealth(20); // Restore 20 health, or any other value
                Destroy(hitInfo.collider.gameObject); // Remove the medkit from the scene

                // Optional: Add feedback for the player (e.g., sound effect, UI update)
            }
        }

        }

    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthUI();
        if (health <= 0f)
        {
            Die();
        }
    }

    private void RestoreHealth(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + health.ToString();
        }
    }

    private void Die()
    {
        Debug.Log("Player Died!");
        gameObject.SetActive(false);
    }
}