using System;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Network.Snapshot;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.UI;

public static partial class OtherPlayersOverlayPatch
{
    private struct RemoteBattleState
    {
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Shield { get; set; }
        public int Block { get; set; }
        public int CurrentPower { get; set; }
        public int PowerPerLevel { get; set; }
        public int MaxPowerLevel { get; set; }
        public bool HasFreshBattleState { get; set; }
    }

    private static bool TryGetRemoteBattleState(PlayerSummary playerSummary, out RemoteBattleState state)
    {
        state = default;
        if (playerSummary == null || string.IsNullOrWhiteSpace(playerSummary.PlayerId))
        {
            return false;
        }

        if (TryGetVirtualAiDebugBattleState(playerSummary.PlayerId, out state))
        {
            return true;
        }

        INetworkPlayer networkPlayer = TryGetRemoteBattleStatePlayer(playerSummary.PlayerId);
        PlayerStateSnapshot snapshot = TryGetLatestRemoteBattleSnapshot(playerSummary.PlayerId);

        bool hasSnapshot = snapshot != null;
        bool hasRuntime = networkPlayer != null &&
            (networkPlayer.maxHP > 0 || networkPlayer.HP > 0 || networkPlayer.block > 0 || networkPlayer.shield > 0 || networkPlayer.GetPowerPerLevelSafe() > 0);

        if (!hasSnapshot && !hasRuntime)
        {
            return false;
        }

        int health = hasSnapshot ? snapshot.Health : networkPlayer.HP;
        int maxHealth = hasSnapshot ? snapshot.MaxHealth : networkPlayer.maxHP;
        int shield = hasSnapshot ? snapshot.Shield : networkPlayer.shield;
        int block = hasSnapshot ? snapshot.Block : networkPlayer.block;
        int currentPower = hasSnapshot ? snapshot.CurrentPower : networkPlayer.GetCurrentPowerSafe();
        int powerPerLevel = hasSnapshot ? snapshot.PowerPerLevel : networkPlayer.GetPowerPerLevelSafe();
        int maxPowerLevel = hasSnapshot ? snapshot.MaxPowerLevel : networkPlayer.GetMaxPowerLevelSafe();

        if (maxHealth <= 0 && health > 0)
        {
            maxHealth = health;
        }

        if (maxPowerLevel <= 0 && powerPerLevel > 0)
        {
            maxPowerLevel = 3;
        }

        state = new RemoteBattleState
        {
            Health = Math.Max(0, health),
            MaxHealth = Math.Max(0, maxHealth),
            Shield = Math.Max(0, shield),
            Block = Math.Max(0, block),
            CurrentPower = Math.Max(0, currentPower),
            PowerPerLevel = Math.Max(0, powerPerLevel),
            MaxPowerLevel = Math.Max(0, maxPowerLevel),
            HasFreshBattleState = hasSnapshot,
        };

        return true;
    }

    private static bool TryGetVirtualAiDebugBattleState(string playerId, out RemoteBattleState state)
    {
        state = default;

        if (!IsVirtualAiDefaultEnabled())
        {
            return false;
        }

        if (!TryGetVirtualAiDebugPlayerPreset(playerId, out var preset))
        {
            return false;
        }

        state = new RemoteBattleState
        {
            Health = preset.Health,
            MaxHealth = preset.MaxHealth,
            Shield = preset.Shield,
            Block = preset.Block,
            CurrentPower = preset.CurrentPower,
            PowerPerLevel = preset.PowerPerLevel,
            MaxPowerLevel = preset.MaxPowerLevel,
            HasFreshBattleState = true,
        };

        return true;
    }

    private static INetworkPlayer TryGetRemoteBattleStatePlayer(string playerId)
    {
        try
        {
            return ServiceProvider?.GetService<INetworkManager>()?.GetPlayer(playerId);
        }
        catch
        {
            return null;
        }
    }

    private static PlayerStateSnapshot TryGetLatestRemoteBattleSnapshot(string playerId)
    {
        PlayerStateSnapshot latest = null;

        if (TurnStartSnapshotReceivePatch.TryGetLastTurnStart(playerId, out TurnStartStateSnapshot turnStart) && turnStart?.playerStateSnapshot != null)
        {
            latest = turnStart.playerStateSnapshot;
        }

        if (TurnEndSnapshotReceivePatch.TryGetLastTurnEnd(playerId, out TurnEndStateSnapshot turnEnd) && turnEnd?.playerStateSnapshot != null)
        {
            if (latest == null || turnEnd.playerStateSnapshot.Timestamp >= latest.Timestamp)
            {
                latest = turnEnd.playerStateSnapshot;
            }
        }

        return latest;
    }
}