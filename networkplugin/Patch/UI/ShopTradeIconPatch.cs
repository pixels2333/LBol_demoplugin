using System;
using System.Linq;
using HarmonyLib;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
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
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

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

        public Vector2 ReturnOriginalAnchoredPosition;
        public Vector2 ReturnOriginalSizeDelta;
        public Vector3 ReturnOriginalScale;
    }

    private static TradeButtonUi _ui;
    private static TMP_FontAsset _defaultFont;
    private static Sprite _whiteSprite;
    private static Texture2D _whiteTexture;

    private static ShopPanel _cachedShopPanel;

    // Throttle spammy logs since this patch runs every frame via GameDirector.Update.
    private static TradeUiUpdateState _lastState;
    private static bool _hasLastState;
    private static float _nextStateLogTime;
    private static string _lastStateLogKey;

    private static float _nextShopPanelFindTime;

    [HarmonyPatch(typeof(ShopPanel), "OnShown")]
    [HarmonyPostfix]
    private static void ShopPanel_OnShown_Postfix(ShopPanel __instance)
    {
        try
        {
            if (__instance == null)
            {
                return;
            }

            _cachedShopPanel = __instance;
            _hasLastState = false; // reset state throttle per-open
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
                // Shop is closing; destroy the injected UI to avoid leaking objects.
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
            return ServiceProvider?.GetService<ConfigManager>();
        }
        catch
        {
            return null;
        }
    }

    private static bool IsTradeEnabledAndConnected()
    {
        return TradeUiMessages.IsTradeEnabledAndConnected(out _);
    }

    private static bool TryGetShopPanel(out ShopPanel shopPanel)
    {
        shopPanel = null;

        // Prefer reusing a cached instance if it's still alive.
        try
        {
            if (_cachedShopPanel != null && _cachedShopPanel.gameObject != null)
            {
                shopPanel = _cachedShopPanel;
                return true;
            }
        }
        catch
        {
            _cachedShopPanel = null;
        }

        // Fast path: use UiManager when available.
        try
        {
            shopPanel = UiManager.GetPanel<ShopPanel>();
        }
        catch
        {
            shopPanel = null;
        }

        // Fallback: Unity lookup (throttled) in case UiManager returns null.
        if (shopPanel == null)
        {
            float now = Time.unscaledTime;
            if (now >= _nextShopPanelFindTime)
            {
                _nextShopPanelFindTime = now + 1.0f;
                try
                {
                    shopPanel = UnityEngine.Object.FindObjectOfType<ShopPanel>(true);
                }
                catch
                {
                    shopPanel = null;
                }

                if (shopPanel == null)
                {
                    try
                    {
                        var all = Resources.FindObjectsOfTypeAll<ShopPanel>();
                        if (all != null && all.Length > 0)
                        {
                            shopPanel = all[0];
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        if (shopPanel != null)
        {
            _cachedShopPanel = shopPanel;
            return true;
        }

        return false;
    }

    private static void LogStateThrottled(TradeUiUpdateState state, string message, float intervalSeconds = 2.0f)
    {
        try
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
        catch
        {
            // ignored
        }
    }

    private static bool ShouldShow(ShopPanel shopPanel)
    {
        if (shopPanel == null || !shopPanel.IsVisible)
        {
            return false;
        }

        return IsTradeEnabledAndConnected();
    }

    [HarmonyPatch(typeof(GameDirector), "Update")]
    [HarmonyPostfix]
    private static void GameDirector_Update_Postfix()
    {
        try
        {
            // Lightweight watchdog: only when shop is currently visible.
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

        // Avoid doing heavy work every frame.
        if (_ui == null || _ui.Root == null || _ui.ShopPanel != shopPanel)
        {
            EnsureUi(shopPanel);
        }
        
        if (_ui != null && _ui.Root != null)
        {
            _ui.Root.SetActive(true);
        }

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

        Transform barParent = cardServiceButton.transform.parent; // 获取按钮的父节点，用于插入新按钮
        if (barParent == null)
        {
            Plugin.Logger?.LogWarning("[ShopTradeIcon] cardServiceButton 没有父节点，无法插入容器。");
            return;
        }

        _defaultFont ??= FindDefaultFont(barParent); // 查找默认字体，如果未找到则赋值

        // 不创建新布局容器：保持原版层级，只通过 RectTransform 调整三按钮位置/大小。
        // 2. 准备按钮资产
        var left = cardServiceButton.GetComponent<RectTransform>(); // 获取卡牌服务按钮的RectTransform
        var right = returnButton.GetComponent<RectTransform>(); // 获取返回按钮的RectTransform
        
        // 记录原始信息用于恢复
        var ui = new TradeButtonUi // 创建新的TradeButtonUi实例
        {
            ShopPanel = shopPanel, // 关联商店面板
            CardServiceButton = cardServiceButton, // 卡牌服务按钮
            ReturnButton = returnButton, // 返回按钮
            CardServiceOriginalAnchoredPosition = left.anchoredPosition, // 记录卡牌服务按钮原始位置
            CardServiceOriginalSizeDelta = left.sizeDelta, // 记录卡牌服务按钮原始尺寸
            CardServiceOriginalScale = left.localScale, // 记录卡牌服务按钮原始缩放
            ReturnOriginalAnchoredPosition = right.anchoredPosition, // 记录返回按钮原始位置
            ReturnOriginalSizeDelta = right.sizeDelta, // 记录返回按钮原始尺寸
            ReturnOriginalScale = right.localScale // 记录返回按钮原始缩放
        };

        // 克隆生成“交易”按钮（与原版按钮同父节点，保证渲染层级/遮罩行为一致）
        GameObject midGo = UnityEngine.Object.Instantiate(cardServiceButton.gameObject, barParent, false); // 克隆卡牌服务按钮作为交易按钮
        midGo.name = "NetworkPlugin_TradeButton"; // 设置新按钮名称
        RectTransform mid = midGo.GetComponent<RectTransform>(); // 获取交易按钮的RectTransform
        Button tradeButton = midGo.GetComponent<Button>(); // 获取交易按钮组件
        tradeButton.onClick = new Button.ButtonClickedEvent(); // 重置点击事件
        tradeButton.onClick.AddListener(() => OnTradeButtonClicked(shopPanel)); // 添加点击监听器
        CleanTooltipComponents(midGo); // 清理工具提示组件

        // 3. 仅通过属性调整位置/大小（不使用容器布局）
        // 约束：按钮尺寸只允许在原始基础上缩小最多 20%；不做其他兜底策略。
        ApplyThreeButtonLayout_NoReparent( // 应用三按钮布局，无需重新父化
            left, // 左侧按钮（卡牌服务）
            mid, // 中间按钮（交易）
            right, // 右侧按钮（返回）
            ui.CardServiceOriginalAnchoredPosition, // 左侧原始位置
            ui.ReturnOriginalAnchoredPosition, // 右侧原始位置
            0f, // 边距X
            0f, // 内边距X
            6f, // 间距X
            1f); // 缩放因子

        // 4. 原生按钮内部样式保持不动；仅对交易按钮做“无图标 + 文字全居中”处理。
        FixInternalButtonLayout(mid, 0f); // 修复交易按钮内部布局，字体大小参数为0（不强制修改）

        // 5. 记录 UI 句柄（Root 指向交易按钮本体，关闭商店时销毁）
        ui.Root = midGo; // 设置UI根对象为交易按钮

        _ui = ui; // 赋值全局UI实例
        _ui.Button = tradeButton; // 设置按钮组件
        
        Plugin.Logger?.LogInfo($"[ShopTradeIcon] 已插入交易按钮：在原版按钮条中手动调整三按钮布局");
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

    private static void ApplyCompactButtonStyle_NoThrow(Button button)
    {
        try
        {
            if (button == null)
            {
                return;
            }

            var rt = button.GetComponent<RectTransform>();
            if (rt == null)
            {
                return;
            }

            // Uniform scale down; keeps anchors/layout intact while making room.
            rt.localScale = new Vector3(0.85f, 0.85f, 1f);

            // Slight horizontal nudge so the group feels centered after scaling.
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y);
        }
        catch
        {
            // ignored
        }
    }

    private static void BuildFloatingButton(ShopPanel shopPanel, Transform parent)
    {
        _defaultFont ??= FindDefaultFont(parent);

        var root = new GameObject("NetworkPlugin_ShopTradeButton");
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

        var iconGo = new GameObject("Icon");
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

        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(root.transform, false);
        var labelRect = labelGo.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(60f, 0f);
        labelRect.offsetMax = new Vector2(-10f, 0f);

        var label = labelGo.AddComponent<TextMeshProUGUI>();
        label.text = "交易";
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
                    // 1. 恢复两个原生按钮的属性（不改父子关系）
                    if (_ui.CardServiceButton != null)
                    {
                        var rt = _ui.CardServiceButton.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            rt.anchoredPosition = _ui.CardServiceOriginalAnchoredPosition;
                            rt.sizeDelta = _ui.CardServiceOriginalSizeDelta;
                            rt.localScale = _ui.CardServiceOriginalScale;
                        }
                    }

                    if (_ui.ReturnButton != null)
                    {
                        var rt = _ui.ReturnButton.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            rt.anchoredPosition = _ui.ReturnOriginalAnchoredPosition;
                            rt.sizeDelta = _ui.ReturnOriginalSizeDelta;
                            rt.localScale = _ui.ReturnOriginalScale;
                        }
                    }

                    // 2. 销毁注入的交易按钮
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
            // ignored
        }
        finally
        {
            _ui = null;
        }
    }

    private static void FixInternalButtonLayout(RectTransform rect, float fontSize)
    {
        if (rect == null) return;

        try
        {
            rect.localScale = Vector3.one;

            // 1. 文字全居中排版 (暂时没有图标)
            var label = rect.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                if (rect.name.Contains("Trade")) label.text = "交易";

                var lRect = label.rectTransform;
                lRect.anchorMin = Vector2.zero;
                lRect.anchorMax = Vector2.one;
                lRect.pivot = new Vector2(0.5f, 0.5f);
                lRect.offsetMin = lRect.offsetMax = Vector2.zero;

                // 不强行改字体大小：保持原版按钮的字体规格，只改对齐。
                label.alignment = TextAlignmentOptions.Center;
                label.enableAutoSizing = false;
            }

            // 2. 隐藏图标 (按用户要求)
            var iconTr = rect.GetComponentsInChildren<Image>(true)
                .FirstOrDefault(img => img.name.ToLower().Contains("icon"))?.rectTransform;
            if (iconTr != null)
            {
                iconTr.gameObject.SetActive(false);
            }
        }
        catch { }
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
            // Layout rule requested by user:
            // - The available boundary is defined by the LEFT button's left edge and the RIGHT button's right edge.
            // - Pack three buttons left->mid->right in one row with spacing.
            // - Allow scaling below 0.8 (no minimum), never upscale.

            float gap = Mathf.Max(0f, spacingX);

            // Prefer rect.width for rendered width; fall back to sizeDelta when rect is not ready.
            float leftW0 = Mathf.Abs(left.rect.width);
            float midW0 = Mathf.Abs(mid.rect.width);
            float rightW0 = Mathf.Abs(right.rect.width);
            if (leftW0 <= 1e-3f) leftW0 = Mathf.Abs(left.sizeDelta.x);
            if (midW0 <= 1e-3f) midW0 = Mathf.Abs(mid.sizeDelta.x);
            if (rightW0 <= 1e-3f) rightW0 = Mathf.Abs(right.sizeDelta.x);

            float leftH = left.sizeDelta.y;
            float midH = mid.sizeDelta.y;
            float rightH = right.sizeDelta.y;

            // Copy struct parameters to locals first.
            // Some analyzers mis-report member access on parameters named like Harmony patch args.
            Vector2 leftOrig = leftOriginal;
            Vector2 rightOrig = rightOriginal;

            // Convert center-based anchors to edge-based boundaries.
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

            // Apply widths via sizeDelta.x while preserving sign.
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

    private static void ForceButtonLabelCenter(Button button, string expectedText)
    {
        if (button == null)
        {
            return;
        }

        try
        {
            var tmps = button.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (tmps == null || tmps.Length == 0)
            {
                return;
            }

            foreach (var t in tmps)
            {
                if (t == null)
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(expectedText) && t.text != expectedText)
                {
                    continue;
                }

                t.alignment = TextAlignmentOptions.Center;
                t.enableWordWrapping = false;
                t.overflowMode = TextOverflowModes.Overflow;
                break;
            }
        }
        catch
        {
            // ignored
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

    private static Transform TryGetShopPanelRoot(ShopPanel shopPanel)
    {
        try
        {
            var root = Traverse.Create(shopPanel).Field("root").GetValue<RectTransform>();
            return root != null ? root.transform : null;
        }
        catch
        {
            return null;
        }
    }

    private static TMP_FontAsset FindDefaultFont(Transform root)
    {
        try
        {
            var tmp = root.GetComponentInChildren<TextMeshProUGUI>(true);
            return tmp != null ? tmp.font : null;
        }
        catch
        {
            return null;
        }
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
    {
        try
        {
            return Resources.Load<Sprite>("UI/Icons/TradeIcon") ?? Resources.Load<Sprite>("UI/Icons/DefaultIcon");
        }
        catch
        {
            return null;
        }
    }

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

            TradePanel tradePanel = NetworkPlugin.UI.Panels.TradePanelRuntimeFactory.GetOrCreate(parent);

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

