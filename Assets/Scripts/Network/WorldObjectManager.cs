using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WorldObjectManager : NetworkBehaviour
{
    public static WorldObjectManager Instance;
    public List<GameObject> WorldObjectPrefabs;

    private void Awake() => Instance = this;

    // ==========================
    // OBJECT SPAWN
    // ==========================

    [ServerRpc(RequireOwnership = false)]
    public void SpawnObjectServerRpc(string prefabName, Vector3 pos, Quaternion rot)
    {
        var prefab = WorldObjectPrefabs.Find(p => p.name == prefabName);
        if (prefab == null)
        {
            Debug.LogError($"[WorldObjectManager] Prefab not found: {prefabName}");
            return;
        }

        var obj = Instantiate(prefab, pos, rot);
        obj.GetComponent<NetworkObject>().Spawn();
    }

    // ==========================
    // OBJECT MOVE
    // ==========================

    [ServerRpc(RequireOwnership = false)]
    public void MoveObjectServerRpc(ulong id, Vector3 pos, Quaternion rot)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(id, out var no))
        {
            no.transform.SetPositionAndRotation(pos, rot);
        }
    }

    // ==========================
    // Object Delete
    // ==========================

    [ServerRpc(RequireOwnership = false)]
    public void DeleteObjectServerRpc(ulong objectId)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var no))
        {
            no.Despawn();
            Destroy(no.gameObject);
        }
    }

    // ==========================
    // COLLECT MAP DATA
    // ==========================

    public MapData CollectMapData()
    {
        MapData data = new();

        foreach (var no in FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
        {
            if (!no.CompareTag("WorldObject")) continue;

            data.objects.Add(new ObjectData
            {
                PrefabName = no.name.Replace("(Clone)", ""),
                Position = no.transform.position,
                Rotation = no.transform.rotation
            });
        }

        return data;
    }

    public void SaveMap() => MapSaveLoad.Save(CollectMapData());


    // ==========================
    // LOAD MAP
    // ==========================

    public void LoadMap()
    {
        var data = MapSaveLoad.Load();
        foreach (var obj in data.objects)
        {
            SpawnObjectServerRpc(obj.PrefabName, obj.Position, obj.Rotation);
        }
    }


}
