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
            reason = "Trading is disabled in config.";
            return false;
        }

        var client = TryGetNetworkClient();
        if (client == null || !client.IsConnected)
        {
            reason = "Trading is not available (not connected).";
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
        => ShowTopMessage("Trade UI is not available: TradePanel instance not found (missing prefab/scene UI).");
}
