using BepInEx.Configuration;
using System;

namespace NetworkPlugin.Configuration;

public partial class ConfigManager
{
        #region 游戏平衡配置

        public ConfigEntry<float> EnemyHpMultiplier { get; private set; }

        public ConfigEntry<float> EnemyDamageMultiplier { get; private set; }

        public ConfigEntry<float> RewardGoldMultiplier { get; private set; }

        public ConfigEntry<float> RewardExpMultiplier { get; private set; }

        public ConfigEntry<int> BattleAutoReviveHpPercent { get; private set; }

    #endregion

        private void BindGameBalanceSettings(ConfigFile configFile)
    {

        EnemyHpMultiplier = configFile.Bind(
            "GameBalance.Difficulty",
            "EnemyHpMultiplier",
            1.0f,
            "敌方血量调整系数，用于平衡多人游戏难度\n" +
            "1.0 = 标准血量\n" +
            "1.5 = 1.5倍血量\n" +
            "2.0 = 2倍血量"
        );

        EnemyDamageMultiplier = configFile.Bind(
            "GameBalance.Difficulty",
            "EnemyDamageMultiplier",
            1.0f,
            "敌方伤害调整系数，用于平衡多人游戏难度\n" +
            "1.0 = 标准伤害\n" +
            "1.2 = 1.2倍伤害\n" +
            "1.5 = 1.5倍伤害"
        );

        RewardGoldMultiplier = configFile.Bind(
            "GameBalance.Rewards",
            "RewardGoldMultiplier",
            1.0f,
            "奖励金币调整系数，用于平衡多人游戏奖励\n" +
            "1.0 = 标准奖励\n" +
            "1.5 = 1.5倍奖励\n" +
            "2.0 = 2倍奖励"
        );

        RewardExpMultiplier = configFile.Bind(
            "GameBalance.Rewards",
            "RewardExpMultiplier",
            1.0f,
            "奖励经验调整系数，用于平衡多人游戏奖励\n" +
            "1.0 = 标准经验\n" +
            "1.5 = 1.5倍经验\n" +
            "2.0 = 2倍经验"
        );

        BattleAutoReviveHpPercent = configFile.Bind(
            "GameBalance.Battle",
            "BattleAutoReviveHpPercent",
            10,
            "联机战斗胜利后自动复活死亡玩家时恢复的最大生命值百分比\n" +
            "默认值 10 表示恢复 10% 最大生命值\n" +
            "有效范围为 1-100"
        );
    }

        public float GetEnemyHpMultiplier(int playerCount = 1)
    {
        if (EnemyHpMultiplier == null)
            return playerCount;

        return EnemyHpMultiplier.Value * playerCount;
    }

        public float GetEnemyDamageMultiplier()
    {
        return EnemyDamageMultiplier?.Value ?? 1.0f;
    }

        public float GetGoldRewardMultiplier()
    {
        return RewardGoldMultiplier?.Value ?? 1.0f;
    }

        public float GetExpRewardMultiplier()
    {
        return RewardExpMultiplier?.Value ?? 1.0f;
    }

        public int GetBattleAutoReviveHpPercent()
    {
        int percent = BattleAutoReviveHpPercent?.Value ?? 10;
        if (percent < 1)
        {
            return 1;
        }

        return percent > 100 ? 100 : percent;
    }

        public int CalculateBattleAutoReviveHp(int maxHp)
    {
        if (maxHp <= 0)
        {
            return 1;
        }

        return Math.Max(1, (int)Math.Ceiling(maxHp * (GetBattleAutoReviveHpPercent() / 100d)));
    }

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
