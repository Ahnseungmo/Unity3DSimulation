using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCController : NetworkBehaviour
{
    public enum NPCState
    {
        MoveOutdoorDoor,
        IndoorTransition,   // 이동 개념 없음
        MoveToTable,
        Eating,
        ExitIndoor,
        Leaving
    }

    NavMeshAgent agent;
    [SerializeField]
    NPCState state;

    Table targetTable;
    int seatIndex = -1;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            agent.enabled = false;
            return;
        }

        ChangeState(NPCState.MoveOutdoorDoor);
    }

    void Update()
    {
        if (!IsServer)
            return;

        if (HasArrived())
            OnArrived();
    }

    // ==========================
    // STATE MACHINE
    // ==========================

    void ChangeState(NPCState next)
    {
        state = next;

        switch (state)
        {
            case NPCState.MoveOutdoorDoor:
                agent.SetDestination(
                    InteriorManager.Instance.GetOutdoorDoorPosition());
                break;

            case NPCState.IndoorTransition:
                // 이동 개념 없음
                break;

            case NPCState.MoveToTable:
                FindTableAndMove();
                break;

            case NPCState.Eating:
                StartCoroutine(EatRoutine());
                break;

            case NPCState.ExitIndoor:
                agent.SetDestination(
                    InteriorManager.Instance.GetIndoorDoorPosition());
                break;

            case NPCState.Leaving:
                agent.SetDestination(
                    InteriorManager.Instance.GetOutdoorExitPosition());
                break;
        }
    }

    void OnArrived()
    {
        Debug.Log($"Arrived state={state}");

        switch (state)
        {
            case NPCState.MoveOutdoorDoor:
                StartCoroutine(EnterIndoorRoutine());
                break;

            case NPCState.MoveToTable:
                ChangeState(NPCState.Eating);
                break;

            case NPCState.ExitIndoor:
                StartCoroutine(ExitIndoorRoutine());
                break;

            case NPCState.Leaving:
                NPCManager.Instance.DespawnNPC(NetworkObject);
                break;
        }
    }

    // ==========================
    // BEHAVIOR
    // ==========================

    IEnumerator EnterIndoorRoutine()
    {
        state = NPCState.IndoorTransition;

        agent.isStopped = true;
        agent.ResetPath();
        agent.enabled = false;

        Vector3 spawn = InteriorManager.Instance.GetIndoorSpawnPosition();

        if (!NavMesh.SamplePosition(spawn, out var hit, 3f, NavMesh.AllAreas))
        {
            Debug.LogError("Indoor spawn invalid");
            yield break;
        }

        transform.position = hit.position;

        yield return null;

        agent.enabled = true;
        agent.Warp(hit.position);
        agent.isStopped = false;

        ChangeState(NPCState.MoveToTable);
    }

    void FindTableAndMove()
    {
        if (!agent.enabled || !agent.isOnNavMesh)
            return;

        if (InteriorManager.Instance.TryFindAvailableTable(
            out targetTable, out seatIndex))
        {
            Vector3 seatPos = targetTable.GetSeatPosition(seatIndex);

            if (NavMesh.SamplePosition(seatPos, out var hit, 1.5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }

            targetTable.LeaveSeat(seatIndex);
        }

        ChangeState(NPCState.ExitIndoor);
    }

    IEnumerator EatRoutine()
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(5f);

        if (targetTable != null)
            targetTable.LeaveSeat(seatIndex);

        agent.isStopped = false;
        ChangeState(NPCState.ExitIndoor);
    }

    IEnumerator ExitIndoorRoutine()
    {
        agent.isStopped = true;
        agent.ResetPath();
        agent.enabled = false;

        Vector3 exit = InteriorManager.Instance.GetOutdoorExitPosition();

        if (NavMesh.SamplePosition(exit, out var hit, 3f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }

        yield return null;

        agent.enabled = true;
        agent.Warp(transform.position);
        agent.isStopped = false;

        ChangeState(NPCState.Leaving);
    }

    // ==========================
    // UTIL
    // ==========================

    bool HasArrived()
    {
        // 이동 상태에서만 도착 판정
        if (state != NPCState.MoveOutdoorDoor &&
            state != NPCState.MoveToTable &&
            state != NPCState.ExitIndoor &&
            state != NPCState.Leaving)
            return false;

        if (!agent.enabled || !agent.isOnNavMesh)
            return false;

        if (agent.pathPending)
            return false;

        return agent.remainingDistance <= agent.stoppingDistance + 0.2f &&
               agent.velocity.sqrMagnitude < 0.01f;
    }

    private void OnDestroy()
    {
        Debug.Log($"NPC DESTROYED : {name} | IsServer={IsServer}");
    }
}
