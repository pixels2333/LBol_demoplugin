using BepInEx.Configuration;

namespace NetworkPlugin.Configuration;

public partial class ConfigManager
{
        #region 功能开关

        public ConfigEntry<bool> EnableGapStationExtensions { get; private set; }

        public ConfigEntry<bool> AllowTrading { get; private set; }

        public ConfigEntry<bool> AllowRevival { get; private set; }

        public ConfigEntry<bool> DebugFakePlayersForTrade { get; private set; }

        public ConfigEntry<bool> DebugVirtualPlayerAiDefault { get; private set; }

        public ConfigEntry<bool> DebugOtherPlayersOverlay { get; private set; }

    #endregion

        private void BindFeatureToggles(ConfigFile configFile)
    {

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

        DebugFakePlayersForTrade = configFile.Bind(
            "General.Toggles",
            "DebugFakePlayersForTrade",
            false,
            "调试：为交易面板注入虚拟玩家（无需真实联机），用于测试交易 UI 的伙伴选择列表"
        );

        DebugVirtualPlayerAiDefault = configFile.Bind(
            "General.Toggles",
            "DebugVirtualPlayerAiDefault",
            false,
            "调试：生成虚拟远程玩家(aidefault)，用于离线测试远程玩家渲染与交易面板"
        );

        DebugOtherPlayersOverlay = configFile.Bind(
            "General.Toggles",
            "DebugOtherPlayersOverlay",
            false,
            "调试：开启 OtherPlayersOverlay 的高频诊断日志（默认关闭，避免日志刷屏）"
        );
    }
}
