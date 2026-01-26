using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
using NetworkPlugin.Network;
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
    private static TMP_FontAsset _defaultFont;

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
            // keep going and attempt to create
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
        _multiplayerButton = UnityEngine.Object.Instantiate(template, parent);
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

        _startGameMultiplayerButton = UnityEngine.Object.Instantiate(template, parent);
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
            var canvas = any != null ? any.GetComponentInParent<Canvas>() : null;
            return canvas != null ? canvas.GetComponent<RectTransform>() : null;
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

        var t = new Vector3[4];
        var c = new Vector3[4];
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

        var t = new Vector3[4];
        var c = new Vector3[4];
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
            var fields = AccessTools.GetDeclaredFields(panel.GetType())
                .Where(f => typeof(Button).IsAssignableFrom(f.FieldType))
                .ToList();

            foreach (var f in fields)
            {
                var b = f.GetValue(panel) as Button;
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
                var candidates = buttons
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
                // UI 反馈迭代中直接重建，避免历史样式残留导致“看起来没变化”。
                UnityEngine.Object.Destroy(_overlayRoot);
                _overlayRoot = null;
            }

            _defaultFont ??= FindDefaultFont(panelTransform);

            var root = new GameObject(OverlayRootName);
            root.transform.SetParent(panelTransform, false);
            root.transform.SetAsLastSibling();
            _overlayRoot = root;

            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 0f);
            rootRect.anchorMax = new Vector2(1f, 1f);
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            // 半透明遮罩背景，拦截点击。
            var bg = root.AddComponent<Image>();
            // 参考 MessageDialog 的视觉：全屏暗化背景（提升对比度），并保留拦截点击。
            bg.color = new Color(0f, 0f, 0f, 0.62f);
            bg.raycastTarget = true;

            // 参考 MessageDialog 的出现动效：简单的淡入 + 轻微缩放。
            // 注意：这里不直接复用 MessageDialog 实例，因为 UiManager 只允许同时显示一个 Dialog。
            var rootGroup = root.AddComponent<CanvasGroup>();
            rootGroup.alpha = 0f;
            rootGroup.interactable = false;
            rootGroup.blocksRaycasts = true;

            // 中间面板整体缩放：期望 3 倍，但必须保证不超出屏幕。
            // 这里用 rootRect 的尺寸做自适应，尽量接近 3 倍。
            float panelScale = 3f;
            try
            {
                const float baseW = 520f;
                const float baseH = 380f;

                // 用户诉求：高度变小，宽度铺满屏幕。
                // 用接近原始宽度的基准计算缩放，避免容器过窄；高度略收紧。
                const float widthFactor = 1.00f;
                const float heightFactor = 0.95f;

                float maxW = Mathf.Max(1f, rootRect.rect.width * 0.92f);
                float maxH = Mathf.Max(1f, rootRect.rect.height * 0.92f);
                float fitScale = Mathf.Min(maxW / (baseW * widthFactor), maxH / (baseH * heightFactor));
                panelScale = Mathf.Min(3f, fitScale);
                // 兜底：避免极端情况下缩放为 0。
                panelScale = Mathf.Max(1f, panelScale);
            }
            catch
            {
                panelScale = 3f;
            }

            // 中央容器。
            var container = new GameObject("Container");
            container.transform.SetParent(root.transform, false);
            var containerRect = container.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);

            // 与上面缩放逻辑保持一致：更宽 + 更矮。
            const float containerWidthFactor = 1.00f;
            const float containerHeightFactor = 0.95f;
            containerRect.sizeDelta = new Vector2(520f * panelScale * containerWidthFactor, 380f * panelScale * containerHeightFactor);
            containerRect.anchoredPosition = Vector2.zero;

            // 绑定入场动画。
            try
            {
                var animator = root.AddComponent<OverlayIntroAnimator>();
                // AddComponent 在激活对象上会立即触发 OnEnable；先禁用确保 Init 先完成。
                animator.enabled = false;
                animator.Init(rootGroup, containerRect);
                animator.enabled = true;
            }
            catch
            {
                // ignored
            }

            var containerBg = container.AddComponent<Image>();
            // 用户诉求：中间面板背景透明。
            containerBg.color = new Color(0f, 0f, 0f, 0f);
            containerBg.raycastTarget = false;

            // 参考 MessageDialog 的上下分隔线：铺满屏幕宽度，但高度（上下间距）更小。
            // 这里把分隔线放在 root 上，并用屏幕高度的比例来收紧“边界高度”。
            try
            {
                float yAbs = 220f;
                try
                {
                    float maxByContainer = (containerRect.sizeDelta.y * 0.5f) - 30f;
                    float desired = rootRect.rect.height * 0.22f; // 1080p 下约 238px
                    yAbs = Mathf.Clamp(desired, 160f, Mathf.Max(160f, maxByContainer));
                }
                catch
                {
                    // ignored
                }

                var gold = new Color(0.86f, 0.73f, 0.34f, 0.9f);
                CreateHorizontalRule(root.transform, "TopRule", anchorY: 0.5f, y: yAbs, height: 4f, color: gold);
                CreateHorizontalRule(root.transform, "BottomRule", anchorY: 0.5f, y: -yAbs-60, height: 4f, color: gold);
            }
            catch
            {
                // ignored
            }

            // 标题。
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(container.transform, false);
            var titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -18f * panelScale);
            titleRect.sizeDelta = new Vector2(0f, 48f * panelScale);

            var title = titleGo.AddComponent<TextMeshProUGUI>();
            title.text = "多人游戏";
            // 用户最新诉求：红框标题文字需要更大。
            title.fontSize = Mathf.Clamp(30f * panelScale, 30f, 96f);
            title.alignment = TextAlignmentOptions.Midline;
            title.color = Color.white;
            title.raycastTarget = false;
            if (_defaultFont != null)
            {
                title.font = _defaultFont;
            }

            // 说明。
            var descGo = new GameObject("Description");
            descGo.transform.SetParent(container.transform, false);
            var descRect = descGo.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0f, 1f);
            descRect.anchorMax = new Vector2(1f, 1f);
            descRect.pivot = new Vector2(0.5f, 1f);
            descRect.anchoredPosition = new Vector2(0f, -74f * panelScale);
            descRect.sizeDelta = new Vector2(-40f * panelScale, 70f * panelScale);

            var desc = descGo.AddComponent<TextMeshProUGUI>();
            desc.text = "请选择联机方式：\n房主：启动本机服务器并连接\n加入：连接到配置的服务器";
            // 用户诉求：说明文字变大。
            desc.fontSize = 18f * panelScale;
            desc.alignment = TextAlignmentOptions.TopLeft;
            desc.color = Color.white;
            desc.raycastTarget = false;
            if (_defaultFont != null)
            {
                desc.font = _defaultFont;
            }

            // 按钮区域。
            var buttonsGo = new GameObject("Buttons");
            buttonsGo.transform.SetParent(container.transform, false);
            var buttonsRect = buttonsGo.AddComponent<RectTransform>();
            buttonsRect.anchorMin = new Vector2(0.5f, 0f);
            buttonsRect.anchorMax = new Vector2(0.5f, 0f);
            buttonsRect.pivot = new Vector2(0.5f, 0f);
            // 用户最新诉求：黄框按钮变窄（收窄按钮区域宽度即可，子按钮会随布局一起变窄）。
            float buttonsWidth = 380f * panelScale;
            try
            {
                buttonsWidth = Mathf.Min(buttonsWidth, containerRect.sizeDelta.x * 0.60f);
            }
            catch
            {
                // ignored
            }
            buttonsRect.sizeDelta = new Vector2(buttonsWidth, 196f * panelScale);
            // 用户诉求：叙述文字与按钮区域间距更大一点 → 按钮整体往下挪一点。
            buttonsRect.anchoredPosition = new Vector2(0f, 10f * panelScale);

            var layout = buttonsGo.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.spacing = 12f * panelScale;
            layout.padding = new RectOffset(0, 0, 0, 0);

            // 房主
            var hostBtn = UnityEngine.Object.Instantiate(template, buttonsGo.transform);
            hostBtn.name = "NetworkPlugin_HostButton";
            hostBtn.onClick.RemoveAllListeners();
            hostBtn.onClick.AddListener(() =>
            {
                HideOverlay();

                // 若存在可继续的存档，提示用户选择“继续存档”还是“开新游戏”。
                try
                {
                    GameRunSaveData save = Singleton<GameMaster>.Instance?.GameRunSaveData;
                    if (save != null && Singleton<GameMaster>.Instance.CurrentGameRun == null)
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
                }
                catch
                {
                    // ignored
                }

                TryHostLocalServerAndConnect();
            });
            TrySetButtonText(hostBtn, "做房主");
            TryScaleButtonText(hostBtn, panelScale);

            // 加入
            var joinBtn = UnityEngine.Object.Instantiate(template, buttonsGo.transform);
            joinBtn.name = "NetworkPlugin_JoinButton";
            joinBtn.onClick.RemoveAllListeners();
            joinBtn.onClick.AddListener(() =>
            {
                HideOverlay();
                ShowJoinConfirmDialog();
            });
            TrySetButtonText(joinBtn, "加入房主");
            TryScaleButtonText(joinBtn, panelScale);

            // 返回
            var backBtn = UnityEngine.Object.Instantiate(template, buttonsGo.transform);
            backBtn.name = "NetworkPlugin_BackButton";
            backBtn.onClick.RemoveAllListeners();
            backBtn.onClick.AddListener(HideOverlay);
            TrySetButtonText(backBtn, "返回");
            TryScaleButtonText(backBtn, panelScale);

            // 如果模板按钮默认是不可交互或隐藏（例如存档存在时 newGameButton 被隐藏），强制保证 overlay 内按钮可用。
            foreach (var b in new[] { hostBtn, joinBtn, backBtn })
            {
                if (b == null) continue;
                b.interactable = true;
                b.gameObject.SetActive(true);

                // 统一尺寸。
                var r = b.GetComponent<RectTransform>();
                if (r != null)
                {
                    // 用户诉求：按钮高度小一点点。
                    r.sizeDelta = new Vector2(r.sizeDelta.x, 58f * panelScale);
                }
            }

            // 最后兜底：确保容器不会超出画布。
            try
            {
                var canvasRect = TryGetRootCanvasRectTransform(panelTransform);
                if (canvasRect != null)
                {
                    ClampToContainer(containerRect, canvasRect, paddingWorld: 0f);
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

    private sealed class OverlayIntroAnimator : MonoBehaviour
    {
        private CanvasGroup _rootGroup;
        private RectTransform _container;
        private bool _played;

        public void Init(CanvasGroup rootGroup, RectTransform container)
        {
            _rootGroup = rootGroup;
            _container = container;

            // 如果组件已经处于可用状态（例如后续被启用），确保不会因为 OnEnable 早于 Init 而漏播动画。
            if (isActiveAndEnabled && !_played && _rootGroup != null)
            {
                _played = true;
                StartCoroutine(Play());
            }
        }

        private void OnEnable()
        {
            if (_played)
            {
                return;
            }

            // Init 可能晚于 OnEnable（AddComponent 的时序）；等到 Init 填充引用后再播。
            if (_rootGroup == null)
            {
                return;
            }

            _played = true;
            StartCoroutine(Play());
        }

        private IEnumerator Play()
        {
            if (_rootGroup == null)
            {
                yield break;
            }

            if (_container != null)
            {
                _container.localScale = new Vector3(0.92f, 0.92f, 1f);
            }

            const float duration = 0.22f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / duration);
                // easeOutCubic
                float e = 1f - Mathf.Pow(1f - p, 3f);

                _rootGroup.alpha = e;
                if (_container != null)
                {
                    float s = Mathf.Lerp(0.92f, 1f, e);
                    _container.localScale = new Vector3(s, s, 1f);
                }

                yield return null;
            }

            _rootGroup.alpha = 1f;
            _rootGroup.interactable = true;
            _rootGroup.blocksRaycasts = true;
            if (_container != null)
            {
                _container.localScale = Vector3.one;
            }
        }
    }

    private static void CreateHorizontalRule(Transform parent, string name, float anchorY, float y, float height, Color color)
    {
        if (parent == null)
        {
            return;
        }

        var rule = new GameObject(name);
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

            // TMP
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

            // legacy UI.Text
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
        try
        {
            if (_overlayRoot != null)
            {
                _overlayRoot.SetActive(false);
            }
        }
        catch
        {
            // ignored
        }
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
        int port = config?.ServerPort?.Value ?? 7777;
        if (port <= 0)
        {
            port = 7777;
        }
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
