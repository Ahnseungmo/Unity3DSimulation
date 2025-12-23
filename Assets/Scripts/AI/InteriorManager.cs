using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class InteriorManager : MonoBehaviour
{
    public static InteriorManager Instance;

    [Header("Outdoor")]
    [SerializeField] Transform outdoorDoor;
    [SerializeField] Transform outdoorExit;

    [Header("Indoor")]
    [SerializeField] Transform indoorDoor;
    [SerializeField] Transform indoorSpawn;

    [Header("Tables")]
    [SerializeField] List<Table> tables = new();

    void Awake()
    {
        Instance = this;


    }
    private void Start()
    {
        tables.Clear();
        tables.AddRange(FindObjectsByType<Table>(FindObjectsSortMode.None));
    }

    // =====================================================
    // POSITION PROVIDERS
    // =====================================================

    public Vector3 GetOutdoorDoorPosition()
        => outdoorDoor.position;

    public Vector3 GetOutdoorExitPosition()
        => outdoorExit.position;

    public Vector3 GetIndoorDoorPosition()
        => indoorDoor.position;

    public Vector3 GetIndoorSpawnPosition()
        => indoorSpawn.position;

    // =====================================================
    // TABLE MANAGEMENT
    // =====================================================

    public bool TryFindAvailableTable(out Table table, out int seatIndex)
    {
        foreach (var t in tables)
        {
            if (t.TryReserveSeat(out seatIndex))
            {
                table = t;
                return true;
            }
        }

        table = null;
        seatIndex = -1;
        return false;
    }
    public void RegisterTable(Table table)
    {
        if (!tables.Contains(table))
            tables.Add(table);
    }

    public void UnregisterTable(Table table)
    {
        tables.Remove(table);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (indoorSpawn)
            Gizmos.DrawSphere(indoorSpawn.position, 0.3f);

        Gizmos.color = Color.blue;
        if (indoorDoor)
            Gizmos.DrawSphere(indoorDoor.position, 0.3f);
    }
#endif
}
