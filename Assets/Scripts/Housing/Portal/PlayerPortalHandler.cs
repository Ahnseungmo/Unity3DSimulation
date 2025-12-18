using Unity.Netcode;
using UnityEngine;

public class PlayerPortalHandler : NetworkBehaviour
{
    public void Teleport(Vector3 targetPos, Quaternion targetRot)
    {
        if (!IsServer)
            return;

        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.MovePosition(targetPos);
            rb.MoveRotation(targetRot);
        }
        else
        {
            transform.SetPositionAndRotation(targetPos, targetRot);
        }
    }

}
