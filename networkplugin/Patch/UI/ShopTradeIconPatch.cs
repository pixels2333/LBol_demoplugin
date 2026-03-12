using System;
using System.Linq;
using HarmonyLib;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.Units;
using NetworkPlugin.UI.Factories;
using NetworkPlugin.UI.Payloads;
using NetworkPlugin.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.Patch.UI;

/// <summary>
/// 参照 Together in Spire 的 ShopTradeIconPatch.java：
/// - 在商店界面可见时渲染/更新一个“交易(TRADE)”按钮
/// - 当联机且允许交易时显示，点击后尝试打开交易面板（若不可用则提示）
/// </summary>
[HarmonyPatch]
public static class ShopTradeIconPatch
{
    private const string TradeButtonLabelText = "玩家交易";

    private enum TradeUiUpdateState
    {
        NoShopPanel,
        Hidden,
        Visible
    }

    private sealed class TradeButtonUi
    {
        public ShopPanel ShopPanel;
        public GameObject Root;
        public Button Button;
        public Button CardServiceButton;
        public Button ReturnButton;

        public Vector2 CardServiceOriginalAnchoredPosition;
        public Vector2 CardServiceOriginalSizeDelta;
        public Vector3 CardServiceOriginalScale;
        public Vector3 CardServiceOriginalLocalPosition;

        public Vector2 ReturnOriginalAnchoredPosition;
        public Vector2 ReturnOriginalSizeDelta;
        public Vector3 ReturnOriginalScale;
        public Vector3 ReturnOriginalLocalPosition;
    }

    private static TradeButtonUi _ui;
    private static TMP_FontAsset _defaultFont;
    private static Sprite _whiteSprite;
    private static Texture2D _whiteTexture;

    private static ShopPanel _cachedShopPanel;

    // 该补丁每帧都会通过 GameDirector.Update 运行，因此需要节流高频日志。
    private static TradeUiUpdateState _lastState;
    private static bool _hasLastState;
    private static float _nextStateLogTime;
    private static string _lastStateLogKey;

