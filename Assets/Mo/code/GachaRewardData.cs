using UnityEngine;

public enum GachaRewardType { Opportunity067, MaxHP, ReduceDamage, Coin }

[CreateAssetMenu(fileName = "Gacha_", menuName = "SlotGame/GachaRewardData")]
public class GachaRewardData : SlotSymbolData
{
    public GachaRewardType rewardType;
    public string rewardName;
    public Sprite rewardSprite;
    public int baseValue;
    public int jackpotMultiplier = 5;
    public int baseWeightRandom = 10; // สุ่มแยกตู้กาชาต่างหาก
    [TextArea] public string description;
    [TextArea] public string bonusDescription;

    // Implement base properties of SlotSymbolData
    public override string SymbolName => rewardName;
    public override Sprite SymbolSprite => rewardSprite;
    public override int BaseWeight => baseWeightRandom;
}