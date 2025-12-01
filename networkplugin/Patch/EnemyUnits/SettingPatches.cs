using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Configuration;

namespace NetworkPlugin.Patch.EnemyUnits;

/// <summary>
/// 设置补丁类
/// 负责管理多人游戏模式下的各种配置设置
/// 包括难度调整系数、奖励分配、游戏平衡参数等
/// </summary>
public class SettingPatches
{
    #region 静态字段

    /// <summary>
    /// 配置文件实例
    /// </summary>
    private static ConfigFile ConfigFile;

    /// <summary>
    /// 敌方血量调整系数配置项
    /// </summary>
    private static ConfigEntry<float> EnemyHpMultiplierConfig;

    /// <summary>
    /// 敌方伤害调整系数配置项
    /// </summary>
    private static ConfigEntry<float> EnemyDamageMultiplierConfig;

    /// <summary>
    /// 奖励金币调整系数配置项
    /// </summary>
    private static ConfigEntry<float> RewardGoldMultiplierConfig;

    /// <summary>
    /// 奖励经验调整系数配置项
    /// </summary>
    private static ConfigEntry<float> RewardExpMultiplierConfig;

    #endregion

    #region 配置初始化

    /// <summary>
    /// 初始化配置文件
    /// 在插件启动时调用，创建所有必要的配置项
    /// </summary>
    /// <param name="configFile">BepInEx配置文件实例</param>
    public static void InitializeConfig(ConfigFile configFile)
    {
        if (configFile == null)
        {
            Plugin.Logger?.LogError("[SettingPatches] Config file is null");
            return;
        }

        ConfigFile = configFile;

        try
        {
            // 初始化难度平衡配置
            InitializeDifficultySettings();

            // 初始化奖励分配配置
            InitializeRewardSettings();

            Plugin.Logger?.LogInfo("[SettingPatches] Configuration initialized successfully");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[SettingPatches] Error initializing configuration: {ex.Message}");
        }
    }

    /// <summary>
    /// 初始化难度平衡设置
    /// </summary>
    private static void InitializeDifficultySettings()
    {
        // 敌方血量调整系数
        EnemyHpMultiplierConfig = ConfigFile.Bind(
            "Difficulty Balance",
            "Enemy HP Multiplier",
            1.0f,
            "敌方血量调整系数，用于平衡多人游戏难度\n" +
            "1.0 = 标准血量\n" +
            "1.5 = 1.5倍血量\n" +
            "2.0 = 2倍血量"
        );

        // 敌方伤害调整系数
        EnemyDamageMultiplierConfig = ConfigFile.Bind(
            "Difficulty Balance",
            "Enemy Damage Multiplier",
            1.0f,
            "敌方伤害调整系数，用于平衡多人游戏难度\n" +
            "1.0 = 标准伤害\n" +
            "1.2 = 1.2倍伤害\n" +
            "1.5 = 1.5倍伤害"
        );
    }

    /// <summary>
    /// 初始化奖励分配设置
    /// </summary>
    private static void InitializeRewardSettings()
    {
        // 奖励金币调整系数
        RewardGoldMultiplierConfig = ConfigFile.Bind(
            "Reward Distribution",
            "Gold Reward Multiplier",
            1.0f,
            "奖励金币调整系数，用于平衡多人游戏奖励\n" +
            "1.0 = 标准奖励\n" +
            "1.5 = 1.5倍奖励\n" +
            "2.0 = 2倍奖励"
        );

        // 奖励经验调整系数
        RewardExpMultiplierConfig = ConfigFile.Bind(
            "Reward Distribution",
            "Experience Reward Multiplier",
            1.0f,
            "奖励经验调整系数，用于平衡多人游戏奖励\n" +
            "1.0 = 标准经验\n" +
            "1.5 = 1.5倍经验\n" +
            "2.0 = 2倍经验"
        );
    }

    #endregion

    #region 属性访问器

    /// <summary>
    /// 获取敌方血量调整系数
    /// 根据玩家数量和配置设置计算最终的调整系数
    /// </summary>
    /// <param name="playerCount">当前玩家数量</param>
    /// <returns>血量调整系数</returns>
    public static float GetEnemyHpMultiplier(int playerCount = 1)
    {
        if (EnemyHpMultiplierConfig == null)
            return playerCount;

        return EnemyHpMultiplierConfig.Value * playerCount;
    }

    /// <summary>
    /// 获取敌方伤害调整系数
    /// </summary>
    /// <returns>伤害调整系数</returns>
    public static float GetEnemyDamageMultiplier()
    {
        return EnemyDamageMultiplierConfig?.Value ?? 1.0f;
    }

    /// <summary>
    /// 获取奖励金币调整系数
    /// </summary>
    /// <returns>金币奖励调整系数</returns>
    public static float GetGoldRewardMultiplier()
    {
        return RewardGoldMultiplierConfig?.Value ?? 1.0f;
    }

    /// <summary>
    /// 获取奖励经验调整系数
    /// </summary>
    /// <returns>经验奖励调整系数</returns>
    public static float GetExpRewardMultiplier()
    {
        return RewardExpMultiplierConfig?.Value ?? 1.0f;
    }

    #endregion

    #region 配置管理

    /// <summary>
    /// 重新加载配置设置
    /// </summary>
    public static void ReloadConfig()
    {
        try
        {
            ConfigFile?.Reload();
            Plugin.Logger?.LogInfo("[SettingPatches] Configuration reloaded");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[SettingPatches] Error reloading configuration: {ex.Message}");
        }
    }

    /// <summary>
    /// 重置所有设置为默认值
    /// </summary>
    public static void ResetToDefaults()
    {
        try
        {
            if (EnemyHpMultiplierConfig != null)
                EnemyHpMultiplierConfig.Value = 1.0f;

            if (EnemyDamageMultiplierConfig != null)
                EnemyDamageMultiplierConfig.Value = 1.0f;

            if (RewardGoldMultiplierConfig != null)
                RewardGoldMultiplierConfig.Value = 1.0f;

            if (RewardExpMultiplierConfig != null)
                RewardExpMultiplierConfig.Value = 1.0f;

            Plugin.Logger?.LogInfo("[SettingPatches] Configuration reset to defaults");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[SettingPatches] Error resetting configuration: {ex.Message}");
        }
    }

    #endregion
}