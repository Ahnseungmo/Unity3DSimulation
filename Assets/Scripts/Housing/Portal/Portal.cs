using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Portal : NetworkBehaviour
{
    [Header("Linked Portal")]
    public Portal linkedPortal;

    // 방금 텔레포트된 플레이어 락
    private HashSet<ulong> lockedPlayers = new();

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
            return;

        if (!other.CompareTag("Player"))
            return;

        var netObj = other.GetComponent<NetworkObject>();
        if (netObj == null)
            return;

        ulong clientId = netObj.OwnerClientId;

        // 방금 텔레포트된 플레이어면 무시
        if (lockedPlayers.Contains(clientId))
            return;

        TeleportPlayer(netObj);
    }

    private void TeleportPlayer(NetworkObject player)
    {
        var handler = player.GetComponent<PlayerPortalHandler>();
        if (handler == null)
            return;

        linkedPortal.LockPlayer(player.OwnerClientId);

        handler.Teleport(
            linkedPortal.transform.position,
            linkedPortal.transform.rotation
        );
    }

    public void LockPlayer(ulong clientId)
    {
        lockedPlayers.Add(clientId);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer)
            return;

        if (!other.CompareTag("Player"))
            return;

        var netObj = other.GetComponent<NetworkObject>();
        if (netObj == null)
            return;

        // 포탈 영역을 벗어나면 락 해제
        lockedPlayers.Remove(netObj.OwnerClientId);
    }
}
