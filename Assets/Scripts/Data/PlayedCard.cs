[System.Serializable]
public class PlayedCard
{
    public CardData card;
    public int originalRoundPlayed;
    public int currentSlot;

    public PlayerState owner;
    public PlayerState targetPlayer;

    public bool isCancelledByJoker;

    public PlayedCard(CardData card, int originalRoundPlayed, int currentSlot, PlayerState owner)
    {
        this.card = card;
        this.originalRoundPlayed = originalRoundPlayed;
        this.currentSlot = currentSlot;
        this.owner = owner;
        this.targetPlayer = null;
        this.isCancelledByJoker = false;
    }
}