using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileCollision : MonoBehaviour
{
    public GameObject explosionPrefab; // Assign this in the inspector
    public float destructionDelay = 1.0f; // Time in seconds before the explosion is destroyed

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the projectile hits any collider
        if (collision.collider)
        {
            // Instantiate the explosion prefab at the projectile's position and rotation
            GameObject explosionInstance = Instantiate(explosionPrefab, transform.position, transform.rotation);

            // Destroy the explosion instance after a delay
            Destroy(explosionInstance, destructionDelay);

            // Destroy the projectile immediately
            Destroy(gameObject);
        }
    }
}
