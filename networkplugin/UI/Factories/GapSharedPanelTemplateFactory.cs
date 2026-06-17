using System;
using System.Linq;
using System.Reflection;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Factories;

internal static class GapSharedPanelTemplateFactory
{
    internal sealed class GapRuntimePanelTemplate
    {
        public GameObject Root { get; set; }

        public CanvasGroup CanvasGroup { get; set; }

        public RectTransform ContentRoot { get; set; }

        public TextMeshProUGUI TitleText { get; set; }

        public TextMeshProUGUI StatusText { get; set; }

        public TextMeshProUGUI TextTemplate { get; set; }

        public CommonButtonWidget ConfirmButton { get; set; }

        public CommonButtonWidget CancelButton { get; set; }
    }

    internal static GapRuntimePanelTemplate Create(
        Transform preferredParent,
        string runtimeRootName,
        string title,
        string initialStatus,
        string confirmLabel,
        string cancelLabel,
        bool createFrame = true,
        bool layoutInFrame = false)
    {
        CommonButtonWidget confirmTemplate = TryPickButtonTemplate(preferConfirm: true);
        if (confirmTemplate == null)
        {
            return null;
        }

        CommonButtonWidget cancelTemplate = TryPickButtonTemplate(preferConfirm: false) ?? confirmTemplate;
        TextMeshProUGUI textTemplate;
        try
        {
            GameObject dialogPrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            textTemplate = dialogPrefab?.GetComponentInChildren<TextMeshProUGUI>(true)
                ?? UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault(t => t != null && !IsUnderGeneratedRuntimePanel(t.transform));
        }
        catch
        {
            textTemplate = null;
        }

        Transform parent = preferredParent ?? UiManager.Instance?.transform;
        if (parent == null)
        {
            Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            parent = canvas?.transform;
        }

        GameObject root = new GameObject(runtimeRootName);
        root.SetActive(false);
        if (parent != null)
        {
            root.transform.SetParent(parent, false);
        }

        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        CanvasGroup canvasGroup = root.AddComponent<CanvasGroup>();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        Image blocker = root.AddComponent<Image>();
        blocker.color = new Color(0f, 0f, 0f, 0f);
        blocker.raycastTarget = true;

        TextMeshProUGUI frameTextTemplate = textTemplate;
        RectTransform layoutRect = rootRect;
        if (createFrame)
        {
            try
            {
                GameObject framePrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
                if (framePrefab != null)
                {
                    GameObject frame = UnityEngine.Object.Instantiate(framePrefab, root.transform, false);
                    frame.name = runtimeRootName + "_Frame";
                    frame.SetActive(true);

                    RectTransform frameRect = frame.GetComponent<RectTransform>();
                    if (frameRect != null)
                    {
                        ConfigureAnchors(frameRect, new Vector2(0.06f, 0.06f), new Vector2(0.94f, 0.94f));
                    }

                    MessageDialog dialog = frame.GetComponentInChildren<MessageDialog>(true);
                    if (dialog != null)
                    {
                        TextMeshProUGUI mainText = GetDialogField<TextMeshProUGUI>(dialog, "mainText");
                        TextMeshProUGUI subText = GetDialogField<TextMeshProUGUI>(dialog, "subText");
                        Button singleConfirm = GetDialogField<Button>(dialog, "singleConfirmButton");
                        Button dialogConfirm = GetDialogField<Button>(dialog, "confirmButton");
                        Button dialogCancel = GetDialogField<Button>(dialog, "cancelButton");

                        RectTransform buttonRect = dialogCancel?.GetComponent<RectTransform>()
                            ?? dialogConfirm?.GetComponent<RectTransform>()
                            ?? singleConfirm?.GetComponent<RectTransform>();

                        frameTextTemplate = mainText ?? subText ?? textTemplate;

                        if (layoutInFrame)
                        {
                            layoutRect = TryFindCommonAncestorRect(mainText?.rectTransform, buttonRect)
                                ?? TryFindCommonAncestorRect(subText?.rectTransform, buttonRect)
                                ?? frameRect
                                ?? rootRect;
                        }

                        HideDialogText(mainText);
                        HideDialogText(subText);
                        DestroyDialogButton(singleConfirm);
                        DestroyDialogButton(dialogConfirm);
                        DestroyDialogButton(dialogCancel);
                        dialog.enabled = false;
                    }

                    frame.transform.SetAsFirstSibling();
                }
            }
            catch
            {
            }
        }

        Transform layoutParent = layoutRect != null ? layoutRect.transform : root.transform;
        TextMeshProUGUI effectiveTextTemplate = frameTextTemplate ?? textTemplate;

        TextMeshProUGUI titleText = CloneTextOrCreate(effectiveTextTemplate, layoutParent, "Title");
        titleText.text = title;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontSize = Mathf.Max(titleText.fontSize, 34f);
        ConfigureAnchors(titleText.rectTransform, new Vector2(0.18f, 0.86f), new Vector2(0.82f, 0.95f));

        TextMeshProUGUI statusText = CloneTextOrCreate(effectiveTextTemplate, layoutParent, "Status");
        statusText.text = initialStatus;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.fontSize = Mathf.Max(statusText.fontSize, 22f);
        ConfigureAnchors(statusText.rectTransform, new Vector2(0.14f, 0.76f), new Vector2(0.86f, 0.84f));

        GameObject contentGo = new GameObject("ContentRoot");
        contentGo.transform.SetParent(layoutParent, false);
        RectTransform contentRoot = contentGo.AddComponent<RectTransform>();
        ConfigureAnchors(contentRoot, new Vector2(0.08f, 0.24f), new Vector2(0.92f, 0.76f));

        CommonButtonWidget confirmButton = CreateCommonButtonWidget(confirmTemplate, layoutParent, "Confirm", confirmLabel);
        if (confirmButton == null)
        {
            UnityEngine.Object.Destroy(root);
            return null;
        }
        DisableExtraButtons(confirmButton);
        DisableTooltipBehaviours(confirmButton.gameObject);
        ConfigureAnchors(confirmButton.GetComponent<RectTransform>(), new Vector2(0.30f, 0.06f), new Vector2(0.48f, 0.18f));

        CommonButtonWidget cancelButton = CreateCommonButtonWidget(cancelTemplate, layoutParent, "Cancel", cancelLabel);
        if (cancelButton == null)
        {
            UnityEngine.Object.Destroy(root);
            return null;
        }
        DisableExtraButtons(cancelButton);
        DisableTooltipBehaviours(cancelButton.gameObject);
        ConfigureAnchors(cancelButton.GetComponent<RectTransform>(), new Vector2(0.52f, 0.06f), new Vector2(0.70f, 0.18f));

        // ContentRoot 移到最后层，确保其子元素（报价编辑器等覆盖层）的渲染顺序高于确认/取消按钮。
        contentGo.transform.SetAsLastSibling();

        return new GapRuntimePanelTemplate
        {
            Root = root,
            CanvasGroup = canvasGroup,
            ContentRoot = contentRoot,
            TitleText = titleText,
            StatusText = statusText,
            TextTemplate = effectiveTextTemplate,
            ConfirmButton = confirmButton,
            CancelButton = cancelButton,
        };
    }

    internal static TextMeshProUGUI CloneTextOrCreate(TextMeshProUGUI template, Transform parent, string name)
    {
        TextMeshProUGUI text;
        if (template != null)
        {
            // 直接创建对象，避免克隆模板上的意外组件
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.one;
            text = go.AddComponent<TextMeshProUGUI>();
            text.font = template.font;
            text.fontSharedMaterial = template.fontSharedMaterial;
            text.fontMaterial = template.fontMaterial;
            text.color = template.color;
        }
        else
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            text = go.AddComponent<TextMeshProUGUI>();
        }

        text.raycastTarget = false;
        _ = text.rectTransform;
        return text;
    }

    internal static void ConfigureAnchors(RectTransform rect, Vector2 min, Vector2 max)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static RectTransform TryFindCommonAncestorRect(RectTransform a, RectTransform b)
    {
        if (a == null || b == null)
        {
            return null;
        }

        Transform current = a;
        while (current != null)
        {
            Transform other = b;
            while (other != null)
            {
                if (ReferenceEquals(current, other))
                {
                    return current as RectTransform;
                }

                other = other.parent;
            }

            current = current.parent;
        }

        return null;
    }

    internal static void SetButtonLabel(CommonButtonWidget buttonWidget, string label)
    {
        TextMeshProUGUI tmp = buttonWidget?.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp == null)
        {
            return;
        }

        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    internal static void DisableExtraButtons(CommonButtonWidget widget)
    {
        if (widget == null)
        {
            return;
        }

        Button keep = widget.button;
        Button[] buttons = widget.GetComponentsInChildren<Button>(true);
        if (buttons.Length <= 1)
        {
            return;
        }

        foreach (Button button in buttons)
        {
            if (button == null || button == keep)
            {
                continue;
            }

            button.enabled = false;
            button.interactable = false;
        }
    }

    internal static void DisableTooltipBehaviours(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        foreach (Behaviour behaviour in root.GetComponentsInChildren<Behaviour>(true))
        {
            if (behaviour == null)
            {
                continue;
            }

            string name = behaviour.GetType().Name;
            if (name != null && name.IndexOf("Tooltip", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                behaviour.enabled = false;
            }
        }
    }

    private static CommonButtonWidget TryPickButtonTemplate(bool preferConfirm)
    {
        try
        {
            GameObject dialogPrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            if (dialogPrefab != null)
            {
                MessageDialog dialog = dialogPrefab.GetComponent<MessageDialog>();
                if (dialog != null)
                {
                    Button button = preferConfirm
                        ? GetDialogField<Button>(dialog, "singleConfirmButton") ?? GetDialogField<Button>(dialog, "confirmButton")
                        : GetDialogField<Button>(dialog, "cancelButton") ?? GetDialogField<Button>(dialog, "confirmButton") ?? GetDialogField<Button>(dialog, "singleConfirmButton");
                    CommonButtonWidget widget = null;
                    if (button != null)
                    {
                        CommonButtonWidget[] __widgets = button.GetComponentsInParent<CommonButtonWidget>(true);
                        foreach (CommonButtonWidget w in __widgets)
                        {
                            if (ReferenceEquals(w.button, button))
                            {
                                widget = w;
                                break;
                            }
                        }
                        if (widget == null)
                        {
                            widget = button.GetComponentInParent<CommonButtonWidget>(true);
                        }
                    }
                    if (widget != null)
                    {
                        return widget;
                    }
                }
            }

            CommonButtonWidget[] candidates = UnityEngine.Object.FindObjectsByType<CommonButtonWidget>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            CommonButtonWidget best = null;
            int bestScore = int.MaxValue;

            foreach (CommonButtonWidget candidate in candidates)
            {
                if (candidate == null || candidate.button == null || IsUnderGeneratedRuntimePanel(candidate.transform))
                {
                    continue;
                }

                if (preferConfirm
                    && !string.IsNullOrWhiteSpace(candidate.name)
                    && candidate.name.IndexOf("cancel", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                int buttonCount = candidate.GetComponentsInChildren<Button>(true).Length;
                if (buttonCount == 0)
                {
                    continue;
                }

                int nodeCount = candidate.GetComponentsInChildren<Transform>(true).Length;
                int score = (buttonCount * 1000) + nodeCount;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            return best;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsUnderGeneratedRuntimePanel(Transform transform)
    {
        Transform current = transform;
        while (current != null)
        {
            if (!string.IsNullOrWhiteSpace(current.name)
                && current.name.StartsWith("NetworkPlugin_", StringComparison.Ordinal)
                && current.name.IndexOf("Panel", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
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

    private static void HideDialogText(TextMeshProUGUI text)
    {
        if (text == null)
        {
            return;
        }

        text.text = string.Empty;
        text.raycastTarget = false;
        Color color = text.color;
        color.a = 0f;
        text.color = color;
    }

    private static void HideDialogButton(Button button)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.gameObject.SetActive(false);
    }

    private static void DestroyDialogButton(Button button)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        UnityEngine.Object.Destroy(button.gameObject);
    }

    private static CommonButtonWidget CreateCommonButtonWidget(CommonButtonWidget template, Transform parent, string name, string label)
    {
        try
        {
            // 从 MessageDialog prefab 直接创建按钮，与原生对话框按钮样式一致
            GameObject prefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            if (prefab != null)
            {
                MessageDialog dialog = prefab.GetComponent<MessageDialog>();
                if (dialog != null)
                {
                    Button prefabBtn = name.IndexOf("Confirm", StringComparison.OrdinalIgnoreCase) >= 0
                        ? GetDialogField<Button>(dialog, "confirmButton") ?? GetDialogField<Button>(dialog, "singleConfirmButton")
                        : GetDialogField<Button>(dialog, "cancelButton") ?? GetDialogField<Button>(dialog, "confirmButton");

                    if (prefabBtn != null)
                    {
                        GameObject btnGo = UnityEngine.Object.Instantiate(prefabBtn.gameObject, parent, false);
                        btnGo.name = name;
                        btnGo.SetActive(true);
                        Button clonedBtn = btnGo.GetComponent<Button>();
                        clonedBtn.onClick.RemoveAllListeners();
                        var w = btnGo.AddComponent<CommonButtonWidget>();
                        w.button = clonedBtn;
                        SetButtonLabel(w, label);
                        return w;
                    }
                }
            }

            // fallback：创建简单按钮
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.one;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var fallbackWidget = go.AddComponent<CommonButtonWidget>();
            fallbackWidget.button = btn;
            SetButtonLabel(fallbackWidget, label);
            return fallbackWidget;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogWarning($"[GapSharedPanelTemplateFactory] 创建按钮失败: name={name}, {ex.Message}");
            return null;
        }
    }
}
