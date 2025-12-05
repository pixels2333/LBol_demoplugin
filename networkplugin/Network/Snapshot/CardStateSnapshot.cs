using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

/// <summary>
/// 卡牌状态快照
/// </summary>
public class CardStateSnapshot
{
    /// <summary>
    /// 卡牌唯一ID
    /// </summary>
    public string CardId { get; set; } = string.Empty;

    /// <summary>
    /// 卡牌名称
    /// </summary>
    public string CardName { get; set; } = string.Empty;

    /// <summary>
    /// 卡牌类型（攻击/防御/技能等）
    /// </summary>
    public string CardType { get; set; } = "Unknown";

    /// <summary>
    /// 卡牌稀有度
    /// </summary>
    public string Rarity { get; set; } = "Common";

    /// <summary>
    /// 法力消耗
    /// </summary>
    public int ManaCost { get; set; } = 1;

    /// <summary>
    /// 升级次数
    /// </summary>
    public int UpgradeCount { get; set; } = 0;

    /// <summary>
    /// 是否在手中
    /// </summary>
    public bool InHand { get; set; } = false;

    /// <summary>
    /// 是否在牌库
    /// </summary>
    public bool InDeck { get; set; } = false;

    /// <summary>
    /// 是否在弃牌堆
    /// </summary>
    public bool InDiscard { get; set; } = false;

    /// <summary>
    /// 是否被消耗
    /// </summary>
    public bool IsExhausted { get; set; } = false;

    /// <summary>
    /// 是否被放逐
    /// </summary>
    public bool IsBanished { get; set; } = false;

    /// <summary>
    /// 是否是临时卡牌（本回合有效）
    /// </summary>
    public bool IsTemporary { get; set; } = false;

    /// <summary>
    /// 卡牌所在区域的索引
    /// </summary>
    public int ZoneIndex { get; set; } = -1;

    /// <summary>
    /// 额外元数据
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = [];
}
