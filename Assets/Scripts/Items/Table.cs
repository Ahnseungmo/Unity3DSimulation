public class Table : Furniture
{
    public int MaxSeat = 4;
    private bool[] seatOccupied;

    private void Awake()
    {
        seatOccupied = new bool[MaxSeat];
    }

    public bool TrySeatNPC(int seatIndex)
    {
        if (seatIndex < 0 || seatIndex >= MaxSeat) return false;
        if (seatOccupied[seatIndex]) return false;

        seatOccupied[seatIndex] = true;
        return true;
    }

    public void LeaveSeat(int seatIndex)
    {
        if (seatIndex < 0 || seatIndex >= MaxSeat) return;
        seatOccupied[seatIndex] = false;
    }

    public int FindEmptySeat()
    {
        for (int i = 0; i < MaxSeat; i++)
        {
            if (!seatOccupied[i]) return i;
        }
        return -1;
    }
}
