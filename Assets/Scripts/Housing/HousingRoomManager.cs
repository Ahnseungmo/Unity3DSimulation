using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;

public class HousingRoomManager : SingletonMonoBehaviour<HousingRoomManager>
{
    // --- Prefab & Room Size ---
    public GameObject RoomPrefab;    // 실제 생성할 방 프리팹
    public Vector3 RoomSize = new Vector3(4, 4, 4); // 방 1칸의 실제 크기

    public NavMeshSurface NavMeshSurface;

    // --- 데이터 구조 ---
    private HashSet<Vector3Int> rooms = new HashSet<Vector3Int>();
    private Dictionary<Vector3Int, GameObject> roomObjects = new Dictionary<Vector3Int, GameObject>();

    // 3D 연결 판정용 6방향
    static readonly Vector3Int[] dirs =
    {
        new Vector3Int( 1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int( 0, 1, 0),   // 위층
        new Vector3Int( 0,-1, 0),   // 아래층
        new Vector3Int( 0, 0, 1),
        new Vector3Int( 0, 0,-1)
    };

    // ==================================================
    // 방 추가 (실제 RoomPrefab 생성 포함)
    // ==================================================
    public bool AddRoom(Vector3Int gridPos)
    {
        if (rooms.Contains(gridPos))
        {
            Debug.Log("이미 방이 존재합니다: " + gridPos);
            return false;
        }

        rooms.Add(gridPos);

        // 실제 생성 위치 = 방 좌표 * RoomSize
        Vector3 worldPos = GridToWorld(gridPos);

        GameObject obj = Instantiate(RoomPrefab, worldPos, Quaternion.identity, transform);
        obj.name = $"Room {gridPos.x},{gridPos.y},{gridPos.z}";

        roomObjects.Add(gridPos, obj);
        NavMeshSurface.BuildNavMesh();
        Debug.Log("방 추가됨: " + gridPos);
        return true;
    }


    // ==================================================
    // 방 삭제 (분리 체크 + 프리팹 삭제)
    // ==================================================
    public bool RemoveRoom(Vector3Int gridPos)
    {
        if (!rooms.Contains(gridPos))
        {
            Debug.Log("삭제할 방이 없습니다: " + gridPos);
            return false;
        }

        if (!CanRemoveRoom(gridPos))
        {
            Debug.Log("이 방을 삭제하면 집이 분리되므로 삭제할 수 없습니다: " + gridPos);
            return false;
        }

        rooms.Remove(gridPos);

        if (roomObjects.TryGetValue(gridPos, out GameObject obj))
        {
            Destroy(obj);
            roomObjects.Remove(gridPos);
        }
        NavMeshSurface.BuildNavMesh();
        Debug.Log("방 삭제됨: " + gridPos);
        return true;
    }


    // ==================================================
    // 삭제 가능 여부 검사 (전체 연결 구조 유지)
    // ==================================================
    public bool CanRemoveRoom(Vector3Int pos)
    {
        if (!rooms.Contains(pos))
            return false;

        HashSet<Vector3Int> temp = new HashSet<Vector3Int>(rooms);
        temp.Remove(pos);

        int connected = GetConnectedCount(temp);
        return connected == temp.Count;
    }

    // ==================================================
    // BFS로 연결된 방 수 검사
    // ==================================================
    private int GetConnectedCount(HashSet<Vector3Int> nodes)
    {
        if (nodes.Count == 0)
            return 0;

        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Queue<Vector3Int> q = new Queue<Vector3Int>();

        Vector3Int start = nodes.First();
        visited.Add(start);
        q.Enqueue(start);

        while (q.Count > 0)
        {
            Vector3Int cur = q.Dequeue();

            foreach (var d in dirs)
            {
                var next = cur + d;

                if (nodes.Contains(next) && !visited.Contains(next))
                {
                    visited.Add(next);
                    q.Enqueue(next);
                }
            }
        }

        return visited.Count;
    }

    // ==================================================
    // Grid 좌표 → 실제 월드 좌표 변환
    // ==================================================
    private Vector3 GridToWorld(Vector3Int gridPos)
    {
        return new Vector3(
            gridPos.x * RoomSize.x,
            gridPos.y * RoomSize.y,
            gridPos.z * RoomSize.z
        );
    }
}
