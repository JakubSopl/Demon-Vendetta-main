using UnityEngine;
using UnityEngine.AI;

public class EnemyMeeleController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask whatIsGround, whatIsPlayer;

    [SerializeField] private float health;

    // Patroling
    [SerializeField] private Vector3 walkPoint;
    private bool walkPointSet;
    [SerializeField] private float walkPointRange;

    // Attacking
    [SerializeField] private float timeBetweenAttacks;
    private bool alreadyAttacked;
    [SerializeField] private float attackDamage = 10f; // Damage dealt to the player

    // States
    [SerializeField] private float sightRange, attackRange;
    private bool playerInSightRange, playerInAttackRange;

    // Audio
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip deathSound;
    private AudioSource audioSource;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
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
        agent.SetDestination(transform.position); // Stop the enemy's movement

        // Ensure the enemy faces the player only if they are on the same horizontal plane
        float yDifference = Mathf.Abs(transform.position.y - player.position.y);

        if (yDifference < 1.0f) // You can adjust this threshold according to your needs
        {
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }

        if (!alreadyAttacked)
        {
            // Check for a clear line of sight to the player
            RaycastHit hit;
            Vector3 direction = (player.position - transform.position).normalized;

            if (Physics.Raycast(transform.position, direction, out hit, attackRange))
            {
                // Check if the raycast hit the player
                if (hit.transform == player)
                {
                    // The enemy is close enough and has a clear line of sight to the player, proceed with the melee attack
                    // Here you can also trigger an attack animation if your enemy model has animations
                    // Animator.SetTrigger("Attack");

                    // Directly deal damage to the player
                    PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(attackDamage);

                        // Play attack sound
                        if (attackSound != null && audioSource != null)
                        {
                            audioSource.PlayOneShot(attackSound);
                        }
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

    private void OnDestroy()
    {
        // Play death sound
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
