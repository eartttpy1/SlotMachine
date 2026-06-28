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

    private void Start()
    {
        if (damageReductionStat == null) Debug.LogWarning("[GameDataManager] damageReductionStat is not assigned in the Inspector!");
        if (damageBonusStat == null) Debug.LogWarning("[GameDataManager] damageBonusStat is not assigned in the Inspector!");
        if (maxHpStat == null) Debug.LogWarning("[GameDataManager] maxHpStat is not assigned in the Inspector!");
        if (ticketDropStat == null) Debug.LogWarning("[GameDataManager] ticketDropStat is not assigned in the Inspector!");
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
