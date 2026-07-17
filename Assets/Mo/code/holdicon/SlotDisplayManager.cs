using System.Collections.Generic;
using UnityEngine;

public class SlotDisplayManager : MonoBehaviour
{
    public enum SpawnMode { StartIcon, UpgradeShop, GachaShop, SelectionScreen, ItemShop }

    public static bool isDeleteModeActive = false;

    [Header("Configuration")]
    public SpawnMode spawnMode;      // เลือกว่า Manager ตัวนี้จะสร้างไอคอนของสถานะใด
    public GameObject slotItemPrefab; // ลาก Prefab ที่มีสคริปต์ SlotItemDisplay แปะอยู่มาใส่
    public Transform containerParent;  // แหล่งเก็บไอเท็มใน UI (เช่น GameObject ที่มี Grid Layout Group)

    [Header("UI Controls")]
    public UnityEngine.UI.Button deleteButton;

    public List<SlotSymbolData> startIconList = new List<SlotSymbolData>();
    public List<SlotSymbolData> upgradeShopList = new List<SlotSymbolData>();
    public List<GachaRewardData> gachaShopList = new List<GachaRewardData>();
    public List<SlotIconData> allAvailableIcons = new List<SlotIconData>();
    public List<SlotIconData> itemShopList = new List<SlotIconData>();

    [Header("Shop Session Control")]
    public List<SlotIconData> currentShopSessionItems = new List<SlotIconData>();

    private void Start()
    {
        SpawnItems();
    }

    /// <summary>
    /// สั่งสร้างไอเท็มขึ้นมาแสดงผลตามโหมดที่เลือก
    /// </summary>
    public void SpawnItems()
    {
        // ล้างวัตถุเดิมใน Container ออกก่อน (กรณีเปิดปิดหน้าต่างใหม่)
        if (containerParent != null)
        {
            foreach (Transform child in containerParent)
            {
                Destroy(child.gameObject);
            }
        }

        // เริ่มวนลูปสร้างตามโหมดที่ตั้งไว้
        if (spawnMode == SpawnMode.StartIcon)
        {
            int count = 0;
            if (PlayerStats.Instance != null && PlayerStats.Instance.selectedSlotIcons != null && PlayerStats.Instance.selectedSlotIcons.Count > 0)
            {
                foreach (var item in PlayerStats.Instance.selectedSlotIcons)
                {
                    if (item != null)
                    {
                        CreateItem(item, SpawnMode.StartIcon);
                        count++;
                    }
                }
            }
            else
            {
                for (int i = 0; i < startIconList.Count; i++)
                {
                    CreateItem(startIconList[i], SpawnMode.StartIcon);
                    count++;
                }
            }

            // Adjust GridLayoutGroup cell size x based on item count
            if (containerParent != null)
            {
                var grid = containerParent.GetComponent<UnityEngine.UI.GridLayoutGroup>();
                if (grid != null)
                {
                    float cellX = (count <= 6) ? 100f : 65f;
                    grid.cellSize = new Vector2(cellX, grid.cellSize.y);
                }
            }

            // Enable delete button only if item count is greater than 3
            if (deleteButton != null)
            {
                deleteButton.interactable = (count > 3);
            }
        }
        else if (spawnMode == SpawnMode.UpgradeShop)
        {
            List<SlotSymbolData> shopItems = new List<SlotSymbolData>();
            if (PlayerStats.Instance != null && PlayerStats.Instance.selectedSlotIcons != null)
            {
                List<SlotIconData> selectedPool = new List<SlotIconData>(PlayerStats.Instance.selectedSlotIcons);
                selectedPool.RemoveAll(item => item == null);

                if (selectedPool.Count <= 4)
                {
                    foreach (var item in selectedPool) shopItems.Add(item);
                }
                else
                {
                    // Randomly select 4 unique items
                    List<SlotIconData> temp = new List<SlotIconData>(selectedPool);
                    for (int i = 0; i < 4; i++)
                    {
                        int randIdx = UnityEngine.Random.Range(0, temp.Count);
                        shopItems.Add(temp[randIdx]);
                        temp.RemoveAt(randIdx);
                    }
                }
            }

            // Find ticket from original list
            TicketShopItemData ticketItem = null;
            foreach (var item in upgradeShopList)
            {
                if (item is TicketShopItemData)
                {
                    ticketItem = (TicketShopItemData)item;
                    break;
                }
            }

            foreach (var item in shopItems)
            {
                CreateItem(item, SpawnMode.UpgradeShop);
            }
            if (ticketItem != null)
            {
                CreateItem(ticketItem, SpawnMode.UpgradeShop);
            }
        }
        else if (spawnMode == SpawnMode.GachaShop)
        {
            for (int i = 0; i < gachaShopList.Count; i++)
            {
                CreateItem(gachaShopList[i], SpawnMode.GachaShop);
            }
        }
        else if (spawnMode == SpawnMode.SelectionScreen)
        {
            for (int i = 0; i < allAvailableIcons.Count; i++)
            {
                CreateItem(allAvailableIcons[i], SpawnMode.SelectionScreen);
            }
        }
        else if (spawnMode == SpawnMode.ItemShop)
        {
            if (currentShopSessionItems.Count == 0)
            {
                List<SlotIconData> candidates = new List<SlotIconData>();
                foreach (var item in itemShopList)
                {
                    if (item != null)
                    {
                        bool alreadyInSlot = PlayerStats.Instance != null && PlayerStats.Instance.selectedSlotIcons.Contains(item);
                        if (!alreadyInSlot)
                        {
                            candidates.Add(item);
                        }
                    }
                }

                List<SlotIconData> temp = new List<SlotIconData>(candidates);
                int countToSelect = Mathf.Min(5, temp.Count);
                for (int i = 0; i < countToSelect; i++)
                {
                    int randIdx = UnityEngine.Random.Range(0, temp.Count);
                    currentShopSessionItems.Add(temp[randIdx]);
                    temp.RemoveAt(randIdx);
                }
            }

            for (int i = 0; i < currentShopSessionItems.Count; i++)
            {
                CreateItem(currentShopSessionItems[i], SpawnMode.ItemShop);
            }
        }
    }

