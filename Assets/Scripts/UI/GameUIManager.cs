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
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }
}