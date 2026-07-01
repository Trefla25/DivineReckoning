using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(StatController))]
public class PlayerMovementUtils : MonoBehaviour
{
    float lookRotationSpeed = 8f;
    NavMeshAgent agent;
    StatController stats;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<StatController>();
    }

    void Update() => agent.speed = stats.GetValue(StatType.MoveSpeed); // reflects slows/hastes live

    // Small tolerance instead of exact zero: near corners/arrival the velocity can
    // flicker around zero, which would otherwise flip-flop Idle<->Walk every frame.
    public bool IsStopped => agent.velocity.sqrMagnitude < 0.01f;

    public bool HasArrived =>
        !agent.pathPending &&
        agent.remainingDistance <= agent.stoppingDistance &&
        (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f);

    // Assigning agent.destination triggers a path recompute, so skip redundant
    // repaths (chase + hold-to-move call this ~every frame). 0.0625 = (0.25m)^2.
    public void MoveTo(Vector3 point)
    {
        if ((point - agent.destination).sqrMagnitude < 0.0625f) return;
        agent.destination = point;
    }

    public void Stop()
    {
        if (!IsStopped) agent.SetDestination(transform.position);
    }

    public void FaceMoveDirection()
    {
        Vector3 direction = agent.desiredVelocity;
        Vector3 flatDirection = new(direction.x, 0, direction.z);
        if (flatDirection.sqrMagnitude < 0.001f) return;

        Quaternion lookRotation = Quaternion.LookRotation(flatDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookRotationSpeed);
    }

    public void FaceTowards(Vector3 worldPoint)
    {
        Vector3 direction = worldPoint - transform.position;
        direction.y = 0f;                                  // keep upright; ignore height
        if (direction.sqrMagnitude < 0.001f) return;       // already on top of it
        transform.rotation = Quaternion.LookRotation(direction);
    }

    // Eased version used while auto-attacking so the player turns toward the enemy
    // (including while strafing/kiting) instead of snapping.
    public void FaceTowardsSmooth(Vector3 worldPoint)
    {
        Vector3 direction = worldPoint - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookRotationSpeed);
    }
}
