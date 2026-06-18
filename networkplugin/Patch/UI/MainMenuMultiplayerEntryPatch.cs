using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.SaveData;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Panels;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Server;
using TMPro;
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
    private const string LegacySubMenuMultiplayerButtonName = "NetworkPlugin_SubMenuMultiplayerButton";

    private const string OverlayRootName = "NetworkPlugin_MultiplayerOverlay";
    private const string StartGameMultiplayerButtonName = "NetworkPlugin_StartGameMultiplayerButton";

    private static Button _multiplayerButton;
    private static Button _subMenuMultiplayerButton;
    private static MainMenuPanel _lastMainMenuPanel;
    private static Button _startGameMultiplayerButton;
    private static StartGamePanel _lastStartGamePanel;
    private static GameObject _overlayRoot;
    private static PanelAnimator _rootAnimator;
    private static TMP_FontAsset _defaultFont;

    private static NetworkServer _localServer;
    private static bool _localServerRunning;
    public static bool IsLocalServerRunning => _localServerRunning;
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
        => ServiceProvider?.GetService<INetworkClient>();

    /// <summary>
    /// 获取配置管理器（优先从依赖注入解析，失败则回落到插件静态实例）。
    /// </summary>
    /// <returns>配置管理器实例。</returns>
    private static ConfigManager TryGetConfig()
        => ServiceProvider?.GetService<ConfigManager>() ?? Plugin.ConfigManager;

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
            _lastMainMenuPanel = __instance;

            // 主界面入口放到左侧主列表（设定/收集总览/历史详细）中。
            EnsureMultiplayerButton(__instance);
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
            _lastMainMenuPanel = __instance;
            EnsureMultiplayerButton(__instance);
            _multiplayerButton?.gameObject.SetActive(true);
        }
        catch
        {
            // TODO: 应记录异常详情，避免静默失败。
            // 忽略：刷新过程中失败不影响主菜单可用性。
        }
    }

    /// <summary>
    /// 主菜单语言切换后置：部分按钮文案由本地化组件刷新，这里确保“多人游戏”文案不会被覆盖。
    /// </summary>
    [HarmonyPatch(typeof(MainMenuPanel), "OnLocaleChanged")]
    [HarmonyPostfix]
    public static void MainMenuPanel_OnLocaleChanged_Postfix(MainMenuPanel __instance)
    {
        try
        {
            _lastMainMenuPanel = __instance;
            EnsureMultiplayerButton(__instance);
            _multiplayerButton?.gameObject.SetActive(true);
        }
        catch
        {
            // TODO: 应记录异常详情，避免静默失败。
            // ignored
        }
    }

    /// <summary>
    /// StartGamePanel Awake 后置：在“新游戏/角色选择”面板上追加“多人游戏”入口。
    /// </summary>
    [HarmonyPatch(typeof(StartGamePanel), "Awake")]
    [HarmonyPostfix]
    public static void StartGamePanel_Awake_Postfix(StartGamePanel __instance)
    {
        try
        {
            _lastStartGamePanel = __instance;
            EnsureStartGameMultiplayerButton(__instance);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[MainMenuMultiplayerEntry] StartGamePanel_Awake_Postfix 失败: {ex.Message}");
        }
    }

    /// <summary>
    /// StartGamePanel 显示后置：确保多人入口按钮存在并可见。
    /// </summary>
    [HarmonyPatch(typeof(StartGamePanel), "OnShowing")]
    [HarmonyPostfix]
    public static void StartGamePanel_OnShowing_Postfix(StartGamePanel __instance, StartGameData data)
    {
        try
        {
            _lastStartGamePanel = __instance;
            EnsureStartGameMultiplayerButton(__instance);
            _startGameMultiplayerButton?.gameObject.SetActive(true);
        }
        catch
        {
            // TODO: 应记录异常详情，避免静默失败。
            // ignored
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

        // 旧版本误把按钮插到 subMenuButtonGroup，这里清掉我们自己创建的那个，避免用户找不到入口还看到“幽灵按钮”。
        CleanupLegacySubMenuMultiplayerButton(panel);

        // 如果上次缓存的按钮已被销毁（UnityEngine.Object 特殊 null 语义），则清理引用并重新创建。
        if (_multiplayerButton == null)
        {
            // 继续向下执行，尝试重新创建按钮。
        }
        else
        {
            // 按钮还活着：确保可见、文案正确，并尽量保持在期望的位置。
            _multiplayerButton.gameObject.SetActive(true);
            TrySetButtonText(_multiplayerButton, "多人游戏");
            EnsureMultiplayerButtonOrder(panel, _multiplayerButton);
            return;
        }

        Transform parent = TryGetMainMenuButtonGroup(panel);
        if (parent == null)
        {
            Plugin.Logger?.LogWarning("[MainMenuMultiplayerEntry] 未能定位 mainMenuButtonGroup，无法创建多人入口按钮。");
            return;
        }

        // 主列表模板按钮：优先用“设定(UI_Settings)”，否则退回 newGameButton / 扫描。
        Button template = TryFindButtonByPersistentMethodName(parent, "UI_Settings");
        if (template == null)
        {
            if (!TryFindTemplateButton(panel, out template, out string whyTemplateFailed))
            {
                Plugin.Logger?.LogWarning($"[MainMenuMultiplayerEntry] 未能定位主菜单模板按钮，无法创建入口：{whyTemplateFailed}");
                return;
            }
        }

        _defaultFont ??= FindDefaultFont(parent);

        // 克隆模板按钮并替换点击回调。
        _multiplayerButton = CreateButtonFromTemplate(template, parent, MultiplayerButtonName, "多人游戏");
        _multiplayerButton.name = MultiplayerButtonName;

        // 仅套用样式，不套用行为：重置 UnityEvent，避免把模板按钮(如“设定”)的持久化回调一并带过来。
        _multiplayerButton.onClick = new Button.ButtonClickedEvent();
        _multiplayerButton.onClick.AddListener(OpenMultiplayerEntry);

        // 部分菜单按钮会挂本地化脚本，刷新时会把文案改回模板(例如“设定”)。
        // 只移除本地化相关组件，保留 MainMenuButtonWidget 等动画/交互样式组件。
        TryStripLocalizationComponents(_multiplayerButton.gameObject);

        // 设置按钮文案。
        TrySetButtonText(_multiplayerButton, "多人游戏");

        // 把“多人游戏”插入到“设定”和“收集总览”之间。
        Button museumButton = TryFindButtonByPersistentMethodName(parent, "UI_ShowMuseum");
        if (museumButton != null && museumButton.transform.parent == parent)
        {
            // 对于没有 LayoutGroup 的主菜单列表，光改 siblingIndex 不会改变绝对坐标，按钮会重叠。
            // 我们把“收集总览”及其后续按钮整体下移一个步长，把新按钮放到原本“收集总览”的位置。
            Button settingsButton = TryFindButtonByPersistentMethodName(parent, "UI_Settings");
            if (settingsButton != null && settingsButton.transform.parent == parent)
            {
                if (!TryInsertButtonByShiftingAbsoluteLayout(parent, settingsButton, museumButton, _multiplayerButton))
                {
                    _multiplayerButton.transform.SetSiblingIndex(museumButton.transform.GetSiblingIndex());
                }
            }
            else
            {
                _multiplayerButton.transform.SetSiblingIndex(museumButton.transform.GetSiblingIndex());
            }
        }
        else
        {
            // 兜底：插入到“设定”之后；再兜底到模板按钮之后。
            Button settingsButton = TryFindButtonByPersistentMethodName(parent, "UI_Settings");
            if (settingsButton != null && settingsButton.transform.parent == parent)
            {
                int idx = settingsButton.transform.GetSiblingIndex();
                _multiplayerButton.transform.SetSiblingIndex(Mathf.Min(idx + 1, parent.childCount - 1));
            }
            else
            {
                int sibling = template.transform.GetSiblingIndex();
                _multiplayerButton.transform.SetSiblingIndex(Mathf.Min(sibling + 1, parent.childCount - 1));
            }
        }

        _multiplayerButton.gameObject.SetActive(true);
    }

    private static bool TryInsertButtonByShiftingAbsoluteLayout(Transform parent, Button settingsButton, Button museumButton, Button newButton)
    {
        try
        {
            if (parent == null || settingsButton == null || museumButton == null || newButton == null)
            {
                return false;
            }

            // 如果有 LayoutGroup，Unity 会根据 sibling 顺序自动排版，不需要手动改坐标。
            bool hasLayout = parent.GetComponent<HorizontalLayoutGroup>() != null || parent.GetComponent<VerticalLayoutGroup>() != null;
            if (hasLayout)
            {
                newButton.transform.SetSiblingIndex(museumButton.transform.GetSiblingIndex());
                return true;
            }

            var settingsRect = settingsButton.GetComponent<RectTransform>();
            var museumRect = museumButton.GetComponent<RectTransform>();
            var newRect = newButton.GetComponent<RectTransform>();
            if (settingsRect == null || museumRect == null || newRect == null)
            {
                return false;
            }

            // 计算“每一行”的位移步长（通常是固定的）。
            Vector2 settingsPosOld = settingsRect.anchoredPosition;
            Vector2 museumPos = museumRect.anchoredPosition;
            Vector2 step = museumPos - settingsPosOld;
            if (step.sqrMagnitude < 0.001f)
            {
                return false;
            }

            // 稳定方案：把“收集总览”及其后续按钮整体下移一格，把新按钮放到原“收集总览”的位置。
            // 这样不会改动上方按钮（避免你反馈的“位置偏了”），并且能真正挤出一行。
            Vector2 museumPosOld = museumRect.anchoredPosition;
            int museumIndexOld = museumButton.transform.GetSiblingIndex();

            // 先把新按钮插到“收集总览”前面（层级顺序）。
            newButton.transform.SetSiblingIndex(museumIndexOld);

            // 把新按钮摆到“收集总览”原本的位置。
            newRect.anchoredPosition = museumPosOld;

            // 将“收集总览”以及其后所有按钮下移一个步长，腾出一行。
            int newIndex = newButton.transform.GetSiblingIndex();
            for (int i = newIndex + 1; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (child == newButton.transform)
                {
                    continue;
                }

                var rt = child.GetComponent<RectTransform>();
                if (rt == null)
                {
                    continue;
                }

                rt.anchoredPosition += step;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void TryStripLocalizationComponents(GameObject root)
    {
        try
        {
            if (root == null)
            {
                return;
            }

            // 只根据类型名做弱匹配，避免引入对游戏内部组件的硬依赖。
            // 保留按钮动画/交互组件（CommonButtonWidget/MainMenuButtonWidget 等）。
            var behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var b in behaviours)
            {
                if (b == null)
                {
                    continue;
                }

                string n = b.GetType().Name;
                if (string.IsNullOrWhiteSpace(n))
                {
                    continue;
                }

                // 常见的本地化脚本命名：Localized/Localization/Locale/Localize。
                if (n.IndexOf("localiz", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    n.IndexOf("locale", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // 不动主要交互/动画组件。
                    if (n.Equals("CommonButtonWidget", StringComparison.OrdinalIgnoreCase) ||
                        n.Equals("MainMenuButtonWidget", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    UnityEngine.Object.Destroy(b);
                }
            }
        }
        catch
        {
            // TODO: 应记录异常详情，避免静默失败。
            // ignored
        }
    }

    private static void CleanupLegacySubMenuMultiplayerButton(MainMenuPanel panel)
    {
        try
        {
            // 只有旧版确实创建过才会有缓存引用；并且只删我们自己命名的那个。
            if (_subMenuMultiplayerButton == null)
            {
                // 仍然尝试按名字兜底清理一次，避免热更新/重启后残留。
                Transform subGroup = TryGetSubMenuButtonGroup(panel);
                if (subGroup == null)
                {
                    return;
                }

                Transform legacy = subGroup.Find(LegacySubMenuMultiplayerButtonName);
                if (legacy != null)
                {
                    UnityEngine.Object.Destroy(legacy.gameObject);
                }
                return;
            }

            if (_subMenuMultiplayerButton.name == LegacySubMenuMultiplayerButtonName)
            {
                UnityEngine.Object.Destroy(_subMenuMultiplayerButton.gameObject);
            }

            _subMenuMultiplayerButton = null;
        }
        catch
        {
            // ignored
        }
    }

    private static void EnsureMultiplayerButtonOrder(MainMenuPanel panel, Button multiplayerButton)
    {
        try
        {
            if (panel == null || multiplayerButton == null)
            {
                return;
            }

            Transform parent = TryGetMainMenuButtonGroup(panel) ?? multiplayerButton.transform.parent;
            if (parent == null)
            {
                return;
            }

            // 优先把按钮放到“其他选项(子菜单)”按钮之前，这样视觉上位于“新游戏”和“其他选项”之间。
            Button subMenuButton = TryFindButtonByPersistentMethodName(parent, "UI_ShowSubMenu");
            if (subMenuButton != null && subMenuButton.transform.parent == parent)
            {
                multiplayerButton.transform.SetSiblingIndex(subMenuButton.transform.GetSiblingIndex());
                return;
            }

            // 兜底：放到 newGameButton 后面。
            if (TryFindTemplateButton(panel, out Button template, out _))
            {
                if (template != null && template.transform.parent == parent)
                {
                    int sibling = template.transform.GetSiblingIndex();
                    multiplayerButton.transform.SetSiblingIndex(Mathf.Min(sibling + 1, parent.childCount - 1));
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private static Button TryFindButtonByPersistentMethodName(Transform root, string methodName)
    {
        try
        {
            if (root == null || string.IsNullOrWhiteSpace(methodName))
            {
                return null;
            }

            var buttons = root.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                if (b == null)
                {
                    continue;
                }

                try
                {
                    int n = b.onClick?.GetPersistentEventCount() ?? 0;
                    for (int i = 0; i < n; i++)
                    {
                        string m = b.onClick.GetPersistentMethodName(i);
                        if (string.Equals(m, methodName, StringComparison.Ordinal))
                        {
                            return b;
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    /// <summary>
    /// 确保在 StartGamePanel 上存在“多人游戏”按钮。
    /// </summary>
    private static void EnsureStartGameMultiplayerButton(StartGamePanel panel)
    {
        if (panel == null)
        {
            return;
        }

        if (_startGameMultiplayerButton != null)
        {
            _startGameMultiplayerButton.gameObject.SetActive(true);
            // 文案可能会被模板按钮携带的本地化/刷新脚本覆盖，这里每次都强制写回。
            TryStripLocalizationComponents(_startGameMultiplayerButton.gameObject);
            TrySetButtonText(_startGameMultiplayerButton, "多人游戏");
            return;
        }

        Button template;
        try
        {
            template = Traverse.Create(panel).Field("characterConfirmButton").GetValue<Button>();
        }
        catch
        {
            template = null;
        }

        if (template == null)
        {
            Plugin.Logger?.LogWarning("[MainMenuMultiplayerEntry] 未能定位 StartGamePanel.characterConfirmButton，无法创建多人入口按钮。");
            return;
        }

        Transform parent = template.transform.parent;
        if (parent == null)
        {
            Plugin.Logger?.LogWarning("[MainMenuMultiplayerEntry] StartGamePanel 模板按钮父节点为空，无法挂载多人入口按钮。");
            return;
        }

        _defaultFont ??= FindDefaultFont(parent);

        _startGameMultiplayerButton = CreateButtonFromTemplate(template, parent, StartGameMultiplayerButtonName, "多人游戏");
        _startGameMultiplayerButton.name = StartGameMultiplayerButtonName;
        _startGameMultiplayerButton.onClick.RemoveAllListeners();
        _startGameMultiplayerButton.onClick.AddListener(OpenMultiplayerEntryFromStartGame);

        // StartGamePanel 的确认按钮通常带有本地化/刷新组件，克隆后需要剥离，否则会把文案改回“确认”。
        TryStripLocalizationComponents(_startGameMultiplayerButton.gameObject);
        TrySetButtonText(_startGameMultiplayerButton, "多人游戏");
        _startGameMultiplayerButton.interactable = true;
        _startGameMultiplayerButton.gameObject.SetActive(true);

        // 尽量把按钮排到模板按钮右侧/下方（取决于布局组件）。
        try
        {
            int sibling = template.transform.GetSiblingIndex();
            _startGameMultiplayerButton.transform.SetSiblingIndex(Mathf.Min(sibling + 1, parent.childCount - 1));

            bool hasLayout = parent.GetComponent<HorizontalLayoutGroup>() != null || parent.GetComponent<VerticalLayoutGroup>() != null;
            if (!hasLayout)
            {
                var srcRect = template.GetComponent<RectTransform>();
                var dstRect = _startGameMultiplayerButton.GetComponent<RectTransform>();
                if (srcRect != null && dstRect != null)
                {
                    // 规则(按用户最新确认):
                    // - 默认放在“确认”按钮右侧。
                    // - 如果右侧会越出游戏窗口，则放到左侧。
                    // - 间距优先按屏幕宽度/20，同时保证至少 30px。
                    // - 最终钳制在画布范围内，避免任何分辨率下越界。

                    var canvasRect = TryGetRootCanvasRectTransform(parent);
                    float scale = 1f;
                    try
                    {
                        var canvas = parent.GetComponentInParent<Canvas>();
                        if (canvas != null)
                        {
                            scale = Mathf.Max(0.0001f, canvas.scaleFactor);
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    float desiredSpacingPx = Mathf.Max(30f, Screen.width / 20f);
                    float spacing = desiredSpacingPx / scale;
                    float dx = srcRect.rect.width + spacing;

                    Vector2 rightPos = srcRect.anchoredPosition + new Vector2(dx, 0f);
                    Vector2 leftPos = srcRect.anchoredPosition + new Vector2(-dx, 0f);

                    // 先尝试右侧；若会越界则改为左侧。
                    dstRect.anchoredPosition = rightPos;
                    if (canvasRect != null && !IsFullyInside(dstRect, canvasRect, paddingWorld: 0f))
                    {
                        dstRect.anchoredPosition = leftPos;
                    }

                    // 最后兜底：无论选了哪边，都钳制在画布范围内。
                    if (canvasRect != null)
                    {
                        ClampToContainer(dstRect, canvasRect, paddingWorld: 0f);
                    }
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private static RectTransform TryGetRootCanvasRectTransform(Transform any)
    {
        try
        {
            var canvas = any?.GetComponentInParent<Canvas>();
            return canvas?.GetComponent<RectTransform>();
        }
        catch
        {
            return null;
        }
    }

    private static bool IsFullyInside(RectTransform target, RectTransform container, float paddingWorld)
    {
        if (target == null || container == null)
        {
            return true;
        }

        Vector3[] t = new Vector3[4];
        Vector3[] c = new Vector3[4];
        target.GetWorldCorners(t);
        container.GetWorldCorners(c);

        float minX = c[0].x + paddingWorld;
        float maxX = c[2].x - paddingWorld;
        float minY = c[0].y + paddingWorld;
        float maxY = c[2].y - paddingWorld;

        float tMinX = t[0].x;
        float tMaxX = t[2].x;
        float tMinY = t[0].y;
        float tMaxY = t[2].y;

        return tMinX >= minX && tMaxX <= maxX && tMinY >= minY && tMaxY <= maxY;
    }

    private static void ClampToContainer(RectTransform target, RectTransform container, float paddingWorld)
    {
        if (target == null || container == null)
        {
            return;
        }

        Vector3[] t = new Vector3[4];
        Vector3[] c = new Vector3[4];
        target.GetWorldCorners(t);
        container.GetWorldCorners(c);

        float minX = c[0].x + paddingWorld;
        float maxX = c[2].x - paddingWorld;
        float minY = c[0].y + paddingWorld;
        float maxY = c[2].y - paddingWorld;

        float tMinX = t[0].x;
        float tMaxX = t[2].x;
        float tMinY = t[0].y;
        float tMaxY = t[2].y;

        var delta = Vector3.zero;
        if (tMinX < minX)
        {
            delta.x += (minX - tMinX);
        }
        else if (tMaxX > maxX)
        {
            delta.x -= (tMaxX - maxX);
        }

        if (tMinY < minY)
        {
            delta.y += (minY - tMinY);
        }
        else if (tMaxY > maxY)
        {
            delta.y -= (tMaxY - maxY);
        }

        if (delta != Vector3.zero)
        {
            // UI 元素在 Screen Space 下位置就是屏幕/世界坐标，直接修正即可。
            target.position += delta;
        }
    }

    private static Transform TryGetMainMenuButtonGroup(MainMenuPanel panel)
    {
        try
        {
            // 对应游戏源码字段名 mainMenuButtonGroup（private + SerializeField）。
            return Traverse.Create(panel).Field("mainMenuButtonGroup").GetValue<Transform>();
        }
        catch
        {
            return null;
        }
    }

    private static Transform TryGetSubMenuButtonGroup(MainMenuPanel panel)
    {
        try
        {
            // 对应游戏源码字段名 subMenuButtonGroup（private + SerializeField）。
            return Traverse.Create(panel).Field("subMenuButtonGroup").GetValue<Transform>();
        }
        catch
        {
            return null;
        }
    }

    private static bool TryFindTemplateButton(MainMenuPanel panel, out Button template, out string why)
    {
        template = null;
        why = null;

        // 1) 强依赖字段（兼容当前仓库里的 LBoL 源码）。
        try
        {
            template = Traverse.Create(panel).Field("newGameButton").GetValue<Button>();
            if (template != null)
            {
                return true;
            }
        }
        catch
        {
            // ignored
        }

        // 2) 反射扫描所有 Button 字段（字段名可能变）。
        try
        {
            List<FieldInfo> fields = AccessTools.GetDeclaredFields(panel.GetType())
                .Where(f => typeof(Button).IsAssignableFrom(f.FieldType))
                .ToList();

            foreach (var f in fields)
            {
                Button b = f.GetValue(panel) as Button;
                if (b != null)
                {
                    template = b;
                    return true;
                }
            }
        }
        catch
        {
            // ignored
        }

        // 3) 兜底：从面板子节点找按钮（最弱保证：至少能克隆出同风格按钮）。
        try
        {
            var buttons = panel.GetComponentsInChildren<Button>(true);
            if (buttons != null && buttons.Length > 0)
            {
                // 过滤掉我们自己的按钮，避免自我复制。
                List<Button> candidates = buttons
                    .Where(b => b != null && b.name != MultiplayerButtonName)
                    .Where(b => b.GetComponentInChildren<TextMeshProUGUI>(true) != null)
                    .ToList();

                if (candidates.Count > 0)
                {
                    template = candidates[0];
                    return true;
                }

                why = $"panel children has {buttons.Length} Button(s), but no candidate with TextMeshProUGUI";
                return false;
            }

            why = "panel children contains no Button";
            return false;
        }
        catch (Exception ex)
        {
            why = ex.Message;
            return false;
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

            // 兼容 TextMeshProUGUI / TextMeshPro：不同版本/Prefab 的按钮文案组件类型可能不同。
            var labels = button.GetComponentsInChildren<TMP_Text>(true);
            if (labels != null)
            {
                foreach (var label in labels)
                {
                    if (label == null)
                    {
                        continue;
                    }

                    label.text = text;
                    if (_defaultFont != null)
                    {
                        label.font = _defaultFont;
                    }
                }
            }

            // 兼容旧式 UI.Text（部分主菜单文案可能不是 TMP）。
            var legacyTexts = button.GetComponentsInChildren<Text>(true);
            if (legacyTexts != null)
            {
                foreach (var t in legacyTexts)
                {
                    if (t == null)
                    {
                        continue;
                    }

                    t.text = text;
                }
            }
        }
        catch
        {
            // 忽略：设置 UI 文案失败不影响整体流程。
        }
    }

    private static Button CreateButtonFromTemplate(Button template, Transform parent, string name, string labelText)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localScale = Vector3.one;

        Image img = go.AddComponent<Image>();
        if (template != null && template.targetGraphic is Image templateImg)
        {
            img.sprite = templateImg.sprite;
            img.type = templateImg.type;
            img.color = templateImg.color;
        }
        else
        {
            img.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
        }

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        if (template != null)
        {
            btn.transition = template.transition;
            btn.colors = template.colors;
        }

        // 创建文本子对象
        GameObject textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);
        textGo.transform.localScale = Vector3.one;
        var rt = textGo.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = labelText;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        if (_defaultFont != null)
        {
            tmp.font = _defaultFont;
        }

        return btn;
    }

    #endregion

    #region 入口面板（非弹窗）

    /// <summary>
    /// 打开“多人游戏”入口弹窗。
    /// </summary>
    private static void OpenMultiplayerEntry()
    {
        if (!UiManager.IsInitialized)
        {
            return;
        }

        Plugin.Logger?.LogInfo("[MainMenuMultiplayerEntry] 打开多人入口面板（MainMenu）。");

        // 若已连接，则提示是否断开。
        INetworkClient client = TryGetNetworkClient();
        if (client?.IsConnected == true)
        {
            ShowConnectedDialog(client);
            return;
        }

        // 未连接：打开自定义“面板 UI”（非 MessageDialog）。
        ShowMultiplayerEntryOverlayFromMainMenu();
    }

    private static void OpenMultiplayerEntryFromStartGame()
    {
        if (!UiManager.IsInitialized)
        {
            return;
        }

        Plugin.Logger?.LogInfo("[MainMenuMultiplayerEntry] 打开多人入口面板（StartGame）。");

        INetworkClient client = TryGetNetworkClient();
        if (client?.IsConnected == true)
        {
            ShowConnectedDialog(client);
            return;
        }

        ShowMultiplayerEntryOverlayFromStartGame();
    }

    private static void ShowMultiplayerEntryOverlayFromMainMenu()
    {
        MainMenuPanel panel = _lastMainMenuPanel ?? UiManager.GetPanel<MainMenuPanel>();
        if (panel == null)
        {
            Plugin.Logger?.LogWarning("[MainMenuMultiplayerEntry] 无法获取 MainMenuPanel，无法显示多人入口面板。");
            return;
        }

        if (!TryFindTemplateButton(panel, out Button template, out string whyTemplateFailed))
        {
            Plugin.Logger?.LogWarning($"[MainMenuMultiplayerEntry] 无法创建多人入口面板（模板按钮不可用）：{whyTemplateFailed}");
            return;
        }

        ShowMultiplayerEntryOverlay(panel.transform, template);
    }

    private static void ShowMultiplayerEntryOverlayFromStartGame()
    {
        StartGamePanel panel = _lastStartGamePanel ?? UiManager.GetPanel<StartGamePanel>();
        if (panel == null)
        {
            Plugin.Logger?.LogWarning("[MainMenuMultiplayerEntry] 无法获取 StartGamePanel，无法显示多人入口面板。");
            return;
        }

        Button template = _startGameMultiplayerButton;
        if (template == null)
        {
            try
            {
                template = Traverse.Create(panel).Field("characterConfirmButton").GetValue<Button>();
            }
            catch
            {
                template = null;
            }
        }

        if (template == null)
        {
            Plugin.Logger?.LogWarning("[MainMenuMultiplayerEntry] StartGamePanel 模板按钮不可用，无法创建多人入口面板。");
            return;
        }

        ShowMultiplayerEntryOverlay(panel.transform, template);
    }

    private static void ShowMultiplayerEntryOverlay(Transform panelTransform, Button template)
    {
        try
        {
            if (panelTransform == null || template == null)
            {
                return;
            }

            if (_overlayRoot != null)
            {
                UnityEngine.Object.Destroy(_overlayRoot);
                _overlayRoot = null;
                _rootAnimator = null;
            }

            _defaultFont ??= FindDefaultFont(panelTransform);

            // 加载游戏内置的 MessageDialog 预制体
            GameObject dialogPrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            if (dialogPrefab == null)
            {
                Plugin.Logger?.LogError("[MainMenuMultiplayerEntry] 无法加载原生 MessageDialog 预制体！");
                return;
            }

            GameObject root = new GameObject(OverlayRootName);
            root.transform.SetParent(panelTransform, false);
            root.transform.SetAsLastSibling();
            _overlayRoot = root;

            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 0f);
            rootRect.anchorMax = new Vector2(1f, 1f);
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            // 半透明背景，拦截点击
            var bg = root.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.62f);
            bg.raycastTarget = true;

            var rootGroup = root.AddComponent<CanvasGroup>();
            rootGroup.alpha = 0f;
            rootGroup.interactable = false;
            rootGroup.blocksRaycasts = true;

            float panelScale = 3f;
            try
            {
                const float baseW = 520f;
                const float baseH = 380f;
                const float widthFactor = 1.00f;
                const float heightFactor = 0.95f;

                float maxW = Mathf.Max(1f, rootRect.rect.width * 0.92f);
                float maxH = Mathf.Max(1f, rootRect.rect.height * 0.92f);
                float fitScale = Mathf.Min(maxW / (baseW * widthFactor), maxH / (baseH * heightFactor));
                panelScale = Mathf.Min(3f, fitScale);
                panelScale = Mathf.Max(1f, panelScale);
            }
            catch
            {
                panelScale = 3f;
            }

            // 实例化原生弹窗框体作为中央容器
            GameObject frame = UnityEngine.Object.Instantiate(dialogPrefab, root.transform, false);
            frame.name = OverlayRootName + "_Frame";
            frame.SetActive(true);

            RectTransform frameRect = frame.GetComponent<RectTransform>();
            if (frameRect != null)
            {
                frameRect.anchorMin = new Vector2(0.5f, 0.5f);
                frameRect.anchorMax = new Vector2(0.5f, 0.5f);
                frameRect.pivot = new Vector2(0.5f, 0.5f);
                frameRect.sizeDelta = new Vector2(520f * panelScale, 380f * panelScale);
                frameRect.anchoredPosition = Vector2.zero;
            }

            // 获取原生 MessageDialog 组件并禁用它以防干扰
            MessageDialog dialog = frame.GetComponentInChildren<MessageDialog>(true);
            if (dialog != null)
            {
                dialog.enabled = false;
            }

            // 获取原生各个字段组件
            TextMeshProUGUI mainText = GetDialogField<TextMeshProUGUI>(dialog, "mainText");
            TextMeshProUGUI subText = GetDialogField<TextMeshProUGUI>(dialog, "subText");
            Button singleConfirm = GetDialogField<Button>(dialog, "singleConfirmButton");
            Button confirm = GetDialogField<Button>(dialog, "confirmButton");
            Button cancel = GetDialogField<Button>(dialog, "cancelButton");
            // 确定内容父容器：跟原版 SelectionPanelScaffold 查找逻辑对齐，使用 mainText 的 parent 作为面板根容器
            RectTransform panelRect = mainText?.rectTransform.parent as RectTransform ?? frameRect;

            // 只清除 frame 根节点上的布局与自适应组件，避免干扰子节点（如按钮内部）的对齐
            if (frame != null)
            {
                var rootFitters = frame.GetComponents<ContentSizeFitter>();
                foreach (var fitter in rootFitters) UnityEngine.Object.Destroy(fitter);

                var rootLayouts = frame.GetComponents<LayoutGroup>();
                foreach (var l in rootLayouts) UnityEngine.Object.Destroy(l);
            }

            if (panelRect != null)
            {
                // 递归将 panelRect 及其所有父级（直到 frame）强制设置为拉伸填满，并只清除其自身的布局组件，保留按钮等子物体的内部排版
                Transform current = panelRect;
                while (current != null && current != frame.transform)
                {
                    var rt = current.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchorMin = Vector2.zero;
                        rt.anchorMax = Vector2.one;
                        rt.offsetMin = Vector2.zero;
                        rt.offsetMax = Vector2.zero;
                        rt.pivot = new Vector2(0.5f, 0.5f);
                    }

                    var fitters = current.GetComponents<ContentSizeFitter>();
                    foreach (var f in fitters) UnityEngine.Object.Destroy(f);

                    var layouts = current.GetComponents<LayoutGroup>();
                    foreach (var l in layouts) UnityEngine.Object.Destroy(l);

                    current = current.parent;
                }

                // 强制 panelRect 锚定并拉伸填满父框体，保证子容器坐标系统对齐
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
                panelRect.pivot = new Vector2(0.5f, 0.5f);
            }

            // 隐藏原生 MessageDialog 预制体自带的所有背景 Image 和黄线，只保留其作为纯净容器（保护三个按钮及它们内部的 Image 不被隐藏）
            if (frame != null)
            {
                var nativeImages = frame.GetComponentsInChildren<Image>(true);
                foreach (var img in nativeImages)
                {
                    if (img == null) continue;
                    if (singleConfirm != null && (img.transform == singleConfirm.transform || IsDescendantOf(img.transform, singleConfirm.transform))) continue;
                    if (confirm != null && (img.transform == confirm.transform || IsDescendantOf(img.transform, confirm.transform))) continue;
                    if (cancel != null && (img.transform == cancel.transform || IsDescendantOf(img.transform, cancel.transform))) continue;
                    
                    // 禁用 Image 组件本身使其不渲染，而不直接隐藏其所在的 GameObject（防止误杀 root 节点导致整个弹窗消失）
                    img.enabled = false;
                }
            }

            // 隐藏原生多余的字段和底部按钮
            if (subText != null) subText.gameObject.SetActive(false);
            if (singleConfirm != null) singleConfirm.gameObject.SetActive(false);
            if (confirm != null) confirm.gameObject.SetActive(false);
            if (cancel != null) cancel.gameObject.SetActive(false);

            // 确定按钮模板
            Button buttonTemplate = confirm ?? singleConfirm ?? cancel ?? template;

            // 绑定入场缩放与渐显动画
            try
            {
                _rootAnimator = root.AddComponent<PanelAnimator>();
                _rootAnimator.Init(rootGroup, frameRect);
                _rootAnimator.PlayOpen();
            }
            catch
            {
                // ignored
            }

            // ================== 主大厅面板 Area ==================
            GameObject mainArea = new GameObject("MainArea");
            mainArea.transform.SetParent(panelRect, false);
            var mainAreaRt = mainArea.AddComponent<RectTransform>();
            mainAreaRt.anchorMin = Vector2.zero;
            mainAreaRt.anchorMax = Vector2.one;
            mainAreaRt.offsetMin = Vector2.zero;
            mainAreaRt.offsetMax = Vector2.zero;
            mainAreaRt.pivot = new Vector2(0.5f, 0.5f);
            var mainGroup = mainArea.AddComponent<CanvasGroup>();
            var mainAnim = mainArea.AddComponent<PanelAnimator>();
            mainAnim.Init(mainGroup, mainAreaRt);

            // 设置标题并在去除布局后重新手动排版，避免重叠
            if (mainText != null)
            {
                mainText.text = "多人游戏";
                mainText.alignment = TextAlignmentOptions.Center;
                mainText.fontSize = Mathf.Clamp(24f * panelScale, 24f, 80f);

                var mainTextRt = mainText.rectTransform;
                if (mainTextRt != null)
                {
                    mainTextRt.anchorMin = new Vector2(0f, 1f);
                    mainTextRt.anchorMax = new Vector2(1f, 1f);
                    mainTextRt.pivot = new Vector2(0.5f, 1f);
                    mainTextRt.anchoredPosition = new Vector2(0f, -20f * panelScale);
                    mainTextRt.sizeDelta = new Vector2(-40f * panelScale, 45f * panelScale);
                }
                mainText.transform.SetAsLastSibling();
                mainText.gameObject.SetActive(true);
            }

            // 说明文字
            GameObject descGo = new GameObject("Description");
            descGo.transform.SetParent(mainArea.transform, false);
            var descRect = descGo.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0f, 1f);
            descRect.anchorMax = new Vector2(1f, 1f);
            descRect.pivot = new Vector2(0.5f, 1f);
            descRect.anchoredPosition = new Vector2(0f, -74f * panelScale);
            descRect.sizeDelta = new Vector2(-40f * panelScale, 70f * panelScale);

            var desc = descGo.AddComponent<TextMeshProUGUI>();
            desc.text = "请选择联机方式：\n房主：启动本机服务器并连接\n加入：连接到配置的服务器";
            desc.fontSize = 18f * panelScale;
            desc.alignment = TextAlignmentOptions.TopLeft;
            desc.color = Color.white;
            desc.raycastTarget = false;
            if (_defaultFont != null)
            {
                desc.font = _defaultFont;
            }

            // 按钮区域
            GameObject buttonsGo = new GameObject("Buttons");
            buttonsGo.transform.SetParent(mainArea.transform, false);
            var buttonsRect = buttonsGo.AddComponent<RectTransform>();
            buttonsRect.anchorMin = new Vector2(0.5f, 0f);
            buttonsRect.anchorMax = new Vector2(0.5f, 0f);
            buttonsRect.pivot = new Vector2(0.5f, 0f);
            float buttonsWidth = 380f * panelScale;
            try
            {
                buttonsWidth = Mathf.Min(buttonsWidth, frameRect.sizeDelta.x * 0.60f);
            }
            catch
            {
                // ignored
            }
            buttonsRect.sizeDelta = new Vector2(buttonsWidth, 196f * panelScale);
            buttonsRect.anchoredPosition = new Vector2(0f, 10f * panelScale);

            var layout = buttonsGo.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.spacing = 12f * panelScale;
            layout.padding = new RectOffset(0, 0, 0, 0);

            // 房主（切换到子面板）
            GameObject hostArea = new GameObject("HostArea");
            PanelAnimator hostAnim = null;
            var hostBtn = CreateDialogButton(buttonTemplate, buttonsGo.transform, "NetworkPlugin_HostButton", "做房主", panelScale);
            hostBtn.onClick.AddListener(() =>
            {
                if (mainAnim != null && hostAnim != null)
                {
                    mainAnim.PlayClose(() =>
                    {
                        hostAnim.PlayOpen();
                        if (mainText != null) mainText.text = "做房主";
                    });
                }
                else
                {
                    mainArea.SetActive(false);
                    hostArea.SetActive(true);
                    if (mainText != null) mainText.text = "做房主";
                }
            });

            // 加入（切换到子面板）
            GameObject joinArea = new GameObject("JoinArea");
            PanelAnimator joinAnim = null;
            var joinBtn = CreateDialogButton(buttonTemplate, buttonsGo.transform, "NetworkPlugin_JoinButton", "加入房主", panelScale);
            joinBtn.onClick.AddListener(() =>
            {
                if (mainAnim != null && joinAnim != null)
                {
                    mainAnim.PlayClose(() =>
                    {
                        joinAnim.PlayOpen();
                        if (mainText != null) mainText.text = "加入房主";
                    });
                }
                else
                {
                    mainArea.SetActive(false);
                    joinArea.SetActive(true);
                    if (mainText != null) mainText.text = "加入房主";
                }
            });

            // 返回
            var backBtn = CreateDialogButton(buttonTemplate, buttonsGo.transform, "NetworkPlugin_BackButton", "返回", panelScale);
            backBtn.onClick.AddListener(HideOverlay);

            // 统一尺寸大小
            foreach (var b in new[] { hostBtn, joinBtn, backBtn })
            {
                if (b == null) continue;
                var r = b.GetComponent<RectTransform>();
                if (r != null) r.sizeDelta = new Vector2(r.sizeDelta.x, 58f * panelScale);
            }

            // ================== 加入客户端子面板 Area ==================
            joinArea.transform.SetParent(panelRect, false);
            var joinAreaRt = joinArea.AddComponent<RectTransform>();
            joinAreaRt.anchorMin = Vector2.zero;
            joinAreaRt.anchorMax = Vector2.one;
            joinAreaRt.offsetMin = Vector2.zero;
            joinAreaRt.offsetMax = Vector2.zero;
            joinAreaRt.pivot = new Vector2(0.5f, 0.5f);
            var joinGroup = joinArea.AddComponent<CanvasGroup>();
            joinAnim = joinArea.AddComponent<PanelAnimator>();
            joinAnim.Init(joinGroup, joinAreaRt);
            joinArea.SetActive(false);

            // 表单输入区容器
            GameObject formGo = new GameObject("Form");
            formGo.transform.SetParent(joinArea.transform, false);
            var formRt = formGo.AddComponent<RectTransform>();
            formRt.anchorMin = new Vector2(0.5f, 1f);
            formRt.anchorMax = new Vector2(0.5f, 1f);
            formRt.pivot = new Vector2(0.5f, 1f);
            formRt.sizeDelta = new Vector2(400f * panelScale, 160f * panelScale);
            formRt.anchoredPosition = new Vector2(0f, -74f * panelScale);

            var formLayout = formGo.AddComponent<VerticalLayoutGroup>();
            formLayout.childAlignment = TextAnchor.UpperCenter;
            formLayout.childControlWidth = true;
            formLayout.childControlHeight = true;
            formLayout.childForceExpandWidth = true;
            formLayout.childForceExpandHeight = false;
            formLayout.spacing = 8f * panelScale;

            // 获取配置默认数据
            ConfigManager config = TryGetConfig();
            string curIp = config?.ServerIP?.Value ?? "127.0.0.1";
            string curPort = config?.ServerPort?.Value.ToString() ?? "7777";
            string curName = config?.PlayerNameOverride?.Value;
            if (string.IsNullOrWhiteSpace(curName))
            {
                try
                {
                    curName = Singleton<GameMaster>.Instance?.CurrentProfile?.Name ?? "joiner";
                }
                catch
                {
                    curName = "joiner";
                }
            }

            TMP_InputField ipInput;
            TMP_InputField portInput;
            TMP_InputField nameInput;

            CreateInputRow(formGo.transform, buttonTemplate, "服务器 IP:", "请输入 IP 地址...", curIp, panelScale, out ipInput);
            CreateInputRow(formGo.transform, buttonTemplate, "端口:", "请输入端口号...", curPort, panelScale, out portInput);
            CreateInputRow(formGo.transform, buttonTemplate, "玩家昵称:", "请输入昵称...", curName, panelScale, out nameInput);

            // 子面板底部按钮
            GameObject joinButtonsGo = new GameObject("JoinButtons");
            joinButtonsGo.transform.SetParent(joinArea.transform, false);
            var joinButtonsRt = joinButtonsGo.AddComponent<RectTransform>();
            joinButtonsRt.anchorMin = new Vector2(0.5f, 0f);
            joinButtonsRt.anchorMax = new Vector2(0.5f, 0f);
            joinButtonsRt.pivot = new Vector2(0.5f, 0f);
            joinButtonsRt.sizeDelta = new Vector2(380f * panelScale, 60f * panelScale);
            joinButtonsRt.anchoredPosition = new Vector2(0f, 15f * panelScale);

            var joinButtonsLayout = joinButtonsGo.AddComponent<HorizontalLayoutGroup>();
            joinButtonsLayout.childAlignment = TextAnchor.MiddleCenter;
            joinButtonsLayout.childControlWidth = true;
            joinButtonsLayout.childControlHeight = true;
            joinButtonsLayout.childForceExpandWidth = true;
            joinButtonsLayout.childForceExpandHeight = false;
            joinButtonsLayout.spacing = 20f * panelScale;

            // 开始连接
            var connectBtn = CreateDialogButton(buttonTemplate, joinButtonsGo.transform, "NetworkPlugin_ConnectBtn", "开始连接", panelScale);
            connectBtn.onClick.AddListener(() =>
            {
                string ip = ipInput.text.Trim();
                string portStr = portInput.text.Trim();
                string name = nameInput.text.Trim();

                if (string.IsNullOrWhiteSpace(ip))
                {
                    ShowWarningDialog("IP 地址不能为空。");
                    return;
                }
                if (!int.TryParse(portStr, out int port) || port <= 0 || port > 65535)
                {
                    ShowWarningDialog("请输入有效的端口号（1-65535）。");
                    return;
                }
                if (string.IsNullOrWhiteSpace(name))
                {
                    ShowWarningDialog("昵称不能为空。");
                    return;
                }

                ConfigManager cfg = TryGetConfig();
                if (cfg != null)
                {
                    cfg.ServerIP.Value = ip;
                    cfg.ServerPort.Value = port;
                    cfg.PlayerNameOverride.Value = name;
                    try
                    {
                        cfg.ServerIP.ConfigFile.Save();
                    }
                    catch (Exception ex)
                    {
                        Plugin.Logger?.LogWarning($"[MainMenuMultiplayerEntry] 保存配置文件失败: {ex.Message}");
                    }
                }

                HideOverlay();

                GameRunSaveData save = null;
                try
                {
                    save = Singleton<GameMaster>.Instance?.GameRunSaveData;
                }
                catch
                {
                    save = null;
                }

                if (save != null && Singleton<GameMaster>.Instance?.CurrentGameRun == null)
                {
                    TryConnectToServerAndRestoreAndCatchUp(ip, port, save);
                }
                else
                {
                    TryConnectToServer(ip, port);
                }
            });

            // 返回大厅按钮
            var cancelBtn = CreateDialogButton(buttonTemplate, joinButtonsGo.transform, "NetworkPlugin_CancelBtn", "返回", panelScale);
            cancelBtn.onClick.AddListener(() =>
            {
                if (joinAnim != null && mainAnim != null)
                {
                    joinAnim.PlayClose(() =>
                    {
                        mainAnim.PlayOpen();
                        if (mainText != null) mainText.text = "多人游戏";
                    });
                }
                else
                {
                    joinArea.SetActive(false);
                    mainArea.SetActive(true);
                    if (mainText != null) mainText.text = "多人游戏";
                }
            });

            foreach (var b in new[] { connectBtn, cancelBtn })
            {
                if (b == null) continue;
                var r = b.GetComponent<RectTransform>();
                if (r != null) r.sizeDelta = new Vector2(r.sizeDelta.x, 58f * panelScale);
            }

            // ================== 做房主服务端子面板 Area ==================
            hostArea.transform.SetParent(panelRect, false);
            var hostAreaRt = hostArea.AddComponent<RectTransform>();
            hostAreaRt.anchorMin = Vector2.zero;
            hostAreaRt.anchorMax = Vector2.one;
            hostAreaRt.offsetMin = Vector2.zero;
            hostAreaRt.offsetMax = Vector2.zero;
            hostAreaRt.pivot = new Vector2(0.5f, 0.5f);
            var hostGroup = hostArea.AddComponent<CanvasGroup>();
            hostAnim = hostArea.AddComponent<PanelAnimator>();
            hostAnim.Init(hostGroup, hostAreaRt);
            hostArea.SetActive(false);

            // 表单输入区容器
            GameObject hostFormGo = new GameObject("HostForm");
            hostFormGo.transform.SetParent(hostArea.transform, false);
            var hostFormRt = hostFormGo.AddComponent<RectTransform>();
            hostFormRt.anchorMin = new Vector2(0.5f, 1f);
            hostFormRt.anchorMax = new Vector2(0.5f, 1f);
            hostFormRt.pivot = new Vector2(0.5f, 1f);
            hostFormRt.sizeDelta = new Vector2(400f * panelScale, 200f * panelScale);
            hostFormRt.anchoredPosition = new Vector2(0f, -54f * panelScale);

            var hostFormLayout = hostFormGo.AddComponent<VerticalLayoutGroup>();
            hostFormLayout.childAlignment = TextAnchor.UpperCenter;
            hostFormLayout.childControlWidth = true;
            hostFormLayout.childControlHeight = true;
            hostFormLayout.childForceExpandWidth = true;
            hostFormLayout.childForceExpandHeight = false;
            hostFormLayout.spacing = 6f * panelScale;

            // 获取默认配置
            string hostPort = config?.HostServerPort?.Value.ToString() ?? "7777";
            string hostMaxConn = config?.HostMaxConnections?.Value.ToString() ?? "4";
            string hostKey = config?.HostConnectionKey?.Value ?? "LBoL_Network_Plugin";
            string hostName = config?.HostPlayerNameOverride?.Value;
            if (string.IsNullOrWhiteSpace(hostName))
            {
                try
                {
                    hostName = Singleton<GameMaster>.Instance?.CurrentProfile?.Name ?? "host";
                }
                catch
                {
                    hostName = "host";
                }
            }

            TMP_InputField hostPortInput;
            TMP_InputField hostMaxConnInput;
            TMP_InputField hostKeyInput;
            TMP_InputField hostNameInput;

            CreateInputRow(hostFormGo.transform, buttonTemplate, "监听端口:", "请输入端口号...", hostPort, panelScale, out hostPortInput);
            CreateInputRow(hostFormGo.transform, buttonTemplate, "最大玩家数:", "请输入最大玩家数...", hostMaxConn, panelScale, out hostMaxConnInput);
            CreateInputRow(hostFormGo.transform, buttonTemplate, "连接密钥:", "请输入连接密钥...", hostKey, panelScale, out hostKeyInput);
            CreateInputRow(hostFormGo.transform, buttonTemplate, "玩家昵称:", "请输入昵称...", hostName, panelScale, out hostNameInput);

            // 子面板底部按钮
            GameObject hostButtonsGo = new GameObject("HostButtons");
            hostButtonsGo.transform.SetParent(hostArea.transform, false);
            var hostButtonsRt = hostButtonsGo.AddComponent<RectTransform>();
            hostButtonsRt.anchorMin = new Vector2(0.5f, 0f);
            hostButtonsRt.anchorMax = new Vector2(0.5f, 0f);
            hostButtonsRt.pivot = new Vector2(0.5f, 0f);
            hostButtonsRt.sizeDelta = new Vector2(380f * panelScale, 60f * panelScale);
            hostButtonsRt.anchoredPosition = new Vector2(0f, 15f * panelScale);

            var hostButtonsLayout = hostButtonsGo.AddComponent<HorizontalLayoutGroup>();
            hostButtonsLayout.childAlignment = TextAnchor.MiddleCenter;
            hostButtonsLayout.childControlWidth = true;
            hostButtonsLayout.childControlHeight = true;
            hostButtonsLayout.childForceExpandWidth = true;
            hostButtonsLayout.childForceExpandHeight = false;
            hostButtonsLayout.spacing = 20f * panelScale;

            // 开始连接（房主）
            var startHostBtn = CreateDialogButton(buttonTemplate, hostButtonsGo.transform, "NetworkPlugin_StartHostBtn", "开始做房主", panelScale);
            startHostBtn.onClick.AddListener(() =>
            {
                string portStr = hostPortInput.text.Trim();
                string maxConnStr = hostMaxConnInput.text.Trim();
                string key = hostKeyInput.text.Trim();
                string name = hostNameInput.text.Trim();

                if (!int.TryParse(portStr, out int port) || port <= 0 || port > 65535)
                {
                    ShowWarningDialog("请输入有效的端口号（1-65535）。");
                    return;
                }
                if (!int.TryParse(maxConnStr, out int maxConn) || maxConn <= 0 || maxConn > 1000)
                {
                    ShowWarningDialog("请输入有效的最大玩家数（1-1000）。");
                    return;
                }
                if (string.IsNullOrWhiteSpace(key))
                {
                    ShowWarningDialog("连接密钥不能为空。");
                    return;
                }
                if (string.IsNullOrWhiteSpace(name))
                {
                    ShowWarningDialog("昵称不能为空。");
                    return;
                }

                ConfigManager cfg = TryGetConfig();
                if (cfg != null)
                {
                    cfg.HostServerPort.Value = port;
                    cfg.HostMaxConnections.Value = maxConn;
                    cfg.HostConnectionKey.Value = key;
                    cfg.HostPlayerNameOverride.Value = name;
                    try
                    {
                        cfg.HostServerPort.ConfigFile.Save();
                    }
                    catch (Exception ex)
                    {
                        Plugin.Logger?.LogWarning($"[MainMenuMultiplayerEntry] 保存房主配置失败: {ex.Message}");
                    }
                }

                HideOverlay();

                // 检查是否有存档
                GameRunSaveData save = null;
                try
                {
                    save = Singleton<GameMaster>.Instance?.GameRunSaveData;
                }
                catch
                {
                    save = null;
                }

                if (save != null && Singleton<GameMaster>.Instance?.CurrentGameRun == null)
                {
                    UiManager.GetDialog<MessageDialog>().Show(
                        new MessageContent
                        {
                            Text = "检测到可继续的存档。\n\n确认：作为房主继续存档并开启联机\n取消：作为房主开新游戏（仍保持联机）",
                            Icon = MessageIcon.Warning,
                            Buttons = DialogButtons.ConfirmCancel,
                            OnConfirm = () => TryHostLocalServerAndConnectAndRestore(save),
                            OnCancel = TryHostLocalServerAndConnect,
                        }
                    );
                    return;
                }

                TryHostLocalServerAndConnect();
            });

            // 返回大厅按钮
            var hostCancelBtn = CreateDialogButton(buttonTemplate, hostButtonsGo.transform, "NetworkPlugin_HostCancelBtn", "返回", panelScale);
            hostCancelBtn.onClick.AddListener(() =>
            {
                if (hostAnim != null && mainAnim != null)
                {
                    hostAnim.PlayClose(() =>
                    {
                        mainAnim.PlayOpen();
                        if (mainText != null) mainText.text = "多人游戏";
                    });
                }
                else
                {
                    hostArea.SetActive(false);
                    mainArea.SetActive(true);
                    if (mainText != null) mainText.text = "多人游戏";
                }
            });

            foreach (var b in new[] { startHostBtn, hostCancelBtn })
            {
                if (b == null) continue;
                var r = b.GetComponent<RectTransform>();
                if (r != null) r.sizeDelta = new Vector2(r.sizeDelta.x, 58f * panelScale);
            }

            // 确保容器不超过画布
            try
            {
                var canvasRect = TryGetRootCanvasRectTransform(panelTransform);
                if (canvasRect != null)
                {
                    ClampToContainer(frameRect, canvasRect, paddingWorld: 0f);
                }
            }
            catch
            {
                // ignored
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[MainMenuMultiplayerEntry] 创建多人入口面板失败: {ex}");
        }
    }

    private sealed class PanelAnimator : MonoBehaviour
    {
        private CanvasGroup _group;
        private RectTransform _container;
        private Coroutine _currentAnim;
        private bool _isOpen;

        public void Init(CanvasGroup group, RectTransform container)
        {
            _group = group;
            _container = container;
            if (_group != null)
            {
                _isOpen = gameObject.activeSelf;
                _group.alpha = _isOpen ? 1f : 0f;
                if (_container != null)
                {
                    _container.localScale = _isOpen ? Vector3.one : new Vector3(0.92f, 0.92f, 1f);
                }
            }
        }

        public void PlayOpen(Action onComplete = null)
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            if (_group != null)
            {
                _group.interactable = false;
                _group.blocksRaycasts = true;
            }
            if (_currentAnim != null) StopCoroutine(_currentAnim);
            _isOpen = true;
            _currentAnim = StartCoroutine(Animate(true, onComplete));
        }

        public void PlayClose(Action onComplete = null)
        {
            if (!gameObject.activeInHierarchy)
            {
                onComplete?.Invoke();
                return;
            }
            if (_group != null)
            {
                _group.interactable = false;
                _group.blocksRaycasts = true;
            }
            if (_currentAnim != null) StopCoroutine(_currentAnim);
            _isOpen = false;
            _currentAnim = StartCoroutine(Animate(false, onComplete));
        }

        private IEnumerator Animate(bool isOpen, Action onComplete)
        {
            if (_group == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            const float duration = 0.12f;
            float startAlpha = _group.alpha;
            float targetAlpha = isOpen ? 1f : 0f;
            float startScale = _container != null ? _container.localScale.x : 1f;
            float targetScale = isOpen ? 1f : 0.92f;

            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / duration);
                float e = isOpen ? (1f - Mathf.Pow(1f - p, 3f)) : Mathf.Pow(p, 3f);

                _group.alpha = Mathf.Lerp(startAlpha, targetAlpha, p);
                if (_container != null)
                {
                    float s = Mathf.Lerp(startScale, targetScale, e);
                    _container.localScale = new Vector3(s, s, 1f);
                }

                yield return null;
            }

            _group.alpha = targetAlpha;
            if (_container != null) _container.localScale = new Vector3(targetScale, targetScale, 1f);
            
            _group.interactable = isOpen;
            _group.blocksRaycasts = isOpen;

            if (!isOpen)
            {
                gameObject.SetActive(false);
            }

            onComplete?.Invoke();
        }
    }

    private static void CreateHorizontalRule(Transform parent, string name, float anchorY, float y, float height, Color color)
    {
        if (parent == null)
        {
            return;
        }

        GameObject rule = new GameObject(name);
        rule.transform.SetParent(parent, false);

        var rect = rule.AddComponent<RectTransform>();
        // 分隔线铺满全宽，更接近游戏内 MessageDialog 的边界线观感。
        rect.anchorMin = new Vector2(0f, anchorY);
        rect.anchorMax = new Vector2(1f, anchorY);
        rect.pivot = new Vector2(0.5f, anchorY);
        rect.anchoredPosition = new Vector2(0f, y);
        // 分隔线按全宽绘制，更接近游戏内 MessageDialog 的边界线。
        rect.sizeDelta = new Vector2(0f, height);

        var img = rule.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;

        // 轻微发光/描边效果，增强“弹窗分隔线”的观感。
        try
        {
            var shadow = rule.AddComponent<Shadow>();
            var glow = color;
            glow.a = 0.35f;
            shadow.effectColor = glow;
            shadow.effectDistance = new Vector2(-160f, -2f);
        }
        catch
        {
            // ignored
        }
    }

    private static void TryScaleButtonText(Button button, float scale)
    {
        try
        {
            if (button == null)
            {
                return;
            }

            // TMP 文本组件
            var labels = button.GetComponentsInChildren<TMP_Text>(true);
            if (labels != null)
            {
                foreach (var label in labels)
                {
                    if (label == null)
                    {
                        continue;
                    }

                    // 只放大较小字号的按钮文字；避免把模板里已经很大的标题类文字也放大。
                    if (label.fontSize > 0f && label.fontSize < 40f)
                    {
                        label.fontSize = Mathf.Clamp(label.fontSize * scale, 14f, 72f);
                    }
                }
            }

            // 传统 UI.Text 组件
            var legacyTexts = button.GetComponentsInChildren<Text>(true);
            if (legacyTexts != null)
            {
                foreach (var t in legacyTexts)
                {
                    if (t == null)
                    {
                        continue;
                    }

                    if (t.fontSize > 0 && t.fontSize < 40)
                    {
                        t.fontSize = Mathf.Clamp((int)(t.fontSize * scale), 14, 72);
                    }
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void HideOverlay()
    {
        if (_rootAnimator != null && _overlayRoot != null && _overlayRoot.activeInHierarchy)
        {
            _rootAnimator.PlayClose(() =>
            {
                _overlayRoot.SetActive(false);
            });
        }
        else
        {
            _overlayRoot?.SetActive(false);
        }
    }

    private static T GetDialogField<T>(MessageDialog dialog, string fieldName) where T : class
    {
        if (dialog == null || string.IsNullOrWhiteSpace(fieldName))
        {
            return null;
        }

        FieldInfo field = typeof(MessageDialog).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        return field?.GetValue(dialog) as T;
    }

    private static Button CreateDialogButton(Button template, Transform parent, string name, string labelText, float panelScale)
    {
        if (template == null)
        {
            return null;
        }

        GameObject go = UnityEngine.Object.Instantiate(template.gameObject, parent, false);
        go.name = name;
        Button btn = go.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();

        // 仅套用样式，剥离可能会干扰的本地化组件
        TryStripLocalizationComponents(go);

        // 设置文本与大小
        TrySetButtonText(btn, labelText);
        TryScaleButtonText(btn, panelScale);

        // 添加并配置 LayoutElement，这样 LayoutGroup 才能获取正确的缩放后尺寸，防止布局坍塌/重叠
        var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
        le.preferredHeight = 58f * panelScale;
        le.preferredWidth = -1f;

        btn.interactable = true;
        go.SetActive(true);

        return btn;
    }

    private static void ShowWarningDialog(string message)
    {
        UiManager.GetDialog<MessageDialog>().Show(
            new MessageContent
            {
                Text = message,
                Icon = MessageIcon.Warning,
                Buttons = DialogButtons.Confirm,
                OnConfirm = null,
                OnCancel = null,
            }
        );
    }

    private static GameObject CreateInputRow(Transform parent, Button templateButton, string labelText, string placeholderText, string defaultVal, float panelScale, out TMP_InputField inputField)
    {
        GameObject row = new GameObject("Row_" + labelText);
        row.transform.SetParent(parent, false);
        var rowRt = row.AddComponent<RectTransform>();
        rowRt.sizeDelta = new Vector2(400f * panelScale, 42f * panelScale);

        // 添加 LayoutElement，防止被 parent 的 VerticalLayoutGroup 压扁为 0 像素高度
        var le = row.AddComponent<LayoutElement>();
        le.preferredHeight = 42f * panelScale;
        le.preferredWidth = 400f * panelScale;

        var layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 15f * panelScale;

        // Label
        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(row.transform, false);
        var labelRt = labelGo.AddComponent<RectTransform>();
        labelRt.sizeDelta = new Vector2(130f * panelScale, 40f * panelScale);

        var labelTextComp = labelGo.AddComponent<TextMeshProUGUI>();
        labelTextComp.text = labelText;
        labelTextComp.alignment = TextAlignmentOptions.Right;
        labelTextComp.color = new Color(0.86f, 0.73f, 0.34f, 1f); // Gold color for label
        labelTextComp.fontSize = 18f * panelScale;
        labelTextComp.raycastTarget = false;
        if (_defaultFont != null)
        {
            labelTextComp.font = _defaultFont;
        }

        // Input Field (Pass panelScale to scale font size)
        inputField = CreateInputField(row.transform, templateButton, "InputField", placeholderText, defaultVal, 240f * panelScale, 40f * panelScale, panelScale);
        
        return row;
    }

    private static TMP_InputField CreateInputField(Transform parent, Button templateButton, string name, string placeholderText, string defaultText, float width, float height, float panelScale)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, height);

        // Background image
        var img = go.AddComponent<Image>();
        if (templateButton != null && templateButton.targetGraphic is Image templateImg)
        {
            img.sprite = templateImg.sprite;
            img.type = templateImg.type;
            img.color = new Color(0.08f, 0.08f, 0.08f, 0.85f); // Make input field darker
        }
        else
        {
            img.color = new Color(0.08f, 0.08f, 0.08f, 0.85f);
        }

        // TextArea (Viewport)
        GameObject textArea = new GameObject("TextArea");
        textArea.transform.SetParent(go.transform, false);
        var textAreaRt = textArea.AddComponent<RectTransform>();
        textAreaRt.anchorMin = Vector2.zero;
        textAreaRt.anchorMax = Vector2.one;
        // Adjust padding offset based on scale for correct vertical centering
        textAreaRt.offsetMin = new Vector2(14f * panelScale, 2f * panelScale);
        textAreaRt.offsetMax = new Vector2(-14f * panelScale, -2f * panelScale);
        textArea.AddComponent<RectMask2D>();

        // Placeholder Text
        GameObject placeholderGo = new GameObject("Placeholder");
        placeholderGo.transform.SetParent(textArea.transform, false);
        var placeholderRt = placeholderGo.AddComponent<RectTransform>();
        placeholderRt.anchorMin = Vector2.zero;
        placeholderRt.anchorMax = Vector2.one;
        placeholderRt.offsetMin = Vector2.zero;
        placeholderRt.offsetMax = Vector2.zero;
        
        var placeholderTmp = placeholderGo.AddComponent<TextMeshProUGUI>();
        placeholderTmp.text = placeholderText;
        placeholderTmp.alignment = TextAlignmentOptions.MidlineLeft;
        placeholderTmp.color = new Color(0.6f, 0.6f, 0.6f, 0.6f);
        placeholderTmp.fontSize = 18f * panelScale;
        placeholderTmp.raycastTarget = false;
        if (_defaultFont != null)
        {
            placeholderTmp.font = _defaultFont;
        }

        // Input Text
        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(textArea.transform, false);
        var textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        
        var textTmp = textGo.AddComponent<TextMeshProUGUI>();
        textTmp.text = defaultText;
        textTmp.alignment = TextAlignmentOptions.MidlineLeft;
        textTmp.color = Color.white;
        textTmp.fontSize = 18f * panelScale;
        textTmp.raycastTarget = false;
        if (_defaultFont != null)
        {
            textTmp.font = _defaultFont;
        }

        // Add InputField component
        var inputField = go.AddComponent<TMP_InputField>();
        inputField.textViewport = textAreaRt;
        inputField.textComponent = textTmp;
        inputField.placeholder = placeholderTmp;
        inputField.text = defaultText;
        if (_defaultFont != null)
        {
            inputField.fontAsset = _defaultFont;
        }

        return inputField;
    }

    private static void ShowWarningDialog(string message)
    {
        UiManager.GetDialog<MessageDialog>().Show(
            new MessageContent
            {
                Text = message,
                Icon = MessageIcon.Warning,
                Buttons = DialogButtons.Confirm,
                OnConfirm = null,
                OnCancel = null,
            }
        );
    }

    private static GameObject CreateInputRow(Transform parent, Button templateButton, string labelText, string placeholderText, string defaultVal, float panelScale, out TMP_InputField inputField)
    {
        GameObject row = new GameObject("Row_" + labelText);
        row.transform.SetParent(parent, false);
        var rowRt = row.AddComponent<RectTransform>();
        rowRt.sizeDelta = new Vector2(400f * panelScale, 42f * panelScale);

        var layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 15f * panelScale;

        // Label
        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(row.transform, false);
        var labelRt = labelGo.AddComponent<RectTransform>();
        labelRt.sizeDelta = new Vector2(130f * panelScale, 40f * panelScale);

        var labelTextComp = labelGo.AddComponent<TextMeshProUGUI>();
        labelTextComp.text = labelText;
        labelTextComp.alignment = TextAlignmentOptions.Right;
        labelTextComp.color = new Color(0.86f, 0.73f, 0.34f, 1f); // Gold color for label
        labelTextComp.fontSize = 18f * panelScale;
        labelTextComp.raycastTarget = false;
        if (_defaultFont != null)
        {
            labelTextComp.font = _defaultFont;
        }

        // Input Field (Pass panelScale to scale font size)
        inputField = CreateInputField(row.transform, templateButton, "InputField", placeholderText, defaultVal, 240f * panelScale, 40f * panelScale, panelScale);
        
        return row;
    }

    private static TMP_InputField CreateInputField(Transform parent, Button templateButton, string name, string placeholderText, string defaultText, float width, float height, float panelScale)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, height);

        // Background image
        var img = go.AddComponent<Image>();
        if (templateButton != null && templateButton.targetGraphic is Image templateImg)
        {
            img.sprite = templateImg.sprite;
            img.type = templateImg.type;
            img.color = new Color(0.08f, 0.08f, 0.08f, 0.85f); // Make input field darker
        }
        else
        {
            img.color = new Color(0.08f, 0.08f, 0.08f, 0.85f);
        }

        // TextArea (Viewport)
        GameObject textArea = new GameObject("TextArea");
        textArea.transform.SetParent(go.transform, false);
        var textAreaRt = textArea.AddComponent<RectTransform>();
        textAreaRt.anchorMin = Vector2.zero;
        textAreaRt.anchorMax = Vector2.one;
        // Adjust padding offset based on scale for correct vertical centering
        textAreaRt.offsetMin = new Vector2(14f * panelScale, 2f * panelScale);
        textAreaRt.offsetMax = new Vector2(-14f * panelScale, -2f * panelScale);
        textArea.AddComponent<RectMask2D>();

        // Placeholder Text
        GameObject placeholderGo = new GameObject("Placeholder");
        placeholderGo.transform.SetParent(textArea.transform, false);
        var placeholderRt = placeholderGo.AddComponent<RectTransform>();
        placeholderRt.anchorMin = Vector2.zero;
        placeholderRt.anchorMax = Vector2.one;
        placeholderRt.offsetMin = Vector2.zero;
        placeholderRt.offsetMax = Vector2.zero;
        
        var placeholderTmp = placeholderGo.AddComponent<TextMeshProUGUI>();
        placeholderTmp.text = placeholderText;
        placeholderTmp.alignment = TextAlignmentOptions.MidlineLeft;
        placeholderTmp.color = new Color(0.6f, 0.6f, 0.6f, 0.6f);
        placeholderTmp.fontSize = 18f * panelScale;
        placeholderTmp.raycastTarget = false;
        if (_defaultFont != null)
        {
            placeholderTmp.font = _defaultFont;
        }

        // Input Text
        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(textArea.transform, false);
        var textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        
        var textTmp = textGo.AddComponent<TextMeshProUGUI>();
        textTmp.text = defaultText;
        textTmp.alignment = TextAlignmentOptions.MidlineLeft;
        textTmp.color = Color.white;
        textTmp.fontSize = 18f * panelScale;
        textTmp.raycastTarget = false;
        if (_defaultFont != null)
        {
            textTmp.font = _defaultFont;
        }

        // Add InputField component
        var inputField = go.AddComponent<TMP_InputField>();
        inputField.textViewport = textAreaRt;
        inputField.textComponent = textTmp;
        inputField.placeholder = placeholderTmp;
        inputField.text = defaultText;
        if (_defaultFont != null)
        {
            inputField.fontAsset = _defaultFont;
        }

        return inputField;
    }

    private static TMP_FontAsset FindDefaultFont(Transform any)
    {
        try
        {
            if (any == null)
            {
                return null;
            }

            var label = any.GetComponentInChildren<TextMeshProUGUI>(true);
            return label?.font;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsDescendantOf(Transform child, Transform parent)
    {
        if (child == null || parent == null)
        {
            return false;
        }
        Transform current = child.parent;
        while (current != null)
        {
            if (current == parent)
            {
                return true;
            }
            current = current.parent;
        }
        return false;
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
        if (port <= 0)
        {
            port = 7777;
        }

        // 如果存在本地存档，则提供重连继续流程：
        // 1) 先建立连接
        // 2) 再恢复本地存档
        // 3) 向房主请求完整快照并做追赶（mid-game join）
        GameRunSaveData save = null;
        try
        {
            save = Singleton<GameMaster>.Instance?.GameRunSaveData;
        }
        catch
        {
            save = null;
        }

        if (save != null && Singleton<GameMaster>.Instance?.CurrentGameRun == null)
        {
            UiManager.GetDialog<MessageDialog>().Show(
                new MessageContent
                {
                    Text = $"检测到本地可继续的存档。\n\n将作为客户端加入服务器：{ip}:{port}\n\n确认：重连并继续存档（本地恢复 + 向房主追赶）\n取消：只连接（不恢复存档）",
                    Icon = MessageIcon.Warning,
                    Buttons = DialogButtons.ConfirmCancel,
                    OnConfirm = () => TryConnectToServerAndRestoreAndCatchUp(ip, port, save),
                    OnCancel = () => TryConnectToServer(ip, port),
                }
            );
            return;
        }

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
    internal static void TryHostLocalServerAndConnect()
    {
        ConfigManager config = TryGetConfig();
        int port = config?.HostServerPort?.Value ?? 7777;
        if (port <= 0)
        {
            port = 7777;
        }
        int maxConn = config?.HostMaxConnections?.Value ?? 4;
        string key = config?.HostConnectionKey?.Value ?? "LBoL_Network_Plugin";

        try
        {
            // 若本机服务器未运行，则启动并开启轮询线程。
            if (!_localServerRunning)
            {
                _localServer = new NetworkServer(port, maxConn, key, Plugin.Logger);
                _localServer.Start();
                _localServerRunning = true;
                StartLocalServerLoop();

                Plugin.Logger?.LogInfo($"[MainMenuMultiplayerEntry] Host server started: 127.0.0.1:{port}");
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

    internal static void TryHostLocalServerAndConnectAndRestore(GameRunSaveData save)
    {
        if (save == null)
        {
            return;
        }

        try
        {
            TryHostLocalServerAndConnect();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[MainMenuMultiplayerEntry] 启动房主并连接失败: {ex.Message}");
            return;
        }

        // 等待连接建立后再 Restore（避免先进入局内导致联机状态晚于游戏流程）。
        try
        {
            Singleton<GameMaster>.Instance.StartCoroutine(CoWaitForConnectedThenRestore(save));
        }
        catch
        {
            // ignored
        }
    }

    private static IEnumerator CoWaitForConnectedThenRestore(GameRunSaveData save)
    {
        INetworkClient client = TryGetNetworkClient();

        float start = Time.realtimeSinceStartup;
        const float timeoutSeconds = 8f;

        while (Time.realtimeSinceStartup - start < timeoutSeconds)
        {
            try
            {
                if (client != null && client.IsConnected)
                {
                    break;
                }
            }
            catch
            {
                // ignored
            }

            yield return null;
        }

        bool connected = false;
        try
        {
            connected = client != null && client.IsConnected;
        }
        catch
        {
            connected = false;
        }

        if (!connected)
        {
            Plugin.Logger?.LogWarning("[MainMenuMultiplayerEntry] 等待联机连接超时，已取消继续存档（联机）。");
            UiManager.GetDialog<MessageDialog>().Show(
                new MessageContent
                {
                    Text = "联机连接超时，无法作为房主继续存档。\n\n请检查端口占用或网络模块状态。",
                    Icon = MessageIcon.Warning,
                    Buttons = DialogButtons.Confirm,
                }
            );
            yield break;
        }

        try
        {
            Plugin.Logger?.LogInfo("[MainMenuMultiplayerEntry] 联机已连接，开始恢复存档进入游戏。");
            GameMaster.RestoreGameRun(save);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[MainMenuMultiplayerEntry] 恢复存档失败: {ex.Message}");
            UiManager.GetDialog<MessageDialog>().Show(
                new MessageContent
                {
                    Text = "恢复存档失败，请检查日志。",
                    Icon = MessageIcon.Warning,
                    Buttons = DialogButtons.Confirm,
                }
            );
        }
    }

    /// <summary>
    /// 尝试连接到指定服务器（Join/Host 共用）。
    /// </summary>
    /// <param name="host">服务器地址。</param>
    /// <param name="port">服务器端口。</param>
    internal static void TryConnectToServer(string host, int port)
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

    private static void TryConnectToServerAndRestoreAndCatchUp(string host, int port, GameRunSaveData save)
    {
        if (save == null)
        {
            TryConnectToServer(host, port);
            return;
        }

        TryConnectToServer(host, port);

        try
        {
            Singleton<GameMaster>.Instance.StartCoroutine(CoWaitForConnectedThenRestoreThenCatchUp(save));
        }
        catch
        {
            // ignored
        }
    }

    private static IEnumerator CoWaitForConnectedThenRestoreThenCatchUp(GameRunSaveData save)
    {
        INetworkClient client = TryGetNetworkClient();

        float start = Time.realtimeSinceStartup;
        const float timeoutSeconds = 10f;

        while (Time.realtimeSinceStartup - start < timeoutSeconds)
        {
            bool connected = false;
            try
            {
                connected = client != null && client.IsConnected;
            }
            catch
            {
                connected = false;
            }

            if (connected)
            {
                break;
            }

            yield return null;
        }

        bool ok = false;
        try
        {
            ok = client != null && client.IsConnected;
        }
        catch
        {
            ok = false;
        }

        if (!ok)
        {
            UiManager.GetDialog<MessageDialog>().Show(
                new MessageContent
                {
                    Text = "联机连接超时，无法重连继续存档。\n\n请检查网络模块状态。",
                    Icon = MessageIcon.Warning,
                    Buttons = DialogButtons.Confirm,
                }
            );
            yield break;
        }

        // 等待握手消息（Welcome/PlayerListUpdate）到达，让 MidGameJoin 拿到 selfId/hostId。
        // 这样可以避免刚连接成功就立刻 RequestJoin 失败。
        NetworkPlugin.Network.MidGameJoin.MidGameJoinManager mgrHandshake = null;
        try
        {
            mgrHandshake = ServiceProvider?.GetService<NetworkPlugin.Network.MidGameJoin.MidGameJoinManager>();
        }
        catch
        {
            mgrHandshake = null;
        }

        float hsStart = Time.realtimeSinceStartup;
        const float hsTimeoutSeconds = 6f;
        while (Time.realtimeSinceStartup - hsStart < hsTimeoutSeconds)
        {
            string selfId = string.Empty;
            string hostId = string.Empty;
            try
            {
                selfId = NetworkPlugin.Utils.NetworkIdentityTracker.GetSelfPlayerId() ?? string.Empty;
                hostId = mgrHandshake?.GetLastKnownHostPlayerId() ?? string.Empty;
            }
            catch
            {
                // ignored
            }

            if (!string.IsNullOrWhiteSpace(selfId) && !string.IsNullOrWhiteSpace(hostId))
            {
                break;
            }

            yield return null;
        }

        // 1) 在主线程恢复本地存档
        try
        {
            Plugin.Logger?.LogInfo("[MainMenuMultiplayerEntry] 联机已连接，开始本地恢复存档。");
            GameMaster.RestoreGameRun(save);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[MainMenuMultiplayerEntry] 本地恢复存档失败: {ex.Message}");
            UiManager.GetDialog<MessageDialog>().Show(
                new MessageContent
                {
                    Text = "本地恢复存档失败，请检查日志。",
                    Icon = MessageIcon.Warning,
                    Buttons = DialogButtons.Confirm,
                }
            );
            yield break;
        }

        // 等到恢复后的 GameRun 真正创建完成，避免追赶流程误弹 StartGamePanel。
        float runStart = Time.realtimeSinceStartup;
        const float runTimeoutSeconds = 8f;
        while (Time.realtimeSinceStartup - runStart < runTimeoutSeconds)
        {
            bool hasRun = false;
            try
            {
                hasRun = NetworkPlugin.Utils.GameStateUtils.GetCurrentGameRun() != null;
            }
            catch
            {
                hasRun = false;
            }

            if (hasRun)
            {
                break;
            }

            yield return null;
        }

        // 2) 向房主请求 FullSnapshot 并执行追赶。
        // RoomId 当前没有暴露到 UI，因此先使用一个确定性的占位字符串。
        // Host 和 joiner 必须使用同一个字符串。
        // 另外还要等 hostId 通过 PlayerListUpdate 可用后，再发送 join 请求。
        yield return null;
        yield return null;

        var mgr2 = ServiceProvider?.GetService<NetworkPlugin.Network.MidGameJoin.MidGameJoinManager>();
        if (mgr2 == null)
        {
            yield break;
        }

        float joinGateStart = Time.realtimeSinceStartup;
        const float joinGateTimeoutSeconds = 10f;
        while (Time.realtimeSinceStartup - joinGateStart < joinGateTimeoutSeconds)
        {
            string selfId = string.Empty;
            string hostId = string.Empty;
            try
            {
                selfId = NetworkPlugin.Utils.NetworkIdentityTracker.GetSelfPlayerId() ?? string.Empty;
                hostId = mgr2.GetLastKnownHostPlayerId() ?? string.Empty;
            }
            catch
            {
                // ignored
            }

            if (!string.IsNullOrWhiteSpace(selfId) && !string.IsNullOrWhiteSpace(hostId))
            {
                break;
            }

            yield return null;
        }

        try
        {

            string playerName = "joiner";
            try
            {
                playerName = Singleton<GameMaster>.Instance?.CurrentProfile?.Name ?? playerName;
            }
            catch
            {
                // ignored
            }

            const string roomId = "default";

            mgr2.BeginReconnectAndCatchUp(
                roomId,
                playerName,
                onStatus: s => Plugin.Logger?.LogInfo("[Reconnect] " + s),
                onCompleted: r =>
                {
                    try
                    {
                        if (r?.IsSuccess == true)
                        {
                            Plugin.Logger?.LogInfo("[Reconnect] Catch-up completed.");
                            return;
                        }

                        string err = r?.ErrorMessage ?? "Unknown error";
                        Plugin.Logger?.LogWarning("[Reconnect] Catch-up failed: " + err);
                        UiManager.GetDialog<MessageDialog>().Show(new MessageContent
                        {
                            Text = "追赶同步失败：\n" + err,
                            Icon = MessageIcon.Warning,
                            Buttons = DialogButtons.Confirm,
                        });
                    }
                    catch
                    {
                        // ignored
                    }
                },
                timeoutSeconds: 20);
        }
        catch
        {
            // ignored
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
