using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ShogunEffectManager จัดการการสร้างเอฟเฟกต์ (VFX) ในการต่อสู้
/// โดยอิงจาก Prefab และตำแหน่งเกิด (Transform) ที่กำหนดไว้ล่วงหน้า
/// </summary>
public class ShogunEffectManager : MonoBehaviour
{
    public static ShogunEffectManager Instance { get; private set; }

    [System.Serializable]
    public class EffectGroup
    {
        [Tooltip("เอฟเฟกต์ Prefab ที่ต้องการให้สปอว์น")]
        public GameObject effectPrefab;

        [Tooltip("GameObject หรือตำแหน่งพิกัดที่ต้องการให้เอฟเฟกต์นี้ไปเกิด")]
        public Transform spawnLocation;
    }

    [Header("1. Get Hit Effect Configuration")]
    public EffectGroup getHitEffect;

    [Header("2. Sword Effect Configuration")]
    public EffectGroup swordEffect;

    [Header("3. Great Sword Effect Configuration")]
    public EffectGroup greatSwordEffect;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// สปอว์นเอฟเฟกต์เมื่อตัวละครโดนโจมตี (Get Hit) ตามตำแหน่ง GameObject ที่ตั้งค่าไว้
    /// </summary>
    public void SpawnGetHitEffect()
    {
        SpawnEffect(getHitEffect);
    }

    /// <summary>
    /// สปอว์นเอฟเฟกต์ดาบเดี่ยว (Sword) ตามตำแหน่ง GameObject ที่ตั้งค่าไว้
    /// </summary>
    public void SpawnSwordEffect()
    {
        SpawnEffect(swordEffect);
    }

    /// <summary>
    /// สปอว์นเอฟเฟกต์ดาบเดี่ยว (Sword) แบบระบุตำแหน่งปลายทาง (เช่น ตัวศัตรูที่ถูกเลือก)
    /// </summary>
    public void SpawnSwordEffect(Transform customLocation)
    {
        if (customLocation == null)
        {
            SpawnSwordEffect();
            return;
        }

        if (swordEffect == null || swordEffect.effectPrefab == null)
        {
            Debug.LogWarning($"[ShogunEffectManager] ไม่สามารถสปอว์นได้เนื่องจากลืมใส่ Sword Effect Prefab!");
            return;
        }

        Instantiate(swordEffect.effectPrefab, customLocation.position, customLocation.rotation);
    }

    /// <summary>
    /// สปอว์นเอฟเฟกต์ดาบใหญ่หมู่ (Great Sword) ตามตำแหน่ง GameObject ที่ตั้งค่าไว้
    /// </summary>
    public void SpawnGreatSwordEffect()
    {
        SpawnEffect(greatSwordEffect);
    }

    /// <summary>
    /// สปอว์นเอฟเฟกต์ดาบใหญ่หมู่ (Great Sword) แบบระบุตำแหน่งแสดงผลชั่วคราว
    /// </summary>
    public void SpawnGreatSwordEffect(Transform customLocation)
    {
        if (customLocation == null)
        {
            SpawnGreatSwordEffect();
            return;
        }

        if (greatSwordEffect == null || greatSwordEffect.effectPrefab == null)
        {
            Debug.LogWarning($"[ShogunEffectManager] ไม่สามารถสปอว์นได้เนื่องจากลืมใส่ Great Sword Effect Prefab!");
            return;
        }

        Instantiate(greatSwordEffect.effectPrefab, customLocation.position, customLocation.rotation);
    }

    /// <summary>
    /// ฟังก์ชันภายในสำหรับการ Instantiate เอฟเฟกต์และตรวจสอบความถูกต้องของข้อมูล
    /// </summary>
    private void SpawnEffect(EffectGroup group)
    {
        if (group == null) return;

        if (group.effectPrefab == null)
        {
            Debug.LogWarning($"[ShogunEffectManager] ไม่สามารถสปอว์นได้เนื่องจากลืมใส่ Effect Prefab!");
            return;
        }

        if (group.spawnLocation == null)
        {
            Debug.LogWarning($"[ShogunEffectManager] ไม่สามารถสปอว์น {group.effectPrefab.name} ได้เนื่องจากลืมตั้งค่า Spawn Location GameObject!");
            return;
        }

        // สปอว์น Instance ออกมาตามพิกัดและตำแหน่งที่ถูกกำหนดไว้โดย GameObject นั้นๆ
        Instantiate(group.effectPrefab, group.spawnLocation.position, group.spawnLocation.rotation);
    }
}