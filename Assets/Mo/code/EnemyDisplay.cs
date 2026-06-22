using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EnemyDisplay : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Visual Components")]
    public Image enemyImage;
    public Image borderImage;
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

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
            hpSlider.maxValue = associatedEnemy.maxHP;
            hpSlider.value = associatedEnemy.currentHP;
        }

        if (hpText != null)
        {
            hpText.text = $"{associatedEnemy.currentHP}/{associatedEnemy.maxHP}";
        }

        // Highlight border if we are in TargetSelection state
        if (borderImage != null)
        {
            if (CombatManager.Instance != null && CombatManager.Instance.currentState == CombatManager.CombatState.TargetSelection)
            {
                borderImage.color = Color.yellow;
                borderImage.gameObject.SetActive(true);
            }
            else
            {
                borderImage.gameObject.SetActive(false);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CombatManager.Instance != null && CombatManager.Instance.currentState == CombatManager.CombatState.TargetSelection)
        {
            CombatManager.Instance.SelectEnemyTarget(enemyIndex);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CombatManager.Instance != null && CombatManager.Instance.currentState == CombatManager.CombatState.TargetSelection)
        {
            if (borderImage != null)
            {
                borderImage.gameObject.SetActive(true);
                borderImage.color = Color.red; // Visual cue for hovering on target selection
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CombatManager.Instance != null && CombatManager.Instance.currentState == CombatManager.CombatState.TargetSelection)
        {
            if (borderImage != null)
            {
                borderImage.color = Color.yellow;
            }
        }
    }
}
