using System;
using Microsoft.Extensions.DependencyInjection;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.UI.Components;

/// <summary>
/// 统一管理 Trade 入口的用户提示文案。
/// 保持字符串集中定义，确保 GapOptions 和 Shop 按钮行为一致。
/// </summary>
internal static class TradeUiMessages
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;



    /// <summary>
    /// 检查交易功能是否已启用且已连接到服务器
    /// </summary>
    /// <param name="reason">不可用时的原因描述</param>
    /// <returns>交易可用时返回 true</returns>
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

    /// <summary>
    /// 在屏幕顶部显示提示消息
    /// </summary>
    /// <param name="message">消息内容</param>
    public static void ShowTopMessage(string message)
    {
        if (!UiManager.IsInitialized)
        {
            return;
        }

        UiManager.GetPanel<TopMessagePanel>()?.ShowMessage(message);
    }

    /// <summary>
    /// 显示交易面板缺失的提示
    /// </summary>
    public static void ShowTradePanelMissing()
        => ShowTopMessage("交易界面不可用：未找到 TradePanel 实例（缺少 Prefab/场景 UI）。");
}
