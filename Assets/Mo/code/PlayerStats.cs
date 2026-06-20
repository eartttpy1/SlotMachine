using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("067 Jackpot Chance (%)")]
    [Range(0f, 100f)]
    public float chance067 = 1.0f; // Default 1%

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
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
}
