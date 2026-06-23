using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject upgradePanel;

    [SerializeField] private TextMeshProUGUI pointsText;

    private int currentPoints;

    private void Start()
    {
        LoadData();
        UpdateUI();
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        upgradePanel.SetActive(false);
    }

    public void ShowUpgradeMenu()
    {
        mainMenuPanel.SetActive(false);
        upgradePanel.SetActive(true);

        if (MapManager.Instance != null)
        {
            if (MapManager.Instance.canvasWin != null) MapManager.Instance.canvasWin.SetActive(false);
            if (MapManager.Instance.canvasLose != null) MapManager.Instance.canvasLose.SetActive(false);
            if (MapManager.Instance.gameCanvas != null) MapManager.Instance.gameCanvas.SetActive(false);
            if (MapManager.Instance.mainMenuCanvas != null) MapManager.Instance.mainMenuCanvas.SetActive(true);
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