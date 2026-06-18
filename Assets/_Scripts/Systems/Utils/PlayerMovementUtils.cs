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

    public bool IsStopped => agent.velocity == Vector3.zero;

    public void MoveTo(Vector3 point) => agent.destination = point;
    public void Stop() => agent.SetDestination(transform.position);

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
}
