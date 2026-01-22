using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.Presentation;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch;

/// <summary>
/// 战斗控制器相关补丁。
/// 通过 Harmony 拦截 <see cref="BattleController"/> 的关键流程，并将本地玩家的战斗状态变化同步到联机层。
/// </summary>
/// <remarks>
/// 说明：这里的同步以“本地玩家”为中心，避免在同一房间中对其他玩家的状态进行重复广播造成冲突。
/// </remarks>
[HarmonyPatch]
public class BattleController_Patch
{
    #region 依赖注入与客户端

    /// <summary>
    /// 服务提供者（依赖注入入口），用于解析网络客户端等服务。
    /// </summary>
    // NOTE: 不要缓存 ServiceProvider 的实例引用：插件 Awake 之前可能为 null，且可被重建。
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 网络客户端（通过依赖注入获取）。
    /// </summary>
    private static INetworkClient TryGetNetworkClient()
    {
        try
        {
            return ServiceProvider?.GetService<INetworkClient>();
        }
        catch
        {
            return null;
        }
    }

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
        // PlayerId 规则(按用户最新确认):
        // - UserName: 原游戏存档里填写的玩家名(ProfileSaveData.Name)
        // - IP: 本机 IPv4
        // - PlayerId = UserName + IP 的字符串拼接(为减少歧义，这里使用分隔符拼接)
        // NOTE: 旧的 NetworkIdentityTracker.GetSelfPlayerId() 是“服务器下发的唯一标识”，仍可用于调试/对照。
        try
        {
            // 可选调试覆盖：直接指定 PlayerId。
            string overrideId = Plugin.ConfigManager?.PlayerIdOverride?.Value;
            if (!string.IsNullOrWhiteSpace(overrideId))
            {
                return overrideId;
            }
        }
        catch
        {
            // ignored
        }

        string name = ResolveSelfPlayerName();
        string ip = ResolveSelfIpAddress();

        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(ip))
        {
            // 分隔符拼接仍属于“字符串拼接”；服务端如需更严格格式，可统一改为无分隔符。
            return $"{name}@{ip}";
        }

        // 兜底：保持旧逻辑可用(用于早期阶段 CurrentProfile 为空，或 IP 无法取到)。
        string serverAssigned = NetworkIdentityTracker.GetSelfPlayerId();
        if (!string.IsNullOrWhiteSpace(serverAssigned))
        {
            return serverAssigned;
        }

        return GameStateUtils.GetCurrentPlayerId();
    }

    private static string ResolveSelfPlayerName()
    {
        // 优先从存档/档案读取玩家名：ProfileSaveData.Name。
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
            // ignored
        }

        // 兜底：从当前 PlayerUnit 反射尝试 userName/UserName/Name。
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
            // ignored
        }

        // 最后兜底：使用服务器下发的 id(可能不可读，但至少稳定)。
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

        // 1) 优先尝试从网络连接拿到的本地端点地址(如果实现返回的不是 IPAddress.Any)。
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
            // ignored
        }

        // 2) 枚举本机网卡，选取一个合理的 IPv4。
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

                // 跳过回环与隧道设备。
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

                    // 排除 APIPA 169.254.x.x
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
            // ignored
        }

        // 3) 最终兜底：无法获取本机局域网 IP 时返回 0.0.0.0。
        // NOTE: 按当前需求不使用 ServerIP 作为替代，避免把“服务器地址”误当作“本机地址”。
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

        // 确保 NetworkIdentityTracker 已订阅并能更新 host/self 信息。
        NetworkIdentityTracker.EnsureSubscribed(client);
        isHost = NetworkIdentityTracker.GetSelfIsHost();
        selfPlayerId = ResolveSelfPlayerId();
        return true;
    }

    private static void SendBattleEvent(INetworkClient client, string eventType, object payload)
    {
        // 统一走 GameEvent 通道；NetworkClient 内部会序列化 payload。
        client.SendGameEventData(eventType, payload);
    }

    #endregion

    #region 防回环

    /// <summary>
    /// 用于在应用远端同步/批量重放时临时关闭本地上报，避免回环。
    /// TODO: 远端落地补丁(接收端)在执行本地状态写入前后设置该开关。
    /// </summary>
    public static bool PausePlayerBattleSync { get; set; }

    #endregion

    #region 伤害同步

    /// <summary>
    /// 伤害应用完成后同步（仅同步本地玩家）。
    /// </summary>
    /// <param name="__instance">被补丁的 <see cref="BattleController"/> 实例（Harmony 注入）。</param>
    /// <param name="damageinfo">本次伤害信息（结构体）。</param>
    /// <param name="target">伤害目标单位。</param>
    // BattleController.Damage 的真实签名为:
    // internal DamageInfo Damage(Unit source, Unit target, DamageInfo info, GameEntity actionSource)
    // 这里用 __result 获取最终结算(含格挡/护盾等)后的 DamageInfo。
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

            if (__instance == null || target == null)
            {
                return;
            }

            if (!TryPrepareClient(out INetworkClient client, out bool isHost, out string selfPlayerId))
            {
                return;
            }

            // 只同步玩家单位的伤害（敌人伤害在其他补丁中处理）。
            if (target is not PlayerUnit playerTarget)
            {
                return;
            }

            // 只同步本地玩家：避免同步其他玩家的状态导致彼此覆盖。
            if (__instance.Player == null || __instance.Player != playerTarget)
            {
                return;
            }

            // Host 权威模型:
            // - Client: 上报 Report 给 Host/Server
            // - Host: 对本地变更广播 Broadcast 给房间
            // NOTE: Host 侧转发 *Report -> *Broadcast 已由 `networkplugin/Patch/Network/BattleReportForwardPatch.cs` 实现。
            string playerName = ResolveSelfPlayerName();
            string playerIp = ResolveSelfIpAddress();

            string eventType = isHost
                ? NetworkMessageTypes.BattlePlayerDamageBroadcast
                : NetworkMessageTypes.BattlePlayerDamageReport;

            // 构建“伤害 + 目标快照”的同步数据。
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

            // 记录日志，便于排查同步问题。
            Plugin.Logger?.LogInfo(
                $"[BattlePlayerDamage] {eventType} Total={__result.Amount:F1} (HP: {__result.Damage:F1}, " +
                $"Block: {__result.DamageBlocked:F1}, Shield: {__result.DamageShielded:F1}). " +
                $"Remaining HP: {playerTarget.Hp}/{playerTarget.MaxHp}");
        }
        catch (Exception ex)
        {
            // 捕获异常：防止补丁异常影响游戏主流程。
            Plugin.Logger?.LogError($"[BattlePlayerDamage] Error: {ex.Message}\n{ex.StackTrace}");
        }
    }

    #endregion

    #region 状态效果同步

    // 说明: Unit 已暴露 StatusEffects 只读列表，因此不需要 Traverse 访问私有字段。

    private const int StatusEffectsFullSyncEveryNChanges = 10;
    private static readonly Dictionary<string, HashSet<string>> _cachedStatusEffects = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, int> _statusEffectChangeCounters = new(StringComparer.Ordinal);

    private static HashSet<string> SnapshotStatusEffects(Unit unit)
    {
        if (unit == null)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        try
        {
            // 用 DebugName+TypeName 组合降低冲突风险；DebugName 更适合诊断。
            return unit.StatusEffects
                .Where(se => se != null)
                .Select(se => $"{se.DebugName}|{se.GetType().Name}")
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

        // 只同步本地玩家。
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

        // 首次看到该单位时，直接发一次全量以建立基线。
        shouldFullSync = !_cachedStatusEffects.ContainsKey(targetId) || (counter > 0 && counter % StatusEffectsFullSyncEveryNChanges == 0);

        // 更新缓存。
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
                        effect.Level,
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

    /// <summary>
    /// 添加状态效果完成后，同步目标单位的完整状态列表。
    /// </summary>
    /// <param name="__instance">被补丁的 <see cref="BattleController"/> 实例（Harmony 注入）。</param>
    /// <param name="target">被添加状态效果的单位。</param>
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

            // TODO: 若要更严格，可只在 Added/Stacked/Neutralized 等特定结果时同步。
            SyncStatusEffectsIfNeeded(__instance, target, effect, success, "TryAddStatusEffect");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[BattlePlayerStatusEffects] Error in TryAddStatusEffect_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 移除状态效果完成后，同步目标单位的完整状态列表。
    /// </summary>
    /// <param name="__instance">被补丁的 <see cref="BattleController"/> 实例（Harmony 注入）。</param>
    /// <param name="target">被移除状态效果的单位。</param>
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

    #endregion

    #region 治疗同步

    /// <summary>
    /// 治疗完成后同步目标单位的生命相关数据。
    /// </summary>
    /// <param name="__instance">被补丁的 <see cref="BattleController"/> 实例（Harmony 注入）。</param>
    /// <param name="target">接受治疗的目标单位。</param>
    // BattleController.Heal 的真实签名为:
    // internal int Heal(Unit target, int healValue)
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

            if (__instance == null || target == null)
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
