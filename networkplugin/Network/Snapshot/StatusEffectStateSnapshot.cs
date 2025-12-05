namespace NetworkPlugin.Network.Snapshot;

/// <summary>
/// 状态效果快照
/// </summary>
public class StatusEffectStateSnapshot
{
    /// <summary>
    /// 效果ID
    /// </summary>
    public string EffectId { get; set; } = string.Empty;

    /// <summary>
    /// 效果名称
    /// </summary>
    public string EffectName { get; set; } = string.Empty;

    /// <summary>
    /// 效果类型
    /// </summary>
    public string EffectType { get; set; } = "Unknown";

    /// <summary>
    /// 等级
    /// </summary>
    public int Level { get; set; } = 1;

    /// <summary>
    /// 持续时间（回合数）
    /// </summary>
    public int Duration { get; set; } = 0;

    /// <summary>
    /// 是否是Debuff
    /// </summary>
    public bool IsDebuff { get; set; } = false;

    /// <summary>
    /// 是否是永久性效果
    /// </summary>
    public bool IsPermanent { get; set; } = false;

    /// <summary>
    /// 效果值
    /// </summary>
    public int EffectValue { get; set; } = 0;

    /// <summary>
    /// 效果描述
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// 来源（哪个玩家/敌人施加的）
    /// </summary>
    public string SourceId { get; set; } = "";
}
