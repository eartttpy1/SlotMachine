using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

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

    [Header("DOTween UI Settings")]
    [Tooltip("ล็อกเวลาไปเลยว่าต้องพิมพ์ข้อความเสร็จภายในกี่วินาที (แนะนำ 0.2 - 0.25 วินาที จะพิมพ์ไวพอดีกับจังหวะเกม)")]
    public float turnTypingDuration = 0.2f;
    private Sequence turnTypingSequence;

    [Header("Combat Delays")]
    [Tooltip("เวลาหน่วงก่อนที่ศัตรูตัวแรกจะเริ่มโจมตี")]
    public float delayBeforeEnemyAttack = 1.0f;
    [Tooltip("เวลาหน่วงระหว่างการโจมตีแต่ละ Hit ของศัตรู")]
    public float delayBetweenEnemyHits = 0.5f;
    [Tooltip("เวลาหน่วงระหว่างที่ศัตรูแต่ละตัวสลับกันโจมตี")]
    public float delayBetweenEnemies = 0.8f;

    [Header("Enemies")]
    public List<ScriptableEnemy> enemyTemplates = new List<ScriptableEnemy>();
    public List<EnemyInstance> activeEnemies = new List<EnemyInstance>();

    [Header("UI Enemy Display Container")]
    public Transform enemyContainerParent;
    public GameObject enemyDisplayPrefab;
    private List<EnemyDisplay> spawnedDisplays = new List<EnemyDisplay>();

    [Header("Mini-Games")]
    public GreatSwordMiniGame greatSwordMiniGame;

    [Header("Effect Spawn Locations")]
    [Tooltip("ตำแหน่งที่จะเกิดเอฟเฟกต์ Get Hit (ผู้เล่นโดน)")]
    public Transform getHitSpawnLocation;

    [Tooltip("ตำแหน่งที่จะเกิดเอฟเฟกต์ Sword (ดาบปกติ)")]
    public Transform swordSpawnLocation;

    [Tooltip("ตำแหน่งที่จะเกิดเอฟเฟกต์ Great Sword (ดาบใหญ่)")]
    public Transform greatSwordSpawnLocation;

    [System.Serializable]
    public class EnemyInstance
    {
        public ScriptableEnemy data;
        public int maxHP;
        public int currentHP;
        public float damageMultiplier = 1.0f;
        public int stunTurns = 0;

        public EnemyInstance(ScriptableEnemy enemyData, int level)
        {
            data = enemyData;
            maxHP = enemyData.GetMaxHP(level);
            currentHP = maxHP;
            damageMultiplier = 1.0f;
            stunTurns = 0;
        }
    }

    private int pendingSwordDamage = 0;
    private int pendingStunChance = 0;

    [Header("Player Status Buffs")]
    public int strBuffTurns = 0;
    public float strBuffMultiplier = 0f;
    public int reflectPercent = 0;

    [Header("Player Status Buffs UI")]
    public UnityEngine.UI.Image strBuffIconImage;
    public TMPro.TextMeshProUGUI strBuffTurnsText;

    public void UpdateBuffUI()
    {
        if (strBuffIconImage != null)
        {
            bool hasBuff = strBuffTurns > 0;
            strBuffIconImage.gameObject.SetActive(hasBuff);
            if (hasBuff && strBuffTurnsText != null)
            {
                strBuffTurnsText.text = strBuffTurns.ToString();
            }
        }
    }

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
        if (MapManager.Instance == null && enemyTemplates.Count > 0)
        {
            StartCombat(enemyTemplates, currentLevel);
        }
    }

    [Header("Parry System")]
    public float parryWindowDuration = 0.4f;
    public float parryCooldown = 0.1f;
    private bool isParrying = false;
    private float lastParryTime = -99f;

    private void Update()
    {
        // Right click to parry (MouseButton 1 is Right Click)
        if (currentState == CombatState.EnemyTurn && Input.GetMouseButtonDown(1))
        {
            if (Time.time >= lastParryTime + parryCooldown)
            {
                StartCoroutine(TriggerParryWindow());
            }
        }
    }

    private IEnumerator TriggerParryWindow()
    {
        isParrying = true;
        lastParryTime = Time.time;
        Debug.Log("Parry Active!");
        
        if (detailTurnText != null)
        {
            detailTurnText.text = "<color=#00FFFF>PARRY ACTIVE!</color>";
        }

        yield return new WaitForSeconds(parryWindowDuration);
        isParrying = false;
        Debug.Log("Parry Ended.");
        if (detailTurnText != null && detailTurnText.text.Contains("PARRY ACTIVE"))
        {
            detailTurnText.text = "";
        }
    }

    public void OnEnemyAttackSwing(EnemyInstance enemy, EnemyDisplay display)
    {
        if (PlayerStats.Instance == null || PlayerStats.Instance.currentHP <= 0) return;

        int dmgPerHit = Mathf.RoundToInt(enemy.data.GetBaseDMG(currentLevel) * enemy.damageMultiplier);
        bool hasShield = PlayerStats.Instance.currentShield > 0;

        if (isParrying)
        {
            Debug.Log("Attack Parried!");
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayParrySound();
            }
            AnimateTurnText("Enemy Turn", "<color=#00FFFF>PARRIED!</color>");
        }
        else
        {
            PlayerStats.Instance.TakeDamage(dmgPerHit);
            if (ShogunEffectManager.Instance != null)
            {
                ShogunEffectManager.Instance.SpawnGetHitEffect();
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGetHitSound();
            }
            AnimateTurnText("Enemy Turn", $"{enemy.data.enemyName} Hit: -{dmgPerHit} HP");

            // Reflect!
            if (reflectPercent > 0 && enemy.currentHP > 0)
            {
                int reflectedDmg = Mathf.RoundToInt(dmgPerHit * reflectPercent / 100f);
                if (reflectedDmg > 0)
                {
                    enemy.currentHP = Mathf.Max(0, enemy.currentHP - reflectedDmg);
                    Debug.Log($"Reflected {reflectedDmg} damage to {enemy.data.enemyName}. HP is now {enemy.currentHP}");
                    
                    if (enemy.currentHP <= 0)
                    {
                        if (PlayerStats.Instance != null)
                        {
                            PlayerStats.Instance.coins += enemy.data.coin;
                            PlayerStats.Instance.AddChance067(enemy.data.chance067);
                        }
                        if (display != null)
                        {
                            display.PlayDieAnimation();
                        }
                    }
                    else
                    {
                        if (display != null)
                        {
                            display.PlaySwordHitAnimation();
                            display.PlayImpactAnimation();
                        }
                    }
                    UpdateAllDisplayVisuals();
                }
            }
        }
    }

    public void AnimateTurnText(string turnStr, string detailStr)
    {
        // เคลียร์แอนิเมชันเก่าทิ้งทันทีเมื่อมีข้อความใหม่เข้ามา
        turnTypingSequence?.Kill();
        turnTypingSequence = DOTween.Sequence();

        // 1. จัดการข้อความหลัก (Turn Text)
        if (turnText != null)
        {
            turnText.text = "";
            // ป้องกันปัญหากรณีส่งข้อความว่างเปล่าเข้ามา จะได้ไม่เสียเวลาวิ่งแอนิเมชัน
            if (!string.IsNullOrEmpty(turnStr))
            {
                int turnLen = 0;
                turnTypingSequence.Append(DOTween.To(() => turnLen, x => {
                    turnLen = x;
                    turnText.text = turnStr.Substring(0, turnLen);
                }, turnStr.Length, turnTypingDuration).SetEase(Ease.OutQuad)); // ใช้ OutQuad จะพิมพ์เร็วตอนเริ่มและนุ่มนวลตอนท้าย
            }
        }

        // ใส่ Interval หน่วงเวลาระหว่างข้อความหลักกับข้อความย่อย เฉพาะตอนที่มีข้อความอยู่ทั้งคู่เท่านั้น
        if (!string.IsNullOrEmpty(turnStr) && !string.IsNullOrEmpty(detailStr))
        {
            turnTypingSequence.AppendInterval(0.1f);
        }

        // 2. จัดการข้อความย่อย (Detail Turn Text)
        if (detailTurnText != null)
        {
            detailTurnText.text = "";
            if (!string.IsNullOrEmpty(detailStr))
            {
                int detailLen = 0;
                turnTypingSequence.Append(DOTween.To(() => detailLen, x => {
                    detailLen = x;
                    detailTurnText.text = detailStr.Substring(0, detailLen);
                }, detailStr.Length, turnTypingDuration).SetEase(Ease.OutQuad));
            }
        }
    }

    public void StartCombat(List<ScriptableEnemy> templates, int level)
    {
        currentLevel = level;
        upstat = false;
        nextlevel = false;
        pendingSwordDamage = 0;
        pendingStunChance = 0;
        strBuffTurns = 0;
        strBuffMultiplier = 0f;
        reflectPercent = 0;

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
        AnimateTurnText("Player Turn", "");
        RefreshEnemyDisplays();
        UpdateBuffUI();
        Debug.Log($"Combat Started! Level {level}. {activeEnemies.Count} enemies spawned.");
    }

    public void RefreshEnemyDisplays()
    {
        if (enemyContainerParent == null || enemyDisplayPrefab == null) return;

        foreach (var disp in spawnedDisplays)
        {
            if (disp != null) Destroy(disp.gameObject);
        }
        spawnedDisplays.Clear();

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

        StartCoroutine(ResolvePlayerTurnRoutine(results));
    }

    private IEnumerator ResolvePlayerTurnRoutine(SlotSymbolData[] results)
    {
        int swordCount = 0;
        int greatSwordCount = 0;
        int shieldCount = 0;
        int axeCount = 0;
        int hammerCount = 0;
        int strPotionCount = 0;
        int spikedShieldCount = 0;

        SlotIconData swordData = null;
        SlotIconData greatSwordData = null;
        SlotIconData shieldData = null;
        SlotIconData axeData = null;
        SlotIconData hammerData = null;
        SlotIconData strPotionData = null;
        SlotIconData spikedShieldData = null;

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
                else if (icon.symbolType == SlotSymbol.Axe)
                {
                    axeCount++;
                    axeData = icon;
                }
                else if (icon.symbolType == SlotSymbol.Hammer)
                {
                    hammerCount++;
                    hammerData = icon;
                }
                else if (icon.symbolType == SlotSymbol.StrPotion)
                {
                    strPotionCount++;
                    strPotionData = icon;
                }
                else if (icon.symbolType == SlotSymbol.SpikedShield)
                {
                    spikedShieldCount++;
                    spikedShieldData = icon;
                }
            }
        }

        if (shieldCount > 0 && shieldData != null && PlayerStats.Instance != null)
        {
            int shieldVal = (shieldCount == 3) ? (shieldData.GetCurrentValue() * shieldData.match3Multiplier) : (shieldCount * shieldData.GetCurrentValue());
            PlayerStats.Instance.currentShield += shieldVal;
            Debug.Log($"Shield rolled! Added {shieldVal} shield. Total Shield: {PlayerStats.Instance.currentShield}");
            yield return new WaitForSeconds(0.2f);
        }

        if (spikedShieldCount > 0 && spikedShieldData != null && PlayerStats.Instance != null)
        {
            int baseShield = spikedShieldData.GetCurrentValue();
            int shieldVal = (spikedShieldCount == 3) ? (baseShield * spikedShieldData.match3Multiplier) : (spikedShieldCount * baseShield);
            PlayerStats.Instance.currentShield += shieldVal;
            reflectPercent = 30 + 5 * spikedShieldData.countUpgrade;
            Debug.Log($"Spiked Shield rolled! Added {shieldVal} shield. Reflecting {reflectPercent}% damage.");
            yield return new WaitForSeconds(0.2f);
        }

        if (strPotionCount > 0 && strPotionData != null)
        {
            strBuffTurns = 3;
            float singleBuff = 0.2f + 0.1f * strPotionData.countUpgrade;
            float totalBuff = (strPotionCount == 3) ? (singleBuff * strPotionData.match3Multiplier) : (strPotionCount * singleBuff);
            strBuffMultiplier += totalBuff;
            Debug.Log($"Str Potion rolled! Str Buff added: {totalBuff * 100}%. Total: {strBuffMultiplier * 100}%. Turns remaining: {strBuffTurns}");
            UpdateBuffUI();
            yield return new WaitForSeconds(0.2f);
        }

        if (greatSwordCount > 0 && greatSwordData != null)
        {
            int baseGreatSwordDmg = (greatSwordCount == 3) ? (greatSwordData.GetCurrentValue() * greatSwordData.match3Multiplier) : (greatSwordCount * greatSwordData.GetCurrentValue());
            float totalMultiplier = 1f + strBuffMultiplier;
            if (GameDataManager.Instance != null)
            {
                totalMultiplier += GameDataManager.Instance.GetDamageBonus();
            }
            baseGreatSwordDmg = Mathf.RoundToInt(baseGreatSwordDmg * totalMultiplier);

            int finalGreatSwordDmg = baseGreatSwordDmg;

            // Trigger Mini-Game if assigned
            if (greatSwordMiniGame != null)
            {
                yield return StartCoroutine(greatSwordMiniGame.StartMiniGame(baseGreatSwordDmg, (resultDmg) => 
                {
                    finalGreatSwordDmg = resultDmg;
                }));
            }

            Debug.Log($"GreatSword rolled! Dealing {finalGreatSwordDmg} AoE damage to all enemies.");

            if (ShogunEffectManager.Instance != null)
            {
                Transform spawnPos = greatSwordSpawnLocation != null ? greatSwordSpawnLocation : this.transform;
                ShogunEffectManager.Instance.SpawnGreatSwordEffect(spawnPos);
            }

            // Play animations on all enemies first
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                if (spawnedDisplays.Count > i && spawnedDisplays[i] != null)
                {
                    spawnedDisplays[i].PlayGreatSwordHitAnimation();
                    spawnedDisplays[i].PlayImpactAnimation();
                }
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGreatSwordSound();
            }

            // Wait for GreatSword animation to land before reducing HP on UI
            yield return new WaitForSeconds(0.5f);

            // Apply damage to all enemies
            bool anyDied = false;
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                EnemyInstance enemy = activeEnemies[i];
                enemy.currentHP = Mathf.Max(0, enemy.currentHP - finalGreatSwordDmg);
                if (enemy.currentHP <= 0)
                {
                    anyDied = true;
                    if (spawnedDisplays.Count > i && spawnedDisplays[i] != null)
                    {
                        spawnedDisplays[i].PlayDieAnimation();
                    }
                    if (PlayerStats.Instance != null)
                    {
                        PlayerStats.Instance.coins += enemy.data.coin;
                        PlayerStats.Instance.AddChance067(enemy.data.chance067);
                    }
                }
            }

            UpdateAllDisplayVisuals();

            if (anyDied)
            {
                yield return new WaitForSeconds(1.0f);
                for (int i = activeEnemies.Count - 1; i >= 0; i--)
                {
                    if (activeEnemies[i].currentHP <= 0)
                    {
                        activeEnemies.RemoveAt(i);
                    }
                }
                RefreshEnemyDisplays();
            }
        }

        if (CheckVictoryCondition()) yield break;

        int totalSingleDmg = 0;
        int hammerChance = 0;

        float singleTargetMultiplier = 1f + strBuffMultiplier;
        if (GameDataManager.Instance != null)
        {
            singleTargetMultiplier += GameDataManager.Instance.GetDamageBonus();
        }

        if (swordCount > 0 && swordData != null)
        {
            int baseSword = (swordCount == 3) ? (swordData.GetCurrentValue() * swordData.match3Multiplier) : (swordCount * swordData.GetCurrentValue());
            totalSingleDmg += Mathf.RoundToInt(baseSword * singleTargetMultiplier);
        }

        if (axeCount > 0 && axeData != null)
        {
            int baseAxe = 0;
            if (axeCount == 3)
            {
                baseAxe = UnityEngine.Random.Range(10, 101) * axeData.match3Multiplier;
            }
            else
            {
                for (int i = 0; i < axeCount; i++)
                {
                    baseAxe += UnityEngine.Random.Range(10, 101);
                }
            }
            totalSingleDmg += Mathf.RoundToInt(baseAxe * singleTargetMultiplier);
        }

        if (hammerCount > 0 && hammerData != null)
        {
            int baseHammer = (hammerCount == 3) ? (hammerData.GetCurrentValue() * hammerData.match3Multiplier) : (hammerCount * hammerData.GetCurrentValue());
            totalSingleDmg += Mathf.RoundToInt(baseHammer * singleTargetMultiplier);
            hammerChance = (hammerData.countUpgrade > 0) ? 60 : 50;
        }

        if (totalSingleDmg > 0)
        {
            pendingSwordDamage = totalSingleDmg;
            pendingStunChance = hammerChance;

            if (activeEnemies.Count == 1)
            {
                yield return StartCoroutine(ExecuteSwordAttackRoutine(0));
            }
            else
            {
                currentState = CombatState.TargetSelection;
                AnimateTurnText("Player Turn", "Choose Target!");
                UpdateAllDisplayVisuals();
                Debug.Log($"Single target attack rolled! Pending {pendingSwordDamage} damage (stun chance: {pendingStunChance}%). Please select target.");
            }
        }
        else
        {
            StartCoroutine(EnemyTurnRoutine());
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
        StartCoroutine(ExecuteSwordAttackRoutine(index));
    }

    private IEnumerator ExecuteSwordAttackRoutine(int index)
    {
        if (index < 0 || index >= activeEnemies.Count) yield break;

        Debug.Log($"Attacking enemy {activeEnemies[index].data.enemyName} for {pendingSwordDamage} Sword damage.");

        if (ShogunEffectManager.Instance != null)
        {
            Transform spawnPos = swordSpawnLocation;
            if (spawnPos == null && spawnedDisplays.Count > index)
            {
                EnemyDisplay targetDisplay = spawnedDisplays[index];
                if (targetDisplay != null)
                {
                    spawnPos = targetDisplay.transform;
                }
            }

            if (spawnPos != null)
            {
                ShogunEffectManager.Instance.SpawnSwordEffect(spawnPos);
            }
            else
            {
                ShogunEffectManager.Instance.SpawnSwordEffect();
            }
        }

        // Play animations on the targeted enemy first
        if (spawnedDisplays.Count > index && spawnedDisplays[index] != null)
        {
            spawnedDisplays[index].PlaySwordHitAnimation();
            spawnedDisplays[index].PlayImpactAnimation();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySwordSound();
        }

        // Wait for Sword animation to land before reducing HP on UI
        yield return new WaitForSeconds(0.4f);

        yield return StartCoroutine(DamageEnemy(index, pendingSwordDamage));
        
        if (pendingStunChance > 0 && index < activeEnemies.Count && activeEnemies[index].currentHP > 0)
        {
            if (UnityEngine.Random.Range(0, 100) < pendingStunChance)
            {
                activeEnemies[index].stunTurns = 1;
                Debug.Log($"{activeEnemies[index].data.enemyName} is STUNNED!");
                AnimateTurnText("Player Turn", $"{activeEnemies[index].data.enemyName} STUNNED!");
                yield return new WaitForSeconds(0.6f);
            }
        }

        pendingSwordDamage = 0;
        pendingStunChance = 0;

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

    private IEnumerator DamageEnemy(int index, int damage)
    {
        if (index < 0 || index >= activeEnemies.Count) yield break;

        EnemyInstance enemy = activeEnemies[index];
        enemy.currentHP = Mathf.Max(0, enemy.currentHP - damage);
        Debug.Log($"Enemy {enemy.data.enemyName} HP: {enemy.currentHP}/{enemy.maxHP}");

        if (enemy.currentHP <= 0)
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.coins += enemy.data.coin;
                PlayerStats.Instance.AddChance067(enemy.data.chance067);
                Debug.Log($"Enemy {enemy.data.enemyName} defeated! Reward: +{enemy.data.coin} coins, +{enemy.data.chance067}% chance067");
            }
            if (spawnedDisplays.Count > index && spawnedDisplays[index] != null)
            {
                spawnedDisplays[index].PlayDieAnimation();
            }
            UpdateAllDisplayVisuals();
            yield return new WaitForSeconds(1.0f);

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
            AnimateTurnText("Victory!", "");
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
        AnimateTurnText("Enemy Turn", "Preparing to attack...");

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.isplayerturn = false;
        }
        UpdateAllDisplayVisuals();

        yield return new WaitForSeconds(delayBeforeEnemyAttack);

        for (int i = 0; i < activeEnemies.Count; i++)
        {
            EnemyInstance enemy = activeEnemies[i];

            if (enemy.currentHP <= 0) continue;

            if (enemy.stunTurns > 0)
            {
                enemy.stunTurns--;
                AnimateTurnText("Enemy Turn", $"{enemy.data.enemyName} is STUNNED!");
                yield return new WaitForSeconds(delayBetweenEnemies);
                continue;
            }

            if (PlayerStats.Instance != null && PlayerStats.Instance.currentHP <= 0)
            {
                break;
            }

            int dmgPerHit = Mathf.RoundToInt(enemy.data.GetBaseDMG(currentLevel) * enemy.damageMultiplier);
            int totalHits = enemy.data.countHit;

            Debug.Log($"Enemy {enemy.data.enemyName} turn! Attacks {totalHits} times for {dmgPerHit} dmg each.");

            for (int hit = 0; hit < totalHits; hit++)
            {
                if (spawnedDisplays.Count > i && spawnedDisplays[i] != null)
                {
                    spawnedDisplays[i].PlayAttackAnimation();
                }

                // สั่งพิมพ์ข้อความด้วยความเร็วล็อกวินาทีคงที่ (เช่น 0.2 วินาที)
                AnimateTurnText("Enemy Turn", $"{enemy.data.enemyName} Attacks ({hit + 1}/{totalHits})");

                // หน่วงเวลาระหว่างการโจมตีแต่ละ hit
                yield return new WaitForSeconds(delayBetweenEnemyHits);
            }

            enemy.damageMultiplier *= 1.2f;

            // หน่วงเวลาระหว่างสลับตัวศัตรูในการโจมตี (ถ้ายังเหลือศัตรูตัวถัดไป)
            if (i < activeEnemies.Count - 1)
            {
                yield return new WaitForSeconds(delayBetweenEnemies);
            }
        }

        // Clean up dead enemies (e.g. killed by reflection)
        bool anyReflectDied = false;
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i].currentHP <= 0)
            {
                anyReflectDied = true;
                activeEnemies.RemoveAt(i);
            }
        }
        if (anyReflectDied)
        {
            RefreshEnemyDisplays();
        }

        if (CheckVictoryCondition()) yield break;

        // Decrement buffs
        if (strBuffTurns > 0)
        {
            strBuffTurns--;
            if (strBuffTurns == 0)
            {
                strBuffMultiplier = 0f;
                Debug.Log("Strength Buff expired.");
            }
        }
        reflectPercent = 0;
        UpdateBuffUI();

        if (PlayerStats.Instance != null && PlayerStats.Instance.currentHP <= 0)
        {
            currentState = CombatState.Defeat;
            AnimateTurnText("Defeat!", "");
            upstat = true;
            UpdateAllDisplayVisuals();
            Debug.Log("Player defeated! upstat set to true.");
        }
        else
        {
            currentState = CombatState.PlayerTurn;
            AnimateTurnText("Player Turn", "");

            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.isplayerturn = true;
            }
            UpdateAllDisplayVisuals();

            // Reactivate spin button for the next player turn
            if (MapManager.Instance != null && MapManager.Instance.slotMachine != null)
            {
                GameObject spinBtn = MapManager.Instance.slotMachine.spinButton;
                if (spinBtn != null)
                {
                    spinBtn.SetActive(true);
                    var btn = spinBtn.GetComponent<UnityEngine.UI.Button>();
                    if (btn != null) btn.interactable = true;
                }
            }

            Debug.Log("Player turn starts! Roll slot 1 time.");
        }
    }
}