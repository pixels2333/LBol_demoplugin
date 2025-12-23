using BepInEx;
using BepInEx.Configuration;
using System;

namespace NetworkPlugin.Configuration;

public partial class ConfigManager
{
    /// <summary>
    /// 游戏平衡配置区域 - 管理多人游戏模式下的各种配置设置
    /// 包括难度调整系数、奖励分配、游戏平衡参数等
    /// </summary>
    #region 游戏平衡配置

    /// <summary>
    /// 敌方血量调整系数配置项
    /// </summary>
    public ConfigEntry<float> EnemyHpMultiplier { get; private set; }

    /// <summary>
    /// 敌方伤害调整系数配置项
    /// </summary>
    public ConfigEntry<float> EnemyDamageMultiplier { get; private set; }

    /// <summary>
    /// 奖励金币调整系数配置项
    /// </summary>
    public ConfigEntry<float> RewardGoldMultiplier { get; private set; }

    /// <summary>
    /// 奖励经验调整系数配置项
    /// </summary>
    public ConfigEntry<float> RewardExpMultiplier { get; private set; }

    #endregion

    /// <summary>
    /// 绑定游戏平衡配置
    /// </summary>
    private void BindGameBalanceSettings(ConfigFile configFile)
    {
        // 绑定难度平衡配置
        BindDifficultyBalanceSettings(configFile);

        // 绑定奖励分配配置
        BindRewardDistributionSettings(configFile);
    }

    /// <summary>
    /// 绑定难度平衡设置
    /// </summary>
    private void BindDifficultyBalanceSettings(ConfigFile configFile)
    {
        // 敌方血量调整系数
        EnemyHpMultiplier = configFile.Bind(
            "GameBalance.Difficulty",
            "EnemyHpMultiplier",
            1.0f,
            "敌方血量调整系数，用于平衡多人游戏难度\n" +
            "1.0 = 标准血量\n" +
            "1.5 = 1.5倍血量\n" +
            "2.0 = 2倍血量"
        );

        // 敌方伤害调整系数
        EnemyDamageMultiplier = configFile.Bind(
            "GameBalance.Difficulty",
            "EnemyDamageMultiplier",
            1.0f,
            "敌方伤害调整系数，用于平衡多人游戏难度\n" +
            "1.0 = 标准伤害\n" +
            "1.2 = 1.2倍伤害\n" +
            "1.5 = 1.5倍伤害"
        );
    }

    /// <summary>
    /// 绑定奖励分配设置
    /// </summary>
    private void BindRewardDistributionSettings(ConfigFile configFile)
    {
        // 奖励金币调整系数
        RewardGoldMultiplier = configFile.Bind(
            "GameBalance.Rewards",
            "RewardGoldMultiplier",
            1.0f,
            "奖励金币调整系数，用于平衡多人游戏奖励\n" +
            "1.0 = 标准奖励\n" +
            "1.5 = 1.5倍奖励\n" +
            "2.0 = 2倍奖励"
        );

        // 奖励经验调整系数
        RewardExpMultiplier = configFile.Bind(
            "GameBalance.Rewards",
            "RewardExpMultiplier",
            1.0f,
            "奖励经验调整系数，用于平衡多人游戏奖励\n" +
            "1.0 = 标准经验\n" +
            "1.5 = 1.5倍经验\n" +
            "2.0 = 2倍经验"
        );
    }

    /// <summary>
    /// 获取敌方血量调整系数
    /// 根据玩家数量和配置设置计算最终的调整系数
    /// </summary>
    /// <param name="playerCount">当前玩家数量</param>
    /// <returns>血量调整系数</returns>
    public float GetEnemyHpMultiplier(int playerCount = 1)
    {
        if (EnemyHpMultiplier == null)
            return playerCount;

        return EnemyHpMultiplier.Value * playerCount;
    }

    /// <summary>
    /// 获取敌方伤害调整系数
    /// </summary>
    /// <returns>伤害调整系数</returns>
    public float GetEnemyDamageMultiplier()
    {
        return EnemyDamageMultiplier?.Value ?? 1.0f;
    }

    /// <summary>
    /// 获取奖励金币调整系数
    /// </summary>
    /// <returns>金币奖励调整系数</returns>
    public float GetGoldRewardMultiplier()
    {
        return RewardGoldMultiplier?.Value ?? 1.0f;
    }

    /// <summary>
    /// 获取奖励经验调整系数
    /// </summary>
    /// <returns>经验奖励调整系数</returns>
    public float GetExpRewardMultiplier()
    {
        return RewardExpMultiplier?.Value ?? 1.0f;
    }

    /// <summary>
    /// 重置游戏平衡设置为默认值
    /// </summary>
    public void ResetGameBalanceToDefaults()
    {
        try
        {
            if (EnemyHpMultiplier != null)
                EnemyHpMultiplier.Value = 1.0f;

            if (EnemyDamageMultiplier != null)
                EnemyDamageMultiplier.Value = 1.0f;

            if (RewardGoldMultiplier != null)
                RewardGoldMultiplier.Value = 1.0f;

            if (RewardExpMultiplier != null)
                RewardExpMultiplier.Value = 1.0f;
        }
        catch (Exception ex)
        {
            // 这里可以考虑使用日志记录错误
            System.Console.WriteLine($"[ConfigManager] Error resetting game balance settings: {ex.Message}");
        }
    }
}