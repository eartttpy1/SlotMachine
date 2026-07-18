using UnityEngine;

[CreateAssetMenu(fileName = "UpStat_", menuName = "SlotGame/ScriptableUpStats")]
public class ScriptableUpStats : ScriptableObject
{
    public string statName;
    public Sprite icon;
    public float baseValue;
    public float increaseValue;
    [TextArea] public string descriptionFormat; // e.g. "Damage taken reduced by {0}%"
    public int maxLevel = 10;
    public int baseCost = 100;

    // Keys for PlayerPrefs to persist counts, costs, and current values
    private string LevelKey => "UpStat_Level_" + name;
    private string CostKey => "UpStat_Cost_" + name;
    private string ValueKey => "UpStat_Value_" + name;

    public int CountLevel
    {
        get { return PlayerPrefs.GetInt(LevelKey, 0); }
        set { PlayerPrefs.SetInt(LevelKey, value); PlayerPrefs.Save(); }
    }

    public int Point
    {
        get { return PlayerPrefs.GetInt(CostKey, baseCost); }
        set { PlayerPrefs.SetInt(CostKey, value); PlayerPrefs.Save(); }
    }

    // Dynamic baseValue that updates after buying
    public float CurrentBaseValue
    {
        get 
        { 
            if (CountLevel == 0)
            {
                // Default to 1 for ticket multipliers, 0 for other stats when not purchased yet
                return name.Contains("Ticket") ? 1f : 0f;
            }
            return PlayerPrefs.GetFloat(ValueKey, baseValue); 
        }
        set { PlayerPrefs.SetFloat(ValueKey, value); PlayerPrefs.Save(); }
    }

    public string GetDynamicDescription()
    {
        return string.Format(descriptionFormat, CurrentBaseValue);
    }

    public float GetNextUpgradeIncrement()
    {
        if (CountLevel == 0)
        {
            float level0Value = name.Contains("Ticket") ? 1f : (name.ToLower().Contains("hp") ? 100f : 0f);
            return baseValue - level0Value;
        }
        return increaseValue;
    }

    public string GetUpgradeDescription()
    {
        if (CountLevel >= maxLevel)
        {
            return string.Format(descriptionFormat, increaseValue);
        }
        return string.Format(descriptionFormat, GetNextUpgradeIncrement());
    }

    public string GetNextValueDescription()
    {
        float nextVal = CountLevel == 0 ? baseValue : CurrentBaseValue + increaseValue;
        return string.Format(descriptionFormat, nextVal);
    }

    public void BuyUpgrade()
    {
        if (CountLevel < maxLevel)
        {
            CountLevel++;
            CurrentBaseValue = baseValue + (CountLevel - 1) * increaseValue;
            Point = Mathf.RoundToInt(Point * 1.2f);
        }
    }

    public void ResetUpgrade()
    {
        PlayerPrefs.DeleteKey(LevelKey);
        PlayerPrefs.DeleteKey(CostKey);
        PlayerPrefs.DeleteKey(ValueKey);
        PlayerPrefs.Save();
    }
}
