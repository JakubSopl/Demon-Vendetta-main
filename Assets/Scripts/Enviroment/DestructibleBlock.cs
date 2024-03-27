using UnityEngine;
using System.Collections;

public class DestructibleBlock : MonoBehaviour
{
    public Transform playerTransform;
    public Material newMaterial;
    public float disappearDelay = 2f;
    public float respawnDelay = 3f;
    private bool isDisappearing = false;

    private Renderer blockRenderer;
    private Collider blockCollider; // Reference to the block's collider
    private Material originalMaterial;

    private void Start()
    {
        blockRenderer = GetComponent<Renderer>();
        blockCollider = GetComponent<Collider>(); // Get the collider component
        originalMaterial = blockRenderer.material;
    }

    private void Update()
    {
        if (!isDisappearing && blockCollider.enabled) // Check if the block's collider is enabled
        {
            RaycastHit hit;
            if (Physics.Raycast(playerTransform.position, Vector3.down, out hit))
            {
                if (hit.collider.gameObject == gameObject && hit.distance < 1f)
                {
                    TriggerEffect();
                }
            }
        }
    }

    private void TriggerEffect()
    {
        blockRenderer.material = newMaterial;
        isDisappearing = true;
        StartCoroutine(DisappearAndRespawnAfterDelay());
    }

    private IEnumerator DisappearAndRespawnAfterDelay()
    {
        yield return new WaitForSeconds(disappearDelay);
        blockRenderer.enabled = false; // Disable the renderer instead of the entire GameObject
        blockCollider.enabled = false; // Disable the collider
        yield return new WaitForSeconds(respawnDelay);
        blockRenderer.enabled = true; // Enable the renderer
        blockCollider.enabled = true; // Enable the collider
        blockRenderer.material = originalMaterial;
        isDisappearing = false;
    }
}
