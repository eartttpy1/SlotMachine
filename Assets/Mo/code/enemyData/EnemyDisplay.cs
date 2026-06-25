using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class EnemyDisplay : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Visual Components")]
    public Image enemyImage;
    public Image borderImage;
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    [Header("DOTween Settings")]
    public float punchScaleStrength = 0.2f;
    public float punchDuration = 0.3f;

    private CombatManager.EnemyInstance associatedEnemy;
    private int enemyIndex;

    public void Setup(CombatManager.EnemyInstance enemy, int index)
    {
        associatedEnemy = enemy;
        enemyIndex = index;
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (associatedEnemy == null) return;

        if (enemyImage != null && associatedEnemy.data != null)
        {
            enemyImage.sprite = associatedEnemy.data.sprite;
        }

        if (hpSlider != null)
        {
            // Ensure maxValue is set first to prevent clamping issues
            hpSlider.maxValue = associatedEnemy.maxHP;
            hpSlider.value = associatedEnemy.currentHP;
        }

        if (hpText != null)
        {
            hpText.text = $"{associatedEnemy.currentHP}";
        }

        // Default border state
        if (borderImage != null)
        {
            borderImage.gameObject.SetActive(false);
        }
    }

    public void PlayImpactAnimation()
    {
        if (enemyImage != null)
        {
            enemyImage.transform.DOComplete();
            enemyImage.transform.DOPunchScale(new Vector3(punchScaleStrength, -punchScaleStrength, 0), punchDuration, 10, 1);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CombatManager.Instance != null && CombatManager.Instance.currentState == CombatManager.CombatState.TargetSelection)
        {
            CombatManager.Instance.SelectEnemyTarget(enemyIndex);
        }
    }

    private bool wasTargetSelecting = false;

    private void Update()
    {
        if (CombatManager.Instance != null && CombatManager.Instance.currentState == CombatManager.CombatState.TargetSelection)
        {
            if (borderImage != null)
            {
                borderImage.gameObject.SetActive(true);
                float lerpVal = Mathf.PingPong(Time.time * 5f, 1f); // 5f controls the speed of flashing
                borderImage.color = Color.Lerp(Color.red, Color.white, lerpVal);
            }
            wasTargetSelecting = true;
        }
        else if (wasTargetSelecting)
        {
            wasTargetSelecting = false;
            if (borderImage != null)
            {
                borderImage.gameObject.SetActive(false);
                borderImage.color = Color.white;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (borderImage != null)
        {
            borderImage.gameObject.SetActive(true);
            if (CombatManager.Instance != null && CombatManager.Instance.currentState == CombatManager.CombatState.TargetSelection)
            {
                // Let Update handle the flashing color
            }
            else
            {
                borderImage.color = Color.white; // General hover border
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (borderImage != null)
        {
            if (CombatManager.Instance != null && CombatManager.Instance.currentState == CombatManager.CombatState.TargetSelection)
            {
                return; // Keep border active and flashing
            }
            borderImage.gameObject.SetActive(false);
        }
    }
}
