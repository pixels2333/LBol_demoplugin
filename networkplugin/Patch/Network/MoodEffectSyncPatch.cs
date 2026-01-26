using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Patch.UI;
using NetworkPlugin.Utils;
using UnityEngine;
using LBoL.Core;
using Newtonsoft.Json.Linq;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 心情特效同步补丁
/// 同步Koishi心情（基于UnitEffectName）的循环VFX到其他客户端。
/// 游戏通过UnitView.TryPlayEffectLoop/EndEffectLoop生成心情循环VFX，
/// 因此广播这些调用足以让远程观看者同步显示。
/// 
/// 同步机制：
/// 1. 本地玩家触发心情特效时，广播开始/结束事件
/// 2. 远程客户端收到事件后，在对应的远程玩家UnitView上播放/停止特效
/// 3. 定期广播当前心情状态，用于处理晚加入的玩家或重新创建的视图
/// 4. 使用抑制广播机制避免事件循环
/// </summary>
[HarmonyPatch]
public static class MoodEffectSyncPatch
{
    #region 字段和属性

    /// <summary>
    /// 服务提供者，用于获取网络客户端实例
    /// </summary>
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 心情特效名称列表
    /// </summary>
    private static readonly string[] MoodEffectNames = { "ChaowoLoop", "BenwoLoop", "DunwuLoop" };

    /// <summary>
    /// 当前订阅的网络客户端实例
    /// </summary>
    private static INetworkClient _subscribedClient;

    /// <summary>
    /// 是否已订阅网络客户端事件
    /// </summary>
    private static bool _subscribed;

    /// <summary>
    /// 游戏事件接收回调
    /// </summary>
    private static readonly Action<string, object> _onGameEventReceived = OnGameEventReceived;

    /// <summary>
    /// 连接状态变化回调
    /// </summary>
    private static readonly Action<bool> _onConnectionStateChanged = OnConnectionStateChanged;

    /// <summary>
    /// 广播抑制深度计数器
    /// </summary>
    private static int _suppressBroadcastDepth;

    /// <summary>
    /// 上一帧是否在战斗中
    /// </summary>
    private static bool _wasInBattle;

    /// <summary>
    /// 最后广播的特效名称
    /// </summary>
    private static string _lastBroadcastedEffectName;

    /// <summary>
    /// 按玩家ID缓存的待处理心情状态
    /// </summary>
    private static readonly Dictionary<string, string> _pendingMoodByPlayerId = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 最近一次成功解析的远端心情状态（按玩家ID）。
    /// 用于远端 UnitView 被销毁/重建后，仍可在本地重新应用心情循环。
    /// </summary>
    private static readonly Dictionary<string, string> _lastKnownMoodByPlayerId = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 已应用到远端视图上的最后一次心情状态（按玩家ID），用于避免重复调用 TryPlay/EndEffectLoop。
    /// </summary>
    private static readonly Dictionary<string, string> _lastAppliedMoodByPlayerId = new(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region 辅助类

    /// <summary>
    /// 广播抑制作用域，用于避免事件循环
    /// </summary>
    private sealed class SuppressBroadcastScope : IDisposable
    {
        /// <summary>
        /// 构造函数，增加抑制深度
        /// </summary>
        public SuppressBroadcastScope() => Interlocked.Increment(ref _suppressBroadcastDepth);
        
        /// <summary>
        /// 析构函数，减少抑制深度
        /// </summary>
        public void Dispose() => Interlocked.Decrement(ref _suppressBroadcastDepth);
    }

    #endregion

    #region 属性访问器

    /// <summary>
    /// 获取是否处于广播抑制状态
    /// </summary>
    private static bool IsSuppressed => Volatile.Read(ref _suppressBroadcastDepth) > 0;

    #endregion

    #region Harmony补丁

    /// <summary>
    /// GameDirector.Update的后置补丁，用于每帧更新同步状态
    /// </summary>
    [HarmonyPatch(typeof(GameDirector), "Update")]
    [HarmonyPostfix]
    private static void GameDirector_Update_Postfix()
    {
        try
        {
            INetworkClient client = TryGetClient();
            if (client == null || !client.IsConnected)
            {
                return;
            }

            EnsureSubscribed(client);

            // 将缓冲/记录的心情状态应用到现有的远端视图上。
            // 注意：这只做“本地视图修复”，不做任何定时网络广播。
            ApplyPendingMoodToExistingViews();

            bool inBattle = Singleton<GameDirector>.Instance?.PlayerUnitView != null;
            if (!inBattle && _wasInBattle)
            {
                // 离开战斗：清理本地缓存，避免跨战斗污染。
                _lastBroadcastedEffectName = null;
                lock (_pendingMoodByPlayerId)
                {
                    _pendingMoodByPlayerId.Clear();
                }
                lock (_lastKnownMoodByPlayerId)
                {
                    _lastKnownMoodByPlayerId.Clear();
                    _lastAppliedMoodByPlayerId.Clear();
                }
            }

            _wasInBattle = inBattle;
        }
        catch
        {
            // 忽略异常
        }
    }

    /// <summary>
    /// GameDirector.EnterBattle的后置补丁，用于进入战斗时强制同步
    /// </summary>
    [HarmonyPatch(typeof(GameDirector), nameof(GameDirector.EnterBattle))]
    [HarmonyPostfix]
    private static void GameDirector_EnterBattle_Postfix()
    {
        try
        {
            // 进入战斗时：如当前已经有心情循环，则同步一次。
            _lastBroadcastedEffectName = null;
            BroadcastMoodStateSync(force: true);
        }
        catch
        {
            // 忽略异常
        }
    }

    /// <summary>
    /// GameDirector.LeaveBattle的后置补丁，用于离开战斗时清理状态
    /// </summary>
    [HarmonyPatch(typeof(GameDirector), nameof(GameDirector.LeaveBattle))]
    [HarmonyPostfix]
    private static void GameDirector_LeaveBattle_Postfix()
    {
        _wasInBattle = false;
        _lastBroadcastedEffectName = null;
        lock (_pendingMoodByPlayerId)
        {
            _pendingMoodByPlayerId.Clear();
        }
        lock (_lastKnownMoodByPlayerId)
        {
            _lastKnownMoodByPlayerId.Clear();
            _lastAppliedMoodByPlayerId.Clear();
        }
    }

    /// <summary>
    /// UnitView.TryPlayEffectLoop的后置补丁，用于广播心情特效开始事件
    /// </summary>
    /// <param name="__instance">UnitView实例</param>
    /// <param name="effectName">特效名称</param>
    /// <param name="__result">尝试播放的结果</param>
    [HarmonyPatch(typeof(UnitView), nameof(UnitView.TryPlayEffectLoop))]
    [HarmonyPostfix]
    private static void UnitView_TryPlayEffectLoop_Postfix(UnitView __instance, string effectName, ref bool __result)
    {
        if (!__result || IsSuppressed || !IsMoodEffect(effectName))
        {
            return;
        }

        try
        {
            if (!ReferenceEquals(Singleton<GameDirector>.Instance?.PlayerUnitView, __instance))
            {
                return;
            }

            INetworkClient client = TryGetClient();
            if (client == null || !client.IsConnected)
            {
                return;
            }

            if (!OtherPlayersOverlayPatch.TryGetSelfPlayer(out string selfId, out string selfName))
            {
                return;
            }

            client.SendGameEventData(NetworkMessageTypes.OnMoodEffectLoopStarted, new
            {
                SenderPlayerId = selfId,
                SenderName = selfName,
                EffectName = effectName
            });

            // 需求：不做定时轮询；当心情变化时发送一次“当前心情状态”。
            BroadcastMoodStateSync(force: false);

            Plugin.Logger?.LogDebug($"[MoodEffectSync] 已广播心情开始: playerId={selfId}, effect={effectName}");
        }
        catch
        {
            // 忽略异常
        }
    }

    /// <summary>
    /// UnitView.EndEffectLoop的前置补丁，用于广播心情特效结束事件
    /// </summary>
    /// <param name="__instance">UnitView实例</param>
    /// <param name="effectName">特效名称</param>
    /// <param name="instant">是否立即结束</param>
    [HarmonyPatch(typeof(UnitView), nameof(UnitView.EndEffectLoop))]
    [HarmonyPrefix]
    private static void UnitView_EndEffectLoop_Prefix(UnitView __instance, string effectName, bool instant)
    {
        if (IsSuppressed || !IsMoodEffect(effectName))
        {
            return;
        }

        try
        {
            if (!ReferenceEquals(Singleton<GameDirector>.Instance?.PlayerUnitView, __instance))
            {
                return;
            }

            INetworkClient client = TryGetClient();
            if (client == null || !client.IsConnected)
            {
                return;
            }

            if (!OtherPlayersOverlayPatch.TryGetSelfPlayer(out string selfId, out string selfName))
            {
                return;
            }

            client.SendGameEventData(NetworkMessageTypes.OnMoodEffectLoopEnded, new
            {
                SenderPlayerId = selfId,
                SenderName = selfName,
                EffectName = effectName,
                Instant = instant
            });

            // 需求：心情变化时发送同步事件（结束也算变化）。
            BroadcastMoodStateSync(force: true);

            Plugin.Logger?.LogDebug($"[MoodEffectSync] 已广播心情结束: playerId={selfId}, effect={effectName}, instant={instant}");
        }
        catch
        {
            // 忽略异常
        }
    }

    #endregion

    #region 网络客户端管理

    /// <summary>
    /// 尝试获取网络客户端实例
    /// </summary>
    /// <returns>网络客户端实例，如果获取失败则返回null</returns>
    private static INetworkClient TryGetClient()
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

    /// <summary>
    /// 确保订阅指定网络客户端的事件
    /// </summary>
    /// <param name="client">要订阅的网络客户端</param>
    private static void EnsureSubscribed(INetworkClient client)
    {
        if (_subscribed && ReferenceEquals(_subscribedClient, client))
        {
            return;
        }

        try
        {
            if (_subscribedClient != null)
            {
                _subscribedClient.OnGameEventReceived -= _onGameEventReceived;
                _subscribedClient.OnConnectionStateChanged -= _onConnectionStateChanged;
            }
        }
        catch
        {
            // 忽略异常
        }

        _subscribedClient = client;
        _subscribed = true;

        client.OnGameEventReceived += _onGameEventReceived;
        client.OnConnectionStateChanged += _onConnectionStateChanged;
    }

    #endregion

    #region 事件处理

    /// <summary>
    /// 连接状态变化事件处理
    /// </summary>
    /// <param name="isConnected">是否已连接</param>
    private static void OnConnectionStateChanged(bool isConnected)
    {
        if (!isConnected)
        {
            return;
        }

        // 重新连接时，重新广播当前心情状态（如果有），以便其他客户端可以立即渲染
        try
        {
            _lastBroadcastedEffectName = null;
            BroadcastMoodStateSync(force: true);
        }
        catch
        {
            // 忽略异常
        }
    }

    /// <summary>
    /// 游戏事件接收处理
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="payload">事件负载</param>
    private static void OnGameEventReceived(string eventType, object payload)
    {
        if (eventType != NetworkMessageTypes.OnMoodEffectLoopStarted
            && eventType != NetworkMessageTypes.OnMoodEffectLoopEnded
            && eventType != NetworkMessageTypes.OnMoodEffectStateSync)
        {
            return;
        }

        try
        {
            if (!TryGetPayloadObject(payload, out JObject root))
            {
                return;
            }

            string senderId = GetString(root, "SenderPlayerId");
            if (string.IsNullOrWhiteSpace(senderId))
            {
                // 备用方案：如果只有SenderName，则通过名称解析
                string senderName = GetString(root, "SenderName");
                if (!string.IsNullOrWhiteSpace(senderName))
                {
                    OtherPlayersOverlayPatch.TryResolvePlayerIdByName(senderName, out senderId);
                }
            }

            if (string.IsNullOrWhiteSpace(senderId))
            {
                return;
            }

            if (eventType == NetworkMessageTypes.OnMoodEffectStateSync)
            {
                string current = GetString(root, "CurrentEffectName");
                if (!IsMoodEffect(current))
                {
                    current = null;
                }

                lock (_lastKnownMoodByPlayerId)
                {
                    _lastKnownMoodByPlayerId[senderId] = current;
                }

                if (OtherPlayersOverlayPatch.TryGetRemoteCharacterUnitView(senderId, out UnitView view) && view != null)
                {
                    ApplyMoodStateToView(view, current);
                }
                else
                {
                    lock (_pendingMoodByPlayerId)
                    {
                        _pendingMoodByPlayerId[senderId] = current;
                    }
                }

                return;
            }

            string effectName = GetString(root, "EffectName");
            if (!IsMoodEffect(effectName))
            {
                return;
            }

            if (!OtherPlayersOverlayPatch.TryGetRemoteCharacterUnitView(senderId, out UnitView effectView) || effectView == null)
            {
                lock (_pendingMoodByPlayerId)
                {
                    _pendingMoodByPlayerId[senderId] = eventType == NetworkMessageTypes.OnMoodEffectLoopStarted ? effectName : null;
                }

                lock (_lastKnownMoodByPlayerId)
                {
                    _lastKnownMoodByPlayerId[senderId] = eventType == NetworkMessageTypes.OnMoodEffectLoopStarted ? effectName : null;
                }

                return;
            }

            using (new SuppressBroadcastScope())
            {
                if (eventType == NetworkMessageTypes.OnMoodEffectLoopStarted)
                {
                    ApplyMoodStateToView(effectView, effectName);
                    lock (_lastKnownMoodByPlayerId)
                    {
                        _lastKnownMoodByPlayerId[senderId] = effectName;
                    }
                }
                else
                {
                    bool instant = GetBool(root, "Instant");
                    effectView.EndEffectLoop(effectName, instant);
                    lock (_lastKnownMoodByPlayerId)
                    {
                        _lastKnownMoodByPlayerId[senderId] = null;
                    }
                }
            }
        }
        catch
        {
            // 忽略异常
        }
    }

    #endregion

    #region 状态管理

    /// <summary>
    /// 将缓冲的心情状态应用到现有的视图上
    /// </summary>
    private static void ApplyPendingMoodToExistingViews()
    {
        Dictionary<string, string> snapshot;
        lock (_pendingMoodByPlayerId)
        {
            if (_pendingMoodByPlayerId.Count == 0)
            {
                // 仍然尝试从 lastKnown 表中恢复（处理“远端视图重建后没有再来事件”的情况）。
                snapshot = null;
            }
            else
            {
                snapshot = new Dictionary<string, string>(_pendingMoodByPlayerId, StringComparer.OrdinalIgnoreCase);
            }
        }

        if (snapshot == null)
        {
            lock (_lastKnownMoodByPlayerId)
            {
                if (_lastKnownMoodByPlayerId.Count == 0)
                {
                    return;
                }

                snapshot = new Dictionary<string, string>(_lastKnownMoodByPlayerId, StringComparer.OrdinalIgnoreCase);
            }
        }

        foreach (KeyValuePair<string, string> kv in snapshot)
        {
            if (!OtherPlayersOverlayPatch.TryGetRemoteCharacterUnitView(kv.Key, out UnitView view) || view == null)
            {
                continue;
            }

            // 去重：避免每帧重复对同一远端玩家调用 TryPlay/EndEffectLoop。
            bool shouldApply;
            lock (_lastKnownMoodByPlayerId)
            {
                _lastAppliedMoodByPlayerId.TryGetValue(kv.Key, out string lastApplied);
                shouldApply = !string.Equals(lastApplied, kv.Value, StringComparison.OrdinalIgnoreCase);
                if (shouldApply)
                {
                    _lastAppliedMoodByPlayerId[kv.Key] = kv.Value;
                }
            }

            if (shouldApply)
            {
                ApplyMoodStateToView(view, kv.Value);
            }
        }
    }

    /// <summary>
    /// 广播当前心情状态同步
    /// </summary>
    private static void BroadcastMoodStateSync()
    {
        BroadcastMoodStateSync(force: false);
    }

    /// <summary>
    /// 广播当前心情状态同步。
    /// force=true 时无视去重/限流（用于重连/入战等关键时刻）。
    /// </summary>
    private static void BroadcastMoodStateSync(bool force)
    {
        INetworkClient client = TryGetClient();
        if (client == null || !client.IsConnected)
        {
            return;
        }

        TryGetLocalActiveMoodEffectName(out string effectName);

        // 避免重复广播相同状态；TryPlayEffectLoop在接收端是幂等的，
        // 但我们保持低流量

        if (!OtherPlayersOverlayPatch.TryGetSelfPlayer(out string selfId, out string selfName))
        {
            return;
        }

        // 去重：只有状态变化（或 force）才发送，避免刷屏。
        if (!force)
        {
            bool sameState = string.Equals(effectName, _lastBroadcastedEffectName, StringComparison.OrdinalIgnoreCase);
            if (sameState)
            {
                return;
            }
        }

        client.SendGameEventData(NetworkMessageTypes.OnMoodEffectStateSync, new
        {
            SenderPlayerId = selfId,
            SenderName = selfName,
            CurrentEffectName = effectName
        });

        _lastBroadcastedEffectName = effectName;

        Plugin.Logger?.LogDebug($"[MoodEffectSync] 已广播心情状态: playerId={selfId}, current={effectName ?? "<none>"}, force={force}");
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 检查指定的特效名称是否为心情特效
    /// </summary>
    /// <param name="effectName">特效名称</param>
    /// <returns>如果是心情特效则返回true，否则返回false</returns>
    private static bool IsMoodEffect(string effectName)
    {
        return string.Equals(effectName, "ChaowoLoop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(effectName, "BenwoLoop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(effectName, "DunwuLoop", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 检查指定视图上是否正在播放指定的特效循环
    /// </summary>
    /// <param name="view">UnitView实例</param>
    /// <param name="effectName">特效名称</param>
    /// <returns>如果正在播放则返回true，否则返回false</returns>
    private static bool IsEffectLoopPlaying(UnitView view, string effectName)
    {
        if (view == null || string.IsNullOrWhiteSpace(effectName))
        {
            return false;
        }

        try
        {
            var field = AccessTools.Field(typeof(UnitView), "_effectDictionary");
            if (field == null)
            {
                return false;
            }

            if (field.GetValue(view) is not IDictionary dict)
            {
                return false;
            }

            return dict.Contains(effectName);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 尝试获取本地当前激活的心情特效名称
    /// </summary>
    /// <param name="effectName">输出的特效名称</param>
    /// <returns>如果找到心情特效则返回true，否则返回false</returns>
    private static bool TryGetLocalActiveMoodEffectName(out string effectName)
    {
        effectName = null;

        try
        {
            UnitView playerView = Singleton<GameDirector>.Instance?.PlayerUnitView;
            Unit unit = playerView?.Unit;
            if (unit?.StatusEffects == null)
            {
                return false;
            }

            foreach (StatusEffect se in unit.StatusEffects)
            {
                if (se == null)
                {
                    continue;
                }

                string n = se.UnitEffectName;
                if (IsMoodEffect(n))
                {
                    effectName = n;
                    return true;
                }
            }
        }
        catch
        {
            // 忽略异常
        }

        effectName = null;
        return false;
    }

    /// <summary>
    /// 将心情状态应用到指定的视图上
    /// </summary>
    /// <param name="view">要应用状态的UnitView</param>
    /// <param name="currentEffectName">当前特效名称，如果为null则清除所有心情特效</param>
    private static void ApplyMoodStateToView(UnitView view, string currentEffectName)
    {
        if (view == null)
        {
            return;
        }

        using (new SuppressBroadcastScope())
        {
            // 确保当前心情循环存在（如果有），并清除过时的心情循环而不产生警告
            if (!string.IsNullOrWhiteSpace(currentEffectName))
            {
                view.TryPlayEffectLoop(currentEffectName);
            }

            foreach (string n in MoodEffectNames)
            {
                if (!string.IsNullOrWhiteSpace(currentEffectName) && string.Equals(n, currentEffectName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (IsEffectLoopPlaying(view, n))
                {
                    view.EndEffectLoop(n, true);
                }
            }
        }
    }

    private static bool TryGetPayloadObject(object payload, out JObject root)
    {
        root = null;

        try
        {
            if (payload is JObject jo)
            {
                root = jo;
                return true;
            }

            if (payload is string s)
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    return false;
                }

                root = JObject.Parse(s);
                return true;
            }

            // 最后兜底：把 payload 先序列化为 JSON 再解析。
            root = JObject.Parse(JsonCompat.Serialize(payload));
            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogWarning($"[MoodEffectSync] 解析 payload 失败: type={payload?.GetType().FullName}, err={ex.Message}");
            root = null;
            return false;
        }
    }

    private static string GetString(JObject root, string property)
    {
        if (root == null || string.IsNullOrWhiteSpace(property))
        {
            return null;
        }

        try
        {
            if (!root.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out JToken token) || token == null)
            {
                return null;
            }

            if (token.Type == JTokenType.String)
            {
                return token.Value<string>();
            }

            if (token.Type == JTokenType.Boolean)
            {
                return token.Value<bool>() ? "true" : "false";
            }

            if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
            {
                return token.ToString();
            }

            return token.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static bool GetBool(JObject root, string property)
    {
        try
        {
            string s = GetString(root, property);
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }

            if (bool.TryParse(s, out bool b))
            {
                return b;
            }

            return int.TryParse(s, out int i) && i != 0;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
