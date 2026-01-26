using System;
using System.Reflection;
using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Panels;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using UnityEngine.UI;

namespace NetworkPlugin.Patch.UI;

/// <summary>
/// 退出 / 返回主菜单相关补丁（参照 Together in Spire: ExitGamePatch.java）
///
/// 主要功能：
/// - 联机时隐藏“放弃存档 / 弃档”等危险入口，避免误操作影响所有玩家。
/// - 联机时将“退出游戏 / 返回主菜单”改为“断开联机并返回主菜单”，并弹出警告提示。
/// - 拦截系统退出（GameMaster.OnWantsToQuit），在联机中禁止直接关闭进程，强制使用“安全退出”流程。
/// </summary>
[HarmonyPatch]
public static class ExitGamePatch
{
    #region 依赖注入服务访问

    /// <summary>
    /// 全局服务提供者入口，由 Mod 框架注入。
    /// 用于解析网络客户端和网络管理器等依赖。
    /// </summary>
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 尝试从 DI 容器中获取 <see cref="INetworkClient"/> 实例。
    /// 捕获所有异常，防止因依赖未注册导致补丁崩溃。
    /// </summary>
    private static INetworkClient TryGetNetworkClient()
    {
        try
        {
            return ServiceProvider?.GetService<INetworkClient>();
        }
        catch
        {
            // 如果解析失败，返回 null 即视为未联机。
            return null;
        }
    }

    /// <summary>
    /// 尝试从 DI 容器中获取 <see cref="INetworkManager"/> 实例。
    /// 捕获所有异常，保证补丁的健壮性。
    /// </summary>
    private static INetworkManager TryGetNetworkManager()
    {
        try
        {
            return ServiceProvider?.GetService<INetworkManager>();
        }
        catch
        {
            // 解析失败则视为无网络管理器。
            return null;
        }
    }

    #endregion

    #region 联机状态辅助方法

    /// <summary>
    /// 当前是否处于联机已连接状态。
    /// 只要存在网络客户端且 <c>IsConnected == true</c> 即视为联机中。
    /// </summary>
    private static bool IsMultiplayerConnected()
    {
        return TryGetNetworkClient()?.IsConnected == true;
    }

    /// <summary>
    /// 尝试判断本地客户端是否为房主（Host）。
    /// 通过反射访问 INetworkClient 的 <c>IsHost</c> 属性或字段，避免对具体实现产生强耦合。
    /// 出现异常或找不到字段时，统一视为非房主。
    /// </summary>
    /// <returns>若能成功获取并为 true 则表示是房主，否则为 false。</returns>
    private static bool TryIsHost()
    {
        try
        {
            INetworkClient client = TryGetNetworkClient();
            if (client == null)
                return false;

            Type clientType = client.GetType();

            // 先尝试获取属性 IsHost
            PropertyInfo prop = clientType.GetProperty(
                "IsHost",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );
            if (prop?.PropertyType == typeof(bool))
            {
                return (bool)prop.GetValue(client);
            }

            // 如果没有属性，再尝试字段 IsHost
            FieldInfo field = clientType.GetField(
                "IsHost",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );
            if (field?.FieldType == typeof(bool))
            {
                return (bool)field.GetValue(client);
            }
        }
        catch
        {
            // 任何反射错误都忽略，默认非房主。
        }

        return false;
    }

    #endregion

    #region 断开联机与返回主菜单流程

    /// <summary>
    /// 断开联机连接，并清空网络管理器中记录的所有玩家信息。
    /// - 调用 INetworkClient.Stop() 结束网络会话。
    /// - 调用 INetworkManager.ClearAllPlayers() 清理玩家列表（通过反射，避免硬依赖接口）。
    /// </summary>
    private static void DisconnectMultiplayer()
    {
        try
        {
            // 尝试停止网络客户端（无论是 Host 还是 Client）。
            TryGetNetworkClient()?.Stop();

            Plugin.Logger?.LogInfo("[退出/返回主菜单] 已断开联机连接");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[退出/返回主菜单] 停止网络客户端失败: {ex.Message}");
        }

        try
        {
            INetworkManager manager = TryGetNetworkManager();
            if (manager == null)
                return;

            // 通过反射调用 ClearAllPlayers，兼容不同实现。
            MethodInfo clear = manager
                .GetType()
                .GetMethod(
                    "ClearAllPlayers",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
            clear?.Invoke(manager, null);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[退出/返回主菜单] 清理玩家列表失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 执行“断开联机并返回主菜单”的完整流程。
    /// - 先断开所有联机连接并清理网络状态；
    /// - 若当前在游戏局内，则调用 <see cref="GameMaster.LeaveGameRun"/> 退出游戏局；
    /// - 否则刷新主菜单用户档案信息。
    /// </summary>
    private static void DisconnectAndReturnToMainMenu()
    {
        // 先强制断开联机并清理网络数据。
        DisconnectMultiplayer();

        try
        {
            // 如果当前有正在进行的 GameRun，则通过游戏逻辑退出游戏局。
            if (Singleton<GameMaster>.Instance?.CurrentGameRun != null)
            {
                Plugin.Logger?.LogInfo("[退出/返回主菜单] 当前在游戏局内，开始退出并返回主菜单");
                GameMaster.LeaveGameRun();
            }
            else
            {
                // 不在局内时，刷新主菜单档案信息，保证显示为最新状态。
                Plugin.Logger?.LogInfo("[退出/返回主菜单] 当前不在游戏局内，刷新主菜单档案信息");
                UiManager.GetPanel<MainMenuPanel>()?.RefreshProfile();
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[退出/返回主菜单] 返回主菜单失败: {ex.Message}");
        }
    }

    #endregion

    #region 弹窗辅助方法

    /// <summary>
    /// 在联机状态下弹出退出确认弹窗。
    /// 根据是否为房主显示不同提醒文案，并在确认时执行 <see cref="DisconnectAndReturnToMainMenu"/>。
    /// </summary>
    /// <param name="textKey">
    /// 弹窗主文案对应的本地化 key（例如 "QuitGame" / "ReturnToMainMenu"）。
    /// </param>
    private static void ShowConnectedQuitDialog(string textKey)
    {
        try
        {
            bool isHost = TryIsHost();

            // 房主退出会让整个房间解散，非房主退出只断开本地联机并返回主菜单。
            string subText = isHost
                ? "联机模式：你是房主，退出将导致房间关闭并影响所有已连接玩家。\n是否继续？"
                : "联机模式：退出将断开联机并返回主菜单。\n之后可以重新连接继续游戏。\n是否继续？";

            UiManager.GetDialog<MessageDialog>().Show(
                new MessageContent
                {
                    TextKey = textKey,
                    SubText = subText,
                    Buttons = DialogButtons.ConfirmCancel,
                    Icon = MessageIcon.Warning,
                    // 确认退出时进行安全的断开与回主菜单流程。
                    OnConfirm = DisconnectAndReturnToMainMenu
                }
            );
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[退出/返回主菜单] 显示退出确认弹窗失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 在联机状态下点击“放弃存档 / 弃档”时弹出的阻止弹窗。
    /// 告知玩家该功能在联机模式中被禁用，需要先断开联机再进行。
    /// </summary>
    private static void ShowConnectedAbandonBlockedDialog()
    {
        try
        {
            UiManager.GetDialog<MessageDialog>().Show(
                new MessageContent
                {
                    Text = "联机模式下已禁用“放弃存档/弃档”。\n请先断开联机后再进行该操作。",
                    Buttons = DialogButtons.Confirm,
                    Icon = MessageIcon.Warning
                }
            );
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[退出/返回主菜单] 显示弃档拦截弹窗失败: {ex.Message}");
        }
    }

    #endregion

    #region 主菜单面板补丁

    /// <summary>
    /// 主菜单档案刷新后的补丁。
    /// 在联机状态下隐藏“放弃存档”按钮，以防止玩家在联机中误弃档。
    /// </summary>
    /// <param name="__instance">被补丁的主菜单面板实例。</param>
    [HarmonyPatch(typeof(MainMenuPanel), "RefreshProfile")]
    [HarmonyPostfix]
    public static void MainMenuPanel_RefreshProfile_Postfix(MainMenuPanel __instance)
    {
        try
        {
            // 非联机状态下不做任何修改，保持原生行为。
            if (!IsMultiplayerConnected())
                return;

            // 通过 Traverse 获取私有字段 abandonGameButton，并隐藏其 GameObject。
            Button abandonButton = Traverse.Create(__instance)
                .Field("abandonGameButton")
                .GetValue<Button>();
            abandonButton?.gameObject.SetActive(false);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[退出/返回主菜单] RefreshProfile 补丁失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 主菜单中“放弃存档 / 弃档”点击事件前置补丁。
    /// 联机时阻止原逻辑执行，并弹出功能被禁用提示。
    /// </summary>
    /// <returns>
    /// 返回 true 继续执行原方法；返回 false 阻止原方法。
    /// </returns>
    [HarmonyPatch(typeof(MainMenuPanel), "UI_AbandonGameClicked")]
    [HarmonyPrefix]
    public static bool MainMenuPanel_UI_AbandonGameClicked_Prefix()
    {
        // 非联机：尊重原行为。
        if (!IsMultiplayerConnected())
            return true;

        // 联机：阻止弃档，改为弹出说明弹窗。
        ShowConnectedAbandonBlockedDialog();
        return false;
    }

    /// <summary>
    /// 主菜单中“退出游戏”按钮点击前置补丁。
    /// 联机时改为安全退出流程（弹窗确认 + 断开联机 + 回主菜单）。
    /// </summary>
    [HarmonyPatch(typeof(MainMenuPanel), "UI_QuitGame")]
    [HarmonyPrefix]
    public static bool MainMenuPanel_UI_QuitGame_Prefix()
    {
        if (!IsMultiplayerConnected())
            return true;

        // 联机状态下，弹出带有联机警告的退出确认框。
        ShowConnectedQuitDialog("QuitGame");
        return false;
    }

    #endregion

    #region 设置面板补丁

    /// <summary>
    /// 设置面板中的“返回主菜单（离开本局）”点击前置补丁。
    /// 在联机中，统一走带提示的断开联机流程。
    /// </summary>
    [HarmonyPatch(typeof(SettingPanel), "UI_LeaveGameRun")]
    [HarmonyPrefix]
    public static bool SettingPanel_UI_LeaveGameRun_Prefix()
    {
        if (!IsMultiplayerConnected())
            return true;

        // 弹出“返回主菜单”的联机提示弹窗。
        ShowConnectedQuitDialog("ReturnToMainMenu");
        return false;
    }

    /// <summary>
    /// 设置面板中的“退出游戏”点击前置补丁。
    /// 非联机时保持原逻辑，联机时改为安全退出。
    /// </summary>
    [HarmonyPatch(typeof(SettingPanel), "UI_Quit")]
    [HarmonyPrefix]
    public static bool SettingPanel_UI_Quit_Prefix()
    {
        if (!IsMultiplayerConnected())
            return true;

        ShowConnectedQuitDialog("QuitGame");
        return false;
    }

    #endregion

    #region 游戏主控 GameMaster 补丁

    /// <summary>
    /// 游戏想要退出（例如 Alt+F4 或系统关闭窗口）时的前置补丁。
    /// - 单机：允许按原逻辑处理；
    /// - 联机：禁止直接退出进程，改为弹出带联机提示的退出确认弹窗。
    /// </summary>
    /// <param name="__result">原方法的返回值；这里会在拦截时主动设置为 false。</param>
    /// <returns>
    /// true 表示继续执行原 OnWantsToQuit；
    /// false 表示已处理并阻止原实现。
    /// </returns>
    [HarmonyPatch(typeof(GameMaster), nameof(GameMaster.OnWantsToQuit))]
    [HarmonyPrefix]
    public static bool GameMaster_OnWantsToQuit_Prefix(ref bool __result)
    {
        if (!IsMultiplayerConnected())
            return true;

        // 联机中禁止直接退出，标记为不退出，并弹出带提示的确认框。
        __result = false;
        ShowConnectedQuitDialog("QuitGame");
        return false;
    }

    /// <summary>
    /// 游戏层面的 QuitGame 调用前置补丁。
    /// 在联机时将其重定向到“断开联机并返回主菜单”流程，
    /// 避免出现未正常清理网络状态就直接退出的情况。
    /// </summary>
    [HarmonyPatch(typeof(GameMaster), nameof(GameMaster.QuitGame))]
    [HarmonyPrefix]
    public static bool GameMaster_QuitGame_Prefix()
    {
        if (!IsMultiplayerConnected())
            return true;

        // 联机中拦截原生退出流程，改为先断开联机再回主菜单。
        DisconnectAndReturnToMainMenu();
        return false;
    }

    #endregion
}

