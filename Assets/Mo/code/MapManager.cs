using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("Levels (1 to 20)")]
    public ScriptableMap[] levels = new ScriptableMap[20];
    public int currentLevelIndex = -1;

    [Header("UI Canvases")]
    public GameObject starticon;
    public GameObject gameCanvas;
    public GameObject canvasWin;
    public GameObject canvasLose;
    public GameObject mainMenuCanvas;

    [Header("Buttons")]
    public Button playButton;
    public Button goNextLevelButton;

    [Header("Slot Machine Link")]
    public SlotMachine slotMachine;

    [Header("Map Type UI GameObjects")]
    public List<GameObject> monsterMapObjects = new List<GameObject>();
    public List<GameObject> chestMapObjects = new List<GameObject>();
    public List<GameObject> shopMapObjects = new List<GameObject>();
    public List<GameObject> bossMapObjects = new List<GameObject>();

    [Header("State")]
    public bool isGachaRollFreeThisTurn = false;

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
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(OnPlayPressed);
            playButton.onClick.AddListener(OnPlayPressed);
        }

        if (goNextLevelButton != null)
        {
            goNextLevelButton.onClick.RemoveListener(OnGoNextLevelPressed);
            goNextLevelButton.onClick.AddListener(OnGoNextLevelPressed);
        }

        // Hide win/lose screens initially
        if (canvasWin != null) canvasWin.SetActive(false);
        if (canvasLose != null) canvasLose.SetActive(false);
        if (gameCanvas != null) gameCanvas.SetActive(false);
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
    }

    public void OnPlayPressed()
    {
        if (gameCanvas != null) gameCanvas.SetActive(true);
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (canvasWin != null) canvasWin.SetActive(false);
        if (canvasLose != null) canvasLose.SetActive(false);

        // Deactivate all map UIs initially
        DeactivateAllMapObjects();

        // Reset level index
        currentLevelIndex = -1;

        // Reset player stats on play start if required
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.currentHP = PlayerStats.Instance.maxHP;
            PlayerStats.Instance.currentShield = 0;
            PlayerStats.Instance.coins = 100;
            PlayerStats.Instance.ticket67 = 5;
            PlayerStats.Instance.chance067 = 1.0f;
        }

        // Enable next level button to allow starting map1 (Level 1)
        if (goNextLevelButton != null)
        {
            goNextLevelButton.gameObject.SetActive(true);
        }

        Debug.Log("Game started. Press Go Next Level to load Level 1.");
    }

    private void DeactivateAllMapObjects()
    {
        foreach (var obj in monsterMapObjects) { if (obj != null) obj.SetActive(false); }
        foreach (var obj in chestMapObjects) { if (obj != null) obj.SetActive(false); }
        foreach (var obj in shopMapObjects) { if (obj != null) obj.SetActive(false); }
        foreach (var obj in bossMapObjects) { if (obj != null) obj.SetActive(false); }
    }

    private void SetObjectsActiveForMapType(MapType type, bool active)
    {
        List<GameObject> targetList = null;
        switch (type)
        {
            case MapType.MonsterMap: targetList = monsterMapObjects; break;
            case MapType.Chest: targetList = chestMapObjects; break;
            case MapType.Shop: targetList = shopMapObjects; break;
            case MapType.Boss: targetList = bossMapObjects; break;
        }

        if (targetList != null)
        {
            foreach (var obj in targetList)
            {
                if (obj != null) obj.SetActive(active);
            }
        }
    }

    public void OnGoNextLevelPressed()
    {
        // 1. Deactivate old map's objects
        DeactivateAllMapObjects();

        // Unlock all slot reels for the new level
        if (slotMachine != null && slotMachine.isReelLocked != null)
        {
            for (int i = 0; i < slotMachine.isReelLocked.Length; i++)
            {
                slotMachine.isReelLocked[i] = false;
            }
        }

        // 2. Increment level index
        currentLevelIndex++;

        // Deactivate starticon when starting Level 1 (index 0)
        if (currentLevelIndex == 0)
        {
            if (starticon != null) starticon.SetActive(false);
        }

        // 3. Check for game completion loss condition
        if (currentLevelIndex >= 20 || currentLevelIndex >= levels.Length)
        {
            // Played until level 20 finished but did not get 067
            TriggerLose();
            return;
        }

        ScriptableMap currentMap = levels[currentLevelIndex];
        if (currentMap == null)
        {
            Debug.LogError($"Map for level {currentLevelIndex + 1} is not assigned!");
            return;
        }

        Debug.Log($"Loading Level {currentLevelIndex + 1} ({currentMap.mapType})");

        // 4. Activate current map's objects
        SetObjectsActiveForMapType(currentMap.mapType, true);

        // 5. Initialize map types
        switch (currentMap.mapType)
        {
            case MapType.MonsterMap:
                if (goNextLevelButton != null) goNextLevelButton.gameObject.SetActive(false);
                if (slotMachine != null)
                {
                    slotMachine.currentMode = SlotMachine.SlotMachineMode.SlotIconData;
                    slotMachine.PopulateAvailableSymbols();
                    slotMachine.UpdateSpinButtonText();
                }
                if (CombatManager.Instance != null)
                {
                    CombatManager.Instance.StartCombat(currentMap.enemies, currentLevelIndex + 1);
                }
                break;

            case MapType.Chest:
                if (goNextLevelButton != null) goNextLevelButton.gameObject.SetActive(false);
                isGachaRollFreeThisTurn = true;
                if (slotMachine != null)
                {
                    slotMachine.currentMode = SlotMachine.SlotMachineMode.GachaReward;
                    slotMachine.PopulateAvailableSymbols();
                    slotMachine.UpdateSpinButtonText();
                }
                Debug.Log("Chest Map: Spin Gacha once for free to proceed.");
                break;

            case MapType.Shop:
                if (goNextLevelButton != null) goNextLevelButton.gameObject.SetActive(true);
                if (slotMachine != null)
                {
                    slotMachine.currentMode = SlotMachine.SlotMachineMode.GachaReward;
                    slotMachine.PopulateAvailableSymbols();
                    slotMachine.UpdateSpinButtonText();
                }
                break;

            case MapType.Boss:
                if (goNextLevelButton != null) goNextLevelButton.gameObject.SetActive(false);
                if (slotMachine != null)
                {
                    slotMachine.currentMode = SlotMachine.SlotMachineMode.SlotIconData;
                    slotMachine.PopulateAvailableSymbols();
                    slotMachine.UpdateSpinButtonText();
                }
                if (CombatManager.Instance != null)
                {
                    CombatManager.Instance.StartCombat(currentMap.enemies, currentLevelIndex + 1);
                }
                break;
        }
    }

    public void OnAllEnemiesDefeated()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.currentHP = PlayerStats.Instance.maxHP;
            PlayerStats.Instance.currentShield = 0;
            Debug.Log("All enemies defeated. HP restored to max, Shield reset to 0.");
        }

        if (currentLevelIndex >= 0 && currentLevelIndex < levels.Length && levels[currentLevelIndex] != null)
        {
            ScriptableMap currentMap = levels[currentLevelIndex];
            if (currentMap.mapType == MapType.Boss)
            {
                // Boss defeated -> Switch mode to SlotSymbolData to allow Jackpot spin
                if (slotMachine != null)
                {
                    slotMachine.currentMode = SlotMachine.SlotMachineMode.SlotSymbolData;
                    slotMachine.PopulateAvailableSymbols();
                    slotMachine.UpdateSpinButtonText();
                    Debug.Log("Boss defeated! Slot machine switched to Jackpot Mode (SlotSymbolData).");
                }
            }
        }

        if (goNextLevelButton != null)
        {
            goNextLevelButton.gameObject.SetActive(true);
        }
    }

    public void OnSlotMachineSpinCompleted(SlotSymbolData[] results)
    {
        // Check for jackpot win condition: SlotSymbolData mode and got 0-6-7
        if (slotMachine != null && slotMachine.currentMode == SlotMachine.SlotMachineMode.SlotSymbolData)
        {
            if (results != null && results.Length == 3 &&
                results[0] != null && results[0].SymbolName == "0" &&
                results[1] != null && results[1].SymbolName == "6" &&
                results[2] != null && results[2].SymbolName == "7")
            {
                TriggerWin();
                return;
            }
        }

        // Check if Chest map free spin condition is met
        if (currentLevelIndex >= 0 && currentLevelIndex < levels.Length && levels[currentLevelIndex] != null)
        {
            ScriptableMap currentMap = levels[currentLevelIndex];
            if (currentMap.mapType == MapType.Chest && isGachaRollFreeThisTurn)
            {
                isGachaRollFreeThisTurn = false;
                if (slotMachine != null)
                {
                    slotMachine.UpdateSpinButtonText();
                }
                if (goNextLevelButton != null)
                {
                    goNextLevelButton.gameObject.SetActive(true);
                }
                Debug.Log("Chest Map: Free spin completed! Go Next Level button active.");
            }
        }
    }

    public void TriggerWin()
    {
        if (canvasWin != null) canvasWin.SetActive(true);
        if (goNextLevelButton != null) goNextLevelButton.gameObject.SetActive(false);
        Debug.Log("WIN! 067 Jackpot reached!");
    }

    public void TriggerLose()
    {
        if (canvasLose != null) canvasLose.SetActive(true);
        if (goNextLevelButton != null) goNextLevelButton.gameObject.SetActive(false);
        Debug.Log("LOSE! Game Over.");
    }
}
