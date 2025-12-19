using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class NPCManager : NetworkBehaviour
{
    public static NPCManager Instance;

    [Header("NPC Settings")]
    public NetworkObject NPCPrefab;
    public int MaxNPC = 10;
    public float SpawnInterval = 8f;

    [Header("Spawn Point")]
    public Transform OutdoorSpawnPoint;

    private readonly List<NetworkObject> activeNPCs = new();
    private float spawnTimer;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            enabled = false;
    }

    private void Update()
    {
        if (!IsServer)
            return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= SpawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnNPC();
        }
    }

    void TrySpawnNPC()
    {
        activeNPCs.RemoveAll(npc => npc == null || !npc.IsSpawned);

        if (activeNPCs.Count >= MaxNPC)
            return;

        var npc = Instantiate(NPCPrefab,
            OutdoorSpawnPoint.position,
            OutdoorSpawnPoint.rotation);

        npc.Spawn();
        activeNPCs.Add(npc);
    }

    public void DespawnNPC(NetworkObject npc)
    {
        if (!IsServer || npc == null)
            return;

        activeNPCs.Remove(npc);
        npc.Despawn();
        Destroy(npc.gameObject);
    }
}
