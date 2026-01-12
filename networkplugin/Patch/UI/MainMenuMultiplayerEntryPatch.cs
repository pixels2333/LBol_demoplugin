using System;
using System.Threading;
using HarmonyLib;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Server;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.Patch.UI;

/// <summary>
/// 主菜单增加“多人游戏”入口（参照 Together in Spire: MainMenuButtonsPatch / MainMenuPanelPatch）。
/// - 在 <see cref="MainMenuPanel"/> 主菜单按钮组中克隆一个按钮作为“多人游戏”入口。
/// - 点击后提供 Host / Join 两种快速流程（使用配置的 ServerIP/ServerPort）。
/// </summary>
[HarmonyPatch]
public static class MainMenuMultiplayerEntryPatch
{
    private const string MultiplayerButtonName = "NetworkPlugin_MultiplayerButton";
    private const string SingleplayerText = "单人游戏";

    private static Button _multiplayerButton;
    private static NetworkServer _localServer;
    private static bool _localServerRunning;
    private static Thread _localServerThread;
    private static CancellationTokenSource _localServerCts;

    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

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

    private static ConfigManager TryGetConfig()
    {
        try
        {
            return ServiceProvider?.GetService<ConfigManager>() ?? Plugin.ConfigManager;
        }
        catch
        {
            return Plugin.ConfigManager;
        }
    }

    [HarmonyPatch(typeof(MainMenuPanel), "Awake")]
    [HarmonyPostfix]
    public static void MainMenuPanel_Awake_Postfix(MainMenuPanel __instance)
    {
        try
        {
            EnsureMultiplayerButton(__instance);
            TryRenameSingleplayerButtons(__instance);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[MainMenuMultiplayerEntry] Failed to add button: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(MainMenuPanel), "RefreshProfile")]
    [HarmonyPostfix]
    public static void MainMenuPanel_RefreshProfile_Postfix(MainMenuPanel __instance)
    {
        try
        {
            EnsureMultiplayerButton(__instance);
            TryRenameSingleplayerButtons(__instance);
            _multiplayerButton?.gameObject.SetActive(true);
        }
        catch
        {
            // ignored
        }
    }

    private static void EnsureMultiplayerButton(MainMenuPanel panel)
    {
        if (panel == null)
        {
            return;
        }

        if (_multiplayerButton != null)
        {
            return;
        }

        Button template = Traverse.Create(panel).Field("newGameButton").GetValue<Button>();
        if (template == null)
        {
            return;
        }

        Transform parent = template.transform.parent;
        if (parent == null)
        {
            return;
        }

        _multiplayerButton = UnityEngine.Object.Instantiate(template, parent);
        _multiplayerButton.name = MultiplayerButtonName;

        _multiplayerButton.onClick.RemoveAllListeners();
        _multiplayerButton.onClick.AddListener(OpenMultiplayerEntry);

        TrySetButtonText(_multiplayerButton, "多人游戏");
        TrySetButtonText(template, SingleplayerText);

        int sibling = template.transform.GetSiblingIndex();
        _multiplayerButton.transform.SetSiblingIndex(Mathf.Min(sibling + 1, parent.childCount - 1));
        _multiplayerButton.gameObject.SetActive(true);
    }

    private static void TryRenameSingleplayerButtons(MainMenuPanel panel)
    {
        if (panel == null)
        {
            return;
        }

        try
        {
            Button newGame = Traverse.Create(panel).Field("newGameButton").GetValue<Button>();
            Button restore = Traverse.Create(panel).Field("restoreGameButton").GetValue<Button>();
            Button abandon = Traverse.Create(panel).Field("abandonGameButton").GetValue<Button>();

            TrySetButtonText(newGame, SingleplayerText);
            TrySetButtonText(restore, $"{SingleplayerText} - 继续");
            TrySetButtonText(abandon, $"{SingleplayerText} - 放弃");
        }
        catch
        {
            // ignored
        }
    }

