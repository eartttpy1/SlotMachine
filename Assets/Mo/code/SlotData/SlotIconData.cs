using UnityEngine;

public enum SlotSymbol { GreatSword, Sword, HealingPotion, Shield, Coin }

[CreateAssetMenu(fileName = "SlotIcon_", menuName = "SlotGame/SlotIconData")]
public class SlotIconData : SlotSymbolData
{
    public SlotSymbol symbolType;
    public string iconName;
    public Sprite iconSprite;
    [TextArea] public string skillDescription;
    [TextArea] public string iconBonusDescription;

    [Header("Combat Values")]
    public int baseValue = 15;
    public int match3Multiplier = 5;
    public bool isMultipleTargets = true;
    public int baseWeightRandom = 1; // ค่าน้ำหนักในการสุ่ม (Weighted Random)

    // Implement base properties of SlotSymbolData
    public override string SymbolName => iconName;
    public override Sprite SymbolSprite => iconSprite;
    public override int BaseWeight => baseWeightRandom;
    public override string description 
    {
        get
        {
            // if (!string.IsNullOrEmpty(skillDescription))
            // {
            //     return skillDescription;
            // }

            switch (symbolType)
            {
                case SlotSymbol.Sword:
                    int swordVal = GetCurrentValue();
                    if (GameDataManager.Instance != null)
                    {
                        swordVal = Mathf.RoundToInt(swordVal * (1f + GameDataManager.Instance.GetDamageBonus()));
                    }
                    return $"{swordVal} damage to 1 target";

                case SlotSymbol.GreatSword:
                    int gsVal = GetCurrentValue();
                    if (GameDataManager.Instance != null)
                    {
                        gsVal = Mathf.RoundToInt(gsVal * (1f + GameDataManager.Instance.GetDamageBonus()));
                    }
                    return $"{gsVal} damage to multiple targets \nMiniGame x2";

                case SlotSymbol.Shield:
                    return $"grants {GetCurrentValue()} shield";

                case SlotSymbol.HealingPotion:
                    return $"Heal {GetCurrentValue()} HP";

                case SlotSymbol.Coin:
                    return $"gain {GetCurrentValue()} coin";

                default:
                    return skillDescription;
            }
        }
    }
    public override string bonusDescription => iconBonusDescription;

    [Header("Shop Progression")]
    public int baseUpgradePrice = 10;
    public int countUpgrade 
    {
        get 
        {
            if (PlayerStats.Instance != null)
            {
                return PlayerStats.Instance.GetUpgradeLevel(symbolType);
            }
            return 0;
        }
        set 
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.SetUpgradeLevel(symbolType, value);
            }
        }
    }
    public int upgradeMultiplier = 20;
    [TextArea] public string upgradeDescription;

    public int GetUpgradeIncrement() {
        float increment = GetCurrentValue() * (upgradeMultiplier / 100f);
        return Mathf.Max(1, Mathf.RoundToInt(increment)); // Ensure it increases by at least 1
    }

    public int GetCurrentPrice() {
        return Mathf.CeilToInt(baseUpgradePrice * Mathf.Pow(1.5f, countUpgrade));
    }

    public int GetCurrentValue() {
        int val = baseValue;
        for (int i = 0; i < countUpgrade; i++) {
            float increment = val * (upgradeMultiplier / 100f);
            val += Mathf.Max(1, Mathf.RoundToInt(increment));
        }
        return val;
    }

    public int GetValueWithUpstat(int val) {
        if (symbolType == SlotSymbol.Sword || symbolType == SlotSymbol.GreatSword) {
            if (GameDataManager.Instance != null) {
                return Mathf.RoundToInt(val * (1f + GameDataManager.Instance.GetDamageBonus()));
            }
        }
        return val;
    }

    public void ResetToDefault() {
        countUpgrade = 0;
    }

    private void OnDisable() {
        ResetToDefault();
    }
}
