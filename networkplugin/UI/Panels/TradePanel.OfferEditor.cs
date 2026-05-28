using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.Patch.UI;
using NetworkPlugin.UI.Factories;
using NetworkPlugin.UI.Payloads;
using NetworkPlugin.UI.Rules;
using NetworkPlugin.UI.Widgets;
using NetworkPlugin.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace NetworkPlugin.UI.Panels;
// 报价编辑与文本按钮
public sealed partial class TradePanel
{
private void EnsureOfferEditorOverlay()
    {
        if (_offerEditorRoot is not null)
        {
            Plugin.Logger?.LogInfo($"[TradePanel] EnsureOfferEditorOverlay skipped: existing root name={_offerEditorRoot.name}, activeSelf={_offerEditorRoot.activeSelf}");
            return;
        }

        try
        {
            Plugin.Logger?.LogInfo($"[TradePanel] EnsureOfferEditorOverlay creating: tradeId={_tradeId ?? "<null>"}");
            _offerEditorRoot = new GameObject("TradeOfferEditor");
            _offerEditorRoot.transform.SetParent(GetTradePanelContentParent(), false);

            RectTransform rootRect = _offerEditorRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.00f, -0.35f);
            rootRect.anchorMax = new Vector2(0.26f, -0.06f);
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            // 用户需求：已移除背景。

            if (!TryPickOfferEditorTemplates(out TextMeshProUGUI textTemplate, out CommonButtonWidget buttonTemplate))
            {
                Plugin.Logger?.LogWarning($"[TradePanel] EnsureOfferEditorOverlay failed: template lookup failed, tradeId={_tradeId ?? "<null>"}");
                Destroy(_offerEditorRoot);
                _offerEditorRoot = null;
                return;
            }

            // 卡牌行
            var cardsLabel = Instantiate(textTemplate, _offerEditorRoot.transform, false);
            cardsLabel.name = "CardsLabel";
            cardsLabel.text = "卡牌:";
            cardsLabel.alignment = TextAlignmentOptions.Right;
            ConfigureSingleLineText(cardsLabel);
            SetRect(cardsLabel.rectTransform, 0.02f, 0.70f, 0.25f, 0.95f);

            _cardCountText = Instantiate(textTemplate, _offerEditorRoot.transform, false);
            _cardCountText.name = "CardsValue";
            _cardCountText.text = "0";
            _cardCountText.alignment = TextAlignmentOptions.Left;
            ConfigureSingleLineText(_cardCountText);
            SetRect(_cardCountText.rectTransform, 0.28f, 0.70f, 0.52f, 0.95f);

            // 金币行
            var moneyLabel = Instantiate(textTemplate, _offerEditorRoot.transform, false);
            moneyLabel.name = "MoneyLabel";
            moneyLabel.text = "金币:";
            moneyLabel.alignment = TextAlignmentOptions.Right;
            ConfigureSingleLineText(moneyLabel);
            SetRect(moneyLabel.rectTransform, 0.02f, 0.38f, 0.25f, 0.63f);

            // 已持金币显示值：用户不需要看到，隐藏之。
            _ownedMoneyText = Instantiate(textTemplate, _offerEditorRoot.transform, false);
            _ownedMoneyText.name = "OwnedMoneyValue";
            _ownedMoneyText.text = "0";
            _ownedMoneyText.alignment = TextAlignmentOptions.Left;
            _ownedMoneyText.raycastTarget = false;
            ConfigureSingleLineText(_ownedMoneyText);
            SetRect(_ownedMoneyText.rectTransform, 0.28f, 0.38f, 0.52f, 0.63f);
            _ownedMoneyText.gameObject.SetActive(false);

            // 金币三元组：保持 '-' 和 '+' 在数字两侧等距。
            // 需求：数字位数变化时间距保持不变，三元组整体居中。
            _moneyTripletRoot = new GameObject("MoneyTriplet");
            _moneyTripletRoot.transform.SetParent(_offerEditorRoot.transform, false);
            var moneyTripletRect = _moneyTripletRoot.AddComponent<RectTransform>();
            // 金币 triplet 从标签右侧开始撑到右边，完整显示加减号和数字。
            SetRect(moneyTripletRect, 0.28f, 0.38f, 1.00f, 0.65f);

            // 金币 +/-：明确定位，避免 HorizontalLayoutGroup 导致字体尺寸坍缩为零。
            var minusBtn = CreateTextButton(textTemplate, _moneyTripletRoot.transform, "MoneyMinus", "-");
            SetRect(minusBtn.GetComponent<RectTransform>(), 0.00f, 0.00f, 0.28f, 1.00f);
            minusBtn.onClick.RemoveAllListeners();
            minusBtn.onClick.AddListener(() =>
            {
                if (!CanEditOffer()) return;
                _localMoneyOffer = Math.Max(0, _localMoneyOffer - 1);
                RefreshOfferEditorTexts();
                TrySendOfferUpdate();
            });

            _moneyValueText = Instantiate(textTemplate, _moneyTripletRoot.transform, false);
            _moneyValueText.name = "MoneyValue";
            _moneyValueText.text = "0";
            _moneyValueText.alignment = TextAlignmentOptions.Center;
            _moneyValueText.raycastTarget = false; // 数字本身不可点击
            ConfigureSingleLineText(_moneyValueText);
            SetRect(_moneyValueText.rectTransform, 0.30f, 0.00f, 0.70f, 1.00f);
            _moneyValueBaseFontSize = _moneyValueText.fontSize;

            var plusBtn = CreateTextButton(textTemplate, _moneyTripletRoot.transform, "MoneyPlus", "+");
            SetRect(plusBtn.GetComponent<RectTransform>(), 0.72f, 0.00f, 1.00f, 1.00f);
            plusBtn.onClick.RemoveAllListeners();
            plusBtn.onClick.AddListener(() =>
            {
                if (!CanEditOffer()) return;

                // 将金币报价封妖在持有金币量内；若无法可靠地获取持有金币，
                // 则允许增加以保持 UI 可用（确认时仍会验证负担能力）。
                int owned;
                bool hasOwned = TryGetOwnedMoney(out owned);
                int next = _localMoneyOffer + 1;
                if (hasOwned)
                {
                    next = Math.Min(next, owned);
                }
                next = Math.Min(next, MaxMoneyOffer);
                _localMoneyOffer = next;
                RefreshOfferEditorTexts();
                TrySendOfferUpdate();
            });

            // 展品行
            var exLabel = Instantiate(textTemplate, _offerEditorRoot.transform, false);
            exLabel.name = "ExLabel";
            exLabel.text = "展品:";
            exLabel.alignment = TextAlignmentOptions.Right;
            ConfigureSingleLineText(exLabel);
            SetRect(exLabel.rectTransform, 0.02f, 0.05f, 0.25f, 0.30f);

            _exhibitValueText = Instantiate(textTemplate, _offerEditorRoot.transform, false);
            _exhibitValueText.name = "ExValue";
            _exhibitValueText.text = "0";
            _exhibitValueText.alignment = TextAlignmentOptions.Left;
            ConfigureSingleLineText(_exhibitValueText);
            SetRect(_exhibitValueText.rectTransform, 0.28f, 0.05f, 0.52f, 0.30f);

            // 最终清理：将 button 模板层次带入的意外“取消”/多余按钮对象全部删除，仅保留已知 widget。
            PruneOfferEditorExtraButtons(_offerEditorRoot);

            EnsureOfferActionsOverlay(textTemplate);

            // 确保报价编辑器在框架视觉层上方，避免文字被 dialog mask 遮挡。
            // 在运行时布局中不与底部确认/取消按钮重叠。
            _offerEditorRoot?.transform.SetAsLastSibling();

            RefreshOfferEditorTexts();
            Plugin.Logger?.LogInfo($"[TradePanel] EnsureOfferEditorOverlay ready: tradeId={_tradeId ?? "<null>"}, cardCountText={(_cardCountText is not null)}, ownedMoneyText={(_ownedMoneyText is not null)}, exhibitValueText={(_exhibitValueText is not null)}, offerActionsExists={(_offerActionsRoot is not null)}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[TradePanel] EnsureOfferEditorOverlay exception: {ex.Message}");
            try
            {
                if (_offerEditorRoot is not null)
                {
                    Destroy(_offerEditorRoot);
                }
            }
            catch
            {
                // 忽略
            }
            _offerEditorRoot = null;
        }
    }

    private static void PruneOfferEditorExtraButtons(GameObject offerRoot)
    {
        if (offerRoot is null)
            return;

        HashSet<string> keepRoots = new HashSet<string>(StringComparer.Ordinal)
        {
            "CardsLabel",
            "CardsValue",
            "MoneyLabel",
            "OwnedMoneyValue",
            "MoneyTriplet",
            "ExLabel",
            "ExValue"
        };

        offerRoot.transform.Cast<Transform>()
            .Where(child => child is not null && !keepRoots.Contains(child.name ?? string.Empty))
            .ToList()
            .ForEach(child => Destroy(child.gameObject));

        offerRoot.GetComponentsInChildren<Transform>(true)
            .Where(t => t is not null && !ReferenceEquals(t.gameObject, offerRoot))
            .Where(t =>
            {
                var n = t.name ?? string.Empty;
                return (n.IndexOf("cancel", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        n.IndexOf("close", StringComparison.OrdinalIgnoreCase) >= 0) &&
                       !keepRoots.Contains(n);
            })
            .ToList()
            .ForEach(t => Destroy(t.gameObject));
    }

    private bool TryGetOwnedMoney(out int ownedMoney)
    {
        try
        {
            int best = 0;
            GameRunController run = ActiveGameRun;

            // 主来源：GameRun.Money（在此面板其他地方也使用）。
            best = Math.Max(best, run?.Money ?? 0);

            // 备用方1：Player 可能暴露金币相关属性。
            try
            {
                var p = run?.Player;
                if (p is not null)
                {
                    var t = p.GetType();
                    var prop = t.GetProperty("Money", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (prop is not null && prop.PropertyType == typeof(int))
                    {
                        best = Math.Max(best, (int)prop.GetValue(p));
                    }
                    var field = t.GetField("Money", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field is not null && field.FieldType == typeof(int))
                    {
                        best = Math.Max(best, (int)field.GetValue(p));
                    }
                }
            }
            catch
            {
                // 忽略
            }

            // 备用方2：GameMaster.CurrentGameRun（部分 UI 上下文使用这个）。
            try
            {
                var gm = GameMaster.Instance;
                if (gm is not null)
                {
                    var t = gm.GetType();
                    var prop = t.GetProperty("CurrentGameRun", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var reflectedRun = prop?.GetValue(gm);
                    if (reflectedRun is not null)
                    {
                        var rt = reflectedRun.GetType();
                        var moneyProp = rt.GetProperty("Money", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (moneyProp is not null && moneyProp.PropertyType == typeof(int))
                        {
                            best = Math.Max(best, (int)moneyProp.GetValue(reflectedRun));
                        }
                        var moneyField = rt.GetField("Money", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (moneyField is not null && moneyField.FieldType == typeof(int))
                        {
                            best = Math.Max(best, (int)moneyField.GetValue(reflectedRun));
                        }
                    }
                }
            }
            catch
            {
                // 忽略
            }

            if (best < 0)
            {
                best = 0;
            }

            ownedMoney = best;
            // 只有在至少有已知运行上下文时，才将 0 视为有效。
            return (run is not null) || ownedMoney > 0;
        }
        catch
        {
            ownedMoney = 0;
            return false;
        }
    }

    private void EnsureOfferActionsOverlay(TextMeshProUGUI textTemplate)
    {
        if (_offerActionsRoot is not null)
        {
            return;
        }

        try
        {
            _offerActionsRoot = new GameObject("TradeOfferActions");
            _offerActionsRoot.transform.SetParent(GetTradePanelContentParent(), false);

            var rt = _offerActionsRoot.AddComponent<RectTransform>();

            // 右下区域（绿色框区域），与详情列对齐，保持与报价编辑器的间距一致。
            rt.anchorMin = new Vector2(0.74f, -0.35f);
            rt.anchorMax = new Vector2(1.00f, -0.06f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var pickCardsBtn = CreateTextButton(textTemplate, _offerActionsRoot.transform, "PickCards", "选择卡牌", textTemplate.fontSize * 0.50f);
            SetRect(pickCardsBtn.GetComponent<RectTransform>(), 0.04f, 0.56f, 0.96f, 1f);
            var cardsTmp = pickCardsBtn.GetComponent<TextMeshProUGUI>();
            if (cardsTmp is not null) cardsTmp.alignment = TextAlignmentOptions.Center;

            pickCardsBtn.onClick.RemoveAllListeners();
            pickCardsBtn.onClick.AddListener(() =>
            {
                if (!CanEditOffer())
                {
                    UpdateUIStatus(TryLocalize("Trade.WaitingForItems", "请先开始交易/等待交易开启"));
                    return;
                }

                ShowCardPickerOverlay();
            });

            var pickExhibitsBtn = CreateTextButton(textTemplate, _offerActionsRoot.transform, "PickExhibits", "选择展品", textTemplate.fontSize * 0.50f);
            SetRect(pickExhibitsBtn.GetComponent<RectTransform>(), 0.04f, 0.00f, 0.96f, 0.40f);
            var exhibitsTmp = pickExhibitsBtn.GetComponent<TextMeshProUGUI>();
            if (exhibitsTmp is not null) exhibitsTmp.alignment = TextAlignmentOptions.Center;

            pickExhibitsBtn.onClick.RemoveAllListeners();
            pickExhibitsBtn.onClick.AddListener(() =>
            {
                if (!CanEditOffer())
                {
                    UpdateUIStatus(TryLocalize("Trade.WaitingForItems", "请先开始交易/等待交易开启"));
                    return;
                }

                ShowExhibitPickerOverlay();
            });

            // 初始化时同步报价编辑器的显示状态。
            _offerActionsRoot?.SetActive(_offerEditorRoot is not null && _offerEditorRoot.activeSelf);
        }
        catch
        {
            try
            {
                if (_offerActionsRoot is not null)
                {
                    Destroy(_offerActionsRoot);
                }
            }
            catch
            {
                // 忽略
            }

            _offerActionsRoot = null;
        }
    }

    private sealed class TextButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public float HoverScale = 1.08f;
        public float PressedScale = 1.12f;
        public float AnimationSpeed = 8f;

        private RectTransform _rt;
        private bool _hovering;
        private bool _pressed;
        private float _targetScale = 1f;

        private void Awake()
        {
            _rt = transform as RectTransform;
            ResetScale();
        }

        private void OnEnable()
        {
            ResetScale();
        }

        private void OnDisable()
        {
            _hovering = false;
            _pressed = false;
            ResetScale();
        }

        private void ResetScale()
        {
            _targetScale = 1f;
            if (_rt is not null) _rt.localScale = Vector3.one;
        }

        private void Update()
        {
            if (_rt is null) return;
            float cur = _rt.localScale.x;
            if (Mathf.Approximately(cur, _targetScale))
                return;
            float next = Mathf.Lerp(cur, _targetScale, Time.unscaledDeltaTime * AnimationSpeed);
            _rt.localScale = new Vector3(next, next, 1f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovering = true;
            ApplyScale();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
            _pressed = false;
            ApplyScale();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pressed = true;
            ApplyScale();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pressed = false;
            ApplyScale();
        }

        private void ApplyScale()
        {
            if (_rt is null) return;
            _targetScale = _pressed ? PressedScale : _hovering ? HoverScale : 1f;
        }
    }

    private static Button CreateTextButton(TextMeshProUGUI template, Transform parent, string name, string text, float fontSize = -1f)
    {
        // 从游戏内模板克隆 TMP，保证字体/材质与原生 UI 一致。
        var tmp = Instantiate(template, parent, false);
        tmp.name = name;
        tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = true;
        // 强制初始缩放 1.0，避免模板 scale 残留
        tmp.transform.localScale = Vector3.one;
        ConfigureSingleLineText(tmp);
        if (fontSize > 0f)
        {
            tmp.enableAutoSizing = false;
            tmp.fontSize = fontSize;
            tmp.fontSizeMin = 1f;
            tmp.fontSizeMax = fontSize;
        }
        var c = tmp.color;
        c.a = 1f;
        tmp.color = c;

        var btn = tmp.gameObject.AddComponent<Button>();
        btn.targetGraphic = tmp;
        btn.transition = Selectable.Transition.ColorTint;

        // 使用与游戏内「可点击文字”一致的亮色 hover 效果。
        Color baseColor = tmp.color;
        Color hover = new Color(
            Mathf.Clamp01(baseColor.r * 1.15f),
            Mathf.Clamp01(baseColor.g * 1.15f),
            Mathf.Clamp01(baseColor.b * 1.15f),
            baseColor.a);
        Color pressed = new Color(
            Mathf.Clamp01(baseColor.r * 0.95f),
            Mathf.Clamp01(baseColor.g * 0.95f),
            Mathf.Clamp01(baseColor.b * 0.95f),
            baseColor.a);
        Color disabled = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * 0.35f);

        var colors = btn.colors;
        colors.normalColor = baseColor;
        colors.highlightedColor = hover;
        colors.selectedColor = hover;
        colors.pressedColor = pressed;
        colors.disabledColor = disabled;
        colors.fadeDuration = 0.08f;
        btn.colors = colors;

        var nav = btn.navigation;
        nav.mode = Navigation.Mode.None;
        btn.navigation = nav;

        _ = tmp.gameObject.AddComponent<TextButtonHover>();
        return btn;
    }

    private static void ConfigureSingleLineText(TextMeshProUGUI text)
    {
        if (text is null)
        {
            return;
        }

        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
    }

    private bool TryPickOfferEditorTemplates(out TextMeshProUGUI textTemplate, out CommonButtonWidget buttonTemplate)
    {
        textTemplate = null;
        buttonTemplate = null;

        // 首选复用 vanilla dialog prefab 模板，以保持 TMP 字体/材质与原生 UI 一致。
        try
        {
            GameObject dialogPrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            if (dialogPrefab is not null)
            {
                var dialog = dialogPrefab.GetComponentInChildren<MessageDialog>(true);
                if (dialog is not null)
                {
                    textTemplate = GetDialogField<TextMeshProUGUI>(dialog, "mainText")
                                   ?? GetDialogField<TextMeshProUGUI>(dialog, "subText")
                                   ?? dialogPrefab.GetComponentInChildren<TextMeshProUGUI>(true);

                    // 优先 singleConfirmButton（通常为金/木色确认按钮）或 confirmButton，最后才用 cancelButton。
                    Button targetButton = GetDialogField<Button>(dialog, "singleConfirmButton")
                                       ?? GetDialogField<Button>(dialog, "confirmButton")
                                       ?? GetDialogField<Button>(dialog, "cancelButton");

                    buttonTemplate = TryResolveCommonButtonWidget(targetButton);
                }
                else
                {
                    textTemplate = dialogPrefab.GetComponentInChildren<TextMeshProUGUI>(true);
                }

                // 备用启发式地：若具体字段未找到，按得分选择最佳自定义按钮。
                if (buttonTemplate is null)
                {
                    CommonButtonWidget best = null;
                    int bestScore = int.MaxValue;
                    var widgets = dialogPrefab.GetComponentsInChildren<CommonButtonWidget>(true);
                    foreach (var w in widgets)
                    {
                        if (w is null || w.button is null)
                        {
                            continue;
                        }

                        int buttons = w.GetComponentsInChildren<Button>(true).Length;
                        int nodes = w.GetComponentsInChildren<Transform>(true).Length;

                        if (buttons <= 0)
                        {
                            continue;
                        }

                        int score = (buttons * 1000) + nodes;

                        // 报价编辑器高度优先選 confirm/singleConfirm 按钮外观。
                        if (w.name.IndexOf("confirm", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            score -= 500;
                        }
                        // 严格排除名为 "cancel" 的任何对象。
                        if (w.name.IndexOf("cancel", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            score += 2000;
                        }

                        if (score < bestScore)
                        {
                            bestScore = score;
                            best = w;
                        }
                    }
                    buttonTemplate = best;
                }

                // 最终守卫：报价编辑器需要单一 button widget。
                buttonTemplate = PreferSingleButtonWidget(buttonTemplate);
            }
        }
        catch
        {
            // 忽略
        }

        // 备用：使用面板内已有引用（仍是游戏 UI，但兼容性稍差）。优先 confirmButton 而非 cancelButton。
        textTemplate ??= statusText ?? player1NameText;
        buttonTemplate ??= confirmButton ?? cancelButton;

        buttonTemplate = PreferSingleButtonWidget(buttonTemplate);

        return textTemplate != null && buttonTemplate != null;
    }

    private static CommonButtonWidget TryResolveCommonButtonWidget(Button target)
    {
        // 优先选择明确引用该 Button 的最近 widget。
        var widgets = target.GetComponentsInParent<CommonButtonWidget>(true);
        if (widgets is not null)
        {
            foreach (var w in widgets)
            {
                if (w is null)
                {
                    continue;
                }

                if (ReferenceEquals(w.button, target))
                {
                    return w;
                }
            }
        }

        return target.GetComponentInParent<CommonButtonWidget>();
    }

    private static CommonButtonWidget PreferSingleButtonWidget(CommonButtonWidget template)
    {
        if (template is null)
        {
            return null;
        }

        int buttons = template.GetComponentsInChildren<Button>(true).Length;
        if (buttons <= 1)
        {
            return template;
        }

        CommonButtonWidget best = null;
        int bestNodes = int.MaxValue;
        foreach (var w in template.GetComponentsInChildren<CommonButtonWidget>(true))
        {
            if (w is null || w.button is null)
            {
                continue;
            }

            // 跳过名为 "cancel" 的对象。
            if (!string.IsNullOrWhiteSpace(w.name) && w.name.IndexOf("cancel", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }

            int wButtons = w.GetComponentsInChildren<Button>(true).Length;
            if (wButtons != 1)
            {
                continue;
            }

            int nodes = w.GetComponentsInChildren<Transform>(true).Length;
            if (nodes < bestNodes)
            {
                bestNodes = nodes;
                best = w;
            }
        }

        return best ?? template;
    }

    private bool CanEditOffer()
    {
        if (_isApplyingState || _partnerPickerActive)
        {
            return false;
        }

        // 本地调试会话始终允许编辑。
        if (_localDebugTradeMode)
        {
            return true;
        }

        TradeSyncPatch.TradeSessionState state = TradeSyncPatch.GetLastKnown(_tradeId);
        return state is null || state.Status == TradeSyncPatch.TradeStatus.Open;
    }

    private void RefreshOfferEditorTexts()
    {
        if (_cardCountText is not null) _cardCountText.text = (_player1OfferedCards?.Count ?? 0).ToString();

        if (_ownedMoneyText is not null)
        {
            if (!TryGetOwnedMoney(out int owned))
                owned = ActiveGameRun?.Money ?? 0;
            _ownedMoneyText.text = Math.Max(0, owned).ToString();
        }

        if (_moneyValueText is not null)
        {
            _moneyValueText.text = _localMoneyOffer.ToString();
            // 根据数字位数自动缩放字体
            string s2 = _moneyValueText.text ?? string.Empty;
            int digits = 0;
            for (int i = 0; i < s2.Length; i++)
            {
                char ch = s2[i];
                if (ch >= '0' && ch <= '9')
                {
                    digits++;
                }
            }
            float baseSize = _moneyValueBaseFontSize > 0f ? _moneyValueBaseFontSize : _moneyValueText.fontSize;
            if (digits >= 4)
            {
                float scale = Mathf.Clamp(3f / digits, 0.6f, 1f);
                _moneyValueText.fontSize = baseSize * scale;
            }
            else
            {
                _moneyValueText.fontSize = baseSize;
            }
            if (_moneyTripletRoot != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_moneyTripletRoot.transform as RectTransform);
        }

        if (_exhibitValueText is not null) _exhibitValueText.text = _localExhibitOfferIds.Count.ToString();
        RebuildExhibitPreviews();
    }
}
