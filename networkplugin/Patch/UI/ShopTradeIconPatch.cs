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
        public TextMeshProUGUI Label;
        public Button CardServiceButton;
        public Button ReturnButton;

        public Vector2 CardServiceOriginalAnchoredPosition;
        public Vector2 CardServiceOriginalSizeDelta;
        public Vector3 CardServiceOriginalScale;

        public Vector2 TradeOriginalAnchoredPosition;
        public Vector2 TradeOriginalSizeDelta;
        public Vector3 TradeOriginalScale;

        public Vector2 ReturnOriginalAnchoredPosition;
        public Vector2 ReturnOriginalSizeDelta;
        public Vector3 ReturnOriginalScale;

        // Cache original anchor positions so our adjustments don't accumulate.
        public bool HasOriginalPositions;
        public Vector2 CardServiceOriginalButtonBarAnchoredPosition;
        public Vector2 ReturnOriginalButtonBarAnchoredPosition;
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
        if (_ui != null && _ui.Root != null && _ui.ShopPanel == shopPanel)
        {
            return;
        }

        CleanupUi();

        bool gotButtons = TryGetShopButtons(shopPanel, out Button cardServiceButton, out Button returnButton);
        if (!gotButtons)
        {
            Plugin.Logger?.LogWarning("[ShopTradeIcon] 无法找到商店按钮，无法在中间插入。");
            return;
        }

        // 需求：只复制“卡牌服务”按钮本体，不要把其父容器里可能存在的“移除/升级”等子控件一起克隆出来。
        // 因此这里直接克隆 cardServiceButton 的 GameObject，并插入到与其相同的父容器下。
        var barParent = cardServiceButton.transform.parent;
        if (barParent == null)
        {
            Plugin.Logger?.LogWarning("[ShopTradeIcon] cardServiceButton 没有父节点，无法插入交易按钮。");
            return;
        }

        _defaultFont ??= FindDefaultFont(barParent);

        var root = UnityEngine.Object.Instantiate(cardServiceButton.gameObject, barParent, false);
        root.name = "NetworkPlugin_ShopTradeButton";
        root.SetActive(true);

        var rect = root.GetComponent<RectTransform>();

        // 克隆自按钮容器时，可能会把 Tooltip 相关组件也一并带过来。
        // 交易按钮不需要沿用原按钮的 Tooltip；并且某些 TooltipSource.Title 依赖外部数据，克隆后可能为 null，导致 NRE。
        // 这里直接禁用这些组件，避免悬浮时报错。
        try
        {
            foreach (var behaviour in root.GetComponentsInChildren<Behaviour>(true))
            {
                if (behaviour == null)
                {
                    continue;
                }

                var typeName = behaviour.GetType().Name;
                if (typeName.IndexOf("Tooltip", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    behaviour.enabled = false;
                }
            }
        }
        catch
        {
            // ignored
        }

        // 2. 获取按钮并绑定点击事件。
        var button = root.GetComponent<Button>();
        if (button == null)
        {
            Plugin.Logger?.LogWarning("[ShopTradeIcon] 未找到交易按钮的主 Button，放弃插入。 ");
            UnityEngine.Object.Destroy(root);
            return;
        }

        button.interactable = true;
        button.enabled = true;
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(() =>
        {
            Plugin.Logger?.LogInfo("[ShopTradeIcon] 点击交易按钮");
            OnTradeButtonClicked(shopPanel);
        });

        // 3. 设置文本为“交易”，并继承“卡牌服务”按钮的文本样式以保持一致。
        var sourceLabel = cardServiceButton.GetComponentInChildren<TextMeshProUGUI>(true);
        string sourceLabelText = sourceLabel != null ? sourceLabel.text : null;

        TextMeshProUGUI label = null;
        var allLabels = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        if (allLabels != null && allLabels.Length > 0)
        {
            // 找到主文本组件。
            if (!string.IsNullOrEmpty(sourceLabelText))
            {
                label = allLabels.FirstOrDefault(t => t != null && (t.text == sourceLabelText || t.name.Contains("Label")));
            }
            label ??= allLabels.FirstOrDefault(t => t != null);

            // 无论原有内容是什么，强制修改为“交易”，避免显示重复的“卡牌服务”。
            foreach (var t in allLabels)
            {
                if (t != null)
                {
                    t.text = "交易";
                    t.alignment = TextAlignmentOptions.Center;
                }
            }
        }

        if (label != null && sourceLabel != null)
        {
            // 复制风格关键字段，确保与另外两个按钮一致
            label.font = sourceLabel.font;
            label.fontSize = sourceLabel.fontSize;
            label.fontStyle = sourceLabel.fontStyle;
            label.alignment = TextAlignmentOptions.Center;
            label.characterSpacing = sourceLabel.characterSpacing;
            label.wordSpacing = sourceLabel.wordSpacing;
            label.lineSpacing = sourceLabel.lineSpacing;
            label.enableAutoSizing = sourceLabel.enableAutoSizing;
            label.color = sourceLabel.color;
        }

        if (label != null && _defaultFont != null)
        {
            label.font = _defaultFont;
        }

        // Per user: icon on top + text on bottom, whole button centered.
        // Avoid inheriting a left alignment from certain localized TMP templates.
        try
        {
            if (label != null)
            {
                label.alignment = TextAlignmentOptions.Center;
                label.enableWordWrapping = false;
                label.overflowMode = TextOverflowModes.Overflow;
            }
        }
        catch
        {
            // ignored
        }

        // 4. 放置位置：水平排列在右下角一栏
        var leftRect = cardServiceButton.GetComponent<RectTransform>();
        var midRect = root.GetComponent<RectTransform>();
        var rightRect = returnButton.GetComponent<RectTransform>();

        // Capture original positions once per shop panel so offsets don't stack.
        Vector2 leftOriginal = leftRect != null ? leftRect.anchoredPosition : Vector2.zero;
        Vector2 rightOriginal = rightRect != null ? rightRect.anchoredPosition : Vector2.zero;
        if (_ui != null && _ui.ShopPanel == shopPanel && _ui.HasOriginalPositions)
        {
            leftOriginal = _ui.CardServiceOriginalButtonBarAnchoredPosition;
            rightOriginal = _ui.ReturnOriginalButtonBarAnchoredPosition;
        }

        if (leftRect != null && midRect != null && rightRect != null)
        {
            // User requirement: strictly within the original bounding box.
            // Spacing reduced to 5f, margin set to 0f.
            ApplyThreeButtonLayout_NoReparent(leftRect, midRect, rightRect, leftOriginal, rightOriginal,
                marginX: 0f, 
                paddingX: 2f, 
                spacingX: 5f, 
                scale: 1f);
            Plugin.Logger?.LogInfo("[ShopTradeIcon] 布局更新：严格边界三等分布局 [CardService] [Trade] [Return]");
        }

        // 5. 确保克隆的图标（如果有）也被正确处理
        // 原始按钮通常有一个大的 Image 组件作为装饰图标
        try
        {
            // 查找并调整克隆按钮中的图标位置，确保它相对于按钮中心正确
            foreach (Transform child in root.transform)
            {
                if (child.name.IndexOf("icon", StringComparison.OrdinalIgnoreCase) >= 0 || 
                    child.GetComponent<Image>() != null)
                {
                    child.gameObject.SetActive(true);
                }
            }
        }
        catch { }

        _ui = new TradeButtonUi
        {
            ShopPanel = shopPanel,
            Root = root,
            Button = button,
            Label = label,
            CardServiceButton = cardServiceButton,
            ReturnButton = returnButton,

            HasOriginalPositions = true,
            CardServiceOriginalButtonBarAnchoredPosition = leftOriginal,
            ReturnOriginalButtonBarAnchoredPosition = rightOriginal,

            CardServiceOriginalAnchoredPosition = leftRect.anchoredPosition,
            CardServiceOriginalSizeDelta = leftRect.sizeDelta,
            CardServiceOriginalScale = cardServiceButton.transform.localScale,

            TradeOriginalAnchoredPosition = midRect.anchoredPosition,
            TradeOriginalSizeDelta = midRect.sizeDelta,
            TradeOriginalScale = root.transform.localScale,

            ReturnOriginalAnchoredPosition = rightRect.anchoredPosition,
            ReturnOriginalSizeDelta = rightRect.sizeDelta,
            ReturnOriginalScale = returnButton.transform.localScale
        };

        Plugin.Logger?.LogInfo($"[ShopTradeIcon] 按钮已成功插入并水平排列。");
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
            Button = button,
            Label = label
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
                    if (_ui.CardServiceButton != null)
                    {
                        _ui.CardServiceButton.transform.localScale = _ui.CardServiceOriginalScale;
                        var rt = _ui.CardServiceButton.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            rt.anchoredPosition = _ui.CardServiceOriginalAnchoredPosition;
                            rt.sizeDelta = _ui.CardServiceOriginalSizeDelta;
                        }
                    }

                    if (_ui.ReturnButton != null)
                    {
                        _ui.ReturnButton.transform.localScale = _ui.ReturnOriginalScale;
                        var rt = _ui.ReturnButton.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            rt.anchoredPosition = _ui.ReturnOriginalAnchoredPosition;
                            rt.sizeDelta = _ui.ReturnOriginalSizeDelta;
                        }
                    }

                    if (_ui.Root != null)
                    {
                        _ui.Root.transform.localScale = _ui.TradeOriginalScale;
                        var rt = _ui.Root.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            rt.anchoredPosition = _ui.TradeOriginalAnchoredPosition;
                            rt.sizeDelta = _ui.TradeOriginalSizeDelta;
                        }
                    }
                }
                catch
                {
                    // ignored
                }

                try
                {
                    if (_ui.Root != null)
                    {
                        UnityEngine.Object.Destroy(_ui.Root);
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
            // 按钮通常由一个背景/图标 Image 和一个 TextMeshPro 组成
            var image = rect.GetComponent<Image>();
            var label = rect.GetComponentInChildren<TextMeshProUGUI>(true);

            // 1. 处理图标：如果没有独立的图标物体，Image 本身就是图标
            // 在 LBoL 的商店按钮中，通常有一个名为 "Icon" 的子物体，或者 Image 本身带 Sprite
            var iconTransform = rect.Find("Icon") as RectTransform;
            if (iconTransform != null)
            {
                iconTransform.anchorMin = new Vector2(0.5f, 1f);
                iconTransform.anchorMax = new Vector2(0.5f, 1f);
                iconTransform.pivot = new Vector2(0.5f, 1f);
                iconTransform.sizeDelta = new Vector2(50f, 50f);
                iconTransform.anchoredPosition = new Vector2(0f, -5f);
            }
            else if (image != null && image.sprite != null)
            {
                // 如果没有独立的子物体，我们假设 Image 是背景，不做激进移动
            }

            // 2. 处理文字：移到底部
            if (label != null)
            {
                var labelRect = label.rectTransform;
                labelRect.anchorMin = new Vector2(0f, 0f);
                labelRect.anchorMax = new Vector2(1f, 0f);
                labelRect.pivot = new Vector2(0.5f, 0f);
                labelRect.anchoredPosition = new Vector2(0f, 5f);
                labelRect.sizeDelta = new Vector2(0f, 30f);
                
                label.fontSize = fontSize;
                label.alignment = TextAlignmentOptions.Center;
                label.enableAutoSizing = false; // 禁用自动缩放以强制使用指定字号
                label.margin = new Vector4(2, 0, 2, 0);
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogDebug($"[ShopTradeIcon] 内部布局调整失败 ({rect.name}): {ex.Message}");
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
            // Reset to requested scale.
            left.transform.localScale = new Vector3(scale, scale, 1f);
            mid.transform.localScale = new Vector3(scale, scale, 1f);
            right.transform.localScale = new Vector3(scale, scale, 1f);

            float leftW = left.rect.width;
            float rightW = right.rect.width;

            // 严格计算左右边界：不允许超出原位置外边距
            float leftEdge = leftOriginal.x - (leftW * left.pivot.x) - marginX;
            float rightEdge = rightOriginal.x + (rightW * (1f - right.pivot.x)) + marginX;

            float innerLeft = leftEdge + paddingX;
            float innerRight = rightEdge - paddingX;
            float totalAvailableWidth = innerRight - innerLeft;
            
            if (totalAvailableWidth <= 0f) return;

            // 三等分
            float buttonWidth = (totalAvailableWidth - (2f * spacingX)) / 3f;
            if (buttonWidth <= 0f) return;

            // 商店按钮高度通常为 80-100，我们稍微拉高一点点以容纳上下排版（图上文下）
            float buttonHeight = 110f; 

            // 执行位置与大小分配
            left.sizeDelta = new Vector2(buttonWidth, buttonHeight);
            mid.sizeDelta = new Vector2(buttonWidth, buttonHeight);
            right.sizeDelta = new Vector2(buttonWidth, buttonHeight);

            float y = (leftOriginal.y + rightOriginal.y) * 0.5f;
            
            float leftX = innerLeft + (buttonWidth * left.pivot.x);
            float midX = innerLeft + buttonWidth + spacingX + (buttonWidth * mid.pivot.x);
            float rightX = innerLeft + (2f * (buttonWidth + spacingX)) + (buttonWidth * right.pivot.x);

            left.anchoredPosition = new Vector2(leftX, y);
            mid.anchoredPosition = new Vector2(midX, y);
            right.anchoredPosition = new Vector2(rightX, y);

            // 修正内部图文排版
            float targetFontSize = 18f;
            FixInternalButtonLayout(left, targetFontSize);
            FixInternalButtonLayout(mid, targetFontSize);
            FixInternalButtonLayout(right, targetFontSize);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopTradeIcon] 布局计算出错: {ex}");
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

            // Always let the factory decide whether to reuse a prefab-wired panel, reuse a current runtime panel,
            // or destroy/rebuild old runtime panels (which often look like translucent rectangles).
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