    private void CreateItem(SlotSymbolData data, SpawnMode mode)
    {
        if (slotItemPrefab == null || containerParent == null || data == null) return;

        // สั่ง Instantiate Prefab ออกมาใน Container ที่ต้องการ โดยส่งค่า false เพื่อไม่ให้สเกลและพิกัด UI เพี้ยนบน Canvas
        GameObject newItem = Instantiate(slotItemPrefab, containerParent, false);
        SlotItemDisplay display = newItem.GetComponent<SlotItemDisplay>();

        if (display != null)
        {
            // กำหนดสถานะ Boolean บน Prefab ตามโหมดการ Spawn
            display.isStartIcon = (mode == SpawnMode.StartIcon);
            display.isUpgradeShop = (mode == SpawnMode.UpgradeShop);
            display.isGachaShop = (mode == SpawnMode.GachaShop);
            display.isSelectionItem = (mode == SpawnMode.SelectionScreen);
            display.isItemShop = (mode == SpawnMode.ItemShop);

            // ป้อนข้อมูล ScriptableObject และสั่งให้อัปเดตรูปภาพทันที
            display.symbolData = data;
            display.UpdateVisuals();
        }
    }

    public void ToggleDeleteMode()
    {
        if (PlayerStats.Instance != null && PlayerStats.Instance.selectedSlotIcons.Count <= 3)
        {
            Debug.LogWarning("ไม่สามารถเปิดโหมดลบได้ ต้องมีของในสล็อตอย่างน้อย 3 ชิ้น!");
            isDeleteModeActive = false;
            
            // Refresh to turn off borders if active
            SlotItemDisplay[] displays = FindObjectsOfType<SlotItemDisplay>();
            foreach (var disp in displays)
            {
                if (disp.isStartIcon) disp.UpdateVisuals();
            }
            return;
        }

        isDeleteModeActive = !isDeleteModeActive;
        Debug.Log($"Toggle Delete Mode: {isDeleteModeActive}");

        SlotItemDisplay[] allDisplays = FindObjectsOfType<SlotItemDisplay>();
        foreach (var disp in allDisplays)
        {
            if (disp.isStartIcon) disp.UpdateVisuals();
        }
    }
}
