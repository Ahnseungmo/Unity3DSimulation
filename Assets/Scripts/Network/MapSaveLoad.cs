using System.IO;
using UnityEngine;

public static class MapSaveLoad
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "worldmap.json");

    public static void Save(MapData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[MapSaveLoad] Map saved to {SavePath}");
    }

    public static MapData Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("[MapSaveLoad] No save file found.");
            return new MapData();
        }

        return JsonUtility.FromJson<MapData>(File.ReadAllText(SavePath));
    }
}
