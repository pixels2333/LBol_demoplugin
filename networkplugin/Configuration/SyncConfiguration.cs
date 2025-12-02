using System;

namespace NetworkPlugin.Configuration;

/// <summary>
/// 同步配置类
/// 包含控制同步行为的各种配置选项和性能参数
/// 用于调整同步系统的行为和性能特征
/// </summary>
/// <remarks>
/// <para>
/// 配置类别：
/// - 功能开关：控制各种同步功能的启用状态
/// - 性能参数：调整队列大小和缓存策略
/// - 行为控制：配置同步的触发条件和策略
/// </para>
///
/// <para>
/// 功能开关：
/// - EnableCardSync: 控制卡牌使用、抽取等行为的同步
/// - EnableManaSync: 控制法力消耗、恢复等行为的同步
/// - EnableBattleSync: 控制伤害、状态效果等战斗行为的同步
/// - EnableMapSync: 控制地图探索、节点状态等地图行为的同步
/// </para>
///
/// <para>
/// 性能参数：
/// - MaxQueueSize: 事件队列的最大容量，防止内存过度使用
/// - StateCacheExpiry: 状态缓存的存活时间，控制内存使用效率
/// </para>
/// </remarks>
public class SyncConfiguration
{
    /// <summary>
    /// 卡牌同步开关
    /// 控制卡牌使用、抽取、洗牌等行为的网络同步
    /// </summary>
    public bool EnableCardSync { get; set; } = true;
    // 控制卡牌使用、抽取等行为的同步开关

    /// <summary>
    /// 法力同步开关
    /// 控制法力消耗、恢复、增益等行为的网络同步
    /// </summary>
    public bool EnableManaSync { get; set; } = true;
    // 控制法力消耗、恢复等行为的同步开关

    /// <summary>
    /// 战斗同步开关
    /// 控制伤害计算、状态效果、战斗结果的同步
    /// </summary>
    public bool EnableBattleSync { get; set; } = true;
    // 控制伤害、状态效果等战斗行为的同步开关

    /// <summary>
    /// 地图同步开关
    /// 控制地图探索、节点状态、地图事件的同步
    /// </summary>
    public bool EnableMapSync { get; set; } = true;
    // 控制地图探索、节点状态等地图行为的同步开关

    /// <summary>
    /// 事件队列最大容量
    /// 网络不可用时事件队列的最大条目数量
    /// 超过此容量的新事件会被丢弃
    /// </summary>
    public int MaxQueueSize { get; set; } = 100;
    // 网络不可用时，事件队列的最大容量限制

    /// <summary>
    /// 状态缓存存活时间
    /// 本地状态缓存的存活时间，超过此时间的缓存会被清理
    /// 默认为5分钟，可以根据需要调整
    /// </summary>
    public TimeSpan StateCacheExpiry { get; set; } = TimeSpan.FromMinutes(5);
    // 状态缓存的存活时间，超过此时间的缓存将被自动清理
}