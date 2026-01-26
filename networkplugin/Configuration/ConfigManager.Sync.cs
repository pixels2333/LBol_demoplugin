using BepInEx;
using BepInEx.Configuration;
using System;

namespace NetworkPlugin.Configuration;

public partial class ConfigManager
{
    /// <summary>
    /// 同步配置区域 - 控制同步行为的各种配置选项和性能参数
    /// </summary>
    #region 同步配置

    /// <summary>
    /// 卡牌同步开关
    /// 控制卡牌使用、抽取、洗牌等行为的网络同步
    /// </summary>
    public ConfigEntry<bool> EnableCardSync { get; private set; }

    /// <summary>
    /// 法力同步开关
    /// 控制法力消耗、恢复、增益等行为的网络同步
    /// </summary>
    public ConfigEntry<bool> EnableManaSync { get; private set; }

    /// <summary>
    /// 战斗同步开关
    /// 控制伤害计算、状态效果、战斗结果的同步
    /// </summary>
    public ConfigEntry<bool> EnableBattleSync { get; private set; }

    /// <summary>
    /// 地图同步开关
    /// 控制地图探索、节点状态、地图事件的同步
    /// </summary>
    public ConfigEntry<bool> EnableMapSync { get; private set; }

    /// <summary>
    /// 状态效果同步开关
    /// 控制增益、减益、特殊效果等状态效果的同步
    /// </summary>
    public ConfigEntry<bool> EnableStatusEffectSync { get; private set; }

    /// <summary>
    /// 事件队列最大容量
    /// 网络不可用时事件队列的最大条目数量
    /// 超过此容量的新事件会被丢弃
    /// </summary>
    public ConfigEntry<int> MaxQueueSize { get; private set; }

    /// <summary>
    /// 状态缓存存活时间（分钟）
    /// 本地状态缓存的存活时间，超过此时间的缓存会被清理
    /// 默认为5分钟，可以根据需要调整
    /// </summary>
    public ConfigEntry<float> StateCacheExpiryMinutes { get; private set; }

    /// <summary>
    /// 存档/读档同步开关
    /// 默认关闭：避免在主菜单或未选择存档阶段触发扫描与日志，干扰启动流程。
    /// </summary>
    public ConfigEntry<bool> EnableSaveLoadSync { get; private set; }

    #endregion

    /// <summary>
    /// 绑定同步配置
    /// </summary>
    private void BindSyncConfiguration(ConfigFile configFile)
    {
        // 在Sync.Toggles区域下绑定同步功能开关
        EnableCardSync = configFile.Bind(
            "Sync.Toggles",
            "EnableCardSync",
            true,
            "控制卡牌使用、抽取、洗牌等行为的网络同步"
        );

        EnableManaSync = configFile.Bind(
            "Sync.Toggles",
            "EnableManaSync",
            true,
            "控制法力消耗、恢复、增益等行为的网络同步"
        );

        EnableBattleSync = configFile.Bind(
            "Sync.Toggles",
            "EnableBattleSync",
            true,
            "控制伤害计算、状态效果、战斗结果的同步"
        );

        EnableMapSync = configFile.Bind(
            "Sync.Toggles",
            "EnableMapSync",
            true,
            "控制地图探索、节点状态、地图事件的同步"
        );

        EnableStatusEffectSync = configFile.Bind(
            "Sync.Toggles",
            "EnableStatusEffectSync",
            true,
            "控制增益、减益、特殊效果等状态效果的同步"
        );

        EnableSaveLoadSync = configFile.Bind(
            "Sync.Toggles",
            "EnableSaveLoadSync",
            false,
            "控制存档/读档流程的同步（建议在确认已进入并选择存档后再启用）"
        );

        // 在Sync.Performance区域下绑定同步性能参数
        MaxQueueSize = configFile.Bind(
            "Sync.Performance",
            "MaxQueueSize",
            100,
            "网络不可用时事件队列的最大条目数量，超过此容量的新事件会被丢弃"
        );

        StateCacheExpiryMinutes = configFile.Bind(
            "Sync.Performance",
            "StateCacheExpiryMinutes",
            5.0f,
            "本地状态缓存的存活时间（分钟），超过此时间的缓存会被清理"
        );
    }

    /// <summary>
    /// 获取同步配置实例
    /// 从当前的ConfigEntry值创建SyncConfiguration对象
    /// </summary>
    /// <returns>同步配置实例</returns>
    public SyncConfiguration GetSyncConfiguration()
    {
        return new SyncConfiguration
        {
            EnableCardSync = EnableCardSync?.Value ?? true,
            EnableManaSync = EnableManaSync?.Value ?? true,
            EnableBattleSync = EnableBattleSync?.Value ?? true,
            EnableMapSync = EnableMapSync?.Value ?? true,
            // SyncConfiguration 里暂未声明该字段；这里保持兼容，patch 层自己读取 EnableSaveLoadSync。
            MaxQueueSize = MaxQueueSize?.Value ?? 100,
            StateCacheExpiry = TimeSpan.FromMinutes(StateCacheExpiryMinutes?.Value ?? 5.0f)
        };
    }
}
