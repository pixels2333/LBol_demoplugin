using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.Presentation;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch;

[HarmonyPatch]
public class BattleController_Patch
{
    #region 依赖注入与客户端

    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

        private static INetworkClient TryGetNetworkClient()
        => ServiceProvider?.GetService<INetworkClient>();

    private static bool IsBattleSyncEnabled()
    {
        try
        {
            return Plugin.ConfigManager?.EnableBattleSync?.Value == true;
        }
        catch
        {
            return true;
        }
    }

    private static bool IsStatusEffectSyncEnabled()
    {
        try
        {
            return Plugin.ConfigManager?.EnableStatusEffectSync?.Value == true;
        }
        catch
        {
            return true;
        }
    }

    private static string ResolveSelfPlayerId()
    {

        try
        {

            string overrideId = Plugin.ConfigManager?.PlayerIdOverride?.Value;
            if (!string.IsNullOrWhiteSpace(overrideId))
            {
                return overrideId;
            }
        }
        catch
        {

        }

        string name = ResolveSelfPlayerName();
        string ip = ResolveSelfIpAddress();

        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(ip))
        {

            return $"{name}@{ip}";
        }

        string serverAssigned = NetworkIdentityTracker.GetSelfPlayerId();
        if (!string.IsNullOrWhiteSpace(serverAssigned))
        {
            return serverAssigned;
        }

