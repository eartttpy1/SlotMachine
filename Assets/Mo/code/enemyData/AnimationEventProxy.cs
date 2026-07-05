using UnityEngine;

public class AnimationEventProxy : MonoBehaviour
{
    private EnemyDisplay parentDisplay;

    private void Awake()
    {
        parentDisplay = GetComponentInParent<EnemyDisplay>();
    }

    public void TriggerEnemyAttackSFX()
    {
        if (parentDisplay != null)
        {
            parentDisplay.TriggerEnemyAttackSFX();
        }
    }

    public void PlayGetHitSoundEvent()
    {
        if (parentDisplay != null)
        {
            parentDisplay.PlayGetHitSoundEvent();
        }
    }
}
