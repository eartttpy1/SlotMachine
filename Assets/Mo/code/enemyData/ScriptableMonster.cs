using UnityEngine;

[CreateAssetMenu(fileName = "Monster_", menuName = "SlotGame/Enemy/Monster")]
public class ScriptableMonster : ScriptableEnemy
{
    private void Reset()
    {
        enemyName = "monster1";
        baseHP = 100;
        hpIncreasePerLevel = 30;
        baseDMG = 15;
        dmgIncreasePerLevel = 5;
        countHit = 1;
        coin = 5;
        chance067 = 2.0f;
    }
}
