using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotMachine : MonoBehaviour
{
    [Header("Reel Data Layout")]
    // ลิสต์รายการสัญลักษณ์ทั้งหมดที่มีสิทธิ์สุ่มได้ในตู้นี้
    public List<SlotIconData> availableSymbols = new List<SlotIconData>(); 

    [Header("UI Visual Link Connection")]
    public SlotDisplay[] reelDisplays = new SlotDisplay[3];

    [Header("Reel Status")]
    public bool[] isReelLocked = new bool[3]; // เก็บสถานะปุ่มกดล็อกรีล [รีล1, รีล2, รีล3]
    private SlotIconData[] finalResult = new SlotIconData[3]; // ผลลัพธ์สุดท้ายหลังหมุนเสร็จ

    // ฟังก์ชัน Start สำหรับตั้งค่าเริ่มต้นและเชื่อมต่อข้อมูลระหว่าง SlotMachine กับ SlotDisplay
    private void Start()
    {
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
    private SlotIconData GetWeightedRandomSymbol(int reelIndex)
    {
        int totalWeight = 0;
        
        // คำนวณน้ำหนักรวม
        foreach (var symbol in availableSymbols)
        {
            totalWeight += symbol.baseWeightRandom;
        }

        int randomValue = Random.Range(0, totalWeight);

        foreach (var symbol in availableSymbols)
        {
            if (randomValue < symbol.baseWeightRandom)
            {
                return symbol;
            }
            randomValue -= symbol.baseWeightRandom;
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

        string res0 = finalResult[0] != null ? finalResult[0].iconName : "None";
        string res1 = finalResult[1] != null ? finalResult[1].iconName : "None";
        string res2 = finalResult[2] != null ? finalResult[2].iconName : "None";
        Debug.Log($"สล็อตหยุดหมุน! ผลลัพธ์คือ: [{res0}] [{res1}] [{res2}]");

        // 3. ปลดล็อกรีลที่ไม่ได้ตั้งใจล็อกทิ้งไว้เพื่อเริ่มเทิร์นถัดไป
        // (สามารถรีเซ็ตค่า Lock ตรงนี้ได้เลย)

        // ➔ ส่งต่อข้อมูลรางวัล (Array Size 3) ไปให้ Combat Manager ประมวลผลทำดาเมจทันที!
        // if (CombatManager.Instance != null) {
        //     CombatManager.Instance.ProcessSlotResult(finalResult);
        // }
    }
}