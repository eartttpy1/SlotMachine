using UnityEngine;
using UnityEngine.UI;

public class SlotDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    public Image uiImageDisplay; 

    public void SetupSlotDisplay(SlotSymbolData symbolData)
    {
        if (symbolData != null && uiImageDisplay != null)
        {
            // ดึงไฟล์ภาพ Sprite จาก ScriptableObject มาใส่ใน Component Image ของ Canvas ตรงๆ
            uiImageDisplay.sprite = symbolData.SymbolSprite; 
            
            Debug.Log($"เปลี่ยนรูปภาพบนหน้าจอ UI เป็น: {symbolData.SymbolName} เรียบร้อยแล้ว!");
        }
    }

    public void ClearDisplay(Sprite defaultSprite = null)
    {
        if (uiImageDisplay != null)
        {
            uiImageDisplay.sprite = defaultSprite;
        }
    }
}