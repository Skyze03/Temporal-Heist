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
    public GamePhase currentPhase = GamePhase.TurnPlanning;

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
        currentPhase = GamePhase.TurnPlanning;
        player1.coins = 0;
        player2.coins = 0;

        selectedPlayer1Card = null;
        selectedPlayer1HandIndex = -1;
        selectedTargetSlotIndex = -1;

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
        if (currentPhase != GamePhase.TurnPlanning)
        {
            return;
        }

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

    public bool IsSlotLockedForDisplay(PlayerState player, int slotIndex)
    {
        return IsSlotLockedByBarrier(player, slotIndex);
    }
    private void RefreshAllUI()
    {
        if (uiManager == null)
        {
            return;
        }

        uiManager.SetRoundText(currentRound < maxRounds ? currentRound : maxRounds - 1, maxRounds);
        uiManager.BuildTimelineUI(uiManager.player2TimelineParent, player2, this, false);
        uiManager.BuildTimelineUI(uiManager.player1TimelineParent, player1, this, true);
        uiManager.BuildOpponentHandUI(player2.hand.Count);
        uiManager.BuildPlayerHandUI(player1, this);

        if (currentPhase == GamePhase.GameEnded)
        {
            string resultText;

            if (player1.coins > player2.coins)
            {
                resultText = "You Win!";
            }
            else if (player2.coins > player1.coins)
            {
                resultText = "Opponent Wins!";
            }
            else
            {
                resultText = "Draw!";
            }

            uiManager.SetRevealText(
                $"You: {player1.coins} coins",
                $"Opponent: {player2.coins} coins\n{resultText}"
            );
        }
        else
        {
            string cardText = selectedPlayer1Card != null ? selectedPlayer1Card.displayName : "None";
            string slotText = selectedTargetSlotIndex >= 0 ? $"Slot {selectedTargetSlotIndex + 1}" : "No Slot";

            if (currentPhase == GamePhase.FinalResolution)
            {
                uiManager.SetRevealText(
                    "All turns complete",
                    "Press Resolve to score"
                );
            }
            else
            {
                uiManager.SetRevealText(
                    $"You: {cardText} -> {slotText}",
                    "Opponent: Hidden"
                );
            }
        }
    }

    public bool IsSlotSelectableForCurrentTurn(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= player1.timeline.Length)
        {
            return false;
        }

        // 还没选牌时，不允许选 slot
        if (selectedPlayer1Card == null)
        {
            return false;
        }

        // 如果这个 slot 被 Barrier 锁住，直接不可选
        if (IsSlotLockedByBarrier(player1, slotIndex))
        {
            return false;
        }

        int currentTurnSlot = currentRound;

        // 没有可用 Time Point：只能选当前回合 slot
        if (!HasUsableTimePoint(player1))
        {
            return slotIndex == currentTurnSlot;
        }

        // 有可用 Time Point：可选 earliest usable TP ~ current round
        int earliestTimePoint = GetEarliestUsableTimePointSlot(player1);

        return slotIndex >= earliestTimePoint && slotIndex <= currentTurnSlot;
    }

    private bool IsTimePointCard(CardData card)
    {
        if (card == null) return false;

        return card.effectType == CardEffectType.SetTimePoint;
    }

    private bool IsBarrierCard(CardData card)
    {
        if (card == null) return false;

        return card.effectType == CardEffectType.Barrier;
    }

    private void RebuildTimePointSlots(PlayerState player)
    {
        player.timePointSlots.Clear();

        for (int i = 0; i < player.timeline.Length; i++)
        {
            if (!player.timeline[i].IsEmpty)
            {
                CardData card = player.timeline[i].currentCard.card;

                if (IsTimePointCard(card))
                {
                    player.timePointSlots.Add(i);
                }
            }
        }
    }

    private int GetEarliestBarrierSlot(PlayerState player)
    {
        for (int i = 0; i < player.timeline.Length; i++)
        {
            if (!player.timeline[i].IsEmpty)
            {
                CardData card = player.timeline[i].currentCard.card;

                if (IsBarrierCard(card))
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private int GetEarliestUsableTimePointSlot(PlayerState player)
    {
        if (player.timePointSlots == null || player.timePointSlots.Count == 0)
        {
            return -1;
        }

        int earliestBarrier = GetEarliestBarrierSlot(player);
        int best = -1;

        for (int i = 0; i < player.timePointSlots.Count; i++)
        {
            int tpSlot = player.timePointSlots[i];

            // 如果有 Barrier，则 Barrier 之前的 Time Point 不再 usable
            if (earliestBarrier >= 0 && tpSlot < earliestBarrier)
            {
                continue;
            }

            if (best == -1 || tpSlot < best)
            {
                best = tpSlot;
            }
        }

        return best;
    }

    private bool HasUsableTimePoint(PlayerState player)
    {
        return GetEarliestUsableTimePointSlot(player) >= 0;
    }

    private bool HasUsableTimePointAtResolution(PlayerState player, int currentResolvingSlot)
    {
        int earliestBarrier = GetEarliestBarrierSlot(player);

        for (int i = 0; i <= currentResolvingSlot; i++)
        {
            if (!player.timeline[i].IsEmpty)
            {
                CardData card = player.timeline[i].currentCard.card;

                if (IsTimePointCard(card))
                {
                    // 如果有 barrier，则 barrier 之前的 time point 不算 usable
                    if (earliestBarrier >= 0 && i < earliestBarrier)
                    {
                        continue;
                    }

                    return true;
                }
            }
        }

        return false;
    }
    public void OnPlayer1TargetSlotSelected(int slotIndex)
    {
        if (currentPhase != GamePhase.TurnPlanning)
        {
            return;
        }

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

    private bool IsSlotLockedByBarrier(PlayerState player, int slotIndex)
    {
        int earliestBarrier = GetEarliestBarrierSlot(player);

        if (earliestBarrier < 0)
        {
            return false;
        }

        // Barrier 之前的 slot 被锁
        return slotIndex < earliestBarrier;
    }

    public void ConfirmPlayer1Placement()
    {
        if (selectedPlayer1Card == null || selectedPlayer1HandIndex < 0 || selectedTargetSlotIndex < 0)
        {
            Debug.Log("Cannot confirm: card or slot not selected.");
            return;
        }

        if (!IsSlotSelectableForCurrentTurn(selectedTargetSlotIndex))
        {
            Debug.Log("Cannot confirm: selected slot is not legal.");
            return;
        }

        // 创建运行时牌
        PlayedCard playedCard = new PlayedCard(
            selectedPlayer1Card,
            currentRound,
            selectedTargetSlotIndex,
            player1
        );

        // 放到目标时间线格（允许覆盖旧牌）
        player1.timeline[selectedTargetSlotIndex].currentCard = playedCard;

        // 关键：根据当前 timeline 真实内容重建 time point 列表
        RebuildTimePointSlots(player1);

        // 从手牌移除
        player1.hand.RemoveAt(selectedPlayer1HandIndex);

        // 对手自动出一张
        AutoPlayForPlayer2();

        // 回合前进
        currentRound++;

        // 如果已经完成 12 回合，进入最终结算阶段
        if (currentRound >= maxRounds)
        {
            currentPhase = GamePhase.FinalResolution;
        }

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

        // 关键：根据当前 timeline 真实内容重建 time point 列表
        RebuildTimePointSlots(player2);

        player2.hand.RemoveAt(randomIndex);
    }

    public void OnMainActionButtonPressed()
    {
        if (currentPhase == GamePhase.TurnPlanning)
        {
            ConfirmPlayer1Placement();
        }
        else if (currentPhase == GamePhase.FinalResolution)
        {
            ResolveEntireGame();
        }
    }

    private void ResolveSingleCardAtSlot(PlayerState player, int slotIndex)
    {
        if (player.timeline[slotIndex].IsEmpty)
        {
            return;
        }

        CardData card = player.timeline[slotIndex].currentCard.card;

        switch (card.effectType)
        {
            case CardEffectType.GainCoins:
                player.coins += 5;
                Debug.Log($"{player.playerId} gains +5 from slot {slotIndex + 1}");
                break;

            case CardEffectType.Lottery:
                if (HasUsableTimePointAtResolution(player, slotIndex))
                {
                    player.coins += 10;
                    Debug.Log($"{player.playerId} gains +10 from Lottery at slot {slotIndex + 1}");
                }
                else
                {
                    Debug.Log($"{player.playerId} Lottery failed at slot {slotIndex + 1}");
                }
                break;

            case CardEffectType.SetTimePoint:
            case CardEffectType.Barrier:
            case CardEffectType.None:
                // 这些暂时不直接加分
                break;

            case CardEffectType.Rob:
            case CardEffectType.Camera:
            case CardEffectType.Court:
            case CardEffectType.Joker:
                // 暂时未实现
                Debug.Log($"{player.playerId} has unresolved effect {card.effectType} at slot {slotIndex + 1}");
                break;
        }
    }

    private void ResolveEntireGame()
    {
        if (currentPhase != GamePhase.FinalResolution)
        {
            return;
        }

        Debug.Log("=== FINAL RESOLUTION START ===");

        player1.coins = 0;
        player2.coins = 0;

        for (int slotIndex = 0; slotIndex < maxRounds; slotIndex++)
        {
            Debug.Log($"--- Resolving slot {slotIndex + 1} ---");

            ResolveSingleCardAtSlot(player1, slotIndex);
            ResolveSingleCardAtSlot(player2, slotIndex);
        }

        currentPhase = GamePhase.GameEnded;

        Debug.Log($"Player 1 coins: {player1.coins}");
        Debug.Log($"Player 2 coins: {player2.coins}");

        if (player1.coins > player2.coins)
        {
            Debug.Log("Player 1 wins!");
        }
        else if (player2.coins > player1.coins)
        {
            Debug.Log("Player 2 wins!");
        }
        else
        {
            Debug.Log("Draw!");
        }

        RefreshAllUI();
    }
}