using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask whatIsGround, whatIsPlayer;

    // Flamethrower components
    [SerializeField] private GameObject ignitionFlame;
    [SerializeField] private GameObject flame;
    [SerializeField] private GameObject lightEffect;

    // Patroling
    [SerializeField] private Vector3 walkPoint;
    bool walkPointSet;
    [SerializeField] private float walkPointRange;

    // Attacking
    [SerializeField] private float timeBetweenAttacks;
    private float lastAttackTime = -999;
    [SerializeField] private float attackDamage = 10f; // Damage per second to the player
    [SerializeField] private float rotationSpeed = 5f; // Speed of rotation towards the player

    // Distance management
    [SerializeField] private float minimumDistance = 8f; // Minimum distance from the player

    // States
    [SerializeField] private float sightRange, attackRange;
    private bool flamethrowerActive = false;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();

        // Initially disable flamethrower effects
        ignitionFlame.SetActive(false);
        flame.SetActive(false);
        lightEffect.SetActive(false);
    }

    private void Update()
    {
        // Check for sight and attack range
        bool playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        bool playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();

        // Rotate towards player if flamethrower is active
        if (flamethrowerActive)
        {
            RotateTowardsPlayer();
        }
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
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        if (Vector3.Distance(transform.position, player.position) > minimumDistance)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.SetDestination(transform.position); // Stop moving if too close
        }
    }

    private void AttackPlayer()
    {
        if (!flamethrowerActive)
        {
            EnableFlamethrowerEffects();
            flamethrowerActive = true;
        }

        if (Vector3.Distance(transform.position, player.position) > minimumDistance)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.SetDestination(transform.position); // Maintain minimum distance
        }

        // Apply damage if within attack range and cooldown has passed
        if (Time.time >= lastAttackTime + timeBetweenAttacks)
        {
            lastAttackTime = Time.time;
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    private void RotateTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void EnableFlamethrowerEffects()
    {
        ignitionFlame.SetActive(true);
        flame.SetActive(true);
        lightEffect.SetActive(true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
