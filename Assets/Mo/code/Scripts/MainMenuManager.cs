using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject upgradePanel;

    [SerializeField] private TextMeshProUGUI pointsText;

    [Header("Dynamic Shop Settings")]
    [SerializeField] private GameObject upStatPrefab;
    [SerializeField] private Transform upStatsContainer;
    [SerializeField] private System.Collections.Generic.List<ScriptableUpStats> allUpStats = new System.Collections.Generic.List<ScriptableUpStats>();

    [Header("Description Panel References")]
    [SerializeField] private TextMeshProUGUI shopNameText;
    [SerializeField] private TextMeshProUGUI shopDescriptionText;
    [SerializeField] private TextMeshProUGUI shopValueProgressText;
    [SerializeField] private TextMeshProUGUI shopPriceText;

    private int currentPoints;

    private void Start()
    {
        LoadData();
        UpdateUI();
        ShowMainMenu();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            currentPoints += 1000;
            SaveData();
            UpdateUI();
            Debug.Log($"[Test] Added 1000 points. Current points: {currentPoints}");
        }
    }


    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        upgradePanel.SetActive(false);
        LoadData();
        UpdateUI();
    }

    public void ShowUpgradeMenu()
    {
        mainMenuPanel.SetActive(false);
        upgradePanel.SetActive(true);
        LoadData();
        UpdateUI();

        PopulateUpgradeShop();

        if (MapManager.Instance != null)
        {
            if (MapManager.Instance.canvasWin != null) MapManager.Instance.canvasWin.SetActive(false);
            if (MapManager.Instance.canvasLose != null) MapManager.Instance.canvasLose.SetActive(false);
            if (MapManager.Instance.gameCanvas != null) MapManager.Instance.gameCanvas.SetActive(false);
            if (MapManager.Instance.mainMenuCanvas != null) MapManager.Instance.mainMenuCanvas.SetActive(true);
        }
    }

    public void PopulateUpgradeShop()
    {
        if (upStatsContainer == null || upStatPrefab == null) return;

        // Clear existing instantiated items
        foreach (Transform child in upStatsContainer)
        {
            Destroy(child.gameObject);
        }

        // Instantiate items
        foreach (var stat in allUpStats)
        {
            if (stat == null) continue;

            GameObject obj = Instantiate(upStatPrefab, upStatsContainer);
            UpStatDisplay display = obj.GetComponent<UpStatDisplay>();
            if (display != null)
            {
                // Setup references
                display.statData = stat;
                display.nameText = shopNameText;
                display.descriptionText = shopDescriptionText;
                display.valueProgressText = shopValueProgressText;
                display.priceText = shopPriceText;
                
                // Initialize display visuals
                display.UpdateVisuals();
            }
        }
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt("PlayerPoints", currentPoints);
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        currentPoints = PlayerPrefs.GetInt("PlayerPoints", 0);
    }

    public void UpdateUI()
    {
        if (pointsText != null) pointsText.text = "Points: " + currentPoints;
    }

    public void StartGame()
    {
        if (PlayerStats.Instance != null && PlayerStats.Instance.selectedSlotIcons.Count != 3)
        {
            Debug.LogWarning("ต้องเลือกไอเท็มให้ครบ 3 ชิ้นก่อนเริ่มเกม!");
            return;
        }
        SceneManager.LoadScene("GamePlay");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}