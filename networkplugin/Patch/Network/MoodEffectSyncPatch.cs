using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using HarmonyLib;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Patch.UI;
using NetworkPlugin.Utils;
using LBoL.Core;
using Newtonsoft.Json.Linq;

namespace NetworkPlugin.Patch.Network;

[HarmonyPatch]
public static class MoodEffectSyncPatch
{
    #region 字段和属性

        private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

        private static readonly string[] MoodEffectNames = { "ChaowoLoop", "BenwoLoop", "DunwuLoop" };

        private static INetworkClient _subscribedClient;

        private static bool _subscribed;

        private static readonly Action<string, object> _onGameEventReceived = OnGameEventReceived;

        private static readonly Action<bool> _onConnectionStateChanged = OnConnectionStateChanged;

        private static int _suppressBroadcastDepth;

        private static bool _wasInBattle;

        private static string _lastBroadcastedEffectName;
    private static long _lastBroadcastedAtTicks;
    private static ulong _lastBroadcastedFp;

        private static readonly Dictionary<string, string> _pendingMoodByPlayerId = new(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, string> _lastKnownMoodByPlayerId = new(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, string> _lastAppliedMoodByPlayerId = new(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region 辅助类

        private sealed class SuppressBroadcastScope : IDisposable
    {
                public SuppressBroadcastScope() => Interlocked.Increment(ref _suppressBroadcastDepth);

                public void Dispose() => Interlocked.Decrement(ref _suppressBroadcastDepth);
    }

    #endregion

    #region 属性访问器

        private static bool IsSuppressed => Volatile.Read(ref _suppressBroadcastDepth) > 0;

    #endregion

    #region Harmony补丁

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

            ApplyPendingMoodToExistingViews();

            bool inBattle = Singleton<GameDirector>.Instance?.PlayerUnitView != null;
            if (!inBattle && _wasInBattle)
            {

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

        }
    }

        [HarmonyPatch(typeof(GameDirector), nameof(GameDirector.EnterBattle))]
    [HarmonyPostfix]
    private static void GameDirector_EnterBattle_Postfix()
    {
        try
        {

            _lastBroadcastedEffectName = null;
            BroadcastMoodStateSync(force: true);
        }
        catch
        {

        }
    }

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

            BroadcastMoodStateSync(force: false);

            Plugin.Logger?.LogDebug($"[MoodEffectSync] 已广播心情开始: playerId={selfId}, effect={effectName}");
        }
        catch
        {

        }
    }

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

            BroadcastMoodStateSync(force: true);

            Plugin.Logger?.LogDebug($"[MoodEffectSync] 已广播心情结束: playerId={selfId}, effect={effectName}, instant={instant}");
        }
        catch
        {

        }
    }

    #endregion

    #region 网络客户端管理

        private static INetworkClient TryGetClient()
        => ServiceProvider?.GetService<INetworkClient>();

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

        }

        _subscribedClient = client;
        _subscribed = true;

        client.OnGameEventReceived += _onGameEventReceived;
        client.OnConnectionStateChanged += _onConnectionStateChanged;
    }

    #endregion

    #region 事件处理

        private static void OnConnectionStateChanged(bool isConnected)
    {
        if (!isConnected)
        {
            return;
        }

        try
        {
            _lastBroadcastedEffectName = null;
            BroadcastMoodStateSync(force: true);
        }
        catch
        {

        }
    }

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

        }
    }

    #endregion

    #region 状态管理

        private static void ApplyPendingMoodToExistingViews()
    {
        Dictionary<string, string> snapshot;
        lock (_pendingMoodByPlayerId)
        {
            if (_pendingMoodByPlayerId.Count == 0)
            {

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

        private static void BroadcastMoodStateSync()
    {
        BroadcastMoodStateSync(force: false);
    }

        private static void BroadcastMoodStateSync(bool force)
    {
        INetworkClient client = TryGetClient();
        if (client == null || !client.IsConnected)
        {
            return;
        }

        bool hasEffect = TryGetLocalActiveMoodEffectName(out string effectName);
        string normalized = NormalizeEffectName(effectName, hasEffect);

        if (!OtherPlayersOverlayPatch.TryGetSelfPlayer(out string selfId, out string selfName))
        {
            return;
        }

        long nowTicks = DateTime.UtcNow.Ticks;
        ulong fp = NetLogHelper.ComputeFnv1a64($"{selfId}|{normalized}");
        long minIntervalTicks = force ? TimeSpan.FromMilliseconds(300).Ticks : TimeSpan.FromMilliseconds(150).Ticks;
        if (fp == _lastBroadcastedFp && (nowTicks - _lastBroadcastedAtTicks) >= 0 && (nowTicks - _lastBroadcastedAtTicks) < minIntervalTicks)
        {
            return;
        }

        if (!force)
        {
            bool sameState = string.Equals(normalized, _lastBroadcastedEffectName, StringComparison.OrdinalIgnoreCase);
            if (sameState)
            {
                return;
            }
        }

        client.SendGameEventData(NetworkMessageTypes.OnMoodEffectStateSync, new
        {
            SenderPlayerId = selfId,
            SenderName = selfName,
            CurrentEffectName = normalized
        });

        _lastBroadcastedEffectName = normalized;
        _lastBroadcastedAtTicks = nowTicks;
        _lastBroadcastedFp = fp;

        string json = JsonCompat.Serialize(new { SenderPlayerId = selfId, SenderName = selfName, CurrentEffectName = normalized });
        string summary = NetLogHelper.BuildSummary(NetworkMessageTypes.OnMoodEffectStateSync, json);
        Plugin.Logger?.LogDebug($"[MoodEffectSync] 已广播心情状态: playerId={selfId}, current={normalized}, force={force} ({summary})");
    }

    private static string NormalizeEffectName(string effectName, bool hasEffect)
    {
        if (!hasEffect)
        {

            return "<none>";
        }

        if (string.IsNullOrWhiteSpace(effectName))
        {
            return "<none>";
        }

        return effectName.Trim();
    }

    #endregion

    #region 辅助方法

        private static bool IsMoodEffect(string effectName)
    {
        return string.Equals(effectName, "ChaowoLoop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(effectName, "BenwoLoop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(effectName, "DunwuLoop", StringComparison.OrdinalIgnoreCase);
    }

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

        }

        effectName = null;
        return false;
    }

        private static void ApplyMoodStateToView(UnitView view, string currentEffectName)
    {
        if (view == null)
        {
            return;
        }

        using (new SuppressBroadcastScope())
        {

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
