using BepInEx;
using BepInEx.Configuration;
using System;

namespace NetworkPlugin.Configuration;

public partial class ConfigManager
{
    /// <summary>
    /// 功能开关配置区域 - 控制各种同步功能的启用状态
    /// </summary>
    #region 功能开关

    /// <summary>
    /// 卡牌同步开关
    /// 控制卡牌使用、抽取、洗牌等行为的网络同步
    /// </summary>
    #if false // Duplicate: kept for reference; actual declaration is in Configuration/ConfigManager.Sync.cs
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
    #endif

    /// <summary>
    /// GapStation功能扩展开关
    /// 控制在GapStation中添加交易和复活选项
    /// </summary>
    public ConfigEntry<bool> EnableGapStationExtensions { get; private set; }

    /// <summary>
    /// 交易功能开关
    /// 控制玩家之间的物品交易功能
    /// </summary>
    public ConfigEntry<bool> AllowTrading { get; private set; }

    /// <summary>
    /// 复活功能开关
    /// 控制玩家之间的复活功能
    /// </summary>
    public ConfigEntry<bool> AllowRevival { get; private set; }

    #endregion

    /// <summary>
    /// 绑定功能开关配置
    /// </summary>
    private void BindFeatureToggles(ConfigFile configFile)
    {
        // 在General.Toggles区域下绑定所有功能开关
        EnableCardSync = configFile.Bind(
            "General.Toggles",
            "EnableCardSync",
            true,
            "控制卡牌使用、抽取、洗牌等行为的网络同步"
        );

        EnableManaSync = configFile.Bind(
            "General.Toggles",
            "EnableManaSync",
            true,
            "控制法力消耗、恢复、增益等行为的网络同步"
        );

        EnableBattleSync = configFile.Bind(
            "General.Toggles",
            "EnableBattleSync",
            true,
            "控制伤害计算、状态效果、战斗结果的同步"
        );

        EnableMapSync = configFile.Bind(
            "General.Toggles",
            "EnableMapSync",
            true,
            "控制地图探索、节点状态、地图事件的同步"
        );

        EnableGapStationExtensions = configFile.Bind(
            "General.Toggles",
            "EnableGapStationExtensions",
            true,
            "控制在GapStation中添加交易和复活选项"
        );

        AllowTrading = configFile.Bind(
            "General.Toggles",
            "AllowTrading",
            true,
            "控制玩家之间的物品交易功能"
        );

        AllowRevival = configFile.Bind(
            "General.Toggles",
            "AllowRevival",
            true,
            "控制玩家之间的复活功能"
        );
    }
}
