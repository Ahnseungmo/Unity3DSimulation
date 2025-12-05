using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObjectData
{
    public string PrefabName;
    public Vector3 Position;
    public Quaternion Rotation;
}

[Serializable]
public class MapData
{
    public List<ObjectData> objects = new();
}
