using UnityEngine;

[CreateAssetMenu(fileName = "TicketShopItem_", menuName = "SlotGame/TicketShopItemData")]
public class TicketShopItemData : SlotSymbolData
{
    public string itemName = "Ticket 67";
    public Sprite itemSprite;
    public int amountTicket67 = 1;
    [TextArea] public string itemDescription = "Get 1 ticket 67 for bet 067 mode after boss";
    public int price = 10;

    public override string SymbolName => itemName;
    public override Sprite SymbolSprite => itemSprite;
    public override int BaseWeight => 0;
    public override string description => itemDescription;
    public override string bonusDescription => "";
}
