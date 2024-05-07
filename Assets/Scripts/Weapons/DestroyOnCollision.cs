using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Destroys the projectile upon any collision
        Destroy(gameObject);
    }
}
