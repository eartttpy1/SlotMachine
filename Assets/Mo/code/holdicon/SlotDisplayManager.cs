using System.Collections.Generic;
using UnityEngine;

public class SlotDisplayManager : MonoBehaviour
{
    public enum SpawnMode { StartIcon, UpgradeShop, GachaShop }

    [Header("Configuration")]
    public SpawnMode spawnMode;      // เลือกว่า Manager ตัวนี้จะสร้างไอคอนของสถานะใด
    public GameObject slotItemPrefab; // ลาก Prefab ที่มีสคริปต์ SlotItemDisplay แปะอยู่มาใส่
    public Transform containerParent;  // แหล่งเก็บไอเท็มใน UI (เช่น GameObject ที่มี Grid Layout Group)

    [Header("Data Lists")]
    public List<SlotSymbolData> startIconList = new List<SlotSymbolData>();
    public List<SlotSymbolData> upgradeShopList = new List<SlotSymbolData>();
    public List<GachaRewardData> gachaShopList = new List<GachaRewardData>();

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
            for (int i = 0; i < startIconList.Count; i++)
            {
                CreateItem(startIconList[i], SpawnMode.StartIcon);
            }
        }
        else if (spawnMode == SpawnMode.UpgradeShop)
        {
            for (int i = 0; i < upgradeShopList.Count; i++)
            {
                CreateItem(upgradeShopList[i], SpawnMode.UpgradeShop);
            }
        }
        else if (spawnMode == SpawnMode.GachaShop)
        {
            for (int i = 0; i < gachaShopList.Count; i++)
            {
                CreateItem(gachaShopList[i], SpawnMode.GachaShop);
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

            // ป้อนข้อมูล ScriptableObject และสั่งให้อัปเดตรูปภาพทันที
            display.symbolData = data;
            display.UpdateVisuals();
        }
    }
}
