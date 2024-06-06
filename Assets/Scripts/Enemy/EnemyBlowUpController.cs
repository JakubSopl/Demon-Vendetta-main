using UnityEngine;
using UnityEngine.AI;

public class EnemyBlowUpController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask whatIsGround, whatIsPlayer;

    [SerializeField] private float health;

    // Explosion settings
    [SerializeField] private float explosionRange = 5f; // The radius of the explosion
    [SerializeField] private float explosionDamage = 50f; // The damage dealt by the explosion
    [SerializeField] private GameObject explosionEffectPrefab; // Assign an explosion effect prefab here
    [SerializeField] private AudioClip explosionSound; // The explosion sound effect

    private AudioSource audioSource;

    // States
    [SerializeField] private float sightRange;
    private bool playerInSightRange;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Check for sight range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);

        if (playerInSightRange) ChasePlayer();

        // Check for explosion condition
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= explosionRange)
        {
            Explode();
        }
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void Explode()
    {
        // Play the explosion sound
        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }

        // Instantiate an explosion effect at this location
        if (explosionEffectPrefab != null)
        {
            GameObject explosionEffect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

            // Automatically destroy the explosion effect after its duration
            ParticleSystem ps = explosionEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Destroy(explosionEffect, ps.main.duration);
            }
            else
            {
                // Fallback duration in case there's no Particle System found
                Destroy(explosionEffect, 5.0f);
            }
        }

        // Check for players within the explosion range
        Collider[] playersToDamage = Physics.OverlapSphere(transform.position, explosionRange, whatIsPlayer);
        foreach (var playerCollider in playersToDamage)
        {
            // Apply damage to each player within range
            PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(explosionDamage);
            }
        }

        // Destroy the enemy game object after exploding
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Use this to visualize the range in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}
