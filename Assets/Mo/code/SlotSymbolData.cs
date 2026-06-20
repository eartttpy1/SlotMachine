using UnityEngine;

public abstract class SlotSymbolData : ScriptableObject
{
    public abstract string SymbolName { get; }
    public abstract Sprite SymbolSprite { get; }
    public abstract int BaseWeight { get; }
}
