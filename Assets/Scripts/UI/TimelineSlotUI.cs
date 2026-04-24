using TMPro;
using UnityEngine;

public class TimelineSlotUI : MonoBehaviour
{
    public TMP_Text labelText;

    public void SetText(string text)
    {
        if (labelText != null)
        {
            labelText.text = text;
        }
    }
}