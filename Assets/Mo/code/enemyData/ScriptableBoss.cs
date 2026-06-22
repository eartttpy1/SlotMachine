using UnityEngine;

[CreateAssetMenu(fileName = "Boss_", menuName = "SlotGame/Enemy/Boss")]
public class ScriptableBoss : ScriptableEnemy
{
    private void Reset()
    {
        enemyName = "boss1";
        baseHP = 200;
        hpIncreasePerLevel = 30;
        baseDMG = 20;
        dmgIncreasePerLevel = 5;
        countHit = 3;
        coin = 10;
        chance067 = 5.0f;
    }
}
