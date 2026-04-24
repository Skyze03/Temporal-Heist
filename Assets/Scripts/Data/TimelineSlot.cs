[System.Serializable]
public class TimelineSlot
{
    public int slotIndex;
    public PlayedCard currentCard;
    public bool isLockedByBarrier;

    public bool IsEmpty => currentCard == null;

    public TimelineSlot(int index)
    {
        slotIndex = index;
        currentCard = null;
        isLockedByBarrier = false;
    }
}