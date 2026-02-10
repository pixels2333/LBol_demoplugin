using System;
using Microsoft.Extensions.DependencyInjection;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.UI;

/// <summary>
/// Shared, consistent user-facing messages for Trade entry points.
/// Keep strings centralized so GapOptions and Shop button behave the same.
/// </summary>
internal static class TradeUiMessages
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static ConfigManager TryGetConfig()
    {
        try
        {
            return ServiceProvider?.GetService<ConfigManager>();
        }
        catch
        {
            return null;
        }
    }

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

    public static bool IsTradeEnabledAndConnected(out string reason)
    {
        reason = null;

        var config = TryGetConfig();
        if (config?.AllowTrading?.Value != true)
        {
            reason = "交易功能已在配置中禁用。";
            return false;
        }

        var client = TryGetNetworkClient();
        if (client == null || !client.IsConnected)
        {
            reason = "交易不可用（未连接到服务器）。";
            return false;
        }

        return true;
    }

    public static void ShowTopMessage(string message)
    {
        try
        {
            if (!UiManager.IsInitialized)
            {
                return;
            }

            UiManager.GetPanel<TopMessagePanel>().ShowMessage(message);
        }
        catch
        {
            // ignored
        }
    }

    public static void ShowTradePanelMissing()
        => ShowTopMessage("交易界面不可用：未找到 TradePanel 实例（缺少 Prefab/场景 UI）。");
}
