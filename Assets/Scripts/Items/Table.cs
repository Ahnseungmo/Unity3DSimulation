using UnityEngine;

public class Table : Furniture
{
    public int MaxSeat = 4;
    public Transform[] SeatPoints;

    private bool[] seatOccupied;

    private void Awake()
    {
        seatOccupied = new bool[MaxSeat];
    }

    // 서버에서만 호출
    public bool TryAssignSeat(out int seatIndex)
    {
        seatIndex = -1;

        for (int i = 0; i < MaxSeat; i++)
        {
            if (!seatOccupied[i])
            {
                seatOccupied[i] = true;
                seatIndex = i;
                return true;
            }
        }
        return false;
    }

    public void LeaveSeat(int seatIndex)
    {
        if (seatIndex < 0 || seatIndex >= MaxSeat) return;
        seatOccupied[seatIndex] = false;
    }

    public Vector3 GetSeatPosition(int seatIndex)
    {
        return SeatPoints[seatIndex].position;
    }
}
