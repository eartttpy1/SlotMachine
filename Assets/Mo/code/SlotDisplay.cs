using UnityEngine;
using UnityEngine.UI;

public class SlotDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    public Image uiImageDisplay; 

    public void SetupSlotDisplay(SlotIconData iconData)
    {
        if (iconData != null && uiImageDisplay != null)
        {
            // ดึงไฟล์ภาพ Sprite จาก ScriptableObject มาใส่ใน Component Image ของ Canvas ตรงๆ
            uiImageDisplay.sprite = iconData.iconSprite; 
            
            Debug.Log($"เปลี่ยนรูปภาพบนหน้าจอ UI เป็น: {iconData.iconName} เรียบร้อยแล้ว!");
        }
    }
}