using System;
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

    private sealed class TradeButtonUi
    {
        public ShopPanel ShopPanel;
        public GameObject Root;
        public Button Button;
        public TextMeshProUGUI Label;
    }

    private static TradeButtonUi _ui;
    private static TMP_FontAsset _defaultFont;
    private static Sprite _whiteSprite;
    private static Texture2D _whiteTexture;

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

    private static bool TryGetShopPanel(out ShopPanel shopPanel)
    {
        shopPanel = null;
        if (!UiManager.IsInitialized)
        {
            return false;
        }

        try
        {
            shopPanel = UiManager.GetPanel<ShopPanel>();
            return shopPanel != null;
        }
        catch
        {
            return false;
        }
    }

    private static bool ShouldShow(ShopPanel shopPanel)
    {
        if (shopPanel == null || !shopPanel.IsVisible)
        {
            return false;
        }

        var config = TryGetConfig();
        if (config?.AllowTrading?.Value != true)
        {
            return false;
        }

        var client = TryGetNetworkClient();
        if (client == null || !client.IsConnected)
        {
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(GameDirector), "Update")]
    [HarmonyPostfix]
    private static void GameDirector_Update_Postfix()
    {
        try
        {
            UpdateTradeButtonUi();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopTradeIcon] Update failed: {ex.Message}");
        }
    }

    private static void UpdateTradeButtonUi()
    {
        if (!TryGetShopPanel(out ShopPanel shopPanel))
        {
            SetUiVisible(false);
            return;
        }

        if (!ShouldShow(shopPanel))
        {
            SetUiVisible(false);
            return;
        }

        EnsureUi(shopPanel);
        SetUiVisible(true);
    }

    private static void EnsureUi(ShopPanel shopPanel)
    {
        if (_ui != null && _ui.Root != null && _ui.ShopPanel == shopPanel)
        {
            return;
        }

        CleanupUi();

        Transform parent = TryGetShopPanelRoot(shopPanel) ?? shopPanel.transform;
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
        label.text = "TRADE";
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
            if (!ShouldShow(shopPanel))
            {
                ShowTopMessage("Trading is not available (not connected or disabled).");
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

            ShowTopMessage("Trade UI is not available yet. Use the GapStation trade option or implement TradePanel prefab/runtime UI.");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopTradeIcon] Click failed: {ex.Message}");
        }
    }

    private static void ShowTopMessage(string message)
    {
        try
        {
            if (!UiManager.IsInitialized)
            {
                return;
            }

            UiManager.GetPanel<TopMessagePanel>().ShowMessage(message);
        }
        catch
        {
            // ignored
        }
    }
}

