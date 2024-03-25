using System.Collections;
using UnityEngine;

public class DestructibleBlock : MonoBehaviour
{
    public float destructionDelay = 2.0f; // Time in seconds before the block is destroyed

    private void OnTriggerEnter(Collider other)
    {
        // Checks if the collider entering the trigger zone is the player
        if (other.GetComponent<CharacterController>() != null)
        {
            Debug.Log("Player has entered the trigger zone. Starting countdown to destruction.", this);
            StartCoroutine(DestroyAfterDelay());
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        // Waits for the specified delay
        yield return new WaitForSeconds(destructionDelay);
        Debug.Log("Destruction countdown completed. Destroying the block.", this);

        Destroy(gameObject); // Destroys this block GameObject
    }
}
