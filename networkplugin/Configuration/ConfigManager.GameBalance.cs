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

    /// <summary>
    /// 战斗结束自动复活生命值百分比配置项。
    /// </summary>
    public ConfigEntry<int> BattleAutoReviveHpPercent { get; private set; }

    #endregion

    /// <summary>
    /// 绑定游戏平衡配置
    /// </summary>
    private void BindGameBalanceSettings(ConfigFile configFile)
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

        // 绑定战斗结束自动复活设置。
        BattleAutoReviveHpPercent = configFile.Bind(
            "GameBalance.Battle",
            "BattleAutoReviveHpPercent",
            10,
            "联机战斗胜利后自动复活死亡玩家时恢复的最大生命值百分比\n" +
            "默认值 10 表示恢复 10% 最大生命值\n" +
            "有效范围为 1-100"
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
    /// 获取战斗结束自动复活生命值百分比。
    /// </summary>
    /// <returns>裁剪到 1-100 的百分比数值。</returns>
    public int GetBattleAutoReviveHpPercent()
    {
        int percent = BattleAutoReviveHpPercent?.Value ?? 10;
        if (percent < 1)
        {
            return 1;
        }

        return percent > 100 ? 100 : percent;
    }

    /// <summary>
    /// 根据最大生命值计算战斗结束自动复活后的生命值。
    /// </summary>
    /// <param name="maxHp">最大生命值。</param>
    /// <returns>自动复活后的目标生命值。</returns>
    public int CalculateBattleAutoReviveHp(int maxHp)
    {
        if (maxHp <= 0)
        {
            return 1;
        }

        return Math.Max(1, (int)Math.Ceiling(maxHp * (GetBattleAutoReviveHpPercent() / 100d)));
    }

    /// <summary>
    /// 重置游戏平衡设置为默认值
    /// </summary>
    public void ResetGameBalanceToDefaults()
    {
        try
        {
            if (EnemyHpMultiplier != null) EnemyHpMultiplier.Value = 1.0f;

            if (EnemyDamageMultiplier != null) EnemyDamageMultiplier.Value = 1.0f;

            if (RewardGoldMultiplier != null) RewardGoldMultiplier.Value = 1.0f;

            if (RewardExpMultiplier != null) RewardExpMultiplier.Value = 1.0f;

            if (BattleAutoReviveHpPercent != null) BattleAutoReviveHpPercent.Value = 10;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogWarning($"[ConfigManager] Error resetting game balance settings: {ex.Message}");
        }
    }
}
