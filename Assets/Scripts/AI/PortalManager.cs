using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

public class PortalManager : NetworkBehaviour
{
    public static PortalManager Instance;

    public Transform outdoorDoor;
    public Transform outdoorSpawn;

    public Transform indoorSpawn;
    public Transform indoorDoor;

    void Awake()
    {
        Instance = this;
    }

    public void TeleportToIndoor(NPCController npc)
    {
        Teleport(npc, indoorSpawn.position);
    }

    public void TeleportToOutdoor(NPCController npc)
    {
        Teleport(npc, outdoorSpawn.position);
    }

    void Teleport(NPCController npc, Vector3 targetPos)
    {
        if (!IsServer)
            return;

        NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
        agent.enabled = false;

        if (NavMesh.SamplePosition(
            targetPos,
            out NavMeshHit hit,
            1.5f,
            NavMesh.AllAreas))
        {
            npc.transform.position = hit.position;
        }
        else
        {
            npc.transform.position = targetPos;
        }

        agent.enabled = true;
    }
}
