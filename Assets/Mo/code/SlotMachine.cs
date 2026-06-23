using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            foreach (var item in slotIconList)
            {
                if (item != null) availableSymbols.Add(item);
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

        // ป้องกันการหมุนซ้ำในด่าน Chest
        if (MapManager.Instance != null && MapManager.Instance.currentLevelIndex >= 0 && MapManager.Instance.levels != null)
        {
            if (MapManager.Instance.currentLevelIndex < MapManager.Instance.levels.Length)
            {
                ScriptableMap currentMap = MapManager.Instance.levels[MapManager.Instance.currentLevelIndex];
                if (currentMap != null && currentMap.mapType == MapType.Chest && !MapManager.Instance.isGachaRollFreeThisTurn)
                {
                    Debug.LogWarning("คุณได้ใช้สิทธิ์สุ่มฟรีในด่าน Chest ไปแล้ว! ไม่สามารถสุ่มเพิ่มได้");
                    return;
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

        // เพิ่ม % 067 ของ player เมื่อหมุนสล็อต (ค่าเริ่มต้นเพิ่มครั้งละ 1% หรือสามารถปรับเปลี่ยนได้ตามสะดวก) เก็บไว้ก่อนไม่พอค่อยใช้ เพราะได้ประมาณ 50 up ก้โผล่ 067
        // if (PlayerStats.Instance != null)
        // {
        //     PlayerStats.Instance.AddChance067(1.0f);
        // }

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

    // 🎯 ระบบโกงดวง (Weighted Random) แยกช่องตามที่คุณต้องการ
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
            // จากสูตร: targetWeight / (targetWeight + sumOthersWeight) = targetProbability
            // จะได้: targetWeight = (targetProbability * sumOthersWeight) / (1 - targetProbability)
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
        
        // ในช่วง 1-2 วินาทิตรงนี้ สั่งให้ Artist ทำภาพหมุนวนหลอกตาไปก่อน
        yield return new WaitForSeconds(1.5f); 
        for (int i = 0; i < 3; i++)
        {
            if (reelDisplays[i] != null && finalResult[i] != null)
            {
                // สั่งให้จอ UI ช่องนั้นดึงรูปใน ScriptableObject ที่สุ่มได้ไปโชว์หน้าบ้านทันที!
                reelDisplays[i].SetupSlotDisplay(finalResult[i]); 
            }
        }

        string res0 = finalResult[0] != null ? finalResult[0].SymbolName : "None";
        string res1 = finalResult[1] != null ? finalResult[1].SymbolName : "None";
        string res2 = finalResult[2] != null ? finalResult[2].SymbolName : "None";
        Debug.Log($"สล็อตหยุดหมุน! ผลลัพธ์คือ: [{res0}] [{res1}] [{res2}]");

        // เพิ่มจำนวน Potion ไปยัง PlayerStats หากได้สัญลักษณ์ Potion ในโหมด SlotIconData
        if (currentMode == SlotMachineMode.SlotIconData && PlayerStats.Instance != null)
        {
            int potionsRolled = 0;
            SlotIconData potionIconData = null;
            for (int i = 0; i < 3; i++)
            {
                if (finalResult[i] is SlotIconData iconData && iconData.symbolType == SlotSymbol.HealingPotion)
                {
                    potionsRolled++;
                    potionIconData = iconData;
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
                // ตรวจสอบความเหมือนกันของโหมด Jackpot (ทั้ง 3 สัญลักษณ์เป็นประเภทเดียวกัน)
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
                    // คิดแยกทีละช่องตามปกติ
                    for (int i = 0; i < 3; i++)
                    {
                        ApplyGachaReward(gachaResults[i].rewardType, gachaResults[i].baseValue);
                    }
                }
            }
        }

        // 3. ปลดล็อกรีลที่ไม่ได้ตั้งใจล็อกทิ้งไว้เพื่อเริ่มเทิร์นถัดไป
        // (สามารถรีเซ็ตค่า Lock ตรงนี้ได้เลย)

        // ➔ ส่งต่อข้อมูลรางวัล (Array Size 3) ไปให้ Combat Manager ประมวลผลทำดาเมจทันที!
        if (CombatManager.Instance != null && CombatManager.Instance.gameObject.activeInHierarchy) {
            CombatManager.Instance.ProcessSlotResult(finalResult);
        }

        if (MapManager.Instance != null) {
            MapManager.Instance.OnSlotMachineSpinCompleted(finalResult);
        }

        isSpinning = false;
        if (spinButton != null)
        {
            var btn = spinButton.GetComponent<UnityEngine.UI.Button>();
            if (btn != null) btn.interactable = true;
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