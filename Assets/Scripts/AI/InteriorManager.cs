using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class InteriorManager : NetworkBehaviour
{
    public static InteriorManager Instance;

    [Header("Indoor / Outdoor Transforms")]
    public Transform IndoorSpawnPoint;
    public Transform IndoorDoorPoint;
    public Transform OutdoorDoorPoint;
    public Transform OutdoorExitPoint;

    private void Awake()
    {
        Instance = this;
    }

    // ==========================
    // TABLE QUERY
    // ==========================

    /// <summary>
    /// 현재 실내에 배치된 Table 중
    /// NPC가 앉을 수 있는 테이블을 하나 찾아 반환
    /// </summary>
    public bool TryFindAvailableTable(out Table table, out int seatIndex)
    {
        table = null;
        seatIndex = -1;

        if (!IsServer)
            return false;

        foreach (var t in FindObjectsByType<Table>(FindObjectsSortMode.None))
        {
            if (!t.IsPlaced)
                continue;

            if (t.TryAssignSeat(out seatIndex))
            {
                table = t;
                return true;
            }
        }

        return false;
    }

    // ==========================
    // POSITION HELPERS
    // ==========================

    public Vector3 GetIndoorSpawnPosition()
    {
        Vector3 basePos = IndoorDoorPoint.position;

        if (NavMesh.SamplePosition(basePos, out var hit, 3f, NavMesh.AllAreas))
            return hit.position;

        Debug.LogError("Indoor spawn has no NavMesh nearby");
        return basePos;
    }

    public Vector3 GetIndoorDoorPosition()
        => IndoorDoorPoint.position;

    public Vector3 GetOutdoorDoorPosition()
        => OutdoorDoorPoint.position;

    public Vector3 GetOutdoorExitPosition()
        => OutdoorExitPoint.position;
}
