using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class SlotItemDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI Visuals")]
    public Image iconImage;
    public Image borderObject; // กรอบ Border ที่จะแสดงเมื่อชี้เมาส์
    public TMPro.TextMeshProUGUI maxOverlayText; // ข้อความคำว่า MAX ซ้อนบนไอคอน (Optional)
    public TMPro.TextMeshProUGUI priceOverlayText; // ข้อความแสดงราคาซ้อนบนไอคอน (Optional)

    [Header("Active Data")]
    public SlotSymbolData symbolData; // ข้อมูลสัญลักษณ์ที่ถูกเลือกใช้งานในปัจจุบัน

    [Header("State Configuration (เลือกอันใดอันหนึ่งเป็น true)")]
    public bool isStartIcon;
    public bool isUpgradeShop;
    public bool isGachaShop;
    public bool isSelectionItem;
    public bool isItemShop;

    private Tween blinkTween;

    private void Awake()
    {
        // หากลืมลาก Image ใส่ช่องใน Inspector ระบบจะค้นหาจากตัว GameObject นี้ให้อัตโนมัติ
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
        }
    }

    private void UpdateBlinkState()
    {
        if (blinkTween != null)
        {
            blinkTween.Kill();
            blinkTween = null;
        }

        if (isStartIcon && SlotDisplayManager.isDeleteModeActive)
        {
            if (borderObject != null)
            {
                borderObject.gameObject.SetActive(true);
                borderObject.color = Color.red;
                blinkTween = borderObject.DOFade(0.2f, 0.4f).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
            }
        }
        else
        {
            if (borderObject != null && !isSelectionItem)
            {
                borderObject.gameObject.SetActive(false);
                borderObject.color = Color.white;
            }
        }
    }

    private void OnDestroy()
    {
        if (blinkTween != null)
        {
            blinkTween.Kill();
            blinkTween = null;
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
            if (isSelectionItem && symbolData is SlotIconData selectIcon)
            {
                bool isSelected = PlayerStats.Instance != null && PlayerStats.Instance.selectedSlotIcons.Contains(selectIcon);
                if (borderObject != null)
                {
                    borderObject.gameObject.SetActive(isSelected);
                    borderObject.color = isSelected ? Color.red : Color.white;
                }
                iconImage.color = isSelected ? Color.white : new Color(1f, 1f, 1f, 0.4f);
            }
            else if (isItemShop && symbolData is SlotIconData shopIcon)
            {
                bool isSold = PlayerStats.Instance != null && PlayerStats.Instance.selectedSlotIcons.Contains(shopIcon);
                if (borderObject != null) borderObject.gameObject.SetActive(false);
                iconImage.color = isSold ? new Color(1f, 1f, 1f, 0.4f) : Color.white;
            }
            else
            {
                if (borderObject != null) borderObject.color = Color.white;
                iconImage.color = Color.white;
            }
        }

        if (maxOverlayText != null)
        {
            SlotIconData iconData = symbolData as SlotIconData;
            if (isUpgradeShop && iconData != null && iconData.countUpgrade >= 3)
            {
                maxOverlayText.gameObject.SetActive(true);
                maxOverlayText.text = "MAX";
            }
            else if (isItemShop && iconData != null)
            {
                bool isSold = PlayerStats.Instance != null && PlayerStats.Instance.selectedSlotIcons.Contains(iconData);
                maxOverlayText.gameObject.SetActive(isSold);
                maxOverlayText.text = "SOLD";
            }
            else
            {
                maxOverlayText.gameObject.SetActive(false);
            }
        }

        if (priceOverlayText != null)
        {
            if (isUpgradeShop && symbolData != null)
            {
                SlotIconData iconData = symbolData as SlotIconData;
                TicketShopItemData ticketData = symbolData as TicketShopItemData;

                if (iconData != null)
                {
                    if (iconData.countUpgrade >= 3)
                    {
                        priceOverlayText.text = "-";
                    }
                    else
                    {
                        priceOverlayText.text = $"{iconData.GetCurrentPrice()}";
                    }
                }
                else if (ticketData != null)
                {
                    priceOverlayText.text = $"{ticketData.price}";
                }
                else
                {
                    priceOverlayText.text = "";
                }
            }
            else if (isItemShop && symbolData is SlotIconData shopIcon)
            {
                bool isSold = PlayerStats.Instance != null && PlayerStats.Instance.selectedSlotIcons.Contains(shopIcon);
                priceOverlayText.text = isSold ? "-" : "20";
            }
            else
            {
                priceOverlayText.text = "";
            }
        }

        UpdateBlinkState();
    }

    /// <summary>
    /// เมื่อเมาส์ชี้เข้าไอคอน
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isStartIcon && SlotDisplayManager.isDeleteModeActive)
        {
            // Do not override blink border during enter in delete mode
            return;
        }

        if (borderObject != null)
        {
            borderObject.gameObject.SetActive(true);
            if (isSelectionItem && symbolData is SlotIconData selectIcon)
            {
                bool isSelected = PlayerStats.Instance != null && PlayerStats.Instance.selectedSlotIcons.Contains(selectIcon);
                borderObject.color = isSelected ? Color.red : Color.white;
            }
            else
            {
                borderObject.color = Color.white;
            }
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonHover();
        }

        // ส่งข้อมูลไปยัง Panel แสดงคำอธิบายส่วนกลาง
        if (DescriptionPanel.Instance != null && symbolData != null)
        {
            string name = symbolData.SymbolName;
            string desc = "";
            string bonus = "";
            string price = "";

            if (isStartIcon || isSelectionItem || isItemShop)
            {
                desc = symbolData.description;
                bonus = symbolData.bonusDescription;
                if (isItemShop && symbolData is SlotIconData shopIcon)
                {
                    bool isSold = PlayerStats.Instance != null && PlayerStats.Instance.selectedSlotIcons.Contains(shopIcon);
                    price = isSold ? "SOLD" : "Price: 20 Coins";
                }
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
                        desc = $"{iconData.upgradeDescription}\n{iconData.GetMaxUpgradeValueString()}";
                        price = "Price: MAX";
                    }
                    else
                    {
                        desc = $"{iconData.upgradeDescription}\n{iconData.GetUpgradeValueString()}";
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
        if (isStartIcon && SlotDisplayManager.isDeleteModeActive)
        {
            return;
        }

        if (isSelectionItem && symbolData is SlotIconData selectIcon)
        {
            bool isSelected = PlayerStats.Instance != null && PlayerStats.Instance.selectedSlotIcons.Contains(selectIcon);
            if (borderObject != null)
            {
                borderObject.gameObject.SetActive(isSelected);
                borderObject.color = isSelected ? Color.red : Color.white;
            }
        }
        else
        {
            if (borderObject != null) borderObject.gameObject.SetActive(false);
        }

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
        if (isStartIcon && SlotDisplayManager.isDeleteModeActive && symbolData is SlotIconData deleteIcon)
        {
            if (PlayerStats.Instance != null)
            {
                if (PlayerStats.Instance.selectedSlotIcons.Count > 3)
                {
                    PlayerStats.Instance.selectedSlotIcons.Remove(deleteIcon);
                    Debug.Log($"Deleted {deleteIcon.SymbolName} from slot! Active count: {PlayerStats.Instance.selectedSlotIcons.Count}");
                    SlotDisplayManager.isDeleteModeActive = false;

                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayButtonClick();
                    }

                    SlotDisplayManager[] managers = FindObjectsOfType<SlotDisplayManager>();
                    foreach (var manager in managers)
                    {
                        manager.SpawnItems();
                    }
                }
                else
                {
                    Debug.LogWarning("ไม่สามารถลบได้ ต้องมีของในสล็อตอย่างน้อย 3 ชิ้น!");
                }
            }
            return;
        }

        if (isItemShop && symbolData is SlotIconData shopIcon)
        {
            if (PlayerStats.Instance != null)
            {
                if (PlayerStats.Instance.selectedSlotIcons.Contains(shopIcon))
                {
                    Debug.LogWarning("ไอเท็มนี้ถูกซื้อไปแล้ว!");
                    return;
                }

                int price = 20;
                if (PlayerStats.Instance.coins >= price)
                {
                    PlayerStats.Instance.coins -= price;
                    PlayerStats.Instance.selectedSlotIcons.Add(shopIcon);
                    Debug.Log($"Bought {shopIcon.SymbolName} and added to slot! Active count: {PlayerStats.Instance.selectedSlotIcons.Count}");

                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayButtonClick();
                    }

                    SlotDisplayManager[] managers = FindObjectsOfType<SlotDisplayManager>();
                    foreach (var manager in managers)
                    {
                        manager.SpawnItems();
                    }
                }
                else
                {
                    Debug.LogWarning("เหรียญไม่พอ!");
                }
            }
            return;
        }

        if (isSelectionItem && symbolData is SlotIconData selectIcon)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }

            if (PlayerStats.Instance != null)
            {
                bool isSelected = PlayerStats.Instance.selectedSlotIcons.Contains(selectIcon);
                if (isSelected)
                {
                    PlayerStats.Instance.selectedSlotIcons.Remove(selectIcon);
                    Debug.Log($"Removed {selectIcon.SymbolName} from slot selection. Total: {PlayerStats.Instance.selectedSlotIcons.Count}");
                }
                else
                {
                    if (PlayerStats.Instance.selectedSlotIcons.Count < 3)
                    {
                        PlayerStats.Instance.selectedSlotIcons.Add(selectIcon);
                        Debug.Log($"Added {selectIcon.SymbolName} to slot selection. Total: {PlayerStats.Instance.selectedSlotIcons.Count}");
                    }
                    else
                    {
                        Debug.LogWarning("เลือกไอเท็มได้สูงสุด 3 ชิ้นเท่านั้น!");
                    }
                }
                UpdateVisuals();
            }
            return;
        }

        if (!isUpgradeShop) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

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

                    // ไม่แก้ไข baseValue ตรงๆ ให้ใช้ค่าที่คิดจากข้างนอก (GetCurrentValue) แทนเพื่อไม่ให้ส่งผลกระทบกับตอนเริ่มใหม่
                    iconData.countUpgrade++;

                    Debug.Log($"Upgraded {iconData.SymbolName}! New calculated value: {iconData.GetCurrentValue()}, Level: {iconData.countUpgrade}");

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
