using System.Collections.Generic;
using LBoL.Core.Cards;

namespace NetworkPlugin.UI.Payloads;

/// <summary>
/// 交易详情弹窗的初始化载荷：包含交易双方信息、槽位限制和初始卡牌列表
/// </summary>
public sealed class TradeDetailPayload
{
    /// <summary>交易唯一标识</summary>
    public string TradeId { get; set; }

    /// <summary>当前玩家 ID</summary>
    public string SelfPlayerId { get; set; }

    /// <summary>交易伙伴玩家 ID</summary>
    public string PartnerPlayerId { get; set; }

    /// <summary>交易伙伴玩家显示名称</summary>
    public string PartnerPlayerName { get; set; }

    /// <summary>最大交易槽位数</summary>
    public int MaxTradeSlots { get; set; } = 5;

    /// <summary>初始牌组卡牌列表（用于挑选交易物品）</summary>
    public List<Card> InitialDeckCards { get; set; }
}
