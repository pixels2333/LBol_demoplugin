namespace NetworkPlugin.Network.Snapshot;

/// <summary>
/// 玩家统计快照
/// </summary>
public class PlayerPerformanceSnapshot
{
    /// <summary>
    /// 总伤害
    /// </summary>
    public int TotalDamage { get; set; } = 0;

    /// <summary>
    /// 击败的敌人数量
    /// </summary>
    public int EnemiesDefeated { get; set; } = 0;

    /// <summary>
    /// 战斗胜利次数
    /// </summary>
    public int BattlesWon { get; set; } = 0;

    /// <summary>
    /// 战斗失败次数
    /// </summary>
    public int BattlesLost { get; set; } = 0;

    /// <summary>
    /// 抽牌数量
    /// </summary>
    public int CardsDrawn { get; set; } = 0;

    /// <summary>
    /// 使用卡牌数量
    /// </summary>
    public int CardsPlayed { get; set; } = 0;

    /// <summary>
    /// 获得的宝物数量
    /// </summary>
    public int ExhibitsCollected { get; set; } = 0;

    /// <summary>
    /// 击败的Boss数量
    /// </summary>
    public int BossesDefeated { get; set; } = 0;

    /// <summary>
    /// 发现的秘密房间数
    /// </summary>
    public int SecretsFound { get; set; } = 0;
}