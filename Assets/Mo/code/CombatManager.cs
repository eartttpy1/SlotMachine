using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    public enum CombatState { PlayerTurn, TargetSelection, EnemyTurn, Victory, Defeat }

    [Header("Combat Status")]
    public CombatState currentState;
    public int currentLevel = 1;
    public bool upstat = false;
    public bool nextlevel = false;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI detailTurnText;

    [Header("Enemies")]
    public List<ScriptableEnemy> enemyTemplates = new List<ScriptableEnemy>();
    public List<EnemyInstance> activeEnemies = new List<EnemyInstance>();

    [Header("UI Enemy Display Container")]
    public Transform enemyContainerParent;
    public GameObject enemyDisplayPrefab;
    private List<EnemyDisplay> spawnedDisplays = new List<EnemyDisplay>();

    [System.Serializable]
    public class EnemyInstance
    {
        public ScriptableEnemy data;
        public int maxHP;
        public int currentHP;
        public float damageMultiplier = 1.0f;

        public EnemyInstance(ScriptableEnemy enemyData, int level)
        {
            data = enemyData;
            maxHP = enemyData.GetMaxHP(level);
            currentHP = maxHP;
            damageMultiplier = 1.0f;
        }
    }

    private int pendingSwordDamage = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // For testing/initialization, start battle if templates are set and MapManager is not present
        if (MapManager.Instance == null && enemyTemplates.Count > 0)
        {
            StartCombat(enemyTemplates, currentLevel);
        }
    }

    private void Update()
    {
        if (turnText != null)
        {
            switch (currentState)
            {
                case CombatState.PlayerTurn:
                    turnText.text = "Player Turn";
                    break;
                case CombatState.TargetSelection:
                    turnText.text = "Player Turn";
                    break;
                case CombatState.EnemyTurn:
                    turnText.text = "Enemy Turn";
                    break;
                case CombatState.Victory:
                    turnText.text = "Victory!";
                    break;
                case CombatState.Defeat:
                    turnText.text = "Defeat!";
                    break;
            }
        }

        if (detailTurnText != null)
        {
            switch (currentState)
            {
                case CombatState.PlayerTurn:
                    detailTurnText.text = "";
                    break;
                case CombatState.TargetSelection:
                    detailTurnText.text = "Choose Target!";
                    break;
                case CombatState.Victory:
                    detailTurnText.text = "";
                    break;
                case CombatState.Defeat:
                    detailTurnText.text = "";
                    break;
                case CombatState.EnemyTurn:
                    // Set dynamically in EnemyTurnRoutine to show dmg detail
                    break;
            }
        }
    }

    public void StartCombat(List<ScriptableEnemy> templates, int level)
    {
        currentLevel = level;
        upstat = false;
        nextlevel = false;
        pendingSwordDamage = 0;

        activeEnemies.Clear();
        foreach (var t in templates)
        {
            if (t != null)
            {
                activeEnemies.Add(new EnemyInstance(t, level));
            }
        }

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.currentShield = 0;
            PlayerStats.Instance.isplayerturn = true;
        }

        currentState = CombatState.PlayerTurn;
        RefreshEnemyDisplays();
        Debug.Log($"Combat Started! Level {level}. {activeEnemies.Count} enemies spawned.");
    }

    public void RefreshEnemyDisplays()
    {
        if (enemyContainerParent == null || enemyDisplayPrefab == null) return;

        // Clear existing displays
        foreach (var disp in spawnedDisplays)
        {
            if (disp != null) Destroy(disp.gameObject);
        }
        spawnedDisplays.Clear();

        // Spawn new ones
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            GameObject obj = Instantiate(enemyDisplayPrefab, enemyContainerParent);
            EnemyDisplay disp = obj.GetComponent<EnemyDisplay>();
            if (disp != null)
            {
                disp.Setup(activeEnemies[i], i);
                spawnedDisplays.Add(disp);
            }
        }
    }

    public void ProcessSlotResult(SlotSymbolData[] results)
    {
        if (currentState != CombatState.PlayerTurn)
        {
            Debug.LogWarning("Cannot process slot results: Not Player Turn!");
            return;
        }

        int swordCount = 0;
        int greatSwordCount = 0;
        int shieldCount = 0;

        SlotIconData swordData = null;
        SlotIconData greatSwordData = null;
        SlotIconData shieldData = null;

        foreach (var symbol in results)
        {
            if (symbol is SlotIconData icon)
            {
                if (icon.symbolType == SlotSymbol.Sword)
                {
                    swordCount++;
                    swordData = icon;
                }
                else if (icon.symbolType == SlotSymbol.GreatSword)
                {
                    greatSwordCount++;
                    greatSwordData = icon;
                }
                else if (icon.symbolType == SlotSymbol.Shield)
                {
                    shieldCount++;
                    shieldData = icon;
                }
            }
        }

        // 1. Resolve Shield
        if (shieldCount > 0 && shieldData != null && PlayerStats.Instance != null)
        {
            int shieldVal = (shieldCount == 3) ? (shieldData.baseValue * shieldData.match3Multiplier) : (shieldCount * shieldData.baseValue);
            PlayerStats.Instance.currentShield += shieldVal;
            Debug.Log($"Shield rolled! Added {shieldVal} shield. Total Shield: {PlayerStats.Instance.currentShield}");
        }

        // 2. Resolve GreatSword (AoE Damage to all enemies)
        if (greatSwordCount > 0 && greatSwordData != null)
        {
            int greatSwordDmg = (greatSwordCount == 3) ? (greatSwordData.baseValue * greatSwordData.match3Multiplier) : (greatSwordCount * greatSwordData.baseValue);
            if (GameDataManager.Instance != null)
            {
                greatSwordDmg = Mathf.RoundToInt(greatSwordDmg * (1f + GameDataManager.Instance.GetDamageBonus()));
            }
            Debug.Log($"GreatSword rolled! Dealing {greatSwordDmg} AoE damage to all enemies.");
            DealAoEDamage(greatSwordDmg);
        }

        // Check if all enemies died from AoE before resolving Swords
        if (CheckVictoryCondition()) return;

        // 3. Resolve Sword (Single Target Damage)
        if (swordCount > 0 && swordData != null)
        {
            pendingSwordDamage = (swordCount == 3) ? (swordData.baseValue * swordData.match3Multiplier) : (swordCount * swordData.baseValue);
            if (GameDataManager.Instance != null)
            {
                pendingSwordDamage = Mathf.RoundToInt(pendingSwordDamage * (1f + GameDataManager.Instance.GetDamageBonus()));
            }

            // If only one enemy left, automatically target it
            if (activeEnemies.Count == 1)
            {
                ExecuteSwordAttack(0);
            }
            else
            {
                currentState = CombatState.TargetSelection;
                UpdateAllDisplayVisuals();
                Debug.Log($"Sword rolled! Pending {pendingSwordDamage} damage. Please click/select an enemy to target.");
            }
        }
        else
        {
            // Proceed to enemy turn if no sword damage is pending
            StartCoroutine(EnemyTurnRoutine());
        }
    }

    private void DealAoEDamage(int dmg)
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            DamageEnemy(i, dmg);
        }
    }

    public void SelectEnemyTarget(int index)
    {
        if (currentState != CombatState.TargetSelection || pendingSwordDamage <= 0)
        {
            Debug.LogWarning("No pending sword attack target selection needed.");
            return;
        }

        ExecuteSwordAttack(index);
    }

    private void ExecuteSwordAttack(int index)
    {
        if (index < 0 || index >= activeEnemies.Count) return;

        Debug.Log($"Attacking enemy {activeEnemies[index].data.enemyName} for {pendingSwordDamage} Sword damage.");
        DamageEnemy(index, pendingSwordDamage);
        pendingSwordDamage = 0;

        if (!CheckVictoryCondition())
        {
            StartCoroutine(EnemyTurnRoutine());
        }
    }

    public void UpdateAllDisplayVisuals()
    {
        foreach (var disp in spawnedDisplays)
        {
            if (disp != null) disp.UpdateVisuals();
        }
    }

    private void DamageEnemy(int index, int damage)
    {
        if (index < 0 || index >= activeEnemies.Count) return;

        EnemyInstance enemy = activeEnemies[index];
        enemy.currentHP = Mathf.Max(0, enemy.currentHP - damage);
        Debug.Log($"Enemy {enemy.data.enemyName} HP: {enemy.currentHP}/{enemy.maxHP}");

        if (enemy.currentHP <= 0)
        {
            // Enemy Died! Reward player
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.coins += enemy.data.coin;
                PlayerStats.Instance.AddChance067(enemy.data.chance067);
                Debug.Log($"Enemy {enemy.data.enemyName} defeated! Reward: +{enemy.data.coin} coins, +{enemy.data.chance067}% chance067");
            }
            activeEnemies.RemoveAt(index);
            RefreshEnemyDisplays();
        }
        else
        {
            UpdateAllDisplayVisuals();
        }
    }

    private bool CheckVictoryCondition()
    {
        if (activeEnemies.Count == 0)
        {
            currentState = CombatState.Victory;
            nextlevel = true;
            RefreshEnemyDisplays();
            Debug.Log("Victory! All enemies defeated. nextlevel set to true.");
            if (MapManager.Instance != null)
            {
                MapManager.Instance.OnAllEnemiesDefeated();
            }
            return true;
        }
        return false;
    }

    private IEnumerator EnemyTurnRoutine()
    {
        currentState = CombatState.EnemyTurn;
        if (detailTurnText != null)
        {
            detailTurnText.text = "Preparing to attack...";
        }
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.isplayerturn = false;
        }
        UpdateAllDisplayVisuals();

        yield return new WaitForSeconds(1f);

        // Each enemy turn
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            EnemyInstance enemy = activeEnemies[i];

            // Check if player died before this enemy attacks
            if (PlayerStats.Instance != null && PlayerStats.Instance.currentHP <= 0)
            {
                break;
            }

            int dmgPerHit = Mathf.RoundToInt(enemy.data.GetBaseDMG(currentLevel) * enemy.damageMultiplier);
            int totalHits = enemy.data.countHit;

            Debug.Log($"Enemy {enemy.data.enemyName} turn! Attacks {totalHits} times for {dmgPerHit} dmg each (Multiplier: {enemy.damageMultiplier:F2}).");

            for (int hit = 0; hit < totalHits; hit++)
            {
                if (PlayerStats.Instance != null)
                {
                    // เช็คว่าผู้เล่นมีเกราะ (Shield) เหลืออยู่หรือไม่ก่อนโดนโจมตี
                    bool hasShield = PlayerStats.Instance.currentShield > 0;

                    PlayerStats.Instance.TakeDamage(dmgPerHit);

                    // เล่นเสียงโดนโจมตีตามเงื่อนไขเกราะป้องกัน
                    if (AudioManager.Instance != null)
                    {
                        if (hasShield)
                        {
                            AudioManager.Instance.PlayShieldHitSound();
                        }
                        else
                        {
                            AudioManager.Instance.PlayGetHitSound();
                        }
                    }
                }
                if (detailTurnText != null)
                {
                    detailTurnText.text = $"{enemy.data.enemyName} Hit ({hit + 1}/{totalHits}): -{dmgPerHit} HP";
                }
                yield return new WaitForSeconds(0.4f); // Slightly longer delay to let player read
            }

            // Damage multiplier * 1.2 each turn
            enemy.damageMultiplier *= 1.2f;
        }

        // Check if player die -> upstat = true
        if (PlayerStats.Instance != null && PlayerStats.Instance.currentHP <= 0)
        {
            currentState = CombatState.Defeat;
            upstat = true;
            UpdateAllDisplayVisuals();
            Debug.Log("Player defeated! upstat set to true.");
        }
        else
        {
            // Back to player turn
            currentState = CombatState.PlayerTurn;
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.isplayerturn = true;
            }
            UpdateAllDisplayVisuals();
            Debug.Log("Player turn starts! Roll slot 1 time.");
        }
    }
}