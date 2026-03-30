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
        string cancelLabel)
    {
        CommonButtonWidget confirmTemplate = TryPickButtonTemplate(preferConfirm: true);
        if (confirmTemplate == null)
        {
            return null;
        }

        CommonButtonWidget cancelTemplate = TryPickButtonTemplate(preferConfirm: false) ?? confirmTemplate;
        TextMeshProUGUI textTemplate = TryPickTextTemplate();

        Transform parent = preferredParent ?? UiManager.Instance?.transform;
        if (parent == null)
        {
            Canvas canvas = UnityEngine.Object.FindObjectOfType<Canvas>(true);
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

                    frameTextTemplate = mainText ?? subText ?? textTemplate;

                    HideDialogText(mainText);
                    HideDialogText(subText);
                    HideDialogButton(singleConfirm);
                    HideDialogButton(dialogConfirm);
                    HideDialogButton(dialogCancel);
                    dialog.enabled = false;
                }

                frame.transform.SetAsFirstSibling();
            }
        }
        catch
        {
        }

        TextMeshProUGUI effectiveTextTemplate = frameTextTemplate ?? textTemplate;

        TextMeshProUGUI titleText = CloneTextOrCreate(effectiveTextTemplate, root.transform, "Title");
        titleText.text = title;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontSize = Mathf.Max(titleText.fontSize, 34f);
        ConfigureAnchors(titleText.rectTransform, new Vector2(0.20f, 0.88f), new Vector2(0.80f, 0.96f));

        TextMeshProUGUI statusText = CloneTextOrCreate(effectiveTextTemplate, root.transform, "Status");
        statusText.text = initialStatus;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.fontSize = Mathf.Max(statusText.fontSize, 22f);
        ConfigureAnchors(statusText.rectTransform, new Vector2(0.15f, 0.82f), new Vector2(0.85f, 0.88f));

        GameObject contentGo = new GameObject("ContentRoot");
        contentGo.transform.SetParent(root.transform, false);
        RectTransform contentRoot = contentGo.AddComponent<RectTransform>();
        ConfigureAnchors(contentRoot, new Vector2(0.08f, 0.22f), new Vector2(0.92f, 0.80f));

        CommonButtonWidget confirmButton = UnityEngine.Object.Instantiate(confirmTemplate, root.transform, false);
        if (confirmButton == null)
        {
            UnityEngine.Object.Destroy(root);
            return null;
        }
        confirmButton.name = "Confirm";
        SetButtonLabel(confirmButton, confirmLabel);
        DisableExtraButtons(confirmButton);
        DisableTooltipBehaviours(confirmButton.gameObject);
        ConfigureAnchors(confirmButton.GetComponent<RectTransform>(), new Vector2(0.22f, 0.10f), new Vector2(0.48f, 0.18f));

        CommonButtonWidget cancelButton = UnityEngine.Object.Instantiate(cancelTemplate, root.transform, false);
        if (cancelButton == null)
        {
            UnityEngine.Object.Destroy(root);
            return null;
        }
        cancelButton.name = "Cancel";
        SetButtonLabel(cancelButton, cancelLabel);
        DisableExtraButtons(cancelButton);
        DisableTooltipBehaviours(cancelButton.gameObject);
        ConfigureAnchors(cancelButton.GetComponent<RectTransform>(), new Vector2(0.52f, 0.10f), new Vector2(0.78f, 0.18f));

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
            text = UnityEngine.Object.Instantiate(template, parent, false);
            text.name = name;
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
                    CommonButtonWidget widget = TryResolveCommonButtonWidget(button);
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

    private static CommonButtonWidget TryResolveCommonButtonWidget(Button target)
    {
        if (target == null)
        {
            return null;
        }

        CommonButtonWidget[] widgets = target.GetComponentsInParent<CommonButtonWidget>(true);
        foreach (CommonButtonWidget widget in widgets)
        {
            if (ReferenceEquals(widget.button, target))
            {
                return widget;
            }
        }

        return target.GetComponentInParent<CommonButtonWidget>(true);
    }

    private static TextMeshProUGUI TryPickTextTemplate()
    {
        try
        {
            GameObject dialogPrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            if (dialogPrefab != null)
            {
                TextMeshProUGUI prefabText = dialogPrefab.GetComponentInChildren<TextMeshProUGUI>(true);
                if (prefabText != null)
                {
                    return prefabText;
                }
            }

            return UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(text => text != null && !IsUnderGeneratedRuntimePanel(text.transform));
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
}
