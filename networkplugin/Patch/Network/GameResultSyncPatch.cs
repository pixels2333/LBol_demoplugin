using System;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 参照 Together in Spire: VictoryScreenPatch.java
///
/// 目标：
/// - 在多人联机时同步“本局结算结果”（NormalEnd/TrueEnd/Failure 等），避免各端结算时序不一致。
/// - 使用 SyncOnResult 标记避免回环：收到远端结算事件后，下一次本地结算不再广播。
///
/// 说明：
/// - 目前仅做“广播 + 去重标记”，不强制退出、不强制断开联机；后续可在收到事件时加入 UI/流程控制。
/// - 事件类型以 "On" 开头以进入 GameEvent 通道（见 NetworkClient/NetworkServer 的 IsGameEvent 判定）。
/// </summary>
[HarmonyPatch]
public static class GameResultSyncPatch
{
    private const string GameResultEventType = "OnGameRunResult";

    public static bool SyncOnResult = true;
    public static GameResultType? LastLocalResult;
    public static GameResultType? LastRemoteResult;

    private static long _suppressUntilTicks;

    private static bool IsVictoryResult(GameResultType resultType)
        => resultType != GameResultType.Failure;

    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static bool _subscribed;
    private static INetworkClient _subscribedClient;
    private static readonly Action<string, object> _onGameEventReceived = OnGameEventReceived;
    private static readonly Action<bool> _onConnectionStateChanged = OnConnectionStateChanged;

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

    [HarmonyPatch(typeof(GameDirector), "Update")]
    private static class SubscribeHook
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            INetworkClient client = TryGetNetworkClient();
            if (client == null)
            {
                return;
            }

            EnsureSubscribed(client);
            TickSuppression();
        }
    }

    private static void TickSuppression()
    {
        if (SyncOnResult)
        {
            return;
        }

        if (_suppressUntilTicks <= 0)
        {
            return;
        }

        if (DateTime.Now.Ticks < _suppressUntilTicks)
        {
            return;
        }

        // 如果远端胜利导致本地没有走到结算面板（比如直接 LeaveGameRun），
        // 这里用一个短 TTL 自动恢复标记，避免下一局的胜利同步被永久抑制。
        SyncOnResult = true;
        _suppressUntilTicks = 0;
    }

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
            // ignored
        }

        try
        {
            client.OnGameEventReceived += _onGameEventReceived;
            client.OnConnectionStateChanged += _onConnectionStateChanged;
            _subscribedClient = client;
            _subscribed = true;
        }
        catch
        {
            _subscribedClient = null;
            _subscribed = false;
        }
    }

    private static void OnConnectionStateChanged(bool connected)
    {
        if (connected)
        {
            return;
        }

        SyncOnResult = true;
        LastLocalResult = null;
        LastRemoteResult = null;
        _suppressUntilTicks = 0;
    }

    private static void OnGameEventReceived(string eventType, object payload)
    {
        if (!string.Equals(eventType, GameResultEventType, StringComparison.Ordinal))
        {
            return;
        }

        if (!TryGetJsonElement(payload, out JsonElement root))
        {
            Plugin.Logger?.LogWarning("[GameResultSync] Received OnGameRunResult but payload is not JSON.");
            return;
        }

        string resultTypeString = GetString(root, "ResultType");
        if (string.IsNullOrWhiteSpace(resultTypeString) || !Enum.TryParse(resultTypeString, out GameResultType resultType))
        {
            Plugin.Logger?.LogWarning($"[GameResultSync] Invalid ResultType in payload: '{resultTypeString ?? "<null>"}'");
            return;
        }

        if (!IsVictoryResult(resultType))
        {
            // 需求：只同步胜利，不同步失败。
            return;
        }

        // 收到远端结算：下一次本地结算不再广播，避免回环/风暴。
        SyncOnResult = false;
        _suppressUntilTicks = DateTime.Now.AddSeconds(3).Ticks;
        LastRemoteResult = resultType;
        Plugin.Logger?.LogInfo($"[GameResultSync] Remote game result received: {resultType}");

        // 需求：收到胜利同步后，自动离开本局。
        TryLeaveGameRunFromRemoteResult(resultType);
    }

    private static void TryLeaveGameRunFromRemoteResult(GameResultType resultType)
    {
        try
        {
            if (!IsVictoryResult(resultType))
            {
                return;
            }

            if (Singleton<GameMaster>.Instance?.CurrentGameRun == null)
            {
                return;
            }

            GameMaster.LeaveGameRun();
            Plugin.Logger?.LogInfo("[GameResultSync] LeaveGameRun triggered by remote victory result.");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GameResultSync] Failed to LeaveGameRun on remote victory result: {ex.Message}");
        }
    }

    private static void HandleLocalGameResult(GameResultType resultType)
    {
        LastLocalResult = resultType;

        INetworkClient client = TryGetNetworkClient();
        if (client?.IsConnected != true)
        {
            return;
        }

        if (!SyncOnResult)
        {
            // 这一轮结算由远端触发/已同步过：只恢复标记，避免下一局被永久抑制。
            SyncOnResult = true;
            _suppressUntilTicks = 0;
            return;
        }

        if (!IsVictoryResult(resultType))
        {
            // 需求：只同步胜利，不同步失败。
            return;
        }

        try
        {
            var payload = new
            {
                Sender = client.GetSelf()?.userName,
                ResultType = resultType.ToString(),
                IsTrueEnd = resultType == GameResultType.TrueEnd,
                Timestamp = DateTime.Now.Ticks
            };

            // 走 GameEvent 通道（Server 会广播给除发送方之外的所有客户端）。
            client.SendRequest(GameResultEventType, JsonSerializer.Serialize(payload));
            Plugin.Logger?.LogInfo($"[GameResultSync] Broadcast game result: {resultType}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GameResultSync] Failed to broadcast game result: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(GameResultPanel), "OnShowing")]
    private static class GameResultPanel_OnShowing_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(GameResultData payload)
        {
            if (payload == null)
            {
                return;
            }

            HandleLocalGameResult(payload.Type);
        }
    }

    private static bool TryGetJsonElement(object payload, out JsonElement root)
    {
        try
        {
            if (payload is JsonElement je)
            {
                root = je;
                return true;
            }

            if (payload is string s)
            {
                root = JsonDocument.Parse(s).RootElement;
                return true;
            }
        }
        catch
        {
            // ignored
        }

        root = default;
        return false;
    }

    private static string GetString(JsonElement elem, string property)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return null;
            }

            return p.ValueKind == JsonValueKind.String ? p.GetString() : p.GetRawText();
        }
        catch
        {
            return null;
        }
    }
}
