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

        // Cache original anchor positions so our adjustments don't accumulate.
        public bool HasOriginalPositions;
        public Vector2 CardServiceOriginalAnchoredPosition;
        public Vector2 ReturnOriginalAnchoredPosition;
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
            SetUiVisible(false);
            return;
        }

        bool shouldShow = IsTradeEnabledAndConnected();
        if (!shouldShow)
        {
            SetUiVisible(false);
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
            // 优先改掉“原始卡牌服务文字”，避免抓到其它说明文本。
            if (!string.IsNullOrEmpty(sourceLabelText))
            {
                label = allLabels.FirstOrDefault(t => t != null && t.text == sourceLabelText);
            }

            // 兜底：取第一个
            label ??= allLabels.FirstOrDefault(t => t != null);

            // 把所有等于源按钮文案的 TMP 一并替换，确保不会出现两个“卡牌服务”。
            if (!string.IsNullOrEmpty(sourceLabelText))
            {
                foreach (var t in allLabels)
                {
                    if (t != null && t.text == sourceLabelText)
                    {
                        t.text = "交易";
                    }
                }
            }
            else if (label != null)
            {
                label.text = "交易";
            }
        }

        if (label != null && sourceLabel != null)
        {
            // 复制风格关键字段，确保与另外两个按钮一致
            label.font = sourceLabel.font;
            label.fontSize = sourceLabel.fontSize;
            label.fontStyle = sourceLabel.fontStyle;
            label.alignment = sourceLabel.alignment;
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

        // 4. 放置位置：水平排列在右下角一栏
        var leftRect = cardServiceButton.GetComponent<RectTransform>();
        var midRect = root.GetComponent<RectTransform>();
        var rightRect = returnButton.GetComponent<RectTransform>();

        // Capture original positions once per shop panel so offsets don't stack.
        Vector2 leftOriginal = leftRect != null ? leftRect.anchoredPosition : Vector2.zero;
        Vector2 rightOriginal = rightRect != null ? rightRect.anchoredPosition : Vector2.zero;
        if (_ui != null && _ui.ShopPanel == shopPanel && _ui.HasOriginalPositions)
        {
            leftOriginal = _ui.CardServiceOriginalAnchoredPosition;
            rightOriginal = _ui.ReturnOriginalAnchoredPosition;
        }

        if (leftRect != null && midRect != null && rightRect != null)
        {

            // 统一应用紧凑样式（缩放）以确保三个按钮能放下
            ApplyCompactButtonStyle_NoThrow(cardServiceButton);
            ApplyCompactButtonStyle_NoThrow(button);
            ApplyCompactButtonStyle_NoThrow(returnButton);

            // 强制设置缩放为 0.85f
            cardServiceButton.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            root.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            returnButton.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

            // 水平排列布局
            // 交易与卡牌服务整体左移 30；关闭商店横向向右移动 30，同时纵向与另外两个保持一致。
            float spacing = 265f;
            var basePos = leftOriginal + new Vector2(-30f, 0f);
            leftRect.anchoredPosition = basePos;
            midRect.anchoredPosition = basePos + new Vector2(spacing, 0f);
            rightRect.anchoredPosition = new Vector2(rightOriginal.x + 30f, basePos.y);
            
            Plugin.Logger?.LogInfo($"[ShopTradeIcon] 布局更新：水平排列 [卡牌服务] -> [交易] -> [返回]");
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
            CardServiceOriginalAnchoredPosition = leftOriginal,
            ReturnOriginalAnchoredPosition = rightOriginal
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
            if (_ui?.Root != null)
            {
                UnityEngine.Object.Destroy(_ui.Root);
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

            // 若项目内已经有可用的 TradePanel（例如后续用Prefab/运行时构建完成），则优先打开它。
            TradePanel tradePanel = null;
            try
            {
                tradePanel = UnityEngine.Object.FindObjectOfType<TradePanel>(true);
            }
            catch
            {
                tradePanel = null;
            }

            // If there is no prefab-wired instance, build a minimal panel by cloning in-game UI templates.
            if (tradePanel == null)
            {
                Transform parent = null;
                if (shopPanel != null && shopPanel.transform != null)
                {
                    parent = shopPanel.transform.parent != null ? shopPanel.transform.parent : shopPanel.transform;
                }

                tradePanel = NetworkPlugin.UI.Panels.TradePanelRuntimeFactory.GetOrCreate(parent);
            }

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

