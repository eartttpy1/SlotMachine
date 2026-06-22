using System.Collections.Generic;
using UnityEngine;

public enum MapType
{
    MonsterMap,
    Chest,
    Shop,
    Boss
}

[CreateAssetMenu(fileName = "Map_", menuName = "SlotGame/Map")]
public class ScriptableMap : ScriptableObject
{
    [Header("Visuals")]
    public Sprite mapSprite;

    [Header("Type & Structure")]
    public MapType mapType;

    [Header("Combat Settings (If Monster or Boss)")]
    public List<ScriptableEnemy> enemies = new List<ScriptableEnemy>();
}
