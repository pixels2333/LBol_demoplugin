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

        var ui = new TradeButtonUi
        {
            ShopPanel         = shopPanel,
            CardServiceButton = cardServiceButton,
            ReturnButton      = returnButton,
        };

        // 克隆整个 CardService 容器，以获得背景、边框等完整按钮样式
        GameObject midGo = UnityEngine.Object.Instantiate(leftContainer.gameObject, barParent, false);
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

        // 在容器层级上直接遍历，避免文字节点与 Button 同级时搜不到的问题
        // 只改文字内容，不覆盖字体——克隆已继承 CardService 的原始样式，无需修改
        //TODO: 检查这里是否冗余，如果克隆的层级结构和原来完全一样，理论上不应该有额外的 Text 组件了。
        foreach (var label in midGo.GetComponentsInChildren<TMP_Text>(true))
        {
            if (label == null) continue;
            label.text = "玩家交易";
        }
        foreach (var t in midGo.GetComponentsInChildren<Text>(true))
        {
            if (t == null) continue;
            t.text = "玩家交易";
        }

        // 打印克隆后的完整层级结构，帮助诊断 TextMeshProUGUI 位置
        LogHierarchy(midGo.transform, 0);

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
        _ui        = ui;
        _ui.Button = tradeButton;

        Plugin.Logger?.LogInfo("[ShopTradeIcon] 已插入带样式的交易按钮");
    }

    private static void LogHierarchy(Transform t, int depth)
    {
        try
        {
            string indent = new string('-', depth * 2);
            var comps = string.Join(", ", System.Linq.Enumerable.Select(t.GetComponents<Component>(), c => c?.GetType().Name ?? "null"));
            string textVal = "";
            var tmp = t.GetComponent<TMPro.TextMeshProUGUI>();
            if (tmp != null) textVal = $" [TEXT='{tmp.text}']";
            Plugin.Logger?.LogInfo($"[ShopTradeIcon] {indent}{t.name} ({comps}){textVal}");
            for (int i = 0; i < t.childCount; i++)
                LogHierarchy(t.GetChild(i), depth + 1);
        }
        catch { }
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
                    // 恢复原生 CardService 容器属性
                    if (_ui.CardServiceButton != null)
                    {
                        var ct = _ui.CardServiceButton.transform.parent as RectTransform;
                        if (ct != null)
                        {
                            ct.sizeDelta     = _ui.CardServiceOriginalSizeDelta;
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
                        var rt = _ui.ReturnButton.transform.parent as RectTransform;
                        if (rt != null)
                        {
                            rt.sizeDelta     = _ui.ReturnOriginalSizeDelta;
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
            // ignored
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

    private static void TryStripLocalizationComponents(GameObject root)
    {
        try
        {
            if (root == null) return;

            var behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var b in behaviours)
            {
                if (b == null) continue;

                string n = b.GetType().Name;
                if (string.IsNullOrWhiteSpace(n)) continue;

                if (n.IndexOf("localiz", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    n.IndexOf("locale", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    UnityEngine.Object.Destroy(b);
                }
            }
        }
        catch
        {
            // ignored
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

