using System.Collections;
using Unity.Netcode;
using UnityEngine.AI;
using UnityEngine;
using Steamworks;
using GanzSe;
using System.Security.Cryptography;

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
    Animator animator;

    Table targetTable;
    int seatIndex = -1;
    float nextSeatRetryTime;
    float arrivalBlockUntil;
    
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        ModularHeroController controller = GetComponentInChildren<ModularHeroController>();
        controller.SetAromorPart(Random.Range((int)0,(int)20));
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

    // ★ 중요: NPC가 사라지거나 연결이 끊길 때 반드시 좌석을 해제해야 함
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            ReleaseCurrentSeat();
        }
    }

    void Update()
    {
        if (!IsServer) return;
        UpdateAnimation();
        // 좌석 대기 중일 때 재시도 로직
        if (state == NPCState.WaitingForSeat)
        {
            if (Time.time >= nextSeatRetryTime)
            {
                // 다시 테이블 이동 시도 (상태 전환을 통해 로직 재사용)
                ChangeState(NPCState.MoveToTable);
            }
            return;
        }

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
                agent.SetDestination(InteriorManager.Instance.GetOutdoorDoorPosition());
                break;

            case NPCState.MoveToTable:
                // ★ 주석 해제 및 통합된 함수 호출
                TryFindAndMoveToTable();
                break;

            case NPCState.Eating:
                StartCoroutine(EatRoutine());
                break;

            case NPCState.ExitIndoor:
                agent.SetDestination(InteriorManager.Instance.GetIndoorDoorPosition());
                break;

            case NPCState.Leaving:
                agent.SetDestination(InteriorManager.Instance.GetOutdoorExitPosition());
                break;

            case NPCState.WaitingForSeat:
                agent.isStopped = true;
                agent.ResetPath(); // 명확하게 경로 초기화
                nextSeatRetryTime = Time.time + 1.0f; // 1초 뒤 재시도
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
                // ★ NPCManager가 없다면 NetworkObject.Despawn() 사용
                if (NPCManager.Instance != null)
                    NPCManager.Instance.DespawnNPC(NetworkObject);
                else
                    NetworkObject.Despawn();
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

        // ★ NavMesh 위에 정확히 안착시키기 위해 SamplePosition 사용
        if (NavMesh.SamplePosition(spawn, out var hit, 3f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }
        else
        {
            transform.position = spawn; // 실패 시 원래 위치라도 사용
        }

        yield return null; // 한 프레임 대기 (물리/NavMesh 갱신)

        EnableAgent(transform.position);

        ChangeState(NPCState.MoveToTable);
    }

    // ★ 중복된 함수들을 하나로 통합하고 정리함
    void TryFindAndMoveToTable()
    {
        // 1. 이미 잡은 자리가 있다면 해제 (혹시 모를 에러 방지)
        if (targetTable != null) ReleaseCurrentSeat();

        // 2. 테이블 찾기 시도
        if (InteriorManager.Instance.TryFindAvailableTable(out targetTable, out seatIndex))
        {
            Vector3 seatPos = targetTable.GetSeatPosition(seatIndex);

            // 3. 이동 가능한 위치인지 확인
            if (NavMesh.SamplePosition(seatPos, out var hit, 1.5f, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
                // 상태는 이미 MoveToTable이므로 변경 불필요
                return;
            }

            // 이동 불가능한 위치면 즉시 해제
            ReleaseCurrentSeat();
        }

        // 4. 실패 시 대기 상태로 전환
        ChangeState(NPCState.WaitingForSeat);
    }

    void OnArriveAtSeat()
    {
        if (targetTable == null || seatIndex < 0)
        {
            ChangeState(NPCState.WaitingForSeat);
            return;
        }

        // 좌석 Transform
        Transform seat = targetTable.SeatPoints[seatIndex];

        // 위치 & 방향 스냅
        agent.isStopped = true;
        agent.ResetPath();

        transform.position = seat.position;
        transform.rotation = Quaternion.LookRotation(-seat.forward , Vector3.up);

        targetTable.OccupySeat(seatIndex);
        ChangeState(NPCState.Eating);
    }

    IEnumerator EatRoutine()
    {
        // 회전 등 먹는 애니메이션 로직 추가 가능
        agent.isStopped = true;

        animator.SetFloat("AxisX", 0f);
        animator.SetFloat("AxisY", 0f);

        animator.SetTrigger("Sit");
        yield return new WaitForSeconds(5f);

        ReleaseCurrentSeat(); // 다 먹었으니 자리 비움
        animator.SetTrigger("Stand");

        agent.isStopped = false;
        ChangeState(NPCState.ExitIndoor);
    }

    IEnumerator ExitIndoorRoutine()
    {
        state = NPCState.IndoorTransition;

        DisableAgent();

        Vector3 exit = InteriorManager.Instance.GetOutdoorExitPosition();
        if (NavMesh.SamplePosition(exit, out var hit, 3f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }
        else
        {
            transform.position = exit;
        }

        yield return null;

        EnableAgent(transform.position);

        ChangeState(NPCState.Leaving);
    }

    // =========================
    // UTIL & HELPER
    // =========================

    // 좌석 해제 로직을 별도 함수로 분리하여 재사용성 높임
    void ReleaseCurrentSeat()
    {
        if (targetTable != null && seatIndex != -1)
        {
            targetTable.ReleaseSeat(seatIndex);
        }
        targetTable = null;
        seatIndex = -1;
    }

    void DisableAgent()
    {
        if (agent.isOnNavMesh && !agent.isStopped) agent.isStopped = true;
        agent.ResetPath();
        agent.enabled = false;
    }

    void EnableAgent(Vector3 pos)
    {
        agent.enabled = true;
        agent.Warp(pos);
        agent.isStopped = false;
        // Warp 직후 HasArrived가 true로 뜨는 것을 방지하기 위한 지연 시간
        arrivalBlockUntil = Time.time + 0.25f;
    }

    bool HasArrived()
    {
        if (Time.time < arrivalBlockUntil) return false;

        if (!agent.enabled || !agent.isOnNavMesh) return false;
        if (agent.pathPending) return false; // 경로 계산 중이면 도착 아님

        // 남은 거리가 정지 거리보다 작고, 속도가 거의 0일 때
        return agent.remainingDistance <= agent.stoppingDistance + 0.1f; // 오차 범위 약간 축소
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        // NavMesh 이동 속도
        Vector3 worldVel = agent.velocity;
        Vector3 localVel = transform.InverseTransformDirection(worldVel);

        // Player와 동일한 파라미터 구조
        float forward = localVel.z / agent.speed;
        float right = localVel.x / agent.speed;

        animator.SetFloat("AxisY", forward);
        animator.SetFloat("AxisX", right);
    }
}