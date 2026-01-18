using System;
using System.Threading;
using HarmonyLib;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Panels;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Server;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.Patch.UI;

/// <summary>
/// 在主菜单增加“多人游戏”入口（参考 Together in Spire: MainMenuButtonsPatch / MainMenuPanelPatch）。
/// </summary>
/// <remarks>
/// 实现方式：
/// - 在 <see cref="MainMenuPanel"/> 中克隆一个模板按钮作为“多人游戏”按钮。
/// - 点击后提供 Host / Join 两条快捷路径：
///   - 确认：启动本机服务器并连接（Host）
///   - 取消：连接到配置的服务器（Join）
/// </remarks>
[HarmonyPatch]
public static class MainMenuMultiplayerEntryPatch
{
    #region 常量与字段

    private const string MultiplayerButtonName = "NetworkPlugin_MultiplayerButton";
    private const string SingleplayerText = "单人游戏";

    private static Button _multiplayerButton;

    private static NetworkServer _localServer;
    private static bool _localServerRunning;
    private static Thread _localServerThread;
    private static CancellationTokenSource _localServerCts;

    #endregion

    #region 依赖注入获取

    /// <summary>
    /// 依赖注入服务提供者。
    /// </summary>
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 尝试从依赖注入解析网络客户端。
    /// </summary>
    /// <returns>解析成功返回 <see cref="INetworkClient"/>，失败返回 null。</returns>
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

    /// <summary>
    /// 获取配置管理器（优先从依赖注入解析，失败则回落到插件静态实例）。
    /// </summary>
    /// <returns>配置管理器实例。</returns>
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

    #endregion

    #region Harmony 补丁入口

    /// <summary>
    /// 主菜单 Awake 后置：确保多人游戏按钮存在，并尝试重命名单人按钮文案。
    /// </summary>
    /// <param name="__instance">主菜单面板实例。</param>
    [HarmonyPatch(typeof(MainMenuPanel), "Awake")]
    [HarmonyPostfix]
    public static void MainMenuPanel_Awake_Postfix(MainMenuPanel __instance)
    {
        try
        {
            // 确保按钮已创建。
            EnsureMultiplayerButton(__instance);

            // 尝试统一单人按钮文案。
            TryRenameSingleplayerButtons(__instance);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[MainMenuMultiplayerEntry] 添加按钮失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 主菜单刷新档案后置：确保按钮存在并重新显示。
    /// </summary>
    /// <param name="__instance">主菜单面板实例。</param>
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
            // 忽略：刷新过程中失败不影响主菜单可用性。
        }
    }

    #endregion

    #region 按钮构建与文案

    /// <summary>
    /// 确保“多人游戏”按钮被创建并挂载到主菜单按钮组。
    /// </summary>
    /// <param name="panel">主菜单面板。</param>
    private static void EnsureMultiplayerButton(MainMenuPanel panel)
    {
        // 面板为空时直接返回。
        if (panel == null)
        {
            return;
        }

        // 已创建则不重复创建。
        if (_multiplayerButton != null)
        {
            return;
        }

        // 从主菜单中取一个现有按钮作为模板（通常是 newGameButton）。
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

        // 克隆模板按钮并替换点击回调。
        _multiplayerButton = UnityEngine.Object.Instantiate(template, parent);
        _multiplayerButton.name = MultiplayerButtonName;

        _multiplayerButton.onClick.RemoveAllListeners();
        _multiplayerButton.onClick.AddListener(OpenMultiplayerEntry);

        // 设置按钮文案。
        TrySetButtonText(_multiplayerButton, "多人游戏");
        TrySetButtonText(template, SingleplayerText);

        // 让多人按钮紧挨模板按钮之后。
        int sibling = template.transform.GetSiblingIndex();
        _multiplayerButton.transform.SetSiblingIndex(Mathf.Min(sibling + 1, parent.childCount - 1));
        _multiplayerButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// 尝试把单人相关按钮的文字统一命名。
    /// </summary>
    /// <param name="panel">主菜单面板。</param>
    private static void TryRenameSingleplayerButtons(MainMenuPanel panel)
    {
        if (panel == null)
        {
            return;
        }

        try
        {
            // 通过字段名获取按钮引用。
            Button newGame = Traverse.Create(panel).Field("newGameButton").GetValue<Button>();
            Button restore = Traverse.Create(panel).Field("restoreGameButton").GetValue<Button>();
            Button abandon = Traverse.Create(panel).Field("abandonGameButton").GetValue<Button>();

            // 统一按钮文案。
            TrySetButtonText(newGame, SingleplayerText);
            TrySetButtonText(restore, $"{SingleplayerText} - 继续");
            TrySetButtonText(abandon, $"{SingleplayerText} - 放弃");
        }
        catch
        {
            // 忽略：UI 结构变化时不强依赖此功能。
        }
    }

    /// <summary>
    /// 尝试设置按钮上的 TextMeshPro 文本。
    /// </summary>
    /// <param name="button">目标按钮。</param>
    /// <param name="text">设置的文本。</param>
    private static void TrySetButtonText(Button button, string text)
    {
        try
        {
            if (button == null)
            {
                return;
            }

            // 主菜单按钮通常使用 TextMeshProUGUI。
            var label = button.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = text;
            }
        }
        catch
        {
            // 忽略：设置 UI 文案失败不影响整体流程。
        }
    }

    #endregion

    #region 弹窗流程

    /// <summary>
    /// 打开“多人游戏”入口弹窗。
    /// </summary>
    private static void OpenMultiplayerEntry()
    {
        // 若已连接，则提示是否断开。
        INetworkClient client = TryGetNetworkClient();
        if (client?.IsConnected == true)
        {
            ShowConnectedDialog(client);
            return;
        }

        // 未连接：提示选择 Host/Join。
        UiManager.GetDialog<MessageDialog>().Show(
            new MessageContent
            {
                Text = "选择联机方式：\n\n- 确认：作为房主启动本机服务器并连接\n- 取消：作为客户端加入配置的服务器",
                Icon = MessageIcon.Warning,
                Buttons = DialogButtons.ConfirmCancel,
                OnConfirm = TryHostLocalServerAndConnect,
                OnCancel = ShowJoinConfirmDialog,
            }
        );
    }

    /// <summary>
    /// 已处于联机状态时的提示弹窗。
    /// </summary>
    /// <param name="client">网络客户端。</param>
    private static void ShowConnectedDialog(INetworkClient client)
    {
        UiManager.GetDialog<MessageDialog>().Show(
            new MessageContent
            {
                Text = "当前已处于联机状态。\n\n确认：断开联机\n取消：关闭",
                Icon = MessageIcon.Warning,
                Buttons = DialogButtons.ConfirmCancel,
                OnConfirm = () => Disconnect(client),
                OnCancel = null,
            }
        );
    }

    /// <summary>
    /// 加入服务器确认弹窗（显示将连接的地址与端口）。
    /// </summary>
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
                OnCancel = null,
            }
        );
    }

    #endregion

    #region 连接与断开

    /// <summary>
    /// 尝试启动本机服务器并连接（Host 流程）。
    /// </summary>
    private static void TryHostLocalServerAndConnect()
    {
        ConfigManager config = TryGetConfig();
        int port = config?.ServerPort?.Value ?? 7777;
        int maxConn = config?.RelayServerMaxConnections?.Value ?? 8;
        string key = config?.RelayServerConnectionKey?.Value ?? "LBoL_Network_Plugin";

        try
        {
            // 若本机服务器未运行，则启动并开启轮询线程。
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
            Plugin.Logger?.LogError($"[MainMenuMultiplayerEntry] 启动本机服务器失败: {ex.Message}");
            UiManager.GetDialog<MessageDialog>().Show(
                new MessageContent
                {
                    Text = "启动本机服务器失败，请检查日志。",
                    Icon = MessageIcon.Warning,
                    Buttons = DialogButtons.Confirm,
                }
            );
            return;
        }

        // 启动成功后连接本机。
        TryConnectToServer("127.0.0.1", port);
    }

    /// <summary>
    /// 尝试连接到指定服务器（Join/Host 共用）。
    /// </summary>
    /// <param name="host">服务器地址。</param>
    /// <param name="port">服务器端口。</param>
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
                    Buttons = DialogButtons.Confirm,
                }
            );
            return;
        }

        // 确保客户端已启动（重复启动可能抛异常，因此做容错）。
        try
        {
            client.Start();
        }
        catch
        {
            // 忽略：可能已启动。
        }

        // 发起连接。
        try
        {
            client.ConnectToServer(host, port);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[MainMenuMultiplayerEntry] 连接失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 断开连接，并停止本机服务器轮询与实例。
    /// </summary>
    /// <param name="client">网络客户端。</param>
    private static void Disconnect(INetworkClient client)
    {
        try
        {
            client?.Stop();
        }
        catch
        {
            // 忽略：断开失败不影响后续资源回收。
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
            // 忽略：停止服务器失败时继续清理引用。
        }
        finally
        {
            _localServer = null;
            _localServerRunning = false;
        }
    }

    #endregion

    #region 本机服务器轮询线程

    /// <summary>
    /// 启动本机服务器事件轮询线程。
    /// </summary>
    private static void StartLocalServerLoop()
    {
        try
        {
            // 避免重复启动。
            StopLocalServerLoop();

            _localServerCts = new CancellationTokenSource();
            CancellationToken token = _localServerCts.Token;

            // 轮询线程：周期性调用服务器 PollEvents。
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
                        // 忽略：单次轮询失败不应终止线程。
                    }

                    Thread.Sleep(15);
                }
            })
            {
                IsBackground = true,
                Name = "NetworkPlugin.LocalServerPoll",
            };

            _localServerThread.Start();
        }
        catch
        {
            // 忽略：启动轮询失败不应导致主菜单不可用。
        }
    }

    /// <summary>
    /// 停止本机服务器事件轮询线程并释放取消令牌。
    /// </summary>
    private static void StopLocalServerLoop()
    {
        try
        {
            _localServerCts?.Cancel();
        }
        catch
        {
            // 忽略：取消失败继续进行 Join/Dispose。
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
            // 忽略：Join 失败不阻断清理。
        }
        finally
        {
            _localServerThread = null;
            _localServerCts?.Dispose();
            _localServerCts = null;
        }
    }

    #endregion
}
