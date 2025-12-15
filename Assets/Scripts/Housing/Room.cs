using System.Collections.Generic;
using UnityEngine;

public enum RoomDirection
{
    North,
    South,
    East,
    West,
    Up,
    Down
}

public class Room : MonoBehaviour
{
    [System.Serializable]
    public struct WallEntry
    {
        public RoomDirection dir;
        public GameObject wall;
    }

    public WallEntry[] wallEntries;

    private Dictionary<RoomDirection, GameObject> walls;

    private void Awake()
    {
        walls = new Dictionary<RoomDirection, GameObject>();
        foreach (var e in wallEntries)
            walls[e.dir] = e.wall;
    }

    public void SetWall(RoomDirection dir, bool active)
    {
        if (walls.TryGetValue(dir, out var wall))
            wall.SetActive(active);
    }
}
