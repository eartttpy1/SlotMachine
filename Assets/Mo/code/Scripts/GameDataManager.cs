using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    [Header("Scriptable UpStats References")]
    public ScriptableUpStats damageReductionStat;
    public ScriptableUpStats damageBonusStat;
    public ScriptableUpStats maxHpStat;
    public ScriptableUpStats ticketDropStat;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public float GetDamageReduction()
    {
        return damageReductionStat != null ? (damageReductionStat.CurrentBaseValue / 100f) : 0f;
    }

    public float GetDamageBonus()
    {
        return damageBonusStat != null ? (damageBonusStat.CurrentBaseValue / 100f) : 0f;
    }

    public int GetMaxHP()
    {
        if (maxHpStat == null) return 100;
        return maxHpStat.CountLevel == 0 ? 100 : Mathf.RoundToInt(maxHpStat.CurrentBaseValue);
    }

    public float GetTicketMultiplier()
    {
        return ticketDropStat != null ? ticketDropStat.CurrentBaseValue : 1f;
    }
}
