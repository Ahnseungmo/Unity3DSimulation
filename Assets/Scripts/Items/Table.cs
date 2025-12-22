using UnityEngine;

public class Table : Furniture
{
    public int MaxSeat = 4;
    public Transform[] SeatPoints;

    // 0 = 비어있음
    // 1 = 예약됨 (이동 중)
    // 2 = 점유됨 (착석 완료)

    private int[] seatState;

    void Awake()
    {
        seatState = new int[MaxSeat];
    }

    // =========================
    // RESERVE (이동 시작 시)
    // =========================
    public bool TryReserveSeat(out int seatIndex)
    {
        seatIndex = -1;

        for (int i = 0; i < MaxSeat; i++)
        {
            if (seatState[i] == 0)
            {
                seatState[i] = 1; // 예약
                seatIndex = i;
                return true;
            }
        }
        return false;
    }

    // =========================
    // OCCUPY (도착 시)
    // =========================
    public void OccupySeat(int seatIndex)
    {
        if (!IsValidSeat(seatIndex)) return;
        seatState[seatIndex] = 2;
    }

    // =========================
    // RELEASE (실패 / 퇴장)
    // =========================
    public void ReleaseSeat(int seatIndex)
    {
        if (!IsValidSeat(seatIndex)) return;
        seatState[seatIndex] = 0;
    }

    public Vector3 GetSeatPosition(int seatIndex)
    {
        return SeatPoints[seatIndex].position;
    }

    bool IsValidSeat(int index)
    {
        return index >= 0 && index < MaxSeat;
    }
}
