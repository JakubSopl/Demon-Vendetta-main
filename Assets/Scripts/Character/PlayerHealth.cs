using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField]
    private float health;
    [SerializeField]
    private float maxHealth;

    public TextMeshProUGUI healthText;
    public LayerMask deadlyLayerMask;


    public Image fadeImage; // Assign this in the inspector
    public Transform cameraTransform; // Assign the camera transform in the inspector
    public Transform feetTransform;
    public GameObject[] uiCanvases; // Assign the UI canvases in the inspector
    public GameObject[] weapons; // Assign the weapon game objects in the inspector

    private float voidThreshold = -10f; // Height threshold to trigger the void effect
    private bool isFading = false;
    private float fadeDuration = 2.0f;
    private float fadeTimer = 0.0f;
    private bool isDying = false;
    private bool fallingIntoVoid = false;

    private void Start()
    {
        UpdateHealthUI();
    }

    private void Update()
    {
        //CheckForDeadlySurface();
        CheckForMedKit();
        CheckForVoidFall();
    }

    private void CheckForDeadlySurface()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f, deadlyLayerMask))
        {
            Die(false);
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

    private void CheckForVoidFall()
    {
        if (transform.position.y < voidThreshold && !isFading)
        {
            isFading = true; // Start the fade effect
        }

        if (isFading)
        {
            if (fadeTimer < fadeDuration)
            {
                fadeTimer += Time.deltaTime;
                float alpha = Mathf.Clamp01(fadeTimer / fadeDuration);
                fadeImage.color = new Color(0, 0, 0, alpha); // Increase the transparency to create a fade effect
            }
            else
            {
                Die(true); // The player dies when the fade is complete
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
            Die(false);
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

    private void Die(bool isVoidFall)
    {
        if (!isDying)
        {
            isDying = true;
            fallingIntoVoid = isVoidFall;
            Debug.Log("Player Died!");

            // Disable all weapons
            foreach (var weapon in weapons)
            {
                weapon.SetActive(false);
            }

            // Disable all UI canvases
            foreach (var canvas in uiCanvases)
            {
                canvas.SetActive(false);
            }

            StartCoroutine(FallAndFade());
        }
    }

    private IEnumerator FallAndFade()
    {
        // Start fading immediately
        fadeTimer = 0.0f; // Reset the fade timer
        StartCoroutine(FadeScreen());

        if (!fallingIntoVoid)
        {
            // Fall effect
            float fallDuration = 2.0f;
            float fallTimer = 0.0f;
            Quaternion initialRotation = cameraTransform.rotation;
            Quaternion finalRotation = Quaternion.Euler(cameraTransform.eulerAngles.x, cameraTransform.eulerAngles.y, cameraTransform.eulerAngles.z + 90); // Rotate 90 degrees on the Z axis

            Vector3 initialPosition = transform.position;
            Vector3 finalPosition = new Vector3(transform.position.x, feetTransform.position.y, transform.position.z);

            while (fallTimer < fallDuration)
            {
                fallTimer += Time.deltaTime;
                float t = fallTimer / fallDuration;
                cameraTransform.rotation = Quaternion.Slerp(initialRotation, finalRotation, t);
                transform.position = Vector3.Lerp(initialPosition, finalPosition, t); // Move the player down to the ground level
                yield return null;
            }
        }

        // Ensure the player dies after the fade out
        gameObject.SetActive(false); // The player dies when the process is complete
    }

    private IEnumerator FadeScreen()
    {
        while (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(fadeTimer / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha); // Increase the transparency to create a fade effect
            yield return null;
        }

        if (fallingIntoVoid)
        {
            // Disable the player object after fade out
            gameObject.SetActive(false);
        }
    }
}