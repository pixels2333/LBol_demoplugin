using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core.GapOptions;
using LBoL.Core.Stations;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.UI.Factories;
using NetworkPlugin.UI.Models;
using NetworkPlugin.UI.Components;
using NetworkPlugin.UI.Payloads;
using NetworkPlugin.UI.Panels;
using NetworkPlugin.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.Patch.UI;

[HarmonyPatch]
public class GapOptionsPanel_Patch
{
        private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

        private static ConfigManager ConfigManager => ServiceProvider?.GetService<ConfigManager>();

    private static bool _pendingDrinkTeaCompletion;
    private static string _pendingDrinkTeaOptionId;
    private static string _pendingDrinkTeaOptionName;

        private static INetworkManager GetNetworkManager()
    {
        try
        {
            return ServiceProvider?.GetService<INetworkManager>();
        }
        catch
        {
            return null;
        }
    }

        [HarmonyPatch(typeof(GapOptionsPanel), "OnShowing")]
    [HarmonyPostfix]
    public static void OnShowing_Postfix(GapOptionsPanel __instance, GapStation gapStation)
    {
        try
        {

            if (ConfigManager?.EnableGapStationExtensions?.Value != true)
            {
                Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 跳过额外按钮注入：EnableGapStationExtensions=false");
                return;
            }

            INetworkManager networkManager = GetNetworkManager();
            bool isConnected = networkManager != null && networkManager.IsConnected;

            _pendingDrinkTeaCompletion = false;
            _pendingDrinkTeaOptionId = null;
            _pendingDrinkTeaOptionName = null;
            if (isConnected)
            {
                GapOptionsSyncPatch.BroadcastGapOptionsEvent(NetworkMessageTypes.GapStationEntered, "GapStation", gapStation?.GetType().Name);
            }
            else
            {
                Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 当前未连接网络：仍注入交易/治疗按钮，点击时再提示可用性");
            }

            Plugin.Logger?.LogInfo(
                $"[GapOptionsPanel_Patch] 开始注入额外按钮: baseOptions={gapStation?.GapOptions?.Count() ?? 0}, AllowTrading={ConfigManager?.AllowTrading?.Value}, AllowRevival={ConfigManager?.AllowRevival?.Value}");

            bool addTrade = ConfigManager?.AllowTrading?.Value == true;
            bool addTreat = ConfigManager?.AllowRevival?.Value == true;
            int baseOptionCount = gapStation?.GapOptions?.Count() ?? 0;
            int appendIndex = baseOptionCount;

            if (addTrade)
            {
                AddTradeOption(__instance, baseOptionCount, appendIndex);
                appendIndex++;
            }

            if (addTreat)
            {
                AddTreatOption(__instance, baseOptionCount, appendIndex);
            }

        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] OnShowing_Postfix错误: {ex.Message}");
        }
    }

        private static void AddTradeOption(GapOptionsPanel panel, int baseOptionCount, int optionIndex)
    {
        try
        {
            RuntimeTradeGapOption tradeOption = new();

            Traverse traverse = Traverse.Create(panel);
            Transform optionsLayout = traverse.Field("optionsLayout").GetValue<Transform>();
            GapOptionWidget template = traverse.Field("template").GetValue<GapOptionWidget>();
            AssociationList<GapOptionType, Sprite> spriteTable = traverse.Field("spriteTable").GetValue<AssociationList<GapOptionType, Sprite>>();
            List<GapOptionWidget> _options = traverse.Field("_options").GetValue<List<GapOptionWidget>>();

            if (optionsLayout != null &&
                template != null &&
                spriteTable != null &&
                _options != null)
            {

                GapOptionWidget tradeWidget = CreateGapOptionWidget(optionsLayout, "TradeOption", template);
                tradeWidget.Parent = panel;

                InitializeRuntimeGapOptionWidget(tradeWidget, tradeOption, GetTeaStyleSprite(spriteTable));

                _options.Add(tradeWidget);

                var (optionPos, optionScale) = ComputeAppendedOptionLayout(panel, baseOptionCount, optionIndex);
                tradeWidget.transform.localScale = optionScale;

                tradeWidget.transform.DOLocalMove(optionPos, 1f, false)
                    .From(optionPos - new Vector3(4000f, 0f, 0f), true, false)
                    .SetEase(DG.Tweening.Ease.OutCubic);

                tradeWidget.transform.SetAsFirstSibling();

                Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 已添加交易选项");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] AddTradeOption错误: {ex.Message}");
        }
    }

        private static void AddTreatOption(GapOptionsPanel panel, int baseOptionCount, int optionIndex)
    {
        try
        {
            RuntimeTreatGapOption treatOption = new();

            Traverse traverse = Traverse.Create(panel);
            Transform optionsLayout = traverse.Field("optionsLayout").GetValue<Transform>();
            GapOptionWidget template = traverse.Field("template").GetValue<GapOptionWidget>();
            AssociationList<GapOptionType, Sprite> spriteTable = traverse.Field("spriteTable").GetValue<AssociationList<GapOptionType, Sprite>>();
            List<GapOptionWidget> _options = traverse.Field("_options").GetValue<List<GapOptionWidget>>();

            if (optionsLayout != null &&
                template != null &&
                spriteTable != null &&
                _options != null)
            {
                GapOptionWidget treatWidget = CreateGapOptionWidget(optionsLayout, "TreatOption", template);
                treatWidget.Parent = panel;

                InitializeRuntimeGapOptionWidget(treatWidget, treatOption, GetTeaStyleSprite(spriteTable));

                _options.Add(treatWidget);

                var (optionPos, optionScale) = ComputeAppendedOptionLayout(panel, baseOptionCount, optionIndex);
                treatWidget.transform.localScale = optionScale;
                treatWidget.transform.DOLocalMove(optionPos, 1f, false)
                    .From(optionPos - new Vector3(4000f, 0f, 0f), true, false)
                    .SetEase(DG.Tweening.Ease.OutCubic);
                treatWidget.transform.SetAsFirstSibling();

                Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 已添加治疗选项");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] AddTreatOption错误: {ex.Message}");
        }
    }

        [HarmonyPatch(typeof(GapOptionsPanel), "OptionClicked")]
    [HarmonyPrefix]
    public static bool OptionClicked_Prefix(GapOptionsPanel __instance, GapOption option)
    {
        try
        {

            if (IsCustomTradeOption(option))
            {
                try
                {
                    Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 处理交易选项");

                    if (!TradeUiMessages.IsTradeEnabledAndConnected(out string reason))
                    {
                        Plugin.Logger?.LogWarning($"[GapOptionsPanel_Patch] 交易不可用: {reason ?? "<no-reason>"}");
                        TradeUiMessages.ShowTopMessage(reason ?? "交易不可用。");
                        return false;
                    }

                    Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 交易可用，准备获取 TradePanel 实例。");
                    TradePanel tradePanel = GetOrCreateTradePanel(__instance?.transform.parent);
                    if (tradePanel != null)
                    {
                        Plugin.Logger?.LogInfo($"[GapOptionsPanel_Patch] 已获取 TradePanel: name={tradePanel.name}, activeSelf={tradePanel.gameObject.activeSelf}, activeInHierarchy={tradePanel.gameObject.activeInHierarchy}");
                        Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 准备启动 TradePanel.ShowTradeAsync 协程。");
                        __instance.StartCoroutine(tradePanel.ShowTradeAsync(new TradePayload()));
                        Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 已调用 TradePanel.ShowTradeAsync。");
                    }
                    else
                    {
                        Plugin.Logger?.LogWarning("[GapOptionsPanel_Patch] 未能获取 TradePanel 实例。");
                        TradeUiMessages.ShowTradePanelMissing();
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] 处理交易选项错误: {ex.Message}");
                }
                return false;
            }

            if (IsCustomTreatOption(option))
            {
                try
                {
                    Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 处理治疗选项");

                    INetworkManager networkManager = GetNetworkManager();
                    if (networkManager == null || !networkManager.IsConnected)
                    {
                        if (ConfigManager?.DebugVirtualPlayerAiDefault?.Value == true || ConfigManager?.DebugFakePlayersForTrade?.Value == true)
                        {
                            Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 调试模式：跳过网络检查，允许离线治疗");
                        }
                        else
                        {
                            TradeUiMessages.ShowTopMessage("治疗不可用：网络未连接。");
                            return false;
                        }
                    }

                    ResurrectPanel resurrectPanel = GetOrCreateResurrectPanel(__instance?.transform.parent);
                    if (resurrectPanel != null)
                    {
                        List<DeadPlayerEntry> candidates = BuildTreatmentCandidates();
                        Plugin.Logger?.LogInfo($"[GapOptionsPanel_Patch] 治疗候选构建完成: total={candidates.Count}, ai1={candidates.FirstOrDefault(p => string.Equals(p.PlayerId, "aidefault", StringComparison.OrdinalIgnoreCase))?.PlayerName}, ai2={candidates.FirstOrDefault(p => string.Equals(p.PlayerId, "aidefault2", StringComparison.OrdinalIgnoreCase))?.PlayerName}");

                        if (candidates.Count == 0)
                        {
                            TradeUiMessages.ShowTopMessage("当前没有可治疗的队友。");
                            return false;
                        }

                        ResurrectPayload payload = new ResurrectPayload
                        {
                            Players = candidates,
                            CanCancel = true,
                            CostCalculator = desiredHp => desiredHp,
                        };

                        __instance.StartCoroutine(ShowTreatFlowAndCompleteAsync(__instance, resurrectPanel, payload));
                    }
                    else
                    {
                        TradeUiMessages.ShowTopMessage("治疗面板不可用。");
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] 处理治疗选项错误: {ex.Message}");
                }
                return false;
            }

            TryBroadcastBuiltInGapOptionsSelection(option);

            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] OptionClicked_Prefix错误: {ex.Message}");
            return true;
        }
    }

    [HarmonyPatch(typeof(GapOptionsPanel), nameof(GapOptionsPanel.SelectedAndHide))]
    [HarmonyPostfix]
    public static void SelectedAndHide_Postfix()
    {
        if (!_pendingDrinkTeaCompletion)
        {
            return;
        }

        string optionId = _pendingDrinkTeaOptionId;
        string optionName = _pendingDrinkTeaOptionName;

        _pendingDrinkTeaCompletion = false;
        _pendingDrinkTeaOptionId = null;
        _pendingDrinkTeaOptionName = null;

        GapOptionsSyncPatch.BroadcastGapOptionsEvent(NetworkMessageTypes.DrinkTeaCompleted, optionId, optionName);
    }

    #region 辅助方法

        private static Sprite GetTeaStyleSprite(AssociationList<GapOptionType, Sprite> spriteTable)
    {
        if (spriteTable != null && spriteTable.TryGetValue(GapOptionType.DrinkTea, out Sprite teaSprite) && teaSprite != null)
        {
            return teaSprite;
        }

        return Resources.Load<Sprite>("UI/Icons/DefaultIcon");
    }

        private static Vector3 GetDefaultOptionPos(GapOptionsPanel panel)
    {
        try
        {
            return Traverse.Create(panel).Field("defaultOptionPos").GetValue<Vector3>();
        }
        catch
        {
            return Vector3.zero;
        }
    }

        private static Vector3 GetOptionPadding(GapOptionsPanel panel)
    {
        try
        {
            return Traverse.Create(panel).Field("optionPadding").GetValue<Vector3>();
        }
        catch
        {
            return new Vector3(200f, 0f, 0f);
        }
    }

        private static (Vector3 Position, Vector3 Scale) ComputeAppendedOptionLayout(GapOptionsPanel panel, int baseOptionCount, int optionIndex)
    {
        Vector3 defaultPos = GetDefaultOptionPos(panel);
        Vector3 optionPadding = GetOptionPadding(panel);

        Vector3 spacing = new Vector3(optionPadding.x + (baseOptionCount * 10f), optionPadding.y, 0f);
        bool useTwoRows = baseOptionCount > 4;

        if (useTwoRows)
        {
            Vector3 pos = defaultPos
                - new Vector3(500f, 0f, 0f)
                + spacing * (optionIndex % 4)
                + new Vector3(900f, -200f, 0f) * (optionIndex / 4);
            return (pos, new Vector3(0.8f, 0.8f, 0.8f));
        }

        return (defaultPos + spacing * optionIndex, Vector3.one);
    }

        private static bool IsCustomTradeOption(GapOption option)
    {
        return option is RuntimeGapOption runtime && string.Equals(runtime.Id, "Trade", StringComparison.Ordinal);
    }

        private static bool IsCustomTreatOption(GapOption option)
    {
        return option is RuntimeGapOption runtime && string.Equals(runtime.Id, "Treat", StringComparison.Ordinal);
    }

    [HarmonyPatch(typeof(GapOptionWidget), nameof(GapOptionWidget.OnLocalizeChanged))]
    [HarmonyPrefix]
    public static bool GapOptionWidget_OnLocalizeChanged_Prefix(GapOptionWidget __instance)
    {
        try
        {
            GapOption option = Traverse.Create(__instance).Field("_option").GetValue<GapOption>();
            if (option is RuntimeGapOption runtime)
            {
                ApplyRuntimeGapOptionPresentation(__instance, runtime);

                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] GapOptionWidget_OnLocalizeChanged错误: {ex.Message}");
            return true;
        }
    }

    private static void InitializeRuntimeGapOptionWidget(GapOptionWidget widget, RuntimeGapOption option, Sprite sprite)
    {
        if (widget == null || option == null)
        {
            return;
        }

        Traverse widgetTraverse = Traverse.Create(widget);
        widgetTraverse.Field("_option").SetValue(option);

        Image image = widgetTraverse.Field("image").GetValue<Image>();
        if (image != null)
        {
            image.sprite = sprite;
        }

        ApplyRuntimeGapOptionPresentation(widget, option);
    }

    private static void ApplyRuntimeGapOptionPresentation(GapOptionWidget widget, RuntimeGapOption option)
    {
        if (widget == null || option == null)
        {
            return;
        }

        TextMeshProUGUI optionName = Traverse.Create(widget).Field("optionName").GetValue<TextMeshProUGUI>();
        TextMeshProUGUI optionTipText = Traverse.Create(widget).Field("optionTipText").GetValue<TextMeshProUGUI>();

        if (optionName != null)
        {
            optionName.text = option.DisplayName;
        }

        if (optionTipText != null)
        {
            optionTipText.text = option.DisplayDescription;
        }
    }

    private static List<DeadPlayerEntry> BuildTreatmentCandidates()
    {
        List<DeadPlayerEntry> result = [];
        INetworkManager networkManager = GetNetworkManager();
        if (networkManager == null)
        {
            return result;
        }

        Dictionary<string, (string PlayerName, bool IsConnected, bool IsHost)> candidatePlayers = new(StringComparer.Ordinal);

        void AddOrUpdateCandidate(string playerId, string playerName, bool isConnected, bool isHost)
        {
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return;
            }

            if (!candidatePlayers.TryGetValue(playerId, out (string PlayerName, bool IsConnected, bool IsHost) existing))
            {
                candidatePlayers[playerId] = (playerName, isConnected, isHost);
                return;
            }

            candidatePlayers[playerId] =
            (
                string.IsNullOrWhiteSpace(existing.PlayerName) ? playerName : existing.PlayerName,
                existing.IsConnected || isConnected,
                existing.IsHost || isHost
            );
        }

        foreach ((string PlayerId, string PlayerName, bool IsConnected, bool IsHost) player in OtherPlayersOverlayPatch.SnapshotPlayers())
        {
            AddOrUpdateCandidate(player.PlayerId, player.PlayerName, player.IsConnected, player.IsHost);
        }

        IEnumerable<INetworkPlayer> allNetworkPlayers = networkManager.GetAllPlayers() ?? Enumerable.Empty<INetworkPlayer>();
        foreach (INetworkPlayer networkPlayer in allNetworkPlayers)
        {
            if (networkPlayer == null)
            {
                continue;
            }

            bool isHost = false;
            try
            {
                isHost = networkPlayer.IsLobbyOwner();
            }
            catch
            {

            }

            AddOrUpdateCandidate(networkPlayer.playerId, networkPlayer.userName, true, isHost);
        }

        string selfPlayerId = networkManager.GetSelf()?.playerId;
        if (!string.IsNullOrWhiteSpace(selfPlayerId))
        {
            AddOrUpdateCandidate(selfPlayerId, GameStateUtils.GetCurrentPlayerName(), true, false);
        }

        if (ConfigManager?.DebugVirtualPlayerAiDefault?.Value == true || ConfigManager?.DebugFakePlayersForTrade?.Value == true)
        {
            AddOrUpdateCandidate("aidefault", "AI Default", true, false);
            AddOrUpdateCandidate("aidefault2", "AI Default 2", true, false);
            AddOrUpdateCandidate("aidefault3", "AI Default 3", true, false);
        }

        foreach (string knownPlayerId in candidatePlayers.Keys.ToList())
        {
            if (IsVirtualAiSimulatedPlayer(knownPlayerId))
            {
                AddOrUpdateCandidate(knownPlayerId, OtherPlayersOverlayPatch.ResolveDisplayName(knownPlayerId, null), true, false);
            }
        }

        foreach ((string playerId, (string playerName, bool isConnected, bool isHost) candidate) in candidatePlayers)
        {
            if (string.Equals(playerId, selfPlayerId, StringComparison.Ordinal) || string.Equals(playerId, "__local__", StringComparison.Ordinal))
            {
                continue;
            }

            INetworkPlayer networkPlayer = ResolveNetworkPlayer(networkManager, playerId);
            if (!TryGetPlayerVitals(playerId, networkPlayer, out int currentHp, out int maxHp))
            {
                continue;
            }

            int healAmount = CalculateHealingAmount(maxHp);
            bool canTreat = currentHp < maxHp;
            int finalHp = Math.Min(maxHp, currentHp + healAmount);

            result.Add(new DeadPlayerEntry
            {
                PlayerId = playerId,
                PlayerName = ResolveTreatmentDisplayName(playerId, candidate.playerName, selfPlayerId),
                CurrentHp = currentHp,
                MaxHp = maxHp,
                ActionValue = healAmount,
                ResurrectionCost = healAmount,
                CanResurrect = canTreat,
                DeadCause = canTreat ? "可治疗" : "生命已满",
                StatusText = canTreat ? $"治疗后 {finalHp}/{maxHp}" : "生命已满",
            });
        }

        return result
            .OrderByDescending(p => p.CanResurrect)
            .ThenBy(p => p.PlayerName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IEnumerator ShowTreatFlowAndCompleteAsync(GapOptionsPanel panel, ResurrectPanel resurrectPanel, ResurrectPayload payload)
    {
        if (panel == null || resurrectPanel == null)
        {
            yield break;
        }

        yield return resurrectPanel.ShowResurrectAsync(payload);

        if (resurrectPanel.DidCompleteTreatment)
        {
            Traverse.Create(panel).Method("SelectedAndHide").GetValue();
        }
    }

    private static INetworkPlayer ResolveNetworkPlayer(INetworkManager networkManager, string playerId)
    {
        if (networkManager == null || string.IsNullOrWhiteSpace(playerId))
        {
            return null;
        }

        INetworkPlayer self = networkManager.GetSelf();
        if (self != null && string.Equals(self.playerId, playerId, StringComparison.Ordinal))
        {
            return self;
        }

        return networkManager.GetPlayer(playerId);
    }

    private static bool TryGetPlayerVitals(string playerId, INetworkPlayer networkPlayer, out int currentHp, out int maxHp)
    {
        return PlayerVitalsHelper.TryResolveVitals(playerId, networkPlayer, out currentHp, out maxHp);
    }

    internal static bool IsVirtualAiSimulatedPlayer(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return false;
        }

        return string.Equals(playerId, "aidefault", StringComparison.OrdinalIgnoreCase)
            || string.Equals(playerId, "aidefault2", StringComparison.OrdinalIgnoreCase)
            || playerId.StartsWith("aidefault", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveTreatmentDisplayName(string playerId, string preferredName, string selfPlayerId)
    {
        string normalizedPreferred = NormalizeTreatmentDisplayName(preferredName, playerId);
        if (!string.IsNullOrWhiteSpace(normalizedPreferred))
        {
            return normalizedPreferred;
        }

        if (IsVirtualAiSimulatedPlayer(playerId))
        {
            return GetVirtualAiTreatmentDisplayName(playerId);
        }

        string resolved = OtherPlayersOverlayPatch.ResolveDisplayName(
            playerId,
            null,
            string.Equals(playerId, selfPlayerId, StringComparison.Ordinal));

        string normalizedResolved = NormalizeTreatmentDisplayName(resolved, playerId);
        if (!string.IsNullOrWhiteSpace(normalizedResolved))
        {
            return normalizedResolved;
        }

        return string.IsNullOrWhiteSpace(playerId) ? "Unknown" : playerId;
    }

    private static string NormalizeTreatmentDisplayName(string displayName, string playerId)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return null;
        }

        string trimmed = displayName.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(playerId)
            && string.Equals(trimmed, playerId, StringComparison.Ordinal)
            && IsVirtualAiSimulatedPlayer(playerId))
        {
            return null;
        }

        return trimmed;
    }

    private static string GetVirtualAiTreatmentDisplayName(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return "AI Default";
        }

        if (string.Equals(playerId, "aidefault3", StringComparison.OrdinalIgnoreCase))
        {
            return "AI Default 3";
        }

        if (string.Equals(playerId, "aidefault2", StringComparison.OrdinalIgnoreCase))
        {
            return "AI Default 2";
        }

        if (string.Equals(playerId, "aidefault", StringComparison.OrdinalIgnoreCase))
        {
            return "AI Default";
        }

        return "AI Default";
    }

    private static int CalculateHealingAmount(int maxHp)
    {
        if (maxHp <= 0)
        {
            return 1;
        }

        return Math.Max(1, Mathf.CeilToInt(maxHp * 0.2f));
    }

    private static void TryBroadcastBuiltInGapOptionsSelection(GapOption option)
    {
        if (option == null)
        {
            return;
        }

        List<string> eventTypes = ResolveGapEventTypes(option);
        if (eventTypes.Count == 0)
        {
            return;
        }

        string cardId = TryGetOptionString(option, "CardId") ?? TryGetOptionString(option, "Id");
        string cardName = TryGetOptionString(option, "CardName") ?? TryGetOptionString(option, "Name");

        bool includesDrinkTeaStarted = false;
        foreach (string eventType in eventTypes)
        {
            GapOptionsSyncPatch.BroadcastGapOptionsEvent(eventType, cardId, cardName);
            if (string.Equals(eventType, NetworkMessageTypes.DrinkTeaStarted, StringComparison.Ordinal))
            {
                includesDrinkTeaStarted = true;
            }
        }

        if (includesDrinkTeaStarted)
        {
            _pendingDrinkTeaCompletion = true;
            _pendingDrinkTeaOptionId = cardId;
            _pendingDrinkTeaOptionName = cardName;
        }
    }

    private static List<string> ResolveGapEventTypes(GapOption option)
    {
        List<string> eventTypes = [];
        string typeText = TryGetOptionString(option, "Type") ?? option.GetType().Name;
        string nameText = TryGetOptionString(option, "Name");
        string idText = TryGetOptionString(option, "Id");

        string merged = $"{typeText}|{nameText}|{idText}";

        if (ContainsAny(merged, "Upgrade", "升级", "Enhance", "强化"))
        {
            eventTypes.Add(NetworkMessageTypes.GapOptionsUpgradeSelected);
        }

        if (ContainsAny(merged, "Remove", "Delete", "Exile", "移除", "删除", "放逐"))
        {
            eventTypes.Add(NetworkMessageTypes.GapOptionsRemoveCard);
        }

        if (ContainsAny(merged, "Tea", "Drink", "Rest", "Recover", "Heal", "喝茶", "休息", "恢复", "疗伤"))
        {
            eventTypes.Add(NetworkMessageTypes.DrinkTeaStarted);
        }

        return eventTypes;
    }

    private static string TryGetOptionString(GapOption option, string propertyName)
    {
        try
        {
            object value = option.GetType().GetProperty(propertyName)?.GetValue(option);
            return value?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static bool ContainsAny(string source, params string[] tokens)
    {
        if (string.IsNullOrWhiteSpace(source) || tokens == null)
        {
            return false;
        }

        foreach (string token in tokens)
        {
            if (!string.IsNullOrWhiteSpace(token) &&
                source.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

        private static TradePanel GetOrCreateTradePanel(Transform parent)
    {
        try
        {
            Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 开始获取 TradePanel 实例。");

            TradePanel panel = null;
            try
            {
                panel = UiManager.GetPanel<TradePanel>();
                if (panel != null)
                {
                    Plugin.Logger?.LogInfo($"[GapOptionsPanel_Patch] 从 UiManager 获取到 TradePanel: name={panel.name}");
                    return panel;
                }

                Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] UiManager 中未找到 TradePanel。");
            }
            catch (InvalidOperationException)
            {
                Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] UiManager 尚未注册 TradePanel，继续查找场景实例。");

            }

            try
            {
                panel = UnityEngine.Object.FindFirstObjectByType<TradePanel>();
                if (panel != null)
                {
                    Plugin.Logger?.LogInfo($"[GapOptionsPanel_Patch] 从场景中找到 TradePanel: name={panel.name}, activeSelf={panel.gameObject.activeSelf}");
                    return panel;
                }

                Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 场景中未找到现成的 TradePanel，准备走运行时工厂。");
            }
            catch
            {

            }

            panel = TradePanelRuntimeFactory.GetOrCreate(parent);
            if (panel != null)
            {
                Plugin.Logger?.LogInfo($"[GapOptionsPanel_Patch] 运行时工厂创建/获取 TradePanel 成功: name={panel.name}, activeSelf={panel.gameObject.activeSelf}");
                return panel;
            }

            Plugin.Logger?.LogWarning("[GapOptionsPanel_Patch] TradePanel UI instance not found and runtime factory failed.");
            TradeUiMessages.ShowTradePanelMissing();
            return null;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] GetOrCreateTradePanel错误: {ex.Message}");
            return null;
        }
    }

        private static ResurrectPanel GetOrCreateResurrectPanel(Transform parent)
    {
        try
        {
            var runtimePreferred = ResurrectPanelRuntimeFactory.GetOrCreate(parent);
            if (runtimePreferred != null)
            {
                Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 优先使用运行时治疗面板实例。");
                return runtimePreferred;
            }

            try
            {
                var panel = UiManager.GetPanel<ResurrectPanel>();
                if (panel != null)
                {
                    Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 使用 UiManager 注册的治疗面板实例。");
                    return panel;
                }
            }
            catch (InvalidOperationException)
            {

                Plugin.Logger?.LogDebug("[GapOptionsPanel_Patch] UiManager 中未找到治疗面板，尝试场景搜索与运行时工厂。");
            }

            try
            {
                var scenePanel = UnityEngine.Object.FindFirstObjectByType<ResurrectPanel>();
                if (scenePanel != null)
                {
                    Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 使用场景中已存在的治疗面板实例。");
                    return scenePanel;
                }
            }
            catch
            {

            }

            var runtimePanel = ResurrectPanelRuntimeFactory.GetOrCreate(parent);
            if (runtimePanel != null)
            {
                Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 已创建运行时治疗面板实例。");
                return runtimePanel;
            }

            Plugin.Logger?.LogWarning("[GapOptionsPanel_Patch] 无法获取治疗面板实例（UiManager/场景/运行时工厂均失败）。");

            return null;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] GetOrCreateResurrectPanel错误: {ex.Message}");
            return null;
        }
    }

    private static GapOptionWidget CreateGapOptionWidget(Transform parent, string name, GapOptionWidget template)
    {
        GapOptionWidget widget = UnityEngine.Object.Instantiate(template, parent, false);
        widget.name = name;
        widget.transform.localScale = Vector3.one;
        return widget;
    }

    #endregion
}
