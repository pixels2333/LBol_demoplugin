using System.Collections.Generic;
using LBoL.Core.Cards;

namespace NetworkPlugin.UI.Payloads;

public sealed class TradeDetailPayload
{
    public string TradeId { get; set; }

    public string SelfPlayerId { get; set; }

    public string PartnerPlayerId { get; set; }

    public string PartnerPlayerName { get; set; }

    public int MaxTradeSlots { get; set; } = 5;

    public List<Card> InitialDeckCards { get; set; }
}
