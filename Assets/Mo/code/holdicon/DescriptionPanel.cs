using UnityEngine;
using TMPro;

public class DescriptionPanel : MonoBehaviour
{
    public static DescriptionPanel Instance { get; private set; }

    [Header("UI Panel Root")]
    public GameObject panelRoot; // ตัว GameObject พื้นหลังพาเนลคำอธิบาย

    [Header("UI Texts")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI bonusDescriptionText;
    public TextMeshProUGUI priceText;

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

        // ซ่อนพาเนลตั้งแต่เริ่มเกม
        HideInfo();
    }

    /// <summary>
    /// แสดงคำอธิบายของสัญลักษณ์บน Panel ส่วนกลาง
    /// </summary>
    public void ShowInfo(string name, string desc, string bonus, string price)
    {
        if (panelRoot != null) panelRoot.SetActive(true);

        if (nameText != null) nameText.text = name;
        if (descriptionText != null) descriptionText.text = desc;

        // ควบคุมการแสดงผลตามข้อความที่มี
        if (bonusDescriptionText != null)
        {
            if (string.IsNullOrEmpty(bonus))
            {
                bonusDescriptionText.gameObject.SetActive(false);
            }
            else
            {
                bonusDescriptionText.gameObject.SetActive(true);
                bonusDescriptionText.text = bonus;
            }
        }

        if (priceText != null)
        {
            if (string.IsNullOrEmpty(price))
            {
                priceText.gameObject.SetActive(false);
            }
            else
            {
                priceText.gameObject.SetActive(true);
                priceText.text = price;
            }
        }
    }

    /// <summary>
    /// ซ่อน Panel เมื่อเมาส์ออก
    /// </summary>
    public void HideInfo()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }
}
