using UnityEngine;
using TMPro;
using DG.Tweening;

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

    [Header("DOTween Settings")]
    public float charTypingSpeed = 0.02f;
    private Sequence typingSequence;

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
    /// แสดงคำอธิบายของสัญลักษณ์บน Panel ส่วนกลาง (เวอร์ชันปรับปรุงเพื่อ DOTween Free)
    /// </summary>
    public void ShowInfo(string name, string desc, string bonus, string price)
    {
        if (panelRoot != null) panelRoot.SetActive(true);

        typingSequence?.Kill();
        typingSequence = DOTween.Sequence();

        // 1. พิมพ์ชื่อไอเทม
        if (nameText != null)
        {
            nameText.text = "";
            int nameLength = 0;
            typingSequence.Append(
                DOTween.To(() => nameLength, x => {
                    nameLength = x;
                    nameText.text = name.Substring(0, nameLength);
                }, name.Length, name.Length * charTypingSpeed).SetEase(Ease.Linear)
            );
        }

        // 2. พิมพ์คำอธิบาย
        if (descriptionText != null)
        {
            descriptionText.text = "";
            int descLength = 0;
            typingSequence.Append(
                DOTween.To(() => descLength, x => {
                    descLength = x;
                    descriptionText.text = desc.Substring(0, descLength);
                }, desc.Length, desc.Length * charTypingSpeed).SetEase(Ease.Linear)
            );
        }

        // 3. ควบคุมการแสดงผลโบนัสพิเศษ (เช่น Great Sword)
        if (bonusDescriptionText != null)
        {
            if (string.IsNullOrEmpty(bonus))
            {
                bonusDescriptionText.gameObject.SetActive(false);
            }
            else
            {
                bonusDescriptionText.gameObject.SetActive(true);
                bonusDescriptionText.text = "";
                int bonusLength = 0;
                typingSequence.Append(
                    DOTween.To(() => bonusLength, x => {
                        bonusLength = x;
                        bonusDescriptionText.text = bonus.Substring(0, bonusLength);
                    }, bonus.Length, bonus.Length * charTypingSpeed).SetEase(Ease.Linear)
                );
            }
        }

        // 4. พิมพ์ราคา
        if (priceText != null)
        {
            if (string.IsNullOrEmpty(price))
            {
                priceText.gameObject.SetActive(false);
            }
            else
            {
                priceText.gameObject.SetActive(true);
                priceText.text = "";
                int priceLength = 0;
                typingSequence.Append(
                    DOTween.To(() => priceLength, x => {
                        priceLength = x;
                        priceText.text = price.Substring(0, priceLength);
                    }, price.Length, price.Length * charTypingSpeed).SetEase(Ease.Linear)
                );
            }
        }
    }

    /// <summary>
    /// ซ่อน Panel เมื่อเมาส์ออก
    /// </summary>
    public void HideInfo()
    {
        typingSequence?.Kill();
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }
}