using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

/// <summary>
/// 宝物（圣物）状态快照
/// </summary>
public class ExhibitStateSnapshot
{
    /// <summary>
    /// 宝物ID
    /// </summary>
    public string ExhibitId { get; set; } = string.Empty;

    /// <summary>
    /// 宝物名称
    /// </summary>
    public string ExhibitName { get; set; } = string.Empty;

    /// <summary>
    /// 宝物类型
    /// </summary>
    public string ExhibitType { get; set; } = "Unknown";

    /// <summary>
    /// 稀有度
    /// </summary>
    public string Rarity { get; set; } = "Common";

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; set; } = false;

    /// <summary>
    /// 是否熄灯（暂时失效）
    /// </summary>
    public bool IsBlackout { get; set; } = false;

    /// <summary>
    /// 是否有计数器
    /// </summary>
    public bool HasCounter { get; set; } = false;

    /// <summary>
    /// 计数器值
    /// </summary>
    public int Counter { get; set; } = 0;

    /// <summary>
    /// 图标名称
    /// </summary>
    public string IconName { get; set; } = "";

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// 是否是Boss专属宝物
    /// </summary>
    public bool IsBossExhibit { get; set; } = false;

    /// <summary>
    /// 是否是起始宝物
    /// </summary>
    public bool IsStarterExhibit { get; set; } = false;

    /// <summary>
    /// 额外配置
    /// </summary>
    public Dictionary<string, object> Config { get; set; } = [];
}
