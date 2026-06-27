using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseUI;

    // เพิ่มช่องสำหรับลากแผ่นหลังเบลอมาใส่
    [Tooltip("ลากวัตถุ BlurBackground มาใส่ช่องนี้")]
    public GameObject blurBackground;

    [Tooltip("ลากวัตถุ MainMenu_Panel มาใส่ช่องนี้")]
    public GameObject mainMenuPanel;

    [Tooltip("ลากวัตถุ MainmenuCanvas มาใส่ช่องนี้")]
    public GameObject mainMenuCanvas;

    [Header("URP Volume Reference")]
    public Volume urpVolume;

    private bool isPaused = false;
    private Image blockerImage;

    void Start()
    {
        // เริ่มเกมมา ให้สั่งปิดทั้งเมนูและแผ่นหลังเบลอพร้อมกัน
        if (pauseUI != null) pauseUI.SetActive(false);
        if (blurBackground != null) blurBackground.SetActive(false);
        if (urpVolume != null) urpVolume.weight = 0f;
        Time.timeScale = 1f;

        CreateBlocker();
    }

    void Update()
    {
        if (mainMenuPanel != null && mainMenuPanel.activeInHierarchy)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        pauseUI.SetActive(true);
        if (blurBackground != null) blurBackground.SetActive(true); // เปิดหลังเบลอตอน Pause

        if (urpVolume != null) urpVolume.weight = 1f;
        Time.timeScale = 0f;
        isPaused = true;

        if (blockerImage != null) blockerImage.gameObject.SetActive(true);
    }

    public void ResumeGame()
    {
        pauseUI.SetActive(false);
        if (blurBackground != null) blurBackground.SetActive(false); // ปิดหลังเบลอตอนเล่นต่อ

        if (urpVolume != null) urpVolume.weight = 0f;
        Time.timeScale = 1f;
        isPaused = false;

        if (blockerImage != null) blockerImage.gameObject.SetActive(false);
    }

    public void OpenSettings()
    {
        Debug.Log("เปิดหน้าตั้งค่า (Settings Clicked!)");
    }

    public void FleeToMainMenu()
    {
        Time.timeScale = 1f;

        if (pauseUI != null) pauseUI.SetActive(false);
        if (blurBackground != null) blurBackground.SetActive(false); // มั่นใจว่าปิดหลังเบลอชัวร์ๆ ก่อนรีเซ็ต
        if (urpVolume != null) urpVolume.weight = 0f;
        if (blockerImage != null) blockerImage.gameObject.SetActive(false);

        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource audio in allAudioSources)
        {
            audio.Stop();
        }

        // โหลดซีนตัวเองใหม่เพื่อเคลียร์ระบบต่อสู้และการ Roll ค้างแบบ Surrender ล้างไพ่ร้อยเปอร์เซ็นต์
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);

        isPaused = false;
    }

    private void CreateBlocker()
    {
        if (pauseUI == null) return;

        GameObject blocker = new GameObject("PauseBlocker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        blocker.transform.SetParent(pauseUI.transform, false);
        blocker.transform.SetAsFirstSibling();

        RectTransform rect = blocker.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        blockerImage = blocker.GetComponent<Image>();
        blockerImage.color = new Color(0, 0, 0, 0.01f);
        blockerImage.gameObject.SetActive(false);
    }
}