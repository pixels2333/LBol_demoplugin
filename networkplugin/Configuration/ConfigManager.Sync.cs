using BepInEx.Configuration;
using System;

namespace NetworkPlugin.Configuration;

public partial class ConfigManager
{
        #region 同步配置

        public ConfigEntry<bool> EnableCardSync { get; private set; }

        public ConfigEntry<bool> EnableManaSync { get; private set; }

        public ConfigEntry<bool> EnableBattleSync { get; private set; }

        public ConfigEntry<bool> EnableMapSync { get; private set; }

        public ConfigEntry<bool> EnableStatusEffectSync { get; private set; }

        public ConfigEntry<int> MaxQueueSize { get; private set; }

        public ConfigEntry<float> StateCacheExpiryMinutes { get; private set; }

        public ConfigEntry<bool> EnableSaveLoadSync { get; private set; }

        public ConfigEntry<bool> EnableNatDetection { get; private set; }

        public ConfigEntry<bool> EnableUpnpExperimental { get; private set; }

    #endregion

        private void BindSyncConfiguration(ConfigFile configFile)
    {

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
            "(Deprecated) 旧版存档同步已废弃：联机不再传输存档 bytes。仅保留本地 Restore + FullSnapshot 追赶。"
        );

        EnableNatDetection = configFile.Bind(
            "Sync.Toggles",
            "EnableNatDetection",
            true,
            "是否启用 NAT 类型检测（STUN）。关闭后不做 NAT 探测。"
        );

        EnableUpnpExperimental = configFile.Bind(
            "Sync.Toggles",
            "EnableUpnpExperimental",
            false,
            "UPnP 实验开关：当前仅用于状态标注，不执行真实端口映射。"
        );

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

        public SyncConfiguration GetSyncConfiguration()
    {
        return new SyncConfiguration
        {
            EnableCardSync = EnableCardSync?.Value ?? true,
            EnableManaSync = EnableManaSync?.Value ?? true,
            EnableBattleSync = EnableBattleSync?.Value ?? true,
            EnableMapSync = EnableMapSync?.Value ?? true,
            EnableStatusEffectSync = EnableStatusEffectSync?.Value ?? true,
            EnableNatDetection = EnableNatDetection?.Value ?? true,
            EnableUpnpExperimental = EnableUpnpExperimental?.Value ?? false,

            MaxQueueSize = MaxQueueSize?.Value ?? 100,
            StateCacheExpiry = TimeSpan.FromMinutes(StateCacheExpiryMinutes?.Value ?? 5.0f)
        };
    }
}
