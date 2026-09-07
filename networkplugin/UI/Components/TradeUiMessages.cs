using System;
using Microsoft.Extensions.DependencyInjection;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.UI.Components;

internal static class TradeUiMessages
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

        public static bool IsTradeEnabledAndConnected(out string reason)
    {
        reason = null;

        var serviceProvider = ServiceProvider;
        var config = serviceProvider?.GetService<ConfigManager>();
        if (config?.AllowTrading?.Value != true)
        {
            reason = "交易功能已在配置中禁用。";
            return false;
        }

        var client = serviceProvider?.GetService<INetworkClient>();
        if (client == null || !client.IsConnected)
        {
            if (config?.DebugVirtualPlayerAiDefault?.Value == true || config?.DebugFakePlayersForTrade?.Value == true)
            {
                return true;
            }
            reason = "交易不可用（未连接到服务器）。";
            return false;
        }

        return true;
    }

        public static void ShowTopMessage(string message)
    {
        if (!UiManager.IsInitialized)
        {
            return;
        }

        UiManager.GetPanel<TopMessagePanel>()?.ShowMessage(message);
    }

        public static void ShowTradePanelMissing()
        => ShowTopMessage("交易界面不可用：未找到 TradePanel 实例（缺少 Prefab/场景 UI）。");
}
