using System.Collections;
using Unity.Netcode;
using UnityEngine.AI;
using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCController : NetworkBehaviour
{
    public enum NPCState
    {
        MoveOutdoorDoor,
        IndoorTransition,
        MoveToTable,
        Eating,
        ExitIndoor,
        Leaving,
        WaitingForSeat
    }

    NavMeshAgent agent;

    [SerializeField]
    NPCState state;

    Table targetTable;
    int seatIndex = -1;

    float arrivalBlockUntil;

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
        if (!IsServer) return;

        if (HasArrived())
            OnArrived();
    }

    // =========================
    // STATE MACHINE
    // =========================

    void ChangeState(NPCState next)
    {
        state = next;

        switch (state)
        {
            case NPCState.MoveOutdoorDoor:
                agent.SetDestination(
                    InteriorManager.Instance.GetOutdoorDoorPosition());
                break;

            case NPCState.MoveToTable:
                TryMoveToTable();
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

            case NPCState.WaitingForSeat:
                arrivalBlockUntil = Time.time + 1.0f;
                break;
        }
    }

    void OnArrived()
    {
        switch (state)
        {
            case NPCState.MoveOutdoorDoor:
                StartCoroutine(EnterIndoorRoutine());
                break;

            case NPCState.MoveToTable:
                OnArriveAtSeat();
                break;

            case NPCState.ExitIndoor:
                StartCoroutine(ExitIndoorRoutine());
                break;

            case NPCState.Leaving:
                NPCManager.Instance.DespawnNPC(NetworkObject);
                break;
        }
    }

    // =========================
    // BEHAVIOR
    // =========================

    IEnumerator EnterIndoorRoutine()
    {
        state = NPCState.IndoorTransition;

        DisableAgent();

        Vector3 spawn = InteriorManager.Instance.GetIndoorSpawnPosition();
        NavMesh.SamplePosition(spawn, out var hit, 3f, NavMesh.AllAreas);

        transform.position = hit.position;

        yield return null;

        EnableAgent(hit.position);

        ChangeState(NPCState.MoveToTable);
    }

    void TryMoveToTable()
    {
        if (InteriorManager.Instance.TryFindAvailableTable(out targetTable, out seatIndex))
        {
            Vector3 seatPos = targetTable.GetSeatPosition(seatIndex);
            if (NavMesh.SamplePosition(seatPos, out var hit, 1.5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }

            targetTable.ReleaseSeat(seatIndex);
            targetTable = null;
            seatIndex = -1;
        }

        ChangeState(NPCState.WaitingForSeat);
    }

    void OnArriveAtSeat()
    {
        if (targetTable == null || seatIndex < 0)
        {
            ChangeState(NPCState.WaitingForSeat);
            return;
        }

        targetTable.OccupySeat(seatIndex);
        ChangeState(NPCState.Eating);
    }

    IEnumerator EatRoutine()
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(5f);

        targetTable?.ReleaseSeat(seatIndex);
        targetTable = null;
        seatIndex = -1;

        agent.isStopped = false;
        ChangeState(NPCState.ExitIndoor);
    }

    IEnumerator ExitIndoorRoutine()
    {
        state = NPCState.IndoorTransition;

        DisableAgent();

        Vector3 exit = InteriorManager.Instance.GetOutdoorExitPosition();
        NavMesh.SamplePosition(exit, out var hit, 3f, NavMesh.AllAreas);

        transform.position = hit.position;

        yield return null;

        EnableAgent(hit.position);

        ChangeState(NPCState.Leaving);
    }

    // =========================
    // UTIL
    // =========================

    void DisableAgent()
    {
        agent.isStopped = true;
        agent.ResetPath();
        agent.enabled = false;
    }

    void EnableAgent(Vector3 pos)
    {
        agent.enabled = true;
        agent.Warp(pos);
        agent.isStopped = false;
        arrivalBlockUntil = Time.time + 0.25f;
    }

    bool HasArrived()
    {
        if (Time.time < arrivalBlockUntil) return false;

        if (!agent.enabled || !agent.isOnNavMesh) return false;
        if (agent.pathPending) return false;

        return agent.remainingDistance <= agent.stoppingDistance + 0.2f &&
               agent.velocity.sqrMagnitude < 0.01f;
    }
}
