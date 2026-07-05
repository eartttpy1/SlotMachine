using UnityEngine;

public abstract class ScriptableEnemy : ScriptableObject
{
    [Header("Base Info")]
    public string enemyName;
    public Sprite sprite;
    public AnimatorOverrideController animationOverrideController;

    [Header("Stats Settings")]
    public int baseHP = 100;
    public int hpIncreasePerLevel = 30;
    
    public int baseDMG = 15;
    public int dmgIncreasePerLevel = 5;

    public int countHit = 1;
    public int coin = 5;
    public float chance067 = 2.0f;

    public virtual int GetMaxHP(int level)
    {
        return baseHP + level * hpIncreasePerLevel;
    }

    public virtual int GetBaseDMG(int level)
    {
        return baseDMG + level * dmgIncreasePerLevel;
    }
}
