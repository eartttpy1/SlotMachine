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

    [Header("Hit Animations")]
    public GameObject swordHitObject;
    public GameObject greatSwordHitObject;

    [Header("Enemy Animator")]
    public Animator enemyAnimator;

    private CombatManager.EnemyInstance associatedEnemy;
    private int enemyIndex;

    public void Setup(CombatManager.EnemyInstance enemy, int index)
    {
        associatedEnemy = enemy;
        enemyIndex = index;
        UpdateVisuals();

        // Ensure hit animations are inactive on startup
        if (swordHitObject != null) swordHitObject.SetActive(false);
        if (greatSwordHitObject != null) greatSwordHitObject.SetActive(false);

        if (enemyAnimator == null)
        {
            enemyAnimator = GetComponent<Animator>();
        }
        if (enemyAnimator == null)
        {
            enemyAnimator = GetComponentInChildren<Animator>();
        }

        if (enemyAnimator != null && associatedEnemy != null && associatedEnemy.data != null && associatedEnemy.data.animationOverrideController != null)
        {
            enemyAnimator.runtimeAnimatorController = associatedEnemy.data.animationOverrideController;
        }
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

    public void PlaySwordHitAnimation()
    {
        if (swordHitObject != null)
        {
            swordHitObject.SetActive(false);
            swordHitObject.SetActive(true);
            Animator anim = swordHitObject.GetComponent<Animator>();
            if (anim != null)
            {
                anim.Play(0, -1, 0f);
            }
        }
    }

    public void PlayGreatSwordHitAnimation()
    {
        if (greatSwordHitObject != null)
        {
            greatSwordHitObject.SetActive(false);
            greatSwordHitObject.SetActive(true);
            Animator anim = greatSwordHitObject.GetComponent<Animator>();
            if (anim != null)
            {
                anim.Play(0, -1, 0f);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CombatManager.Instance != null && CombatManager.Instance.currentState == CombatManager.CombatState.TargetSelection)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySwordSound();
            }
            CombatManager.Instance.SelectEnemyTarget(enemyIndex);
        }
        else
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
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
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonHover();
        }

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

    public void PlayAttackAnimation()
    {
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("ATK");
        }
    }

    public void PlayDieAnimation()
    {
        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool("isDie", true);
        }
    }

    // Call this via Unity Animation Event on the enemy's attack clip
    public void TriggerEnemyAttackSFX()
    {
        if (CombatManager.Instance != null && associatedEnemy != null)
        {
            CombatManager.Instance.OnEnemyAttackSwing(associatedEnemy, this);
        }
    }

    // Direct sound play option for Animation Events
    public void PlayGetHitSoundEvent()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGetHitSound();
        }
    }
}
