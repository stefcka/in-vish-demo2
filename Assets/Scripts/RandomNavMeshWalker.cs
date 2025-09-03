using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RandomNavMeshWalker : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3.5f;
    public float arrivalRadius = 1.0f;
    public float roamRadius = 10.0f;
    public float rotationSpeed = 5.0f;

    private NavMeshAgent agent;
    private Vector3 startPosition;
    private NavMeshPath testPath;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;          // we handle rotation manually
        agent.autoBraking = false;            // smooth, continuous motion
        agent.speed = walkSpeed;

        startPosition = transform.position;
        testPath = new NavMeshPath();

        PickNewDestination();
    }

    void Update()
    {
        // Keep agent speed in sync
        if (agent.speed != walkSpeed)
            agent.speed = walkSpeed;

        // Snap transform to agent's internal position for jitter-free movement
        transform.position = agent.nextPosition;

        // Only rotate if we're actually moving
        Vector3 moveDir = agent.desiredVelocity;
        moveDir.y = 0f; // ignore vertical components
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }

        // Pick a new point when within stopping distance
        if (!agent.pathPending && agent.remainingDistance <= arrivalRadius)
            PickNewDestination();
    }

    private void PickNewDestination()
    {
        Vector3 cand;
        if (GetNavigableRandomPoint(startPosition, roamRadius, out cand))
            agent.SetDestination(cand);
    }

    private bool GetNavigableRandomPoint(Vector3 center, float maxDist, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 rnd = center + Random.insideUnitSphere * maxDist;
            NavMeshHit hit;
            if (!NavMesh.SamplePosition(rnd, out hit, 1f, NavMesh.AllAreas))
                continue;

            if (!NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, testPath)
                || testPath.status != NavMeshPathStatus.PathComplete)
                continue;

            result = hit.position;
            return true;
        }

        result = transform.position;
        return false;
    }
}
