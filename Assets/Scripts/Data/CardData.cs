using UnityEngine;

[CreateAssetMenu(fileName = "NewCardData", menuName = "TimeTimeline/Card Data")]
public class CardData : ScriptableObject
{
    public CardRank rank;
    public CardEffectType effectType;

    public string displayName;

    [TextArea]
    public string description;

    public int pointValue;
    public int coinValue;
}