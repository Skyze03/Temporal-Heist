using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Card Database")]
    public List<CardData> allCards = new List<CardData>();

    [Header("UI")]
    public GameUIManager uiManager;

    [Header("Runtime Players")]
    public PlayerState player1;
    public PlayerState player2;

    [Header("Game State")]
    public int currentRound = 0;
    public int maxRounds = 12;

    private CardData selectedPlayer1Card;
    private int selectedPlayer1HandIndex = -1;
    private int selectedTargetSlotIndex = -1;

    private void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        player1 = new PlayerState("Player 1");
        player2 = new PlayerState("Player 2");

        DealStartingHand(player1);
        DealStartingHand(player2);

        currentRound = 0;
        selectedPlayer1Card = null;
        selectedPlayer1HandIndex = -1;

        RefreshAllUI();

        Debug.Log("Game started.");
        Debug.Log(player1.playerId + " hand count: " + player1.hand.Count);
        Debug.Log(player2.playerId + " hand count: " + player2.hand.Count);
    }

    private void DealStartingHand(PlayerState player)
    {
        player.hand.Clear();

        CardData card2 = FindCard(CardRank.Two);
        CardData card3 = FindCard(CardRank.Three);
        CardData card4 = FindCard(CardRank.Four);

        player.hand.Add(card2);
        player.hand.Add(card3);
        player.hand.Add(card4);
        player.hand.Add(card2);

        for (int i = 0; i < 8; i++)
        {
            CardData randomCard = allCards[Random.Range(0, allCards.Count)];
            player.hand.Add(randomCard);
        }
    }

    private CardData FindCard(CardRank rank)
    {
        foreach (CardData card in allCards)
        {
            if (card.rank == rank)
            {
                return card;
            }
        }

        Debug.LogError("Missing card data for rank: " + rank);
        return null;
    }

    public void OnPlayer1CardSelected(int handIndex)
    {
        if (handIndex < 0 || handIndex >= player1.hand.Count)
        {
            return;
        }

        selectedPlayer1HandIndex = handIndex;
        selectedPlayer1Card = player1.hand[handIndex];

        Debug.Log("Player 1 selected card: " + selectedPlayer1Card.displayName);

        if (uiManager != null)
        {
            string slotText = selectedTargetSlotIndex >= 0 ? $"Slot {selectedTargetSlotIndex + 1}" : "No Slot";
            uiManager.SetRevealText(
                $"You: {selectedPlayer1Card.displayName} -> {slotText}",
                "Opponent: Hidden"
            );

            RefreshAllUI();
        }
    }

    private void RefreshAllUI()
    {
        if (uiManager == null)
        {
            return;
        }

        uiManager.SetRoundText(currentRound, maxRounds);
        uiManager.BuildTimelineUI(uiManager.player2TimelineParent, player2, this, false);
        uiManager.BuildTimelineUI(uiManager.player1TimelineParent, player1, this, true);
        uiManager.BuildOpponentHandUI(player2.hand.Count);
        uiManager.BuildPlayerHandUI(player1, this);

        string cardText = selectedPlayer1Card != null ? selectedPlayer1Card.displayName : "None";
        string slotText = selectedTargetSlotIndex >= 0 ? $"Slot {selectedTargetSlotIndex + 1}" : "No Slot";

        uiManager.SetRevealText(
            $"You: {cardText} -> {slotText}",
            "Opponent: Hidden"
        );
    }

    public bool IsSlotSelectableForCurrentTurn(int slotIndex)
    {
        // 先做最基础版本：
        // 当前 round 为 0 时只能选 slot 0
        // 当前 round 为 1 时能选 slot 0 或 1
        // 当前 round 为 2 时能选 0,1,2
        // 以后再加 barrier / time point 的限制

        return slotIndex >= 0 && slotIndex <= currentRound;
    }

    public void OnPlayer1TargetSlotSelected(int slotIndex)
    {
        if (!IsSlotSelectableForCurrentTurn(slotIndex))
        {
            return;
        }

        selectedTargetSlotIndex = slotIndex;

        Debug.Log("Player 1 target slot selected: " + (slotIndex + 1));

        if (uiManager != null)
        {
            string cardText = selectedPlayer1Card != null ? selectedPlayer1Card.displayName : "None";
            uiManager.SetRevealText(
                $"You: {cardText} -> Slot {slotIndex + 1}",
                "Opponent: Hidden"
            );
        }
    }

    public void ConfirmPlayer1Placement()
    {
        if (selectedPlayer1Card == null || selectedPlayer1HandIndex < 0 || selectedTargetSlotIndex < 0)
        {
            Debug.Log("Cannot confirm: card or slot not selected.");
            return;
        }

        // 创建运行时牌
        PlayedCard playedCard = new PlayedCard(
            selectedPlayer1Card,
            currentRound,
            selectedTargetSlotIndex,
            player1
        );

        // 放到目标时间线格
        player1.timeline[selectedTargetSlotIndex].currentCard = playedCard;

        // 从手牌移除
        player1.hand.RemoveAt(selectedPlayer1HandIndex);

        // 先让 opponent 自动出一张（最基础版本）
        AutoPlayForPlayer2();

        // 回合前进
        currentRound++;

        // 清空本回合选择
        selectedPlayer1Card = null;
        selectedPlayer1HandIndex = -1;
        selectedTargetSlotIndex = -1;

        RefreshAllUI();
    }

    private void AutoPlayForPlayer2()
    {
        if (player2.hand.Count == 0) return;

        int randomIndex = Random.Range(0, player2.hand.Count);
        CardData opponentCard = player2.hand[randomIndex];

        // 最基础版本：对手总是放在当前 round 对应的 slot
        int opponentSlot = currentRound;

        PlayedCard playedCard = new PlayedCard(
            opponentCard,
            currentRound,
            opponentSlot,
            player2
        );

        player2.timeline[opponentSlot].currentCard = playedCard;
        player2.hand.RemoveAt(randomIndex);
    }
}