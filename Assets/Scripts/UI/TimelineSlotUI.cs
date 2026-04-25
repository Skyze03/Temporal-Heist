using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimelineSlotUI : MonoBehaviour
{
    public TMP_Text labelText;
    public Button button;

    private GameManager gameManager;
    private int slotIndex;
    private bool isSelectable;

    public void Setup(string text, GameManager manager, int index, bool selectable)
    {
        if (labelText != null)
        {
            labelText.text = text;
        }

        gameManager = manager;
        slotIndex = index;
        isSelectable = selectable;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickSlot);
            button.interactable = selectable;
        }
    }

    private void OnClickSlot()
    {
        if (!isSelectable) return;

        if (gameManager != null)
        {
            gameManager.OnPlayer1TargetSlotSelected(slotIndex);
        }
    }
}