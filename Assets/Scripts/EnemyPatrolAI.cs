using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrolAI : MonoBehaviour
{
    private enum EnemyState
    {
        Patrol,
        Alert,
        Investigate,
        Chase,
        Search
    }

    [SerializeField] private Transform player;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float normalVisionDistance = 4f;
    [SerializeField] private float flashlightVisionDistance = 12f;
    [SerializeField] private float fieldOfView = 110f;
    [SerializeField] private float hearingMultiplier = 1f;
    [SerializeField] private float alertDuration = 2f;
    [SerializeField] private float searchDuration = 6f;

    private NavMeshAgent agent;
    private PlayerController playerController;
    private EnemyState state;
    private int patrolIndex;
    private float stateTimer;
    private Vector3 lastKnownPosition;
    private float attackCooldown;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (player != null)
            playerController = player.GetComponent<PlayerController>();

        GoToNextPatrolPoint();
    }

    private void Update()
    {
        if (player == null)
            return;

        attackCooldown -= Time.deltaTime;

        if (CanSeePlayer())
        {
            lastKnownPosition = player.position;
            SetState(EnemyState.Chase);
        }
        else if (CanHearPlayer())
        {
            lastKnownPosition = player.position;
            SetState(EnemyState.Alert);
        }

        switch (state)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Alert:
                Alert();
                break;
            case EnemyState.Investigate:
                Investigate();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Search:
                Search();
                break;
        }
    }

    private void Patrol()
    {
        agent.speed = 1.5f;
        if (!agent.pathPending && agent.remainingDistance < 0.4f)
            GoToNextPatrolPoint();
    }

    private void Alert()
    {
        agent.isStopped = true;
        stateTimer -= Time.deltaTime;

        Vector3 direction = lastKnownPosition - transform.position;
        if (direction.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 6f * Time.deltaTime);

        if (stateTimer <= 0f)
            SetState(EnemyState.Investigate);
    }

    private void Investigate()
    {
        agent.isStopped = false;
        agent.speed = 2.2f;
        agent.SetDestination(lastKnownPosition);

        if (!agent.pathPending && agent.remainingDistance < 0.7f)
            SetState(EnemyState.Search);
    }

    private void Chase()
    {
        agent.isStopped = false;
        agent.speed = 4f;
        agent.SetDestination(player.position);

        if (Vector3.Distance(transform.position, player.position) < 1.25f && attackCooldown <= 0f)
        {
            playerController?.TakeDamage(1);
            attackCooldown = 2f;
        }

        if (!CanSeePlayer() && Vector3.Distance(transform.position, player.position) > 6f)
            SetState(EnemyState.Search);
    }

    private void Search()
    {
        agent.isStopped = false;
        agent.speed = 1.8f;
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
            SetState(EnemyState.Patrol);
    }

    private bool CanSeePlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        float visionDistance = playerController != null && playerController.FlashlightOn ? flashlightVisionDistance : normalVisionDistance;
        if (distance > visionDistance)
            return false;

        Vector3 direction = (player.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, direction) > fieldOfView * 0.5f)
            return false;

        return !Physics.Linecast(transform.position + Vector3.up * 1.5f, player.position + Vector3.up, out RaycastHit hit)
            || hit.transform == player;
    }

    private bool CanHearPlayer()
    {
        if (playerController == null || playerController.NoiseRadius <= 0f)
            return false;

        return Vector3.Distance(transform.position, player.position) <= playerController.NoiseRadius * hearingMultiplier;
    }

    private void SetState(EnemyState nextState)
    {
        if (state == nextState)
            return;

        state = nextState;
        agent.isStopped = false;
        stateTimer = nextState == EnemyState.Alert ? alertDuration : searchDuration;
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        agent.isStopped = false;
        agent.SetDestination(patrolPoints[patrolIndex].position);
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }
}