        return GameStateUtils.GetCurrentPlayerId();
    }

    private static string ResolveSelfPlayerName()
    {

        try
        {
            string profileName = Singleton<GameMaster>.Instance?.CurrentProfile?.Name;
            if (!string.IsNullOrWhiteSpace(profileName))
            {
                return profileName;
            }
        }
        catch
        {

        }

        try
        {
            object player = GameStateUtils.GetCurrentPlayer();
            if (player != null)
            {
                var prop = player.GetType().GetProperty("userName")
                           ?? player.GetType().GetProperty("UserName")
                           ?? player.GetType().GetProperty("Name");
                if (prop != null && prop.PropertyType == typeof(string))
                {
                    string name = prop.GetValue(player) as string;
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        return name;
                    }
                }
            }
        }
        catch
        {

        }

        string id = NetworkIdentityTracker.GetSelfPlayerId();
        if (!string.IsNullOrWhiteSpace(id))
        {
            return id;
        }

        return "Unknown";
    }

    private static readonly object _ipLock = new();
    private static string _cachedSelfIp;

    private static string ResolveSelfIpAddress()
    {
        lock (_ipLock)
        {
            if (!string.IsNullOrWhiteSpace(_cachedSelfIp))
            {
                return _cachedSelfIp;
            }
        }

        try
        {
            INetworkClient client = TryGetNetworkClient();
            IPAddress addr = client?.LocalEndPoint?.Address;
            if (addr != null &&
                addr.AddressFamily == AddressFamily.InterNetwork &&
                !IPAddress.Any.Equals(addr) &&
                !IPAddress.Loopback.Equals(addr))
            {
                string ip = addr.ToString();
                lock (_ipLock)
                {
                    _cachedSelfIp = ip;
                }
                return ip;
            }
        }
        catch
        {

        }

        try
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni == null)
                {
                    continue;
                }

                if (ni.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                {
                    continue;
                }

                IPInterfaceProperties props;
                try
                {
                    props = ni.GetIPProperties();
                }
                catch
                {
                    continue;
                }

                if (props?.UnicastAddresses == null)
                {
                    continue;
                }

                foreach (UnicastIPAddressInformation uni in props.UnicastAddresses)
                {
                    IPAddress a = uni?.Address;
                    if (a == null || a.AddressFamily != AddressFamily.InterNetwork)
                    {
                        continue;
                    }

                    if (IPAddress.Loopback.Equals(a) || IPAddress.Any.Equals(a))
                    {
                        continue;
                    }

                    byte[] bytes = a.GetAddressBytes();
                    if (bytes.Length == 4 && bytes[0] == 169 && bytes[1] == 254)
                    {
                        continue;
                    }

                    string ip = a.ToString();
                    lock (_ipLock)
                    {
                        _cachedSelfIp = ip;
                    }
                    return ip;
                }
            }
        }
        catch
        {

        }

        return "0.0.0.0";
    }

    private static bool TryPrepareClient(out INetworkClient client, out bool isHost, out string selfPlayerId)
    {
        client = null;
        isHost = false;
        selfPlayerId = null;

        if (!IsBattleSyncEnabled())
        {
            return false;
        }

        client = TryGetNetworkClient();
        if (client == null || !client.IsConnected)
        {
            return false;
        }

        NetworkIdentityTracker.EnsureSubscribed(client);
        isHost = NetworkIdentityTracker.GetSelfIsHost();
        selfPlayerId = ResolveSelfPlayerId();
        return true;
    }

    private static void SendBattleEvent(INetworkClient client, string eventType, object payload)
    {

        client.SendGameEventData(eventType, payload);
    }

    #endregion

    #region 防回环

        public static bool PausePlayerBattleSync { get; set; }

    #endregion

    #region 伤害同步

        [HarmonyPatch(typeof(BattleController), "Damage")]
    [HarmonyPostfix]
    public static void Damage_Postfix(
        BattleController __instance,
        Unit source,
        Unit target,
        DamageInfo info,
        GameEntity actionSource,
        DamageInfo __result)
    {
        try
        {
            if (PausePlayerBattleSync)
            {
                return;
            }

            if (target == null)
            {
                return;
            }

            if (!TryPrepareClient(out INetworkClient client, out bool isHost, out string selfPlayerId))
            {
                return;
            }

            if (target is not PlayerUnit playerTarget)
            {
                return;
            }

            if (__instance.Player == null || __instance.Player != playerTarget)
            {
                return;
            }

            string playerName = ResolveSelfPlayerName();
            string playerIp = ResolveSelfIpAddress();

            string eventType = isHost
                ? NetworkMessageTypes.BattlePlayerDamageBroadcast
                : NetworkMessageTypes.BattlePlayerDamageReport;

            var payload = new
            {
                Timestamp = DateTime.Now.Ticks,
                PlayerId = selfPlayerId,
                PlayerName = playerName,
                PlayerIp = playerIp,
                IsHost = isHost,
                Round = __instance.RoundCounter,
                SourceId = source?.Id,
                TargetId = target.Id,
                ActionSource = actionSource?.GetType().Name,
                Damage = new
                {
                    TotalDamage = __result.Amount,
                    HpDamage = __result.Damage,
                    BlockedDamage = __result.DamageBlocked,
                    ShieldedDamage = __result.DamageShielded,
                    __result.DamageType,
                    __result.IsGrazed,
                    __result.IsAccuracy,
                    __result.OverDamage,
                },
                TargetState = new
                {
                    playerTarget.Hp,
                    playerTarget.MaxHp,
                    playerTarget.Block,
                    playerTarget.Shield,
                    Status = playerTarget.Status.ToString(),
                    playerTarget.IsAlive,
                },
            };

            SendBattleEvent(client, eventType, payload);

            Plugin.Logger?.LogInfo(
                $"[BattlePlayerDamage] {eventType} Total={__result.Amount:F1} (HP: {__result.Damage:F1}, " +
                $"Block: {__result.DamageBlocked:F1}, Shield: {__result.DamageShielded:F1}). " +
                $"Remaining HP: {playerTarget.Hp}/{playerTarget.MaxHp}");
        }
        catch (Exception ex)
        {

            Plugin.Logger?.LogError($"[BattlePlayerDamage] Error: {ex.Message}\n{ex.StackTrace}");
        }
    }

    #endregion

    #region 状态效果同步

    private const int StatusEffectsFullSyncEveryNChanges = 10;
    private static readonly Dictionary<string, HashSet<string>> _cachedStatusEffects = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, int> _statusEffectChangeCounters = new(StringComparer.Ordinal);

    private static HashSet<string> SnapshotStatusEffects(Unit unit)
    {
        if (unit?.StatusEffects == null)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        try
        {
            return unit.StatusEffects
                .Where(se => se != null)
                .Select(se =>
                {
                    int level = se.HasLevel ? se.Level : (se.HasCount ? se.Count : 0);
                    int duration = se.HasDuration ? se.Duration : 0;
                    return $"{se.GetType().Name}:{level}:{duration}";
                })
                .ToHashSet(StringComparer.Ordinal);
        }
        catch
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }
    }

    private static void SyncStatusEffectsIfNeeded(BattleController battle, Unit target, StatusEffect effect, bool success, string reason)
    {
        if (!success)
        {
            return;
        }

        if (!IsStatusEffectSyncEnabled())
        {
            return;
        }

        if (PausePlayerBattleSync)
        {
            return;
        }

        if (battle == null || target == null)
        {
            return;
        }

        if (target is not PlayerUnit playerTarget)
        {
            return;
        }

        if (battle.Player == null || battle.Player != playerTarget)
        {
            return;
        }

        if (!TryPrepareClient(out INetworkClient client, out bool isHost, out string selfPlayerId))
        {
            return;
        }

        string playerName = ResolveSelfPlayerName();
        string playerIp = ResolveSelfIpAddress();

        string targetId = target.Id;
        HashSet<string> current = SnapshotStatusEffects(target);

        if (!_cachedStatusEffects.TryGetValue(targetId, out HashSet<string> previous))
        {
            previous = new HashSet<string>(StringComparer.Ordinal);
        }

        List<string> added = current.Except(previous).ToList();
        List<string> removed = previous.Except(current).ToList();

        bool hasDelta = added.Count > 0 || removed.Count > 0;
        bool shouldFullSync;

        if (!_statusEffectChangeCounters.TryGetValue(targetId, out int counter))
        {
            counter = 0;
        }

        if (hasDelta)
        {
            counter++;
            _statusEffectChangeCounters[targetId] = counter;
        }

        shouldFullSync = !_cachedStatusEffects.ContainsKey(targetId) || (counter > 0 && counter % StatusEffectsFullSyncEveryNChanges == 0);

        _cachedStatusEffects[targetId] = current;

        if (hasDelta)
        {
            string deltaEventType = isHost
                ? NetworkMessageTypes.BattlePlayerStatusEffectsDeltaBroadcast
                : NetworkMessageTypes.BattlePlayerStatusEffectsDeltaReport;

            var deltaPayload = new
            {
                Timestamp = DateTime.Now.Ticks,
                PlayerId = selfPlayerId,
                PlayerName = playerName,
                PlayerIp = playerIp,
                IsHost = isHost,
                Round = battle.RoundCounter,
                TargetId = targetId,
                Reason = reason,
                Effect = effect == null
                    ? null
                    : new
                    {
                        effect.DebugName,
                        Type = effect.GetType().Name,
                        HasLevel = SafeGetStatusEffectHasLevel(effect),
                        Level = SafeGetStatusEffectLevelOrNull(effect),
                    },
                Added = added,
                Removed = removed,
                ChangeCounter = counter,
            };

            SendBattleEvent(client, deltaEventType, deltaPayload);
        }

        if (shouldFullSync)
        {
            string fullEventType = isHost
                ? NetworkMessageTypes.BattlePlayerStatusEffectsFullBroadcast
                : NetworkMessageTypes.BattlePlayerStatusEffectsFullReport;

            var fullPayload = new
            {
                Timestamp = DateTime.Now.Ticks,
                PlayerId = selfPlayerId,
                PlayerName = playerName,
                PlayerIp = playerIp,
                IsHost = isHost,
                Round = battle.RoundCounter,
                TargetId = targetId,
                Reason = hasDelta ? $"{reason}_PeriodicFull" : $"{reason}_InitialFull",
                StatusEffects = current.OrderBy(s => s, StringComparer.Ordinal).ToList(),
                ChangeCounter = counter,
            };

            SendBattleEvent(client, fullEventType, fullPayload);
        }
    }

    private static bool SafeGetStatusEffectHasLevel(StatusEffect effect)
    {
        try
        {
            return effect != null && effect.HasLevel;
        }
        catch
        {
            return false;
        }
    }

    private static int? SafeGetStatusEffectLevelOrNull(StatusEffect effect)
    {
        try
        {
            if (effect == null || !effect.HasLevel)
            {
                return null;
            }

            return effect.Level;
        }
        catch
        {
            return null;
        }
    }

        [HarmonyPatch(typeof(BattleController), "TryAddStatusEffect")]
    [HarmonyPostfix]
    public static void TryAddStatusEffect_Postfix(
        BattleController __instance,
        Unit target,
        StatusEffect effect,
        StatusEffectAddResult? __result)
    {
        try
        {
            bool success = __result != null;

            SyncStatusEffectsIfNeeded(__instance, target, effect, success, "TryAddStatusEffect");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[BattlePlayerStatusEffects] Error in TryAddStatusEffect_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }

        [HarmonyPatch(typeof(BattleController), "RemoveStatusEffect")]
    [HarmonyPostfix]
    public static void RemoveStatusEffect_Postfix(
        BattleController __instance,
        Unit target,
        StatusEffect effect,
        bool __result)
    {
        try
        {
            SyncStatusEffectsIfNeeded(__instance, target, effect, __result, "RemoveStatusEffect");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[BattlePlayerStatusEffects] Error in RemoveStatusEffect_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }

    [HarmonyPatch(typeof(StatusEffect), "Level", MethodType.Setter)]
    [HarmonyPatch(typeof(StatusEffect), "Count", MethodType.Setter)]
    [HarmonyPatch(typeof(StatusEffect), "Duration", MethodType.Setter)]
    internal static class StatusEffect_PropertyChanged_Sync
    {
        [HarmonyPostfix]
        public static void Postfix(StatusEffect __instance)
        {
            try
            {
                if (__instance?.Owner == null || __instance.Owner.Battle == null)
                {
                    return;
                }

                if (__instance.Owner is EnemyUnit enemy)
                {
                    EnemySyncPatch.SyncEnemyStatusEffectChanged(enemy);
                }
                else if (__instance.Owner is PlayerUnit player)
                {
                    SyncStatusEffectsIfNeeded(player.Battle, player, __instance, true, "PropertyChanged");
                }
            }
            catch
            {
            }
        }
    }

    #endregion

    #region 治疗同步

    [HarmonyPatch(typeof(BattleController), "Heal")]
    [HarmonyPostfix]
    public static void Heal_Postfix(BattleController __instance, Unit target, int healValue, int __result)
    {
        try
        {
            if (PausePlayerBattleSync)
            {
                return;
            }

            if (target == null)
            {
                return;
            }

            if (!TryPrepareClient(out INetworkClient client, out bool isHost, out string selfPlayerId))
            {
                return;
            }

            if (target is not PlayerUnit playerTarget)
            {
                return;
            }

            if (__instance.Player == null || __instance.Player != playerTarget)
            {
                return;
            }

            string playerName = ResolveSelfPlayerName();
            string playerIp = ResolveSelfIpAddress();

            string eventType = isHost
                ? NetworkMessageTypes.BattlePlayerHealBroadcast
                : NetworkMessageTypes.BattlePlayerHealReport;

            var payload = new
            {
                Timestamp = DateTime.Now.Ticks,
                PlayerId = selfPlayerId,
                PlayerName = playerName,
                PlayerIp = playerIp,
                IsHost = isHost,
                Round = __instance.RoundCounter,
                TargetId = target.Id,
                HealValue = healValue,
                ActualHeal = __result,
                TargetState = new
                {
                    playerTarget.Hp,
                    playerTarget.MaxHp,
                    playerTarget.Block,
                    playerTarget.Shield,
                    Status = playerTarget.Status.ToString(),
                    playerTarget.IsAlive,
                },
            };

            SendBattleEvent(client, eventType, payload);

            Plugin.Logger?.LogInfo(
                $"[BattlePlayerHeal] {eventType} Actual={__result} After HP {playerTarget.Hp}/{playerTarget.MaxHp} Block {playerTarget.Block} Shield {playerTarget.Shield}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[BattlePlayerHeal] Error: {ex.Message}\n{ex.StackTrace}");
        }
    }

    #endregion
}
