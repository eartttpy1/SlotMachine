using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotItemDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Visuals")]
    public Image iconImage;
    public Image borderObject; // กรอบ Border ที่จะแสดงเมื่อชี้เมาส์

    [Header("Active Data")]
    public SlotSymbolData symbolData; // ข้อมูลสัญลักษณ์ที่ถูกเลือกใช้งานในปัจจุบัน

    [Header("State Configuration (เลือกอันใดอันหนึ่งเป็น true)")]
    public bool isStartIcon;
    public bool isUpgradeShop;
    public bool isGachaShop;

    private void Awake()
    {
        // หากลืมลาก Image ใส่ช่องใน Inspector ระบบจะค้นหาจากตัว GameObject นี้ให้อัตโนมัติ
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
        }
    }

    private void Start()
    {
        // เริ่มต้นด้วยการปิด Border
        if (borderObject != null) borderObject.gameObject.SetActive(false);

        // แสดงผลรูปภาพสัญลักษณ์
        UpdateVisuals();
    }

    private void OnValidate()
    {
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (iconImage != null && symbolData != null)
        {
            iconImage.sprite = symbolData.SymbolSprite;
        }
    }

    /// <summary>
    /// เมื่อเมาส์ชี้เข้าไอคอน
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (borderObject != null) borderObject.gameObject.SetActive(true);

        // ส่งข้อมูลไปยัง Panel แสดงคำอธิบายส่วนกลาง
        if (DescriptionPanel.Instance != null && symbolData != null)
        {
            string name = symbolData.SymbolName;
            string desc = "";
            string bonus = "";
            string price = "";

            if (isStartIcon)
            {
                desc = symbolData.description;
                bonus = symbolData.bonusDescription;
            }
            else if (isUpgradeShop)
            {
                SlotIconData iconData = symbolData as SlotIconData;
                if (iconData != null)
                {
                    desc = iconData.upgradeDescription;
                    price = $"Price: {iconData.GetCurrentPrice()} Coins";
                }
                else
                {
                    desc = symbolData.description;
                }
            }
            else if (isGachaShop)
            {
                desc = symbolData.description;
                bonus = symbolData.bonusDescription;
            }

            DescriptionPanel.Instance.ShowInfo(name, desc, bonus, price);
        }
    }

    /// <summary>
    /// เมื่อเมาส์ชี้ออกจากไอคอน
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (borderObject != null) borderObject.gameObject.SetActive(false);

        // สั่งให้ Panel ส่วนกลางซ่อนตัวหรือล้างข้อมูล
        if (DescriptionPanel.Instance != null)
        {
            DescriptionPanel.Instance.HideInfo();
        }
    }
}
