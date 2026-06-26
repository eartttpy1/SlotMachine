using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ShogunEffectManager �Ѵ��á�����ҧ�Ϳ࿡�� (VFX) 㹡�õ�����
/// ���ԧ�ҡ Prefab ��е��˹��Դ (Transform) ����˹������ǧ˹��
/// </summary>
public class ShogunEffectManager : MonoBehaviour
{
    public static ShogunEffectManager Instance { get; private set; }

    [System.Serializable]
    public class EffectGroup
    {
        [Tooltip("�Ϳ࿡�� Prefab ����ͧ������ʻ���")]
        public GameObject effectPrefab;

        [Tooltip("GameObject ���͵��˹觾ԡѴ����ͧ�������Ϳ࿡������Դ")]
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
    /// ʻ����Ϳ࿡������͵���Ф�ⴹ���� (Get Hit) ������˹� GameObject ����駤�����
    /// </summary>
    public void SpawnGetHitEffect()
    {
        SpawnEffect(getHitEffect);
    }

    /// <summary>
    /// ʻ����Ϳ࿡��Һ����� (Sword) ������˹� GameObject ����駤�����
    /// </summary>
    public void SpawnSwordEffect()
    {
        SpawnEffect(swordEffect);
    }

    /// <summary>
    /// ʻ����Ϳ࿡��Һ����� (Sword) Ẻ�кص��˹觻��·ҧ (�� ����ѵ�ٷ��١���͡)
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
            Debug.LogWarning($"[ShogunEffectManager] �������öʻ��������ͧ�ҡ������ Sword Effect Prefab!");
            return;
        }

        Instantiate(swordEffect.effectPrefab, customLocation.position, customLocation.rotation);
    }

    /// <summary>
    /// ʻ����Ϳ࿡��Һ�˭����� (Great Sword) ������˹� GameObject ����駤�����
    /// </summary>
    public void SpawnGreatSwordEffect()
    {
        SpawnEffect(greatSwordEffect);
    }

    /// <summary>
    /// ʻ����Ϳ࿡��Һ�˭����� (Great Sword) Ẻ�кص��˹��ʴ��Ū��Ǥ���
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
            Debug.LogWarning($"[ShogunEffectManager] �������öʻ��������ͧ�ҡ������ Great Sword Effect Prefab!");
            return;
        }

        Instantiate(greatSwordEffect.effectPrefab, customLocation.position, customLocation.rotation);
    }
    
    /// <summary>
    /// �ѧ��ѹ��������Ѻ��� Instantiate �Ϳ࿡����е�Ǩ�ͺ�����١��ͧ�ͧ������
    /// </summary>
    private void SpawnEffect(EffectGroup group)
    {
        if (group == null) return;

        if (group.effectPrefab == null)
        {
            Debug.LogWarning($"[ShogunEffectManager] �������öʻ��������ͧ�ҡ������ Effect Prefab!");
            return;
        }

        if (group.spawnLocation == null)
        {
            Debug.LogWarning($"[ShogunEffectManager] �������öʻ��� {group.effectPrefab.name} �����ͧ�ҡ�����駤�� Spawn Location GameObject!");
            return;
        }

        // ʻ��� Instance �͡�ҵ���ԡѴ��е��˹觷��١��˹������ GameObject ����
        Instantiate(group.effectPrefab, group.spawnLocation.position, group.spawnLocation.rotation);
    }
}