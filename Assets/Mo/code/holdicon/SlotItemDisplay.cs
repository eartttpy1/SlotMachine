using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotItemDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI Visuals")]
    public Image iconImage;
    public Image borderObject; // กรอบ Border ที่จะแสดงเมื่อชี้เมาส์
    public TMPro.TextMeshProUGUI maxOverlayText; // ข้อความคำว่า MAX ซ้อนบนไอคอน (Optional)

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

        if (maxOverlayText != null)
        {
            SlotIconData iconData = symbolData as SlotIconData;
            if (isUpgradeShop && iconData != null && iconData.countUpgrade >= 3)
            {
                maxOverlayText.gameObject.SetActive(true);
                maxOverlayText.text = "MAX";
            }
            else
            {
                maxOverlayText.gameObject.SetActive(false);
            }
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
                TicketShopItemData ticketData = symbolData as TicketShopItemData;

                if (iconData != null)
                {
                    if (iconData.countUpgrade >= 3)
                    {
                        name = $"[MAX] {iconData.SymbolName}";
                        desc = iconData.upgradeDescription;
                        price = "Price: MAX";
                    }
                    else
                    {
                        desc = $"{iconData.upgradeDescription}\nValue: {iconData.GetCurrentValue()} -> {iconData.GetCurrentValue() + iconData.GetUpgradeIncrement()}";
                        price = $"Price: {iconData.GetCurrentPrice()} Coins";
                    }
                }
                else if (ticketData != null)
                {
                    desc = ticketData.description;
                    price = $"Price: {ticketData.price} Coins";
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

    /// <summary>
    /// เมื่อคลิกที่ไอคอน
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isUpgradeShop) return;

        if (PlayerStats.Instance == null) return;

        SlotIconData iconData = symbolData as SlotIconData;
        TicketShopItemData ticketData = symbolData as TicketShopItemData;

        if (iconData != null)
        {
            if (iconData.countUpgrade < 3)
            {
                int price = iconData.GetCurrentPrice();
                if (PlayerStats.Instance.coins >= price)
                {
                    PlayerStats.Instance.coins -= price;

                    // เอา GetCurrentValue ไปบวกเพิ่มใน basevalue ของแต่ละชิ้นเลย เพื่อเป็นการอัพเดทค่าตามการ upgrade
                    int increment = iconData.GetUpgradeIncrement();
                    iconData.baseValue += increment;
                    iconData.countUpgrade++;

                    Debug.Log($"Upgraded {iconData.SymbolName}! New baseValue: {iconData.baseValue}, Level: {iconData.countUpgrade}");

                    UpdateVisuals();
                    // อัปเดตข้อมูลคำอธิบายและราคาทันทีหลังซื้อ
                    OnPointerEnter(eventData);
                }
                else
                {
                    Debug.LogWarning("Coins ไม่เพียงพอสำหรับการอัปเกรด!");
                }
            }
            else
            {
                Debug.LogWarning("สัญลักษณ์นี้อัปเกรดสูงสุดแล้ว!");
            }
        }
        else if (ticketData != null)
        {
            if (PlayerStats.Instance.coins >= ticketData.price)
            {
                PlayerStats.Instance.coins -= ticketData.price;
                int multiplier = 1;
                if (GameDataManager.Instance != null)
                {
                    multiplier = Mathf.RoundToInt(GameDataManager.Instance.GetTicketMultiplier());
                }
                PlayerStats.Instance.ticket67 += multiplier;
                Debug.Log($"ซื้อ Ticket 67 สำเร็จ! ได้รับ: {multiplier} ใบ, คงเหลือตั๋ว: {PlayerStats.Instance.ticket67} ใบ");

                UpdateVisuals();
                // อัปเดตข้อมูลคำอธิบายและราคาทันทีหลังซื้อ
                OnPointerEnter(eventData);
            }
            else
            {
                Debug.LogWarning("Coins ไม่เพียงพอสำหรับการซื้อตั๋ว!");
            }
        }
    }
}
