using UnityEngine;

public class DroneEnemyController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask whatIsGround, whatIsPlayer;

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 1000f;
    [SerializeField] private float projectileLifetime = 5f;
    [SerializeField] private float attackDamage = 10f;

    [SerializeField] private float health;
    [SerializeField] private float maxAltitude = 50f;
    [SerializeField] private float minAltitude = 10f;
    [SerializeField] private float patrolSpeed = 5f;
    [SerializeField] private float chaseSpeed = 10f;
    [SerializeField] private float sightRange, attackRange;
    private bool alreadyAttacked;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
    }

    private void Update()
    {
        float playerDistance = Vector3.Distance(transform.position, player.position);
        bool playerInSightRange = playerDistance <= sightRange;
        bool playerInAttackRange = playerDistance <= attackRange;

        MaintainAltitude();

        if (!playerInSightRange && !playerInAttackRange) Patrol();
        else if (playerInSightRange && !playerInAttackRange) Chase();
        else if (playerInAttackRange) Attack();
    }

    private void MaintainAltitude()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, maxAltitude + 5f, whatIsGround))
        {
            float targetAltitude = Mathf.Clamp(hit.distance, minAltitude, maxAltitude);
            Vector3 pos = transform.position;
            pos.y += (targetAltitude - hit.distance) * Time.deltaTime;
            transform.position = pos;
        }
    }

    private void Patrol()
    {
        // Simplified patrol logic: hover in place or move randomly
        // Implement as needed for your game design
    }

    private void Chase()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized * chaseSpeed;
        dirToPlayer.y = 0; // Maintain altitude
        transform.position += dirToPlayer * Time.deltaTime;
    }



    private void Attack()
    {
        if (!alreadyAttacked)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            Quaternion projectileRotation = Quaternion.LookRotation(direction);
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, projectileRotation);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(direction * projectileSpeed, ForceMode.VelocityChange);
            }
            Destroy(projectile, projectileLifetime); // Clean up the projectile

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), attackDamage); // Reset the attack flag after a delay
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
}
