using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GreatSwordMiniGame : MonoBehaviour
{
    [Header("UI References (Optional)")]
    [Tooltip("If assigned, UI will be spawned inside this Canvas. Otherwise, it will find or create one.")]
    public Canvas parentCanvas;

    [Header("Font Setup")]
    public TMP_FontAsset customPixelFont;

    [Header("Settings")]
    public float chargeSpeed = 1.0f; // Time in seconds to fill the gauge fully
    public float critThresholdMin = 0.8f;
    public float critThresholdMax = 0.9f;
    public float overheatThreshold = 1.0f; // Fixed: Added missing variable

    [Header("Damage Multipliers")]
    public float critMultiplier = 2.0f; // MAX DAMAGE
    public float earlyReleasePenaltyMultiplier = 0.5f; // Max damage multiplier for early release
    public float overheatMultiplier = 0.1f; // Severe penalty

    private GameObject miniGamePanel;
    private Slider chargeSlider;
    private TextMeshProUGUI feedbackText;
    private Image fillImage;

    private bool isCharging = false;
    private int calculatedDamage = 0;
    private Tween shakeTween;
    private Vector2 originalSliderPos;

    private void Awake()
    {
        GenerateUIProgrammatically();
    }

    private void SetUIGameObject(GameObject obj)
    {
        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer > -1) obj.layer = uiLayer;

        obj.transform.localScale = Vector3.one;

        RectTransform rt = obj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition3D = Vector3.zero;
            rt.localRotation = Quaternion.identity;
        }
    }

    private void GenerateUIProgrammatically()
    {
        // 1. Find or Create Canvas
        Canvas canvas = parentCanvas;
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>(); // Fixed: Updated obsolete FindObjectOfType
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("GreatSwordCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObj.AddComponent<GraphicRaycaster>();
            }
        }

        // Ensure canvas has high sorting order to render above everything
        canvas.sortingOrder = 100;
        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer > -1) canvas.gameObject.layer = uiLayer;

        // 2. Create Panel (The parent container for our UI)
        miniGamePanel = new GameObject("GreatSwordMiniGamePanel");
        miniGamePanel.transform.SetParent(canvas.transform, false);
        SetUIGameObject(miniGamePanel);

        // Position on the left side, designed to sit below the "Player Turn" frame
        RectTransform panelRt = miniGamePanel.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.05f, 0.3f);
        panelRt.anchorMax = new Vector2(0.25f, 0.45f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = Vector2.zero;
        panelRt.sizeDelta = Vector2.zero;

        // 3. Create Slider Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(panelRt, false);
        SetUIGameObject(bgObj);

        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f); // Dark UI color
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        // Make the background take the lower part of the panel
        bgRt.anchorMin = new Vector2(0f, 0f);
        bgRt.anchorMax = new Vector2(1f, 0.4f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // 4. Create Fill Area and Fill
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(bgRt, false);
        SetUIGameObject(fillAreaObj);

        RectTransform fillAreaRt = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.offsetMin = new Vector2(5, 5); // Slight padding
        fillAreaRt.offsetMax = new Vector2(-5, -5);

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaRt, false);
        SetUIGameObject(fillObj);

        fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.yellow; // Will change dynamically
        RectTransform fillRt = fillObj.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;

        // 5. Create Sweet Spot Zone indicator (On top of Fill Area so it's clearly visible)
        GameObject sweetSpotObj = new GameObject("SweetSpot");
        sweetSpotObj.transform.SetParent(bgRt, false); // Sibling to Fill Area, drawn over it
        SetUIGameObject(sweetSpotObj);

        Image sweetSpotImg = sweetSpotObj.AddComponent<Image>();
        sweetSpotImg.color = new Color(1f, 0f, 0f, 0.5f); // Semi-transparent RED Sweet Spot
        RectTransform ssRt = sweetSpotObj.GetComponent<RectTransform>();
        ssRt.anchorMin = new Vector2(critThresholdMin, 0);
        ssRt.anchorMax = new Vector2(critThresholdMax, 1);
        ssRt.offsetMin = Vector2.zero;
        ssRt.offsetMax = Vector2.zero;

        // 6. Setup Slider Component
        chargeSlider = panelRt.gameObject.AddComponent<Slider>();
        chargeSlider.interactable = false;
        chargeSlider.transition = Selectable.Transition.None;
        chargeSlider.fillRect = fillRt;
        chargeSlider.value = 0f;

        // 7. Create Feedback Text
        GameObject textObj = new GameObject("FeedbackText");
        textObj.transform.SetParent(panelRt, false);
        SetUIGameObject(textObj);

        feedbackText = textObj.AddComponent<TextMeshProUGUI>();
        feedbackText.alignment = TextAlignmentOptions.Bottom;
        feedbackText.fontSize = 42;
        if (customPixelFont != null)
        {
            feedbackText.font = customPixelFont;
        }
        feedbackText.overflowMode = TextOverflowModes.Overflow;
        feedbackText.color = Color.white;

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        // Make text fill the upper part of the panel
        textRt.anchorMin = new Vector2(0f, 0.5f);
        textRt.anchorMax = new Vector2(1f, 1f);
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        // Hide initially
        miniGamePanel.SetActive(false);
        originalSliderPos = panelRt.anchoredPosition;
    }

    public IEnumerator StartMiniGame(int baseDamage, Action<int> onComplete)
    {
        if (miniGamePanel == null || chargeSlider == null)
        {
            Debug.LogError("GreatSwordMiniGame: UI failed to generate! Bypassing minigame.");
            onComplete?.Invoke(baseDamage);
            yield break;
        }

        // Setup
        miniGamePanel.SetActive(true);
        chargeSlider.value = 0f;
        UpdateFillColor();

        if (feedbackText != null)
        {
            feedbackText.text = "HOLD TO CHARGE!";
            feedbackText.transform.localScale = Vector3.one;
            feedbackText.color = Color.white;
        }

        isCharging = false;
        calculatedDamage = 0;

        // Wait for player to press the button to start charging
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space));

        isCharging = true;
        if (feedbackText != null) feedbackText.text = "CHARGING...";

        // Start shaking the slider panel to add tension
        shakeTween = chargeSlider.GetComponent<RectTransform>()
            .DOShakePosition(10f, strength: new Vector3(5f, 5f, 0f), vibrato: 20)
            .SetLoops(-1, LoopType.Restart);

        // Charge loop
        while (isCharging)
        {
            chargeSlider.value += (Time.deltaTime / chargeSpeed);
            UpdateFillColor();

            // Check for overheat
            if (chargeSlider.value >= overheatThreshold)
            {
                isCharging = false;
                chargeSlider.value = 1f;
                FinishMiniGame(Mathf.RoundToInt(baseDamage * overheatMultiplier), "<color=#FF0000>MISS! OVERHEAT!</color>");
                break;
            }

            // Check for release
            if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space))
            {
                isCharging = false;
                EvaluateRelease(baseDamage);
                break;
            }

            yield return null;
        }

        // Wait a short moment to show the result to the player
        yield return new WaitForSeconds(1.2f);

        // Cleanup
        shakeTween?.Kill();
        chargeSlider.GetComponent<RectTransform>().anchoredPosition = originalSliderPos;
        miniGamePanel.SetActive(false);

        onComplete?.Invoke(calculatedDamage);
    }

    private void UpdateFillColor()
    {
        if (fillImage == null) return;

        float val = chargeSlider.value;
        if (val >= critThresholdMin && val <= critThresholdMax)
        {
            fillImage.color = Color.red; // In the Sweet Spot!
        }
        else if (val > critThresholdMax)
        {
            fillImage.color = new Color(0.5f, 0f, 0f); // Dark red (Overheating)
        }
        else
        {
            // Gradient from Yellow to Orange based on early progress
            float t = val / critThresholdMin;
            fillImage.color = Color.Lerp(Color.yellow, new Color(1f, 0.5f, 0f), t);
        }
    }

    private void EvaluateRelease(int baseDamage)
    {
        float val = chargeSlider.value;
        if (val >= critThresholdMin && val <= critThresholdMax)
        {
            // CRIT
            FinishMiniGame(Mathf.RoundToInt(baseDamage * critMultiplier), "<color=#FF0000>CRIT!! MAX DAMAGE!</color>");
        }
        else if (val < critThresholdMin)
        {
            // Early Release - Penalty applied
            float t = val / critThresholdMin;
            float penaltyScale = Mathf.Lerp(0.1f, earlyReleasePenaltyMultiplier, t);
            FinishMiniGame(Mathf.RoundToInt(baseDamage * penaltyScale), "WEAK HIT...");
        }
    }

    private void FinishMiniGame(int finalDamage, string feedback)
    {
        calculatedDamage = finalDamage;
        if (feedbackText != null)
        {
            feedbackText.text = feedback;
            feedbackText.transform.DOComplete();
            feedbackText.transform.DOPunchScale(Vector3.one * 0.5f, 0.5f, 10, 1);
        }
    }
}