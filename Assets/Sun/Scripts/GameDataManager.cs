using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    // Keys for PlayerPrefs
    private const string DamageReductionLevelKey = "DamageReductionLevel";
    private const string MaxHPLevelKey = "MaxHPLevel";
    private const string DamageBonusLevelKey = "DamageBonusLevel";

    // Levels for stats
    public int DamageReductionLevel { get; private set; }
    public int MaxHPLevel { get; private set; }
    public int DamageBonusLevel { get; private set; }

    // Configuration for scaling
    [Header("Stat Scaling Settings")]
    public float baseDamageReduction = 0f;
    public float damageReductionPerLevel = 0.05f; // 5%

    public int baseMaxHP = 200;
    public int maxHpPerLevel = 50; // Replace with desired X amount

    public float baseDamageBonus = 0f;
    public float damageBonusPerLevel = 0.20f; // 20%

    private void Awake()
    {
        // Implement Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Make it persistent across scenes
        LoadData();
    }

    private void LoadData()
    {
        DamageReductionLevel = PlayerPrefs.GetInt(DamageReductionLevelKey, 0);
        MaxHPLevel = PlayerPrefs.GetInt(MaxHPLevelKey, 0);
        DamageBonusLevel = PlayerPrefs.GetInt(DamageBonusLevelKey, 0);
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt(DamageReductionLevelKey, DamageReductionLevel);
        PlayerPrefs.SetInt(MaxHPLevelKey, MaxHPLevel);
        PlayerPrefs.SetInt(DamageBonusLevelKey, DamageBonusLevel);
        PlayerPrefs.Save();
    }

    // Getters for actual calculated stats
    public float GetDamageReduction() => baseDamageReduction + (DamageReductionLevel * damageReductionPerLevel);
    public int GetMaxHP() => baseMaxHP + (MaxHPLevel * maxHpPerLevel);
    public float GetDamageBonus() => baseDamageBonus + (DamageBonusLevel * damageBonusPerLevel);

    // Upgrade methods
    public void UpgradeDamageReduction()
    {
        DamageReductionLevel++;
        SaveData();
    }

    public void UpgradeMaxHP()
    {
        MaxHPLevel++;
        SaveData();
    }

    public void UpgradeDamageBonus()
    {
        DamageBonusLevel++;
        SaveData();
    }
}
