using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class AITest : NetworkBehaviour
{
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.stoppingDistance = agent.radius;
        agent.autoBraking = true;
    }

    public void MoveTo(Vector3 targetPosition)
    {
        if (NavMesh.SamplePosition(
            targetPosition,
            out NavMeshHit hit,
            2.0f,
            NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
    bool HasArrived()
    {
        if (agent.pathPending) return false;

        return agent.remainingDistance <= agent.stoppingDistance &&
               !agent.hasPath;
    }

    void Update()
    {
        if (!HasArrived() && agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRot =
                Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * 10f);
        }


        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                MoveTo(hit.point);
            }
        }
    }
}
