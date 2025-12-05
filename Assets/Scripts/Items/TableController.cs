using Unity.Netcode;
using UnityEngine;

public class TableController : NetworkBehaviour
{
    private Table table;

    private void Awake()
    {
        table = GetComponent<Table>();
    }

    // NPC가 서버에게 좌석 요청
    public void RequestSeatNPC(int seatIndex, ulong npcId)
    {
        SeatNPCServerRpc(seatIndex, npcId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SeatNPCServerRpc(int seatIndex, ulong npcId)
    {
        if (table.TrySeatNPC(seatIndex))
        {
            UpdateSeatClientRpc(seatIndex, npcId);
        }
    }

    [ClientRpc]
    private void UpdateSeatClientRpc(int seatIndex, ulong npcId)
    {
        // NPC 앉는 연출 처리, 애니메이션, 위치 스냅 등
        Debug.Log($"NPC {npcId} seated at {seatIndex}");
    }
}
