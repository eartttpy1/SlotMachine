using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject upgradePanel;

    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private TextMeshProUGUI dmgText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI defText;
    [SerializeField] private TextMeshProUGUI ticketText;

    private int currentPoints;
    private int dmgLevel, hpLevel, defLevel, ticketChanceLevel;

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
    }

    private int GetCost(int currentLevel)
    {
        return 100 * (int)Mathf.Pow(1.2f, currentLevel);
    }

    public void UpgradeDmg()
    {
        int cost = GetCost(dmgLevel);
        if (currentPoints >= cost)
        {
            currentPoints -= cost;
            dmgLevel++;
            SaveData();
            UpdateUI();
        }
    }

    public void UpgradeHp()
    {
        int cost = GetCost(hpLevel);
        if (currentPoints >= cost)
        {
            currentPoints -= cost;
            hpLevel++;
            SaveData();
            UpdateUI();
        }
    }

    public void UpgradeDef()
    {
        int cost = GetCost(defLevel);
        if (currentPoints >= cost)
        {
            currentPoints -= cost;
            defLevel++;
            SaveData();
            UpdateUI();
        }
    }

    public void UpgradeTicketChance()
    {
        int cost = GetCost(ticketChanceLevel);
        if (currentPoints >= cost)
        {
            currentPoints -= cost;
            ticketChanceLevel++;
            SaveData();
            UpdateUI();
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt("PlayerPoints", currentPoints);
        PlayerPrefs.SetInt("DmgLv", dmgLevel);
        PlayerPrefs.SetInt("HpLv", hpLevel);
        PlayerPrefs.SetInt("DefLv", defLevel);
        PlayerPrefs.SetInt("TicketChanceLv", ticketChanceLevel);
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        currentPoints = PlayerPrefs.GetInt("PlayerPoints", 500); // ������������� 500 ����ʵ�
        dmgLevel = PlayerPrefs.GetInt("DmgLv", 0);
        hpLevel = PlayerPrefs.GetInt("HpLv", 0);
        defLevel = PlayerPrefs.GetInt("DefLv", 0);
        ticketChanceLevel = PlayerPrefs.GetInt("TicketChanceLv", 0);
    }

    private void UpdateUI()
    {
        if (pointsText != null) pointsText.text = "Points: " + currentPoints;

        // �� \n ���͢�鹺�÷Ѵ���� �Ѵ����� ������麹 ��� Lv �Ѻ Cost �����¡ѹ
        if (dmgText != null)
            dmgText.text = "Add 20% Dmg\nLv." + dmgLevel + " (Cost: " + GetCost(dmgLevel) + ")";

        if (hpText != null)
            hpText.text = "Start 200 HP\nLv." + hpLevel + " (Cost: " + GetCost(hpLevel) + ")";

        if (defText != null)
            defText.text = "Reduce Dmg 5%\nLv." + defLevel + " (Cost: " + GetCost(defLevel) + ")";

        if (ticketText != null)
        {
            int currentChance = 1 << ticketChanceLevel; // �ӹǳ % �͡�ʴ�ͻ��դٳ
            ticketText.text = "Ticket Drop " + currentChance + "%\nLv." + ticketChanceLevel + " (Cost: " + GetCost(ticketChanceLevel) + ")";
        }
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