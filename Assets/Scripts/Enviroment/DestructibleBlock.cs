using System.Collections;
using UnityEngine;

public class DestructibleBlock : MonoBehaviour
{
    public Transform playerTransform; // Assign your player's Transform in the inspector
    public Material newMaterial; // Assign the new material in the inspector
    public float disappearDelay = 2f; // Time before the block disappears after the player steps on it
    private bool isDisappearing = false; // Flag to ensure we only start the coroutine once

    private Renderer blockRenderer; // To hold the renderer component of the block

    private void Start()
    {
        blockRenderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        if (!isDisappearing)
        {
            RaycastHit hit;
            // Cast a ray downward from the player's position to detect the block
            if (Physics.Raycast(playerTransform.position, Vector3.down, out hit))
            {
                // Check if the ray hits this block and is within a certain distance (e.g., just above the block)
                if (hit.collider.gameObject == gameObject && hit.distance < 1f) // Adjust distance as needed
                {
                    TriggerEffect();
                }
            }
        }
    }

    private void TriggerEffect()
    {
        // Change the material of the block as soon as the player is detected right above it
        blockRenderer.material = newMaterial;
        isDisappearing = true;
        // Start the coroutine to disappear the block after a delay
        StartCoroutine(DisappearAfterDelay());
    }

    private IEnumerator DisappearAfterDelay()
    {
        yield return new WaitForSeconds(disappearDelay);
        gameObject.SetActive(false); // Hides the block, or you could use Destroy(gameObject) to completely remove it
    }
}
