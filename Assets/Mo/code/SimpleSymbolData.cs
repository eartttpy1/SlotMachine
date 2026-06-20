using UnityEngine;

[CreateAssetMenu(fileName = "SimpleSymbol_", menuName = "SlotGame/SimpleSymbolData")]
public class SimpleSymbolData : SlotSymbolData
{
    public string symbolName;
    public Sprite symbolSprite;
    public int baseWeightRandom = 10;

    public override string SymbolName => symbolName;
    public override Sprite SymbolSprite => symbolSprite;
    public override int BaseWeight => baseWeightRandom;
}
