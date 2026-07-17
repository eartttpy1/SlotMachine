using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SlotMachine : MonoBehaviour
{
    public enum SlotMachineMode { SlotIconData, GachaReward, SlotSymbolData }

    [Header("Reel Data Layout")]
    public SlotMachineMode currentMode; // เลือกโหมดใช้งานของตู้สล็อตนี้

    // ลิสต์ที่จะสุ่มจริงในตู้ (ระบบจะดึงจากตัวแปรด้านล่างตามโหมดที่เลือก)
    public List<SlotSymbolData> availableSymbols = new List<SlotSymbolData>();

    [Header("Data Lists")]
    public List<SlotIconData> slotIconList = new List<SlotIconData>();
    public List<GachaRewardData> gachaRewardList = new List<GachaRewardData>();
    public List<SlotSymbolData> slotSymbolList = new List<SlotSymbolData>();



    public void PopulateAvailableSymbols()
    {
        availableSymbols.Clear();
        if (currentMode == SlotMachineMode.SlotIconData)
        {
            if (PlayerStats.Instance != null && PlayerStats.Instance.selectedSlotIcons != null && PlayerStats.Instance.selectedSlotIcons.Count >= 3)
            {
                foreach (var item in PlayerStats.Instance.selectedSlotIcons)
                {
                    if (item != null) availableSymbols.Add(item);
                }
            }
            else
            {
                foreach (var item in slotIconList)
                {
                    if (item != null) availableSymbols.Add(item);
                }
            }
        }
        else if (currentMode == SlotMachineMode.GachaReward)
        {
            foreach (var item in gachaRewardList)
            {
                if (item != null) availableSymbols.Add(item);
            }
        }
        else if (currentMode == SlotMachineMode.SlotSymbolData)
        {
            foreach (var item in slotSymbolList)
            {
                if (item != null) availableSymbols.Add(item);
            }
        }
    }

    [Header("UI Visual Link Connection")]
    public SlotDisplay[] reelDisplays = new SlotDisplay[3];
    public Sprite defaultSprite;
    public TMPro.TextMeshProUGUI spinButtonText;
    public GameObject spinButton;
    [Header("gacha")]
    int useGachaCoin = 20;

    [Header("Reel Status")]
    public bool[] isReelLocked = new bool[3]; // เก็บสถานะปุ่มกดล็อกรีล [รีล1, รีล2, รีล3]
    private SlotSymbolData[] finalResult = new SlotSymbolData[3]; // ผลลัพธ์สุดท้ายหลังหมุนเสร็จ
    private bool isSpinning = false;

    [Header("DOTween Settings")]
    public float shakeDuration = 0.4f;
    public float shakeStrength = 0.5f;

    private void OnValidate()
    {
        PopulateAvailableSymbols();
        UpdateSpinButtonText();
    }

    // ฟังก์ชัน Start สำหรับตั้งค่าเริ่มต้นและเชื่อมต่อข้อมูลระหว่าง SlotMachine กับ SlotDisplay
    private void Start()
    {
        PopulateAvailableSymbols();
        UpdateSpinButtonText();

        if (spinButton != null)
        {
            spinButton.SetActive(true);
        }

        for (int i = 0; i < 3; i++)
        {
            // กำหนดค่าสัญลักษณ์เริ่มต้น หากมีสัญลักษณ์ให้เลือกใน availableSymbols
            if (availableSymbols.Count > 0 && finalResult[i] == null)
            {
                finalResult[i] = availableSymbols[0];
            }

            // แสดงผลสัญลักษณ์เริ่มต้นบนจอ UI
            if (reelDisplays[i] != null && finalResult[i] != null)
            {
                reelDisplays[i].SetupSlotDisplay(finalResult[i]);
            }
        }
    }

    public void ClearAllReels()
    {
        for (int i = 0; i < reelDisplays.Length; i++)
        {
            if (reelDisplays[i] != null)
            {
                reelDisplays[i].ClearDisplay(defaultSprite);
            }
        }
        for (int i = 0; i < finalResult.Length; i++)
        {
            finalResult[i] = null;
        }
    }

    public void UpdateSpinButtonText()
    {
        if (spinButtonText != null)
        {
            bool isChestFree = false;
            bool isShop = false;

            if (MapManager.Instance != null && MapManager.Instance.currentLevelIndex >= 0 && MapManager.Instance.levels != null)
            {
                if (MapManager.Instance.currentLevelIndex < MapManager.Instance.levels.Length)
                {
                    ScriptableMap currentMap = MapManager.Instance.levels[MapManager.Instance.currentLevelIndex];
                    if (currentMap != null)
                    {
                        if (currentMap.mapType == MapType.Chest && MapManager.Instance.isGachaRollFreeThisTurn)
                        {
                            isChestFree = true;
                        }
                        else if (currentMap.mapType == MapType.Shop)
                        {
                            isShop = true;
                        }
                    }
                }
            }

            if (currentMode == SlotMachineMode.SlotSymbolData)
            {
                spinButtonText.text = "1 ticket67";
            }
            else if (isChestFree)
            {
                spinButtonText.text = "Free Spin";
            }
            else if (isShop || currentMode == SlotMachineMode.GachaReward)
            {
                spinButtonText.text = "20 $";
            }
            else
            {
                spinButtonText.text = "Roll";
            }
        }
    }

    [Header("Debug Controls")]
    public bool debugForceThreePotions = false; // เปิดเพื่อให้หมุนได้ Potion 3 ช่องเสมอเมื่อเทสต์

    // ฟังก์ชันหลักที่ปุ่ม SPIN จะวิ่งมาเรียกใช้งาน
    public void SpinSlotMachine()
    {
        if (isSpinning) return;

        if (CombatManager.Instance != null && CombatManager.Instance.currentState == CombatManager.CombatState.TargetSelection)
        {
            Debug.LogWarning("กรุณาเลือกเป้าหมายการโจมตีก่อนทำการหมุนสล็อตครั้งถัดไป!");
            return;
        }

        bool isChestMap = false;

        // ป้องกันการหมุนซ้ำในด่าน Chest
        if (MapManager.Instance != null && MapManager.Instance.currentLevelIndex >= 0 && MapManager.Instance.levels != null)
        {
            if (MapManager.Instance.currentLevelIndex < MapManager.Instance.levels.Length)
            {
                ScriptableMap currentMap = MapManager.Instance.levels[MapManager.Instance.currentLevelIndex];
                if (currentMap != null && currentMap.mapType == MapType.Chest)
                {
                    isChestMap = true;
                    if (!MapManager.Instance.isGachaRollFreeThisTurn)
                    {
                        Debug.LogWarning("คุณได้ใช้สิทธิ์สุ่มฟรีในด่าน Chest ไปแล้ว! ไม่สามารถสุ่มเพิ่มได้");
                        return;
                    }
                }
            }
        }

        // Validate resources first
        if (currentMode == SlotMachineMode.SlotSymbolData)
        {
            if (PlayerStats.Instance != null)
            {
                if (PlayerStats.Instance.ticket67 <= 0)
                {
                    Debug.LogWarning("ไม่มีตั๋ว ticket67 เหลืออยู่! ไม่สามารถหมุนสล็อตได้");
                    return;
                }
            }
        }
        else if (currentMode == SlotMachineMode.GachaReward)
        {
            if (PlayerStats.Instance != null)
            {
                bool isFree = MapManager.Instance != null && MapManager.Instance.isGachaRollFreeThisTurn;
                if (!isFree && PlayerStats.Instance.coins < useGachaCoin)
                {
                    Debug.LogWarning($"จำนวน Coin ไม่เพียงพอสำหรับการสุ่มกาชา (ต้องใช้ {useGachaCoin} Coin)");
                    return;
                }
            }
        }

        // Validation passed: Lock the spin button and start spinning
        isSpinning = true;
        if (spinButton != null)
        {
            var btn = spinButton.GetComponent<UnityEngine.UI.Button>();
            if (btn != null) btn.interactable = false;
        }

        if (MapManager.Instance != null && MapManager.Instance.goNextLevelButton != null)
        {
            MapManager.Instance.goNextLevelButton.interactable = false;
        }

        // Hide spinButton during combat on MonsterMap and BossMap
        bool isCombatMap = false;
        if (MapManager.Instance != null && MapManager.Instance.currentLevelIndex >= 0 && MapManager.Instance.levels != null)
        {
            if (MapManager.Instance.currentLevelIndex < MapManager.Instance.levels.Length)
            {
                ScriptableMap currentMap = MapManager.Instance.levels[MapManager.Instance.currentLevelIndex];
                if (currentMap != null && (currentMap.mapType == MapType.MonsterMap || currentMap.mapType == MapType.Boss))
                {
                    isCombatMap = true;
                }
            }
        }
        if (isCombatMap && currentMode == SlotMachineMode.SlotIconData)
        {
            if (spinButton != null)
            {
                spinButton.SetActive(false);
            }
        }

        // เล่นเสียงสปินสล็อต
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySlotSpinSound();
        }

        // เล่นเสียงเปิดกล่องสมบัติเมื่อกดสปินในด่าน Chest
        if (isChestMap && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOpenChestSound();
        }

        // Consume resources
        if (currentMode == SlotMachineMode.SlotSymbolData)
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.ticket67--;
                Debug.Log($"ใช้ตั๋ว 67 ไป 1 ใบ คงเหลือตั๋ว: {PlayerStats.Instance.ticket67} ใบ");
            }
            if (MapManager.Instance != null)
            {
                MapManager.Instance.jackpotSpinCountThisRun++;
            }
        }
        else if (currentMode == SlotMachineMode.GachaReward)
        {
            if (PlayerStats.Instance != null)
            {
                bool isFree = MapManager.Instance != null && MapManager.Instance.isGachaRollFreeThisTurn;
                if (isFree)
                {
                    Debug.Log("สุ่มกาชาฟรีสำหรับด่าน Chest!");
                }
                else
                {
                    PlayerStats.Instance.coins -= useGachaCoin;
                    Debug.Log($"ใช้ Coin ไป {useGachaCoin} เหรียญในการสุ่มกาชา คงเหลือ Coin: {PlayerStats.Instance.coins} เหรียญ");
                }
            }
        }

        // 1. วนลูปสุ่มหลังบ้านให้เสร็จก่อนแบบถ่วงน้ำหนักแยกอิสระทีละรีล
        for (int i = 0; i < 3; i++)
        {
            if (!isReelLocked[i]) // ถ้ารีลนั้นไม่ได้โดนล็อกไว้ ให้สุ่มใหม่
            {
                finalResult[i] = GetWeightedRandomSymbol(i);
            }
        }

        // ถ้าเปิด debugForceThreePotions ไว้ จะทำการแทนค่าผลลัพธ์ทั้งหมดด้วย Potion
        if (debugForceThreePotions)
        {
            SlotSymbolData potionSymbol = availableSymbols.Find(s => s is SlotIconData iconData && iconData.symbolType == SlotSymbol.HealingPotion);
            if (potionSymbol != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    finalResult[i] = potionSymbol;
                }
            }
        }

        // 2. สั่งเล่นอนิเมชันหน้าบ้านหลอกตา (ส่งข้อมูลผลลัพธ์ไปให้แอนิเมชันแสดงผล)
        StartCoroutine(AnimateSlotsAndSendResult());
    }

    // ระบบโกงดวง (Weighted Random) แยกช่องตามที่คุณต้องการ
    private SlotSymbolData GetWeightedRandomSymbol(int reelIndex)
    {
        float chance067 = PlayerStats.Instance != null ? PlayerStats.Instance.chance067 : 1.0f;
        // ปรับโอกาสให้อยู่ในช่วง 0-100% (จำกัดที่ 99.9% เพื่อป้องกันการหารด้วยศูนย์)
        chance067 = Mathf.Clamp(chance067, 0f, 99.9f);
        float targetProbability = chance067 / 100f;

        SlotSymbolData targetSymbol = null;
        int sumOthersWeight = 0;

        // แยกน้ำหนักของเป้าหมายกับน้ำหนักของสัญลักษณ์อื่น ๆ
        foreach (var symbol in availableSymbols)
        {
            bool isTarget = false;
            if (reelIndex == 0 && symbol.SymbolName == "0") isTarget = true;
            else if (reelIndex == 1 && symbol.SymbolName == "6") isTarget = true;
            else if (reelIndex == 2 && symbol.SymbolName == "7") isTarget = true;

            if (isTarget)
            {
                targetSymbol = symbol;
            }
            else
            {
                sumOthersWeight += symbol.BaseWeight;
            }
        }

        // ถ้ารีลนี้มีสัญลักษณ์เป้าหมาย และมีสัญลักษณ์อื่น ๆ อยู่ด้วย
        if (targetSymbol != null && sumOthersWeight > 0)
        {
            // คำนวณหา targetWeight ที่ทำให้โอกาสสุ่มได้ targetSymbol เท่ากับ targetProbability พอดี
            float calculatedTargetWeight = (targetProbability * sumOthersWeight) / (1f - targetProbability);
            int targetWeight = Mathf.RoundToInt(calculatedTargetWeight);

            int totalWeight = sumOthersWeight + targetWeight;
            int randomValue = Random.Range(0, totalWeight);

            if (randomValue < targetWeight)
            {
                return targetSymbol;
            }
            else
            {
                randomValue -= targetWeight;
                foreach (var symbol in availableSymbols)
                {
                    if (symbol == targetSymbol) continue;
                    if (randomValue < symbol.BaseWeight)
                    {
                        return symbol;
                    }
                    randomValue -= symbol.BaseWeight;
                }
            }
        }

        // หากไม่มีสัญลักษณ์เป้าหมาย หรือไม่มีสัญลักษณ์อื่นเลย ให้ใช้การสุ่มถ่วงน้ำหนักปกติ
        int normalTotalWeight = 0;
        foreach (var symbol in availableSymbols)
        {
            normalTotalWeight += symbol.BaseWeight;
        }

        int normalRandomValue = Random.Range(0, normalTotalWeight);
        foreach (var symbol in availableSymbols)
        {
            if (normalRandomValue < symbol.BaseWeight)
            {
                return symbol;
            }
            normalRandomValue -= symbol.BaseWeight;
        }

        return availableSymbols.Count > 0 ? availableSymbols[0] : null;
    }

    private IEnumerator AnimateSlotsAndSendResult()
    {
        Debug.Log("ตู้สล็อตกำลังหมุนติ้ว ๆ...");

        float duration = 1.5f;
        float elapsed = 0f;
        float cycleInterval = 0.08f;
        float timer = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            timer += Time.deltaTime;

            if (timer >= cycleInterval)
            {
                timer = 0f;
                for (int i = 0; i < 3; i++)
                {
                    if (!isReelLocked[i] && reelDisplays[i] != null && availableSymbols.Count > 0)
                    {
                        int randomIndex = Random.Range(0, availableSymbols.Count);
                        reelDisplays[i].SetupSlotDisplay(availableSymbols[randomIndex]);
                    }
                }
            }
            yield return null;
        }
        for (int i = 0; i < 3; i++)
        {
            if (reelDisplays[i] != null && finalResult[i] != null)
            {
                reelDisplays[i].SetupSlotDisplay(finalResult[i]);
            }
        }

        string res0 = finalResult[0] != null ? finalResult[0].SymbolName : "None";
        string res1 = finalResult[1] != null ? finalResult[1].SymbolName : "None";
        string res2 = finalResult[2] != null ? finalResult[2].SymbolName : "None";
        Debug.Log($"สล็อตหยุดหมุน! ผลลัพธ์คือ: [{res0}] [{res1}] [{res2}]");

        // ตรวจสอบรางวัลและเล่นเสียงเอฟเฟกต์
        if (currentMode == SlotMachineMode.SlotIconData && PlayerStats.Instance != null)
        {
            int potionsRolled = 0;
            SlotIconData potionIconData = null;
            int swordsRolled = 0;
            int greatSwordsRolled = 0;

            for (int i = 0; i < 3; i++)
            {
                if (finalResult[i] is SlotIconData iconData)
                {
                    if (iconData.symbolType == SlotSymbol.HealingPotion)
                    {
                        potionsRolled++;
                        potionIconData = iconData;
                    }
                    else if (iconData.symbolType == SlotSymbol.Sword)
                    {
                        swordsRolled++;
                    }
                    else if (iconData.symbolType == SlotSymbol.GreatSword)
                    {
                        greatSwordsRolled++;
                    }
                }
            }



            if (potionsRolled > 0)
            {
                int totalAdded = potionsRolled;
                if (potionsRolled == 3 && potionIconData != null)
                {
                    totalAdded = 1 * potionIconData.match3Multiplier;
                }
                PlayerStats.Instance.AddPotion(totalAdded);
                Debug.Log($"สุ่มได้ Potion {potionsRolled} ช่อง! ได้รับโพชั่นทั้งหมด: {totalAdded} ขวด (สะสมทั้งหมด: {PlayerStats.Instance.countpotion} ขวด)");
            }

            // เช็คสัญลักษณ์ Coin ในโหมด SlotIconData
            int coinsRolled = 0;
            SlotIconData coinIconData = null;
            for (int i = 0; i < 3; i++)
            {
                if (finalResult[i] is SlotIconData iconData && iconData.symbolType == SlotSymbol.Coin)
                {
                    coinsRolled++;
                    coinIconData = iconData;
                }
            }
            if (coinsRolled > 0 && coinIconData != null)
            {
                int coinBaseValue = coinIconData.GetCurrentValue();
                int totalCoinsAdded = coinsRolled * coinBaseValue;
                if (coinsRolled == 3)
                {
                    totalCoinsAdded = coinBaseValue * coinIconData.match3Multiplier;
                }
                PlayerStats.Instance.coins += totalCoinsAdded;
                Debug.Log($"สุ่มได้ Coin {coinsRolled} ช่อง! ได้รับเหรียญทั้งหมด: {totalCoinsAdded} Coins (สะสมทั้งหมด: {PlayerStats.Instance.coins} Coins)");
            }

            // เช็คสัญลักษณ์ WhiteCoin ในโหมด SlotIconData
            int whiteCoinsRolled = 0;
            SlotIconData whiteCoinIconData = null;
            for (int i = 0; i < 3; i++)
            {
                if (finalResult[i] is SlotIconData iconData && iconData.symbolType == SlotSymbol.WhiteCoin)
                {
                    whiteCoinsRolled++;
                    whiteCoinIconData = iconData;
                }
            }
            if (whiteCoinsRolled > 0 && whiteCoinIconData != null)
            {
                int minCoins, maxCoins;
                whiteCoinIconData.GetWhiteCoinRange(out minCoins, out maxCoins);
                int totalWhiteCoinsAdded = 0;
                for (int i = 0; i < whiteCoinsRolled; i++)
                {
                    totalWhiteCoinsAdded += UnityEngine.Random.Range(minCoins, maxCoins + 1);
                }
                if (whiteCoinsRolled == 3)
                {
                    totalWhiteCoinsAdded *= whiteCoinIconData.match3Multiplier;
                }
                PlayerStats.Instance.coins += totalWhiteCoinsAdded;
                Debug.Log($"สุ่มได้ WhiteCoin {whiteCoinsRolled} ช่อง! ได้รับเหรียญทั้งหมด: {totalWhiteCoinsAdded} Coins (สะสมทั้งหมด: {PlayerStats.Instance.coins} Coins)");
            }
        }

        // ประเมินและแจกรางวัลในโหมด GachaReward
        if (currentMode == SlotMachineMode.GachaReward && PlayerStats.Instance != null)
        {
            bool allGacha = true;
            GachaRewardData[] gachaResults = new GachaRewardData[3];
            for (int i = 0; i < 3; i++)
            {
                if (finalResult[i] is GachaRewardData rData)
                {
                    gachaResults[i] = rData;
                }
                else
                {
                    allGacha = false;
                }
            }

            if (allGacha)
            {
                bool isJackpot = (gachaResults[0].rewardType == gachaResults[1].rewardType) &&
                                 (gachaResults[1].rewardType == gachaResults[2].rewardType);

                if (isJackpot)
                {
                    GachaRewardData jackpotReward = gachaResults[0];
                    int finalValue = jackpotReward.baseValue * jackpotReward.jackpotMultiplier;
                    ApplyGachaReward(jackpotReward.rewardType, finalValue);
                    Debug.Log($"[JACKPOT GACHA] สุ่มได้เหมือนกัน 3 ช่อง! ได้รับรางวัล {jackpotReward.rewardType} คูณเป็น {finalValue}");
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        ApplyGachaReward(gachaResults[i].rewardType, gachaResults[i].baseValue);
                    }
                }
            }
        }

        if (CombatManager.Instance != null && CombatManager.Instance.gameObject.activeInHierarchy)
        {
            CombatManager.Instance.ProcessSlotResult(finalResult);
        }

        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnSlotMachineSpinCompleted(finalResult);
        }

        isSpinning = false;
        
        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(shakeDuration, shakeStrength);

        if (spinButton != null)
        {
            var btn = spinButton.GetComponent<UnityEngine.UI.Button>();
            if (btn != null) btn.interactable = true;
        }

        if (MapManager.Instance != null && MapManager.Instance.goNextLevelButton != null)
        {
            MapManager.Instance.goNextLevelButton.interactable = true;
        }
    }

    private void ApplyGachaReward(GachaRewardType type, int value)
    {
        if (PlayerStats.Instance == null) return;

        switch (type)
        {
            case GachaRewardType.Opportunity067:
                PlayerStats.Instance.AddChance067(value);
                break;
            case GachaRewardType.Coin:
                PlayerStats.Instance.coins += value;
                Debug.Log($"ได้รับ {value} Coins! (เหรียญทั้งหมด: {PlayerStats.Instance.coins})");
                break;
            case GachaRewardType.MaxHP:
                int addedMaxHP = Mathf.RoundToInt(PlayerStats.Instance.maxHP * (value / 100f));
                PlayerStats.Instance.maxHP += addedMaxHP;
                Debug.Log($"เพิ่ม MaxHP ขึ้น {value}% (+{addedMaxHP}) -> MaxHP ใหม่: {PlayerStats.Instance.maxHP}");
                break;
            case GachaRewardType.ReduceHP:
                int reducedHP = Mathf.RoundToInt(PlayerStats.Instance.maxHP * (value / 100f));
                PlayerStats.Instance.currentHP = Mathf.Max(0, PlayerStats.Instance.currentHP - reducedHP);
                Debug.Log($"ลด HP ลง {value}% (-{reducedHP}) -> HP ปัจจุบัน: {PlayerStats.Instance.currentHP}");
                break;
        }
    }
}