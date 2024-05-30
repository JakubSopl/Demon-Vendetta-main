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
    public GameObject playerDiedMenuCanvas;
    public GameObject[] weapons; // Assign the weapon game objects in the inspector

    private float voidThreshold = -10f; // Height threshold to trigger the void effect
    private bool isFading = false;
    private float fadeDuration = 2.0f;
    private float fadeTimer = 0.0f;
    private bool isDying = false;
    private bool fallingIntoVoid = false;

    private bool isOnFire = false; // Flag to check if the player is on fire
    private float fireDamage = 5.0f; // Amount of damage per second from fire
    private float fireCheckInterval = 1.0f; // Time interval between fire checks
    private float fireCheckTimer = 0.0f; // Timer to track fire check intervals

    private void Start()
    {
        UpdateHealthUI();
    }

    private void Update()
    {
        //CheckForDeadlySurface();
        CheckForMedKit();
        CheckForVoidFall();

        CheckForFire();
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
        if (transform.position.y < voidThreshold && !isFading && !isDying)
        {
            isFading = true; // Start the fade effect
            StartCoroutine(FadeToBlackAndDie());
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

            if (!fallingIntoVoid)
            {
                StartCoroutine(FallAndFade());
            }
            else
            {
                // Enable the Player Died Menu Canvas
                playerDiedMenuCanvas.SetActive(true);

                // Unlock the cursor
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }



    private void CheckForFire()
    {
        fireCheckTimer += Time.deltaTime;
        if (fireCheckTimer >= fireCheckInterval)
        {
            fireCheckTimer = 0.0f;

            Ray ray = new Ray(feetTransform.position, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1.5f))
            {
                if (hit.collider.CompareTag("Fire"))
                {
                    if (!isOnFire)
                    {
                        isOnFire = true;
                        StartCoroutine(TakeFireDamage());
                    }
                }
                else
                {
                    isOnFire = false;
                }
            }
            else
            {
                isOnFire = false;
            }
        }
    }

    private IEnumerator FadeToBlackAndDie()
    {
        while (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(fadeTimer / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha); // Increase the transparency to create a fade effect
            yield return null;
        }

        // Ensure the screen stays black
        fadeImage.color = new Color(0, 0, 0, 1);

        Die(true);
    }


    private IEnumerator TakeFireDamage()
    {
        while (isOnFire)
        {
            TakeDamage(fireDamage);
            yield return new WaitForSeconds(1.0f); // Take damage every second
        }
    }
    private IEnumerator FallAndFade()
    {
        // Start fading immediately
        fadeTimer = 0.0f; // Reset the fade timer

        if (!fallingIntoVoid)
        {
            StartCoroutine(FadeScreen());

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


        // Enable the Player Died Menu Canvas
        playerDiedMenuCanvas.SetActive(true);

        // Unlock the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        //DeactivateAllExceptMainCameraAndMenu();
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

    }

}