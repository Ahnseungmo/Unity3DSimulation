using UnityEngine;
using Unity.Netcode;

public class WorldObjectController : NetworkBehaviour
{
    public void RequestMove(Vector3 position, Quaternion rotation)
    {
        WorldObjectManager.Instance.MoveObjectServerRpc(NetworkObjectId, position, rotation);
    }

    public void RequestDelete()
    {
        if (!IsOwner && !IsServer) return;
        WorldObjectManager.Instance.DeleteObjectServerRpc(NetworkObjectId);
    }
}
