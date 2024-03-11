using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask whatIsGround, whatIsPlayer;

    [SerializeField] private float health;

    // Patroling
    [SerializeField] private Vector3 walkPoint;
    bool walkPointSet;
    [SerializeField] private float walkPointRange;

    // Attacking
    [SerializeField] private float timeBetweenAttacks;
    bool alreadyAttacked;
    [SerializeField] private GameObject projectilePrefab; // Assign this in the Inspector
    [SerializeField] private Transform firePoint; // The point from which the projectile will be fired
    [SerializeField] private float projectileSpeed = 1000f; // Adjust based on your needs
    [SerializeField] private float projectileLifetime = 5f; // Lifetime of the projectile in seconds
    [SerializeField] private float attackDamage = 10f; // Damage dealt to the player


    // States
    [SerializeField] private float sightRange, attackRange;
    [SerializeField] private bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        // Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        // Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position); // Make sure the enemy doesn't move

        transform.LookAt(player); // Ensure the enemy faces the player

        if (!alreadyAttacked)
        {
            // Check for a clear line of sight using a raycast
            RaycastHit hit;
            Vector3 direction = (player.position - transform.position).normalized;

            // Perform the raycast from the enemy's position to the player's position
            if (Physics.Raycast(transform.position, direction, out hit))
            {
                // Check if the raycast hit the player
                if (hit.transform == player)
                {
                    // The enemy has a clear line of sight to the player, proceed with the attack

                    // Calculate the rotation needed for the projectile to face the player
                    Quaternion projectileRotation = Quaternion.LookRotation(direction);

                    // Instantiate the projectile at the firePoint position with the calculated rotation
                    GameObject projectile = Instantiate(projectilePrefab, firePoint.position, projectileRotation);

                    // Get the Rigidbody component of the projectile
                    Rigidbody rb = projectile.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        // Apply a forward force to the projectile in the direction it's facing
                        rb.AddForce(direction * projectileSpeed, ForceMode.VelocityChange);
                    }

                    // Optionally, destroy the projectile after 'projectileLifetime' seconds to clean up
                    Destroy(projectile, projectileLifetime);

                    // Deal damage to the player directly (without projectiles)
                    PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(attackDamage);
                    }

                    alreadyAttacked = true;
                    Invoke(nameof(ResetAttack), timeBetweenAttacks); // Reset the attack flag after a delay
                }
            }
        }
    }


    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
