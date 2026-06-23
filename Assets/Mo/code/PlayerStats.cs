using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("067 Jackpot Chance (%)")]
    [Range(0f, 100f)]
    public float chance067 = 1.0f; // Default 1%
    public TextMeshProUGUI chanceText;

    [Header("Player HP")]
    public int currentHP = 50;
    public int maxHP = 100;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI maxHpText;
    public Slider hpSlider;

    [Header("Potion & Healing")]
    public int countpotion = 0;
    public bool isplayerturn = true;
    public TextMeshProUGUI potionCountText;
    public SlotIconData potionData;

    [Header("Player Shield")]
    public int currentShield = 0;
    public int maxShield = 100;
    public TextMeshProUGUI shieldText;
    public TextMeshProUGUI maxShieldText;
    public Slider shieldSlider;

    [Header("Ticket 67")]
    public int ticket67 = 5;
    public TextMeshProUGUI ticket67Text;

    [Header("Gacha Currency")]
    public int coins = 100;
    public TextMeshProUGUI coinsText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CacheAllInitialValues();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdatePotionUI();
    }

    public void Update()
    {
        if (currentHP <= 0 && MapManager.Instance != null)
        {
            MapManager.Instance.TriggerLose();
        }

        if (chanceText != null)
        {
            chanceText.text = "067 : " + chance067.ToString("F1") + "%";
        }
        if (hpText != null)
        {
            hpText.text = currentHP.ToString();
        }
        if (maxHpText != null)
        {
            maxHpText.text = maxHP.ToString();
        }
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }
        if (shieldText != null)
        {
            shieldText.text = currentShield.ToString();
        }
        if (maxShieldText != null)
        {
            maxShieldText.text = maxShield.ToString();
        }
        if (shieldSlider != null)
        {
            shieldSlider.maxValue = maxShield;
            shieldSlider.value = currentShield;
        }
        if (ticket67Text != null)
        {
            ticket67Text.text = ticket67.ToString();
        }
        if (coinsText != null)
        {
            coinsText.text = coins.ToString() + " $";
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            AddChance067(1.0f);
        }
        UpdatePotionUI();
    }

    public void AddPotion(int amount)
    {
        countpotion += amount;
        UpdatePotionUI();
    }

    public void Heal()
    {
        if (currentHP >= maxHP)
        {
            Debug.Log("HP เต็มแล้ว ไม่สามารถใช้ Potion ได้");
            return;
        }

        if (isplayerturn && countpotion > 0)
        {
            countpotion--;
            int healAmount = potionData != null ? potionData.GetCurrentValue() : 15;
            currentHP = Mathf.Min(currentHP + healAmount, maxHP);
            Debug.Log($"Healed for {healAmount}! Potions left: {countpotion}, HP: {currentHP}/{maxHP}");
            UpdatePotionUI();
        }
    }

    public void UpdatePotionUI()
    {
        if (potionCountText != null)
        {
            potionCountText.text = countpotion.ToString();
        }
    }

    /// <summary>
    /// เพิ่ม/ลด โอกาส 067
    /// </summary>
    public void AddChance067(float amount)
    {
        chance067 = Mathf.Clamp(chance067 + amount, 0f, 100f);
        Debug.Log($"โอกาส 067 เปลี่ยนแปลง: {amount:F1}% (ปัจจุบัน: {chance067:F1}%)");
    }

    /// <summary>
    /// เมื่อจัดการมอนสเตอร์สำเร็จ
    /// </summary>
    public void OnMonsterDefeated(bool isBoss)
    {
        float addedAmount = isBoss ? 5f : 2f;
        AddChance067(addedAmount);
    }

    public void TakeDamage(int damage)
    {
        if (currentShield > 0)
        {
            if (currentShield >= damage)
            {
                currentShield -= damage;
                damage = 0;
            }
            else
            {
                damage -= currentShield;
                currentShield = 0;
            }
        }

        if (damage > 0)
        {
            currentHP = Mathf.Max(0, currentHP - damage);
        }
        Debug.Log($"Player took damage! Shield: {currentShield}, HP: {currentHP}/{maxHP}");

        if (currentHP <= 0 && MapManager.Instance != null)
        {
            MapManager.Instance.TriggerLose();
        }
    }

    private void CacheAllInitialValues()
    {
        SlotIconData[] icons = Resources.FindObjectsOfTypeAll<SlotIconData>();
        foreach (var icon in icons)
        {
            icon.CacheInitialValue();
        }
    }

    public void ResetAllScriptableObjects()
    {
        SlotIconData[] icons = Resources.FindObjectsOfTypeAll<SlotIconData>();
        foreach (var icon in icons)
        {
            icon.ResetToDefault();
        }
    }

    private void OnDestroy()
    {
        ResetAllScriptableObjects();
    }
}
