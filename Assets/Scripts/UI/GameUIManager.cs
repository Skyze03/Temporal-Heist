using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text roundText;

    [Header("Timeline Parents")]
    public Transform player1TimelineParent;
    public Transform player2TimelineParent;

    [Header("Hand Parent")]
    public Transform player1HandParent;

    [Header("Reveal Area")]
    public TMP_Text player1RevealText;
    public TMP_Text player2RevealText;

    [Header("Prefabs")]
    public GameObject timelineSlotPrefab;
    public GameObject cardButtonPrefab;

    [Header("Buttons")]
    public Button resolveButton;

    public void SetRoundText(int currentRound, int maxRounds)
    {
        roundText.text = $"Round {currentRound + 1} / {maxRounds}";
    }

    public void ClearParent(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    public void BuildTimelineUI(Transform parent, PlayerState player)
    {
        ClearParent(parent);

        for (int i = 0; i < player.timeline.Length; i++)
        {
            GameObject slotObj = Instantiate(timelineSlotPrefab, parent);
            TimelineSlotUI slotUI = slotObj.GetComponent<TimelineSlotUI>();

            string text = $"[{i + 1}] Empty";

            if (!player.timeline[i].IsEmpty)
            {
                text = $"[{i + 1}] {player.timeline[i].currentCard.card.displayName}";
            }

            if (slotUI != null)
            {
                slotUI.SetText(text);
            }
        }
    }

    public void BuildPlayerHandUI(PlayerState player, GameManager gameManager)
    {
        ClearParent(player1HandParent);

        for (int i = 0; i < player.hand.Count; i++)
        {
            GameObject cardObj = Instantiate(cardButtonPrefab, player1HandParent);
            CardButtonUI cardUI = cardObj.GetComponent<CardButtonUI>();

            if (cardUI != null)
            {
                cardUI.Setup(player.hand[i], gameManager, i);
            }
        }
    }

    public void SetRevealText(string p1Text, string p2Text)
    {
        if (player1RevealText != null)
        {
            player1RevealText.text = p1Text;
        }

        if (player2RevealText != null)
        {
            player2RevealText.text = p2Text;
        }
    }
}