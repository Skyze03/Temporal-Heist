using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardButtonUI : MonoBehaviour
{
    public TMP_Text labelText;
    public Button button;

    private CardData cardData;
    private GameManager gameManager;
    private int handIndex;

    public void Setup(CardData data, GameManager manager, int index)
    {
        cardData = data;
        gameManager = manager;
        handIndex = index;

        if (labelText != null)
        {
            labelText.text = data.displayName;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickCard);
        }
    }

    private void OnClickCard()
    {
        if (gameManager != null)
        {
            gameManager.OnPlayer1CardSelected(handIndex);
        }
    }
}