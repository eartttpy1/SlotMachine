using UnityEngine;

public enum SlotSymbol { GreatSword, Sword, HealingPotion, Shield, Coin }

[CreateAssetMenu(fileName = "SlotIcon_", menuName = "SlotGame/SlotIconData")]
public class SlotIconData : ScriptableObject
{
    public SlotSymbol symbolType;
    public string iconName;
    public Sprite iconSprite;
    [TextArea] public string skillDescription;
    [TextArea] public string bonusDescription;

    [Header("Combat Values")]
    public int baseValue = 15;
    public int match3Multiplier = 3;
    public bool isMultipleTargets = true;
    public int baseWeightRandom = 1; // ค่าน้ำหนักในการสุ่ม (Weighted Random)

    [Header("Shop Progression")]
    public int baseUpgradePrice = 10;
    public int countUpgrade = 0;
    public float upgradeMultiplier = 1.2f;
    [TextArea] public string upgradeDescription;

    public int GetCurrentPrice() {
        return Mathf.CeilToInt(baseUpgradePrice * Mathf.Pow(1.5f, countUpgrade));
    }

    public int GetCurrentValue() {
        // อัปเกรดบวกเพิ่มทีละ 20% และปัดเศษขึ้นตามสเปก (+0.5f แล้วแปลงเป็น int)
        float upgradedValue = baseValue * Mathf.Pow(upgradeMultiplier, countUpgrade);
        return Mathf.FloorToInt(upgradedValue + 0.5f);
    }
}
