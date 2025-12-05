using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

/// <summary>
/// 战斗奖励快照
/// </summary>
public class BattleRewardSnapshot
{
    /// <summary>
    /// 金币奖励
    /// </summary>
    public int GoldReward { get; set; } = 0;

    /// <summary>
    /// 卡牌奖励
    /// </summary>
    public List<string> CardRewards { get; set; } = [];

    /// <summary>
    /// 宝物奖励
    /// </summary>
    public List<string> ExhibitRewards { get; set; } = [];

    /// <summary>
    /// 药水奖励
    /// </summary>
    public List<string> PotionRewards { get; set; } = [];

    /// <summary>
    /// 奖励是否已领取
    /// </summary>
    public bool IsRewardClaimed { get; set; } = false;
}