    [HarmonyPatch(typeof(ShopPanel), "OnShown")]
    [HarmonyPostfix]
    private static void ShopPanel_OnShown_Postfix(ShopPanel __instance)
    {
        try
        {
            _cachedShopPanel = __instance;
            _hasLastState = false;
            LogStateThrottled(TradeUiUpdateState.Visible, "[ShopTradeIcon] ShopPanel 已显示：开始刷新交易按钮", 0.0f);
            UpdateTradeButtonUi(__instance);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopTradeIcon] ShopPanel.OnShown 处理失败：{ex.Message}\n{ex.StackTrace}");
        }
    }

    [HarmonyPatch(typeof(ShopPanel), "OnHiding")]
    [HarmonyPostfix]
    private static void ShopPanel_OnHiding_Postfix(ShopPanel __instance)
    {
        try
        {
            if (_ui?.Root != null)
            {
                // 商店即将关闭，销毁注入的 UI，避免残留对象泄漏。
                CleanupUi();
            }

            if (ReferenceEquals(_cachedShopPanel, __instance))
            {
                _cachedShopPanel = null;
            }

            _hasLastState = false;
            LogStateThrottled(TradeUiUpdateState.Hidden, "[ShopTradeIcon] ShopPanel 开始隐藏：已清理交易按钮", 0.0f);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopTradeIcon] ShopPanel.OnHiding 处理失败：{ex.Message}\n{ex.StackTrace}");
        }
    }

    private static bool IsTradeEnabledAndConnected()
    {
        return TradeUiMessages.IsTradeEnabledAndConnected(out _);
    }

    private static void LogStateThrottled(TradeUiUpdateState state, string message, float intervalSeconds = 2.0f)
    {
        float now = Time.unscaledTime;
        string key = state + ":" + message;
        bool stateChanged = !_hasLastState || state != _lastState;
        bool keyChanged = _lastStateLogKey != key;

        if (stateChanged || keyChanged || now >= _nextStateLogTime)
        {
            Plugin.Logger?.LogInfo(message);
            _lastState = state;
            _hasLastState = true;
            _lastStateLogKey = key;
            _nextStateLogTime = now + intervalSeconds;
        }
    }

    [HarmonyPatch(typeof(GameDirector), "Update")]
    [HarmonyPostfix]
    private static void GameDirector_Update_Postfix()
    {
        try
        {
            // 轻量级看门狗：只在商店当前可见时刷新。
            var shopPanel = _cachedShopPanel;
            if (shopPanel == null)
            {
                return;
            }

            if (!shopPanel.IsVisible)
            {
                return;
            }

            UpdateTradeButtonUi(shopPanel);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopTradeIcon] 更新失败：{ex.Message}\n{ex.StackTrace}");
        }
    }

    private static void UpdateTradeButtonUi(ShopPanel shopPanel)
    {
        if (shopPanel == null || !shopPanel.IsVisible)
        {
            CleanupUi();
            return;
        }

        bool shouldShow = IsTradeEnabledAndConnected();
        if (!shouldShow)
        {
            CleanupUi();
            return;
        }

        // 避免每帧都执行重建之类的重操作。
        if (_ui == null || _ui.Root == null || _ui.ShopPanel != shopPanel)
        {
            EnsureUi(shopPanel);
        }

        _ui?.Root?.SetActive(true);

        SetUiVisible(true);
    }

    private static void EnsureUi(ShopPanel shopPanel)
    {
        if (_ui != null && _ui.Root != null && _ui.ShopPanel == shopPanel) // 如果UI已存在且匹配当前商店面板，则无需重建
        {
            return;
        }

        CleanupUi(); // 清理旧UI以准备重建

        bool gotButtons = TryGetShopButtons(shopPanel, out Button cardServiceButton, out Button returnButton); // 尝试获取商店的卡牌服务和返回按钮
        if (!gotButtons)
        {
            Plugin.Logger?.LogWarning("[ShopTradeIcon] 无法找到商店按钮，无法在中间插入。");
            return;
        }

        // 直接读取 ShopPanel.shopBoard 字段作为统一父节点，确保与 cardServiceButton、returnButton 严格同级
        Transform barParent = null;
        try
        {
            var shopBoardGo = Traverse.Create(shopPanel).Field("shopBoard").GetValue<GameObject>();
            if (shopBoardGo != null)
            {
                barParent = shopBoardGo.transform;
            }
        }
        catch
        {
            barParent = null;
        }

        if (barParent == null)
        {
            Plugin.Logger?.LogWarning("[ShopTradeIcon] 无法读取 shopBoard 字段，无法插入容器。");
            return;
        }

        Plugin.Logger?.LogInfo($"[ShopTradeIcon] barParent = {barParent.name}，cardService parent = {cardServiceButton.transform.parent?.name}，returnButton parent = {returnButton.transform.parent?.name}");

        _defaultFont ??= FindDefaultFont(barParent);

        // 获取 CardService 容器和 ReturnButton 容器作为参照
        RectTransform leftContainer = cardServiceButton.transform.parent as RectTransform;
        RectTransform rightContainer = returnButton.transform.parent as RectTransform;

        if (leftContainer == null || rightContainer == null)
        {
            Plugin.Logger?.LogWarning("[ShopTradeIcon] 无法获取按钮容器，退回到直接按钮引用。");
            leftContainer = cardServiceButton.GetComponent<RectTransform>();
            rightContainer = returnButton.GetComponent<RectTransform>();
        }

        TradeButtonUi ui = new TradeButtonUi
        {
            ShopPanel         = shopPanel,
            CardServiceButton = cardServiceButton,
            ReturnButton      = returnButton,
        };
        
        if (leftContainer != null)
        {
            ui.CardServiceOriginalAnchoredPosition = leftContainer.anchoredPosition;
            ui.CardServiceOriginalSizeDelta = leftContainer.sizeDelta;
            ui.CardServiceOriginalScale = leftContainer.localScale;
            ui.CardServiceOriginalLocalPosition = leftContainer.localPosition;
        }
        
        if (rightContainer != null)
        {
            ui.ReturnOriginalAnchoredPosition = rightContainer.anchoredPosition;
            ui.ReturnOriginalSizeDelta = rightContainer.sizeDelta;
            ui.ReturnOriginalScale = rightContainer.localScale;
            ui.ReturnOriginalLocalPosition = rightContainer.localPosition;
        }
        
        _ui = ui;
        
        GameObject midGo = null;
        try
        {

        // 克隆整个 CardService 容器，以获得背景、边框等完整按钮样式
        midGo = UnityEngine.Object.Instantiate(leftContainer.gameObject, barParent, false);
        midGo.name = "NetworkPlugin_TradeButton";
        RectTransform mid = midGo.GetComponent<RectTransform>();
        
        // 查找克隆后的 Button 组件并重新挂载事件
        Button tradeButton = midGo.GetComponentInChildren<Button>(true);
        if (tradeButton != null)
        {
            tradeButton.onClick = new Button.ButtonClickedEvent();
            tradeButton.onClick.AddListener(() => OnTradeButtonClicked(shopPanel));
        }
        
        CleanTooltipComponents(midGo);

        // 剥离本地化组件，防止文字被游戏本地化系统覆盖回原文
        TryStripLocalizationComponents(midGo);

        ApplyTradeButtonLabel(midGo, tradeButton, TradeButtonLabelText);

        // 设置为用户指定的固定坐标项（参考运行时截图）
        mid.anchorMin        = new Vector2(0.5f, 0.5f);
        mid.anchorMax        = new Vector2(0.5f, 0.5f);
        mid.pivot            = new Vector2(0.5f, 0.5f);
        mid.sizeDelta        = new Vector2(300f, 100f);
        
        // 直接设置本地坐标、缩放和旋转
        mid.localPosition    = new Vector3(1031.00f, -669.00f, 0.00f);
        mid.localScale       = Vector3.one;
        mid.localEulerAngles = Vector3.zero;

        Plugin.Logger?.LogInfo(
            $"[ShopTradeIcon] TradeButton (Container) 已设置为固定坐标: localPos={mid.localPosition}, size={mid.sizeDelta}");

        // 同步调整原生 CardService 容器到截图中的属性
        if (leftContainer != null)
        {
            ui.CardServiceOriginalLocalPosition = leftContainer.localPosition;
            ui.CardServiceOriginalSizeDelta     = leftContainer.sizeDelta;
            leftContainer.sizeDelta      = new Vector2(300f, 100f);
            leftContainer.localPosition  = new Vector3(870f, -720f, 0f);
            Plugin.Logger?.LogInfo($"[ShopTradeIcon] CardService 容器已调整: localPos=870,-720,0 sizeDelta=300,100");
        }

        // 同步调整原生 ReturnButton 容器到截图中的属性
        if (rightContainer != null)
        {
            ui.ReturnOriginalLocalPosition = rightContainer.localPosition;
            ui.ReturnOriginalSizeDelta     = rightContainer.sizeDelta;
            rightContainer.sizeDelta      = new Vector2(300f, 100f);
            rightContainer.localPosition  = new Vector3(1490f, -720f, 0f);
            Plugin.Logger?.LogInfo($"[ShopTradeIcon] ReturnButton 容器已调整: localPos=1490,-720,0 sizeDelta=300,100");
        }

        ui.Root    = midGo;
        ui.Button  = tradeButton;

        Plugin.Logger?.LogInfo("[ShopTradeIcon] 已插入带样式的交易按钮");
        }
        catch
        {
            if (midGo != null)
            {
                UnityEngine.Object.Destroy(midGo);
            }

            CleanupUi();
            throw;
        }
    }

    private static void LogHierarchy(Transform t, int depth)
    {
        string indent = new string('-', depth * 2);
        var comps = string.Join(", ", System.Linq.Enumerable.Select(t.GetComponents<Component>(), c => c?.GetType().Name ?? "null"));
        var tmp = t.GetComponent<TextMeshProUGUI>();
        string textVal = tmp != null ? $" [TEXT='{tmp.text}']" : "";
        Plugin.Logger?.LogInfo($"[ShopTradeIcon] {indent}{t.name} ({comps}){textVal}");
        for (int i = 0; i < t.childCount; i++)
            LogHierarchy(t.GetChild(i), depth + 1);
    }

    private static void ApplyTradeButtonLabel(GameObject root, Button tradeButton, string labelText)
    {
        if (TryResolveTradeLabel(root, tradeButton, out TMP_Text tmpLabel, out Text legacyLabel, out string resolution, out bool shouldDumpHierarchy))
        {
            if (tmpLabel != null)
            {
                tmpLabel.text = labelText;
            }
            else if (legacyLabel != null)
            {
                legacyLabel.text = labelText;
                Plugin.Logger?.LogWarning($"[ShopTradeIcon] 使用 legacy Text 回退设置交易按钮文案: {resolution}");
            }

            if (shouldDumpHierarchy && root != null)
            {
                LogHierarchy(root.transform, 0);
            }

            return;
        }

        Plugin.Logger?.LogWarning($"[ShopTradeIcon] 未能定位交易按钮标题节点，保留克隆后的原始文本。{resolution}");
        if (root != null)
        {
            LogHierarchy(root.transform, 0);
        }
    }

    private static bool TryResolveTradeLabel(
        GameObject root,
        Button tradeButton,
        out TMP_Text tmpLabel,
        out Text legacyLabel,
        out string resolution,
        out bool shouldDumpHierarchy)
    {
        tmpLabel = null;
        legacyLabel = null;
        resolution = string.Empty;
        shouldDumpHierarchy = false;

        Transform primaryRoot = tradeButton != null ? tradeButton.transform : root?.transform;
        if (primaryRoot == null)
        {
            resolution = "按钮根节点为空";
            return false;
        }

        TMP_Text[] primaryTmpLabels = primaryRoot.GetComponentsInChildren<TMP_Text>(true);
        tmpLabel = SelectTradeTmpLabel(primaryTmpLabels);
        if (tmpLabel != null)
        {
            shouldDumpHierarchy = primaryTmpLabels.Length > 1;
            resolution = primaryTmpLabels.Length > 1
                ? $"在按钮子树中命中 {primaryTmpLabels.Length} 个 TMP_Text，已按优先级选择 {tmpLabel.name}"
                : $"在按钮子树中命中 TMP_Text: {tmpLabel.name}";
            return true;
        }

        Text[] primaryLegacyLabels = primaryRoot.GetComponentsInChildren<Text>(true);
        legacyLabel = SelectTradeLegacyLabel(primaryLegacyLabels);
        if (legacyLabel != null)
        {
            shouldDumpHierarchy = true;
            resolution = primaryLegacyLabels.Length > 1
                ? $"在按钮子树中未命中 TMP_Text，但命中 {primaryLegacyLabels.Length} 个 legacy Text，已按优先级选择 {legacyLabel.name}"
                : $"在按钮子树中未命中 TMP_Text，回退到 legacy Text: {legacyLabel.name}";
            return true;
        }

        if (root != null && root.transform != primaryRoot)
        {
            TMP_Text[] rootTmpLabels = root.GetComponentsInChildren<TMP_Text>(true);
            tmpLabel = SelectTradeTmpLabel(rootTmpLabels);
            if (tmpLabel != null)
            {
                shouldDumpHierarchy = true;
                resolution = rootTmpLabels.Length > 1
                    ? $"按钮子树未命中标题，已从克隆根节点的 {rootTmpLabels.Length} 个 TMP_Text 中回退选择 {tmpLabel.name}"
                    : $"按钮子树未命中标题，已从克隆根节点回退到 TMP_Text: {tmpLabel.name}";
                return true;
            }

            Text[] rootLegacyLabels = root.GetComponentsInChildren<Text>(true);
            legacyLabel = SelectTradeLegacyLabel(rootLegacyLabels);
            if (legacyLabel != null)
            {
                shouldDumpHierarchy = true;
                resolution = rootLegacyLabels.Length > 1
                    ? $"按钮子树与根节点均未命中 TMP_Text，已从克隆根节点的 {rootLegacyLabels.Length} 个 legacy Text 中选择 {legacyLabel.name}"
                    : $"按钮子树与根节点均未命中 TMP_Text，已从克隆根节点回退到 legacy Text: {legacyLabel.name}";
                return true;
            }
        }

        resolution = "按钮子树与克隆根节点均未找到可用的标题文本组件";
        return false;
    }

    private static TMP_Text SelectTradeTmpLabel(TMP_Text[] candidates)
    {
        return candidates?
            .Where(label => label != null)
            .OrderByDescending(label => IsPreferredTradeLabelName(label.name))
            .ThenByDescending(label => label.isActiveAndEnabled)
            .ThenByDescending(label => !string.IsNullOrWhiteSpace(label.text))
            .ThenByDescending(label => GetTransformDepth(label.transform))
            .FirstOrDefault();
    }

    private static Text SelectTradeLegacyLabel(Text[] candidates)
    {
        return candidates?
            .Where(label => label != null)
            .OrderByDescending(label => IsPreferredTradeLabelName(label.name))
            .ThenByDescending(label => label.isActiveAndEnabled)
            .ThenByDescending(label => !string.IsNullOrWhiteSpace(label.text))
            .ThenByDescending(label => GetTransformDepth(label.transform))
            .FirstOrDefault();
    }

    private static bool IsPreferredTradeLabelName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        string lowerName = name.ToLowerInvariant();
        return lowerName.Contains("label") || lowerName.Contains("text") || lowerName.Contains("title");
    }

    private static int GetTransformDepth(Transform transform)
    {
        int depth = 0;
        while (transform != null)
        {
            depth++;
            transform = transform.parent;
        }

        return depth;
    }

    private static void CleanTooltipComponents(GameObject go)
    {
        try
        {
            foreach (var b in go.GetComponentsInChildren<Behaviour>(true))
            {
                if (b != null && b.GetType().Name.IndexOf("Tooltip", StringComparison.OrdinalIgnoreCase) >= 0)
                    b.enabled = false;
            }
        }
        catch { }
    }


    private static bool TryGetShopButtons(ShopPanel shopPanel, out Button cardServiceButton, out Button returnButton)
    {
        cardServiceButton = null;
        returnButton = null;

        if (shopPanel == null)
        {
            return false;
        }

        try
        {
            cardServiceButton = Traverse.Create(shopPanel).Field("cardServiceButton").GetValue<Button>();
        }
        catch
        {
            cardServiceButton = null;
        }

        try
        {
            returnButton = Traverse.Create(shopPanel).Field("returnButton").GetValue<Button>();
        }
        catch
        {
            returnButton = null;
        }

        return cardServiceButton != null && returnButton != null;
    }

    private static void BuildFloatingButton(ShopPanel shopPanel, Transform parent)
    {
        _defaultFont ??= FindDefaultFont(parent);

        GameObject root = new GameObject("NetworkPlugin_ShopTradeButton");
        root.transform.SetParent(parent, false);

        var rect = root.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.sizeDelta = new Vector2(200f, 64f);
        rect.anchoredPosition = new Vector2(-20f, -20f);

        var bg = root.AddComponent<Image>();
        bg.sprite = GetWhiteSprite();
        bg.color = new Color(0f, 0f, 0f, 0.55f);

        var button = root.AddComponent<Button>();
        button.targetGraphic = bg;
        button.onClick.AddListener(() => OnTradeButtonClicked(shopPanel));

        GameObject iconGo = new GameObject("Icon");
        iconGo.transform.SetParent(root.transform, false);
        var iconRect = iconGo.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.sizeDelta = new Vector2(44f, 44f);
        iconRect.anchoredPosition = new Vector2(10f, 0f);

        var iconImg = iconGo.AddComponent<Image>();
        iconImg.sprite = TryLoadTradeSprite() ?? GetWhiteSprite();
        iconImg.preserveAspect = true;
        iconImg.color = Color.white;

        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(root.transform, false);
        var labelRect = labelGo.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(60f, 0f);
        labelRect.offsetMax = new Vector2(-10f, 0f);

        var label = labelGo.AddComponent<TextMeshProUGUI>();
        label.text = TradeButtonLabelText;
        label.fontSize = 26f;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.color = Color.white;
        label.raycastTarget = false;
        if (_defaultFont != null)
        {
            label.font = _defaultFont;
        }

        _ui = new TradeButtonUi
        {
            ShopPanel = shopPanel,
            Root = root,
            Button = button
        };
    }

    private static void CleanupUi()
    {
        try
        {
            if (_ui != null)
            {
                try
                {
                    // 恢复原生 CardService 容器属性
                    if (_ui.CardServiceButton != null)
                    {
                        RectTransform ct = _ui.CardServiceButton.transform.parent as RectTransform;
                        if (ct != null)
                        {
                            ct.anchoredPosition = _ui.CardServiceOriginalAnchoredPosition;
                            ct.sizeDelta     = _ui.CardServiceOriginalSizeDelta;
                            ct.localScale    = _ui.CardServiceOriginalScale;
                            ct.localPosition = _ui.CardServiceOriginalLocalPosition;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger?.LogDebug($"[ShopTradeIcon] 恢复 CardService 属性时忽略错误: {ex.Message}");
                }

                try
                {
                    // 恢复原生 ReturnButton 容器属性
                    if (_ui.ReturnButton != null)
                    {
                        RectTransform rt = _ui.ReturnButton.transform.parent as RectTransform;
                        if (rt != null)
                        {
                            rt.anchoredPosition = _ui.ReturnOriginalAnchoredPosition;
                            rt.sizeDelta     = _ui.ReturnOriginalSizeDelta;
                            rt.localScale    = _ui.ReturnOriginalScale;
                            rt.localPosition = _ui.ReturnOriginalLocalPosition;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger?.LogDebug($"[ShopTradeIcon] 恢复 ReturnButton 属性时忽略错误: {ex.Message}");
                }

                try
                {
                    // 销毁注入的交易按钮
                    if (_ui.Root != null)
                    {
                        UnityEngine.Object.Destroy(_ui.Root);
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger?.LogDebug($"[ShopTradeIcon] Cleanup 过程中忽略小错误: {ex.Message}");
                }
            }
        }
        catch
        {
            // 忽略清理阶段的异常，避免影响商店关闭流程。
        }
        finally
        {
            _ui = null;
        }
    }

    private static void ApplyThreeButtonLayout_NoReparent(
        RectTransform left,
        RectTransform mid,
        RectTransform right,
        Vector2 leftOriginal,
        Vector2 rightOriginal,
        float marginX,
        float paddingX,
        float spacingX,
        float scale)
    {
        if (left == null || mid == null || right == null)
        {
            return;
        }

        try
        {
            // 用户要求的布局规则：
            // - 可用边界由左侧按钮的左边缘和右侧按钮的右边缘共同定义。
            // - 将三个按钮按 left -> mid -> right 排成一行，并保留间距。
            // - 允许缩放低于 0.8（不设最小值），但绝不放大。

            float gap = Mathf.Max(0f, spacingX);

            // 优先使用 rect.width 作为实际渲染宽度；若 rect 尚未就绪，再回退到 sizeDelta。
            float leftW0 = Mathf.Abs(left.rect.width);
            float midW0 = Mathf.Abs(mid.rect.width);
            float rightW0 = Mathf.Abs(right.rect.width);
            if (leftW0 <= 1e-3f) leftW0 = Mathf.Abs(left.sizeDelta.x);
            if (midW0 <= 1e-3f) midW0 = Mathf.Abs(mid.sizeDelta.x);
            if (rightW0 <= 1e-3f) rightW0 = Mathf.Abs(right.sizeDelta.x);

            float leftH = left.sizeDelta.y;
            float midH = mid.sizeDelta.y;
            float rightH = right.sizeDelta.y;

            // 先把 struct 参数复制到局部变量。
            // 某些分析器会误报与 Harmony 补丁参数同名的成员访问。
            Vector2 leftOrig = leftOriginal;
            Vector2 rightOrig = rightOriginal;

            // 把基于中心点的锚点换算为基于边缘的边界。
            float L = leftOrig.x - leftW0 * 0.5f;
            float R = rightOrig.x + rightW0 * 0.5f;

            float y = (leftOrig.y + rightOrig.y) * 0.5f;

            float have = (R - L) - (marginX * 2f) - (paddingX * 2f);
            if (have <= 1e-3f)
            {
                return;
            }

            float need = leftW0 + midW0 + rightW0 + gap * 2f;
            if (need <= 1e-3f)
            {
                return;
            }

            float s = have / need;
            if (float.IsNaN(s) || float.IsInfinity(s)) s = 1f;
            s = Mathf.Min(1f, s);
            if (s < 1e-5f) s = 1e-5f;

            float leftW = leftW0 * s;
            float midW = midW0 * s;
            float rightW = rightW0 * s;

            // 通过 sizeDelta.x 应用宽度，同时保留原有符号方向。
            left.sizeDelta = new Vector2(Mathf.Sign(left.sizeDelta.x == 0 ? 1f : left.sizeDelta.x) * leftW, leftH);
            mid.sizeDelta = new Vector2(Mathf.Sign(mid.sizeDelta.x == 0 ? 1f : mid.sizeDelta.x) * midW, midH);
            right.sizeDelta = new Vector2(Mathf.Sign(right.sizeDelta.x == 0 ? 1f : right.sizeDelta.x) * rightW, rightH);

            float startX = L + marginX + paddingX;
            float leftCenterX = startX + leftW * 0.5f;
            float midCenterX = leftCenterX + leftW * 0.5f + gap + midW * 0.5f;
            float rightCenterX = midCenterX + midW * 0.5f + gap + rightW * 0.5f;

            left.anchoredPosition = new Vector2(leftCenterX, y);
            mid.anchoredPosition = new Vector2(midCenterX, y);
            right.anchoredPosition = new Vector2(rightCenterX, y);

            // ===== 详细日志输出：按钮位置、大小、间隙等属性 =====
            Plugin.Logger?.LogInfo(
                $"[ShopTradeIcon] ========== 三按钮布局计算结果 ==========\n" +
                $"【按钮原始宽度】\n" +
                $"  左侧按钮(卡牌服务): {leftW0:F2}\n" +
                $"  中间按钮(交易):     {midW0:F2}\n" +
                $"  右侧按钮(返回):     {rightW0:F2}\n" +
                $"【按钮高度】\n" +
                $"  左侧: {leftH:F2}, 中间: {midH:F2}, 右侧: {rightH:F2}\n" +
                $"【边界信息】\n" +
                $"  左边界(L): {L:F2}, 右边界(R): {R:F2}\n" +
                $"【空间计算】\n" +
                $"  可用宽度(have): {have:F2}\n" +
                $"  需要宽度(need): {need:F2}\n" +
                $"  缩放因子(s):    {s:F4}\n" +
                $"【缩放后的宽度】\n" +
                $"  左侧: {leftW:F2}, 中间: {midW:F2}, 右侧: {rightW:F2}\n" +
                $"【间隙和位置】\n" +
                $"  间隙宽度(gap): {gap:F2}\n" +
                $"  Y坐标(y):      {y:F2}\n" +
                $"【最终中心坐标】\n" +
                $"  左侧(卡牌服务): ({leftCenterX:F2}, {y:F2})\n" +
                $"  中间(交易):     ({midCenterX:F2}, {y:F2})\n" +
                $"  右侧(返回):     ({rightCenterX:F2}, {y:F2})\n" +
                $"【相邻按钮间距】\n" +
                $"  左→中: {midCenterX - leftCenterX - leftW * 0.5f - midW * 0.5f:F2}\n" +
                $"  中→右: {rightCenterX - midCenterX - midW * 0.5f - rightW * 0.5f:F2}\n" +
                $"========================================"
            );
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopTradeIcon] 手动三按钮布局失败: {ex.Message}");
        }
    }

    private static void SetUiVisible(bool visible)
    {
        if (_ui?.Root == null)
        {
            return;
        }

        _ui.Root.SetActive(visible);
    }

    private static void TryStripLocalizationComponents(GameObject root)
    {
        if (root == null) return;

        foreach (var b in root.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (b == null) continue;
            string n = b.GetType().Name;
            if (n.IndexOf("localiz", StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("locale", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                UnityEngine.Object.Destroy(b);
            }
        }
    }



    private static TMP_FontAsset FindDefaultFont(Transform root)
    {
        var tmp = root.GetComponentInChildren<TextMeshProUGUI>(true);
        return tmp?.font;
    }

    private static Sprite GetWhiteSprite()
    {
        if (_whiteSprite != null)
        {
            return _whiteSprite;
        }

        _whiteTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _whiteTexture.SetPixel(0, 0, Color.white);
        _whiteTexture.Apply(false, true);

        _whiteSprite = Sprite.Create(_whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return _whiteSprite;
    }

    private static Sprite TryLoadTradeSprite()
        => Resources.Load<Sprite>("UI/Icons/TradeIcon") ?? Resources.Load<Sprite>("UI/Icons/DefaultIcon");

    private static void OnTradeButtonClicked(ShopPanel shopPanel)
    {
        try
        {
            if (!TradeUiMessages.IsTradeEnabledAndConnected(out string reason))
            {
                TradeUiMessages.ShowTopMessage(reason ?? "交易不可用。");
                return;
            }

            Transform parent = null;
            if (shopPanel != null && shopPanel.transform != null)
            {
                parent = shopPanel.transform.parent != null ? shopPanel.transform.parent : shopPanel.transform;
            }

            TradePanel tradePanel = TradePanelRuntimeFactory.GetOrCreate(parent);

            if (tradePanel != null)
            {
                try
                {
                    tradePanel.Show(new TradePayload());
                    return;
                }
                catch (Exception ex)
                {
                    Plugin.Logger?.LogWarning($"[ShopTradeIcon] TradePanel.Show failed: {ex.Message}");
                }
            }

            TradeUiMessages.ShowTradePanelMissing();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopTradeIcon] Click failed: {ex.Message}");
        }
    }
}

