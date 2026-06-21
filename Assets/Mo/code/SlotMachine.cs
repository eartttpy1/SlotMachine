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

    private void OnValidate()
    {
        PopulateAvailableSymbols();
    }

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

    [Header("Reel Status")]
    public bool[] isReelLocked = new bool[3]; // เก็บสถานะปุ่มกดล็อกรีล [รีล1, รีล2, รีล3]
    private SlotSymbolData[] finalResult = new SlotSymbolData[3]; // ผลลัพธ์สุดท้ายหลังหมุนเสร็จ

    // ฟังก์ชัน Start สำหรับตั้งค่าเริ่มต้นและเชื่อมต่อข้อมูลระหว่าง SlotMachine กับ SlotDisplay
    private void Start()
    {
        PopulateAvailableSymbols();

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

    // ฟังก์ชันหลักที่ปุ่ม SPIN จะวิ่งมาเรียกใช้งาน
    public void SpinSlotMachine()
    {
        if (currentMode == SlotMachineMode.SlotSymbolData)
        {
            if (PlayerStats.Instance != null)
            {
                if (PlayerStats.Instance.ticket67 > 0)
                {
                    PlayerStats.Instance.ticket67--;
                    Debug.Log($"ใช้ตั๋ว 67 ไป 1 ใบ คงเหลือตั๋ว: {PlayerStats.Instance.ticket67} ใบ");
                }
                else
                {
                    Debug.LogWarning("ไม่มีตั๋ว ticket67 เหลืออยู่! ไม่สามารถหมุนสล็อตได้");
                    return;
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

        // 2. สั่งเล่นอนิเมชันหน้าบ้านหลอกตา (ส่งข้อมูลผลลัพธ์ไปให้แอนิเมชันแสดงผล)
        StartCoroutine(AnimateSlotsAndSendResult());
    }

    // 🎯 ระบบโกงดวง (Weighted Random) แยกช่องตามที่คุณต้องการ
    private SlotSymbolData GetWeightedRandomSymbol(int reelIndex)
    {
        float chance067 = PlayerStats.Instance != null ? PlayerStats.Instance.chance067 : 1.0f;
        int totalWeight = 0;
        
        // คำนวณน้ำหนักรวมแบบไดนามิกตามตำแหน่งรีลและค่า % 067
        foreach (var symbol in availableSymbols)
        {
            totalWeight += GetDynamicWeight(symbol, reelIndex, chance067);
        }

        int randomValue = Random.Range(0, totalWeight);

        foreach (var symbol in availableSymbols)
        {
            int currentWeight = GetDynamicWeight(symbol, reelIndex, chance067);
            if (randomValue < currentWeight)
            {
                return symbol;
            }
            randomValue -= currentWeight;
        }
        return availableSymbols.Count > 0 ? availableSymbols[0] : null;
    }

    // คำนวณน้ำหนักของสัญลักษณ์ตามค่า % 067 ของรีลนั้นๆ
    private int GetDynamicWeight(SlotSymbolData symbol, int reelIndex, float chance067)
    {
        int weight = symbol.BaseWeight;

        // ตรวจสอบความถูกต้องของสัญลักษณ์เป้าหมายในแต่ละรีล (Reel 1 -> 0 / Reel 2 -> 6 / Reel 3 -> 7)
        bool isTarget = false;
        if (reelIndex == 0 && symbol.SymbolName == "0") isTarget = true;
        else if (reelIndex == 1 && symbol.SymbolName == "6") isTarget = true;
        else if (reelIndex == 2 && symbol.SymbolName == "7") isTarget = true;

        if (isTarget)
        {
            // เพิ่มน้ำหนักขึ้นตามค่าสะสม % 067 (ตัวอย่าง: เพิ่มขึ้น 10 เท่าของเปอร์เซ็นต์สะสม)
            weight += Mathf.RoundToInt(chance067 * 10f);
        }

        return weight;
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
            for (int i = 0; i < 3; i++)
            {
                if (finalResult[i] is SlotIconData iconData && iconData.symbolType == SlotSymbol.HealingPotion)
                {
                    potionsRolled++;
                }
            }
            if (potionsRolled > 0)
            {
                PlayerStats.Instance.AddPotion(potionsRolled);
                Debug.Log($"สุ่มได้ Potion {potionsRolled} ขวด! จำนวนโพชั่นสะสม: {PlayerStats.Instance.countpotion} ขวด");
            }
        }

        // 3. ปลดล็อกรีลที่ไม่ได้ตั้งใจล็อกทิ้งไว้เพื่อเริ่มเทิร์นถัดไป
        // (สามารถรีเซ็ตค่า Lock ตรงนี้ได้เลย)

        // ➔ ส่งต่อข้อมูลรางวัล (Array Size 3) ไปให้ Combat Manager ประมวลผลทำดาเมจทันที!
        // if (CombatManager.Instance != null) {
        //     CombatManager.Instance.ProcessSlotResult(finalResult);
        // }
    }
}