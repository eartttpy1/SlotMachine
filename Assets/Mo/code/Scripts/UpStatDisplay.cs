using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UpStatDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI Visuals")]
    public Image iconImage;
    public Image borderObject;
    public TextMeshProUGUI maxOverlayText;

    [Header("UpStat Data")]
    public ScriptableUpStats statData;

    [Header("Description Panel References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI valueProgressText;
    public TextMeshProUGUI priceText;

    private void Start()
    {
        if (borderObject != null) borderObject.gameObject.SetActive(false);
        UpdateVisuals();
    }

    private void OnValidate()
    {
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (iconImage != null && statData != null)
        {
            iconImage.sprite = statData.icon;
        }

        if (maxOverlayText != null && statData != null)
        {
            if (statData.CountLevel >= statData.maxLevel)
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (borderObject != null) borderObject.gameObject.SetActive(true);
        ShowInfo();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (borderObject != null) borderObject.gameObject.SetActive(false);
        HideInfo();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (statData == null) return;

        int currentPoints = PlayerPrefs.GetInt("PlayerPoints", 500);
        int cost = statData.Point;

        if (statData.CountLevel < statData.maxLevel)
        {
            if (currentPoints >= cost)
            {
                currentPoints -= cost;
                PlayerPrefs.SetInt("PlayerPoints", currentPoints);
                PlayerPrefs.Save();

                statData.BuyUpgrade();
                Debug.Log($"Upgraded {statData.statName}! Level: {statData.CountLevel}, New Value: {statData.CurrentBaseValue}");

                UpdateVisuals();
                ShowInfo();

                // Refresh main menu points text if MainMenuManager exists
                MainMenuManager mainMenu = FindObjectOfType<MainMenuManager>();
                if (mainMenu != null)
                {
                    mainMenu.Invoke("LoadData", 0f);
                    mainMenu.Invoke("UpdateUI", 0f);
                }
            }
            else
            {
                Debug.LogWarning("คะแนนไม่เพียงพอสำหรับการอัปเกรด!");
            }
        }
        else
        {
            Debug.LogWarning("ระดับการอัปเกรดถึงขั้นสูงสุดแล้ว!");
        }
    }

    private void ShowInfo()
    {
        if (statData == null) return;

        if (nameText != null) nameText.text = statData.statName;
        if (descriptionText != null) descriptionText.text = statData.GetDynamicDescription();
        
        if (valueProgressText != null)
        {
            if (statData.CountLevel >= statData.maxLevel)
            {
                valueProgressText.text = $"{statData.CurrentBaseValue} (MAX)";
            }
            else
            {
                float nextVal = statData.CurrentBaseValue + statData.increaseValue;
                valueProgressText.text = $"{statData.CurrentBaseValue} -> {nextVal}";
            }
        }

        if (priceText != null)
        {
            if (statData.CountLevel >= statData.maxLevel)
            {
                priceText.text = "Price: MAX";
            }
            else
            {
                priceText.text = $"Price: {statData.Point} P";
            }
        }
    }

    private void HideInfo()
    {
        // Clear panel info if desired
    }
}