    private static void TrySetButtonText(Button button, string text)
    {
        try
        {
            if (button == null)
            {
                return;
            }

            var label = button.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = text;
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void OpenMultiplayerEntry()
    {
        INetworkClient client = TryGetNetworkClient();
        if (client?.IsConnected == true)
        {
            ShowConnectedDialog(client);
            return;
        }

        UiManager.GetDialog<MessageDialog>().Show(
            new MessageContent
            {
                Text = "选择联机方式：\n\n- 确认：作为房主启动本机服务器并连接\n- 取消：作为客户端加入配置的服务器",
                Icon = MessageIcon.Warning,
                Buttons = DialogButtons.ConfirmCancel,
                OnConfirm = TryHostLocalServerAndConnect,
                OnCancel = ShowJoinConfirmDialog
            }
        );
    }

    private static void ShowConnectedDialog(INetworkClient client)
    {
        UiManager.GetDialog<MessageDialog>().Show(
            new MessageContent
            {
                Text = "当前已处于联机状态。\n\n确认：断开联机\n取消：关闭",
                Icon = MessageIcon.Warning,
                Buttons = DialogButtons.ConfirmCancel,
                OnConfirm = () => Disconnect(client),
                OnCancel = null
            }
        );
    }

    private static void ShowJoinConfirmDialog()
    {
        ConfigManager config = TryGetConfig();
        string ip = config?.ServerIP?.Value ?? "127.0.0.1";
        int port = config?.ServerPort?.Value ?? 7777;

        UiManager.GetDialog<MessageDialog>().Show(
            new MessageContent
            {
                Text = $"将作为客户端加入服务器：{ip}:{port}\n\n确认：开始连接\n取消：关闭",
                Icon = MessageIcon.Warning,
                Buttons = DialogButtons.ConfirmCancel,
                OnConfirm = () => TryConnectToServer(ip, port),
                OnCancel = null
            }
        );
    }

    private static void TryHostLocalServerAndConnect()
    {
        ConfigManager config = TryGetConfig();
        int port = config?.ServerPort?.Value ?? 7777;
        int maxConn = config?.RelayServerMaxConnections?.Value ?? 8;
        string key = config?.RelayServerConnectionKey?.Value ?? "LBoL_Network_Plugin";

        try
        {
            if (!_localServerRunning)
            {
                _localServer = new NetworkServer(port, maxConn, key, Plugin.Logger);
                _localServer.Start();
                _localServerRunning = true;
                StartLocalServerLoop();
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[MainMenuMultiplayerEntry] Failed to start local server: {ex.Message}");
            UiManager.GetDialog<MessageDialog>().Show(
                new MessageContent
                {
                    Text = "启动本机服务器失败，请检查日志。",
                    Icon = MessageIcon.Warning,
                    Buttons = DialogButtons.Confirm
                }
            );
            return;
        }

        TryConnectToServer("127.0.0.1", port);
    }

    private static void TryConnectToServer(string host, int port)
    {
        INetworkClient client = TryGetNetworkClient();
        if (client == null)
        {
            UiManager.GetDialog<MessageDialog>().Show(
                new MessageContent
                {
                    Text = "网络客户端未初始化（INetworkClient 解析失败）。\n请先确认依赖注入与网络模块已就绪。",
                    Icon = MessageIcon.Warning,
                    Buttons = DialogButtons.Confirm
                }
            );
            return;
        }

        try
        {
            client.Start();
        }
        catch
        {
            // ignored: may already started
        }

        try
        {
            client.ConnectToServer(host, port);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[MainMenuMultiplayerEntry] Connect failed: {ex.Message}");
        }
    }

    private static void Disconnect(INetworkClient client)
    {
        try
        {
            client?.Stop();
        }
        catch
        {
            // ignored
        }

        StopLocalServerLoop();
        try
        {
            if (_localServerRunning)
            {
                _localServer?.Stop();
            }
        }
        catch
        {
            // ignored
        }
        finally
        {
            _localServer = null;
            _localServerRunning = false;
        }
    }

    private static void StartLocalServerLoop()
    {
        try
        {
            StopLocalServerLoop();

            _localServerCts = new CancellationTokenSource();
            CancellationToken token = _localServerCts.Token;

            _localServerThread = new Thread(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        _localServer?.PollEvents();
                    }
                    catch
                    {
                        // ignored
                    }

                    Thread.Sleep(15);
                }
            })
            {
                IsBackground = true,
                Name = "NetworkPlugin.LocalServerPoll"
            };

            _localServerThread.Start();
        }
        catch
        {
            // ignored
        }
    }

    private static void StopLocalServerLoop()
    {
        try
        {
            _localServerCts?.Cancel();
        }
        catch
        {
            // ignored
        }

        try
        {
            if (_localServerThread != null && _localServerThread.IsAlive)
            {
                _localServerThread.Join(200);
            }
        }
        catch
        {
            // ignored
        }
        finally
        {
            _localServerThread = null;
            _localServerCts?.Dispose();
            _localServerCts = null;
        }
    }
}
