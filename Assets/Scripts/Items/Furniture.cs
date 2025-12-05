using UnityEngine;
using Unity.Netcode;

public class Furniture : Item
{
    public bool IsPlaced = false;

    public virtual void OnPlaced(Vector3 pos)
    {
        transform.position = pos;
        IsPlaced = true;
    }
}
