using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum State
    {
        Patrol,
        Chase,
        Attack
    }

    [SerializeField] private State currentState;
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private int damage = 20;
    [SerializeField] private float attackCooldown = 1.5f;

    private int currentWaypoint;
    private float lastAttackTime = -999f;
    private NavMeshAgent agent;
    private Transform player;
    private PlayerHealth playerHealth;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            playerHealth = playerObject.GetComponent<PlayerHealth>();
        }

        currentState = State.Patrol;
        if (waypoints.Length > 0)
            agent.SetDestination(waypoints[0].position);
    }

    private void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                if (distance <= chaseRange && HasLineOfSight())
                    currentState = State.Chase;
                break;
            case State.Chase:
                Chase();
                if (distance > chaseRange)
                    currentState = State.Patrol;
                else if (distance <= attackRange)
                    currentState = State.Attack;
                break;
            case State.Attack:
                Attack();
                if (distance > attackRange)
                    currentState = State.Chase;
                break;
        }
    }

    private void Patrol()
    {
        agent.isStopped = false;
        agent.speed = 1.6f;

        if (waypoints.Length == 0)
            return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            agent.SetDestination(waypoints[currentWaypoint].position);
        }
    }

    private void Chase()
    {
        agent.isStopped = false;
        agent.speed = 3.6f;
        agent.SetDestination(player.position);
    }

    private void Attack()
    {
        agent.isStopped = true;

        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(lookDirection);

        if (Time.time >= lastAttackTime + attackCooldown && HasLineOfSight())
        {
            playerHealth?.TakeDamage(damage);
            lastAttackTime = Time.time;
        }

        agent.isStopped = false;
    }

    private bool HasLineOfSight()
    {
        Vector3 from = transform.position + Vector3.up * 1.5f;
        Vector3 to = player.position + Vector3.up;

        return !Physics.Linecast(from, to, out RaycastHit hit) || hit.transform == player;
    }
}
