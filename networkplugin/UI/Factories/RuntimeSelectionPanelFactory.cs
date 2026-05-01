using System;
using System.Reflection;
using HarmonyLib;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Factories;

internal static class RuntimeSelectionPanelFactory
{
    internal sealed class SelectionPanelScaffold
    {
        public GameObject Root { get; set; }
        public RectTransform PanelRect { get; set; }
        public TextMeshProUGUI TitleText { get; set; }
        public TextMeshProUGUI EmptyText { get; set; }
        public TextMeshProUGUI TextTemplate { get; set; }
        public CommonButtonWidget RowButtonTemplate { get; set; }
        public Button ConfirmButton { get; set; }
        public Button CancelButton { get; set; }
        public CommonButtonWidget ConfirmButtonWidget { get; set; }
        public CommonButtonWidget CancelButtonWidget { get; set; }
        public ScrollRect ScrollRect { get; set; }
        public RectTransform Content { get; set; }
    }

    internal sealed class ScrollAreaScaffold
    {
        public ScrollRect ScrollRect { get; set; }
        public RectTransform Content { get; set; }
    }

    internal sealed class RuntimeSelectionRow : MonoBehaviour
    {
        private static readonly Color SelectedBackground = new(0.20f, 0.36f, 0.48f, 0.92f);
        private static readonly Color NormalBackground = new(0.10f, 0.10f, 0.10f, 0.78f);

        public Button Button { get; set; }
        public Image Background { get; set; }
        public Image LeadingIcon { get; set; }
        public TextMeshProUGUI PrimaryText { get; set; }
        public TextMeshProUGUI SecondaryText { get; set; }
        public GameObject SelectedIndicator { get; set; }

        internal void SetTexts(string primary, string secondary)
        {
            if (PrimaryText != null)
            {
                PrimaryText.text = primary ?? string.Empty;
            }

            if (SecondaryText != null)
            {
                SecondaryText.text = secondary ?? string.Empty;
                SecondaryText.gameObject.SetActive(!string.IsNullOrWhiteSpace(secondary));
            }
        }

        internal void SetSecondaryText(string secondary)
        {
            if (SecondaryText == null)
            {
                return;
            }

            SecondaryText.text = secondary ?? string.Empty;
            SecondaryText.gameObject.SetActive(!string.IsNullOrWhiteSpace(secondary));
        }

        internal void SetLeadingSprite(Sprite sprite)
        {
            if (LeadingIcon == null)
            {
                return;
            }

            LeadingIcon.sprite = sprite;
            LeadingIcon.gameObject.SetActive(sprite != null);
        }

        internal void SetSelected(bool selected)
        {
            if (SelectedIndicator != null)
            {
                SelectedIndicator.SetActive(selected);
            }

            if (Background != null)
            {
                Background.color = selected ? SelectedBackground : NormalBackground;
            }
        }

        internal void SetInteractable(bool interactable)
        {
            if (Button != null)
            {
                Button.interactable = interactable;
            }

            Color primaryColor = interactable ? Color.white : new Color(0.65f, 0.65f, 0.65f, 0.88f);
            Color secondaryColor = interactable ? new Color(0.84f, 0.84f, 0.84f, 0.92f) : new Color(0.58f, 0.58f, 0.58f, 0.80f);

            if (PrimaryText != null)
            {
                PrimaryText.color = primaryColor;
            }

            if (SecondaryText != null)
            {
                SecondaryText.color = secondaryColor;
            }

            if (LeadingIcon != null)
            {
                LeadingIcon.color = interactable ? Color.white : new Color(0.65f, 0.65f, 0.65f, 0.88f);
            }
        }
    }

    internal static SelectionPanelScaffold CreateModal(
        Transform parent,
        string rootName,
        string title,
        string confirmLabel,
        string cancelLabel,
        bool showConfirmButton)
    {
        try
        {
            GameObject dialogPrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            if (dialogPrefab == null)
            {
                return null;
            }

            GameObject root = new(rootName);
            root.SetActive(false);
            root.transform.SetParent(parent, false);

            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            CanvasGroup canvasGroup = root.AddComponent<CanvasGroup>();
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            Image overlay = root.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.55f);
            overlay.raycastTarget = true;

            GameObject frame = UnityEngine.Object.Instantiate(dialogPrefab, root.transform, false);
            frame.name = rootName + "_Frame";
            frame.SetActive(true);

            RectTransform frameRect = frame.GetComponent<RectTransform>();
            if (frameRect != null)
            {
                frameRect.anchorMin = new Vector2(0.10f, 0.06f);
                frameRect.anchorMax = new Vector2(0.90f, 0.94f);
                frameRect.offsetMin = Vector2.zero;
                frameRect.offsetMax = Vector2.zero;
            }

            MessageDialog dialog = frame.GetComponentInChildren<MessageDialog>(true);
            if (dialog == null)
            {
                UnityEngine.Object.Destroy(root);
                return null;
            }

            TextMeshProUGUI mainText = GetDialogField<TextMeshProUGUI>(dialog, "mainText");
            TextMeshProUGUI subText = GetDialogField<TextMeshProUGUI>(dialog, "subText");
            Button singleConfirm = GetDialogField<Button>(dialog, "singleConfirmButton");
            Button confirm = GetDialogField<Button>(dialog, "confirmButton");
            Button cancel = GetDialogField<Button>(dialog, "cancelButton");

            CommonButtonWidget confirmWidget = TryResolveCommonButtonWidget(confirm ?? singleConfirm ?? cancel);
            CommonButtonWidget cancelWidget = TryResolveCommonButtonWidget(cancel ?? confirm ?? singleConfirm);

            RectTransform panelRect = TryFindCommonAncestorRect(mainText?.rectTransform, cancel?.GetComponent<RectTransform>())
                ?? (mainText != null ? mainText.rectTransform.parent as RectTransform : null)
                ?? frameRect;

            if (mainText != null)
            {
                mainText.text = title ?? string.Empty;
                mainText.alignment = TextAlignmentOptions.Center;
                mainText.raycastTarget = false;
                Color c = mainText.color;
                c.a = 1f;
                mainText.color = c;
                mainText.gameObject.SetActive(true);
            }

            if (subText != null)
            {
                subText.text = string.Empty;
                subText.raycastTarget = false;
                subText.alignment = TextAlignmentOptions.Center;
                Color c = subText.color;
                c.a = 0f;
                subText.color = c;
                subText.gameObject.SetActive(true);
            }

            if (singleConfirm != null && !ReferenceEquals(singleConfirm, confirm))
            {
                ResetButtonClick(singleConfirm);
                singleConfirm.gameObject.SetActive(false);
            }

            if (confirm != null)
            {
                ResetButtonClick(confirm);
                confirm.gameObject.SetActive(showConfirmButton);
                SetButtonLabel(confirm, confirmLabel);
            }

            if (cancel != null)
            {
                ResetButtonClick(cancel);
                cancel.gameObject.SetActive(true);
                SetButtonLabel(cancel, cancelLabel);
            }

            ScrollAreaScaffold scrollArea = CreateScrollArea(panelRect, subText?.rectTransform, rootName + "_Scroll");
            if (scrollArea == null)
            {
                UnityEngine.Object.Destroy(root);
                return null;
            }

            dialog.enabled = false;

            if (cancel != null)
            {
                cancel.transform.SetAsLastSibling();
            }

            if (confirm != null)
            {
                confirm.transform.SetAsLastSibling();
            }

            return new SelectionPanelScaffold
            {
                Root = root,
                PanelRect = panelRect,
                TitleText = mainText,
                EmptyText = subText,
                TextTemplate = mainText ?? subText,
                RowButtonTemplate = confirmWidget ?? cancelWidget,
                ConfirmButton = confirm,
                CancelButton = cancel,
                ConfirmButtonWidget = confirmWidget,
                CancelButtonWidget = cancelWidget,
                ScrollRect = scrollArea.ScrollRect,
                Content = scrollArea.Content,
            };
        }
        catch
        {
            return null;
        }
    }

    internal static ScrollAreaScaffold CreateScrollArea(RectTransform panelRect, RectTransform placeholderRect, string scrollName)
    {
        if (panelRect == null)
        {
            return null;
        }

        GameObject scrollGo = new(scrollName);
        scrollGo.transform.SetParent(panelRect, false);
        scrollGo.transform.SetAsLastSibling();

        RectTransform scrollRt = scrollGo.AddComponent<RectTransform>();
        if (placeholderRect != null)
        {
            CopyRectTransform(scrollRt, placeholderRect);
        }
        else
        {
            scrollRt.anchorMin = new Vector2(0.06f, 0.20f);
            scrollRt.anchorMax = new Vector2(0.94f, 0.78f);
            scrollRt.offsetMin = Vector2.zero;
            scrollRt.offsetMax = Vector2.zero;
        }

        Image scrollImage = scrollGo.AddComponent<Image>();
        scrollImage.color = new Color(0f, 0f, 0f, 0f);
        scrollImage.raycastTarget = true;

        ScrollRect scrollRect = scrollGo.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 24f;

        GameObject viewport = new("Viewport");
        viewport.transform.SetParent(scrollGo.transform, false);
        RectTransform viewportRt = viewport.AddComponent<RectTransform>();
        viewportRt.anchorMin = Vector2.zero;
        viewportRt.anchorMax = Vector2.one;
        viewportRt.offsetMin = Vector2.zero;
        viewportRt.offsetMax = Vector2.zero;
        viewport.AddComponent<RectMask2D>();
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0f);
        viewportImage.raycastTarget = true;

        GameObject contentGo = new("Content");
        contentGo.transform.SetParent(viewport.transform, false);
        RectTransform contentRt = contentGo.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = contentGo.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.spacing = 10f;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentGo.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRt;
        scrollRect.content = contentRt;

        return new ScrollAreaScaffold
        {
            ScrollRect = scrollRect,
            Content = contentRt,
        };
    }

    internal static RuntimeSelectionRow CreateRow(
        RectTransform parent,
        CommonButtonWidget styleTemplate,
        TextMeshProUGUI textTemplate,
        string rowName,
        string primaryText,
        string secondaryText,
        Sprite leadingSprite,
        bool selected,
        bool interactable)
    {
        if (parent == null)
        {
            return null;
        }

        GameObject rowGo = new(rowName);
        rowGo.transform.SetParent(parent, false);

        RectTransform rowRect = rowGo.AddComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0f, 1f);
        rowRect.anchorMax = new Vector2(1f, 1f);
        rowRect.pivot = new Vector2(0.5f, 1f);
        rowRect.sizeDelta = new Vector2(0f, 82f);

        LayoutElement layout = rowGo.AddComponent<LayoutElement>();
        layout.preferredHeight = 82f;
        layout.flexibleWidth = 1f;

        Image background = rowGo.AddComponent<Image>();
        background.color = new Color(0.10f, 0.10f, 0.10f, 0.78f);
        background.raycastTarget = true;

        Button button = rowGo.AddComponent<Button>();
        button.targetGraphic = background;
        button.transition = Selectable.Transition.ColorTint;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        button.colors = ResolveColorBlock(styleTemplate);

        GameObject selectedGo = new("SelectedIndicator");
        selectedGo.transform.SetParent(rowGo.transform, false);
        RectTransform selectedRect = selectedGo.AddComponent<RectTransform>();
        selectedRect.anchorMin = new Vector2(0f, 0f);
        selectedRect.anchorMax = new Vector2(0f, 1f);
        selectedRect.pivot = new Vector2(0f, 0.5f);
        selectedRect.sizeDelta = new Vector2(10f, 0f);
        selectedRect.anchoredPosition = Vector2.zero;
        Image selectedImage = selectedGo.AddComponent<Image>();
        selectedImage.color = new Color(0.40f, 0.80f, 1f, 0.95f);

        GameObject content = new("Content");
        content.transform.SetParent(rowGo.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 0f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.offsetMin = new Vector2(20f, 6f);
        contentRect.offsetMax = new Vector2(-12f, -6f);

        GameObject iconGo = new("LeadingIcon");
        iconGo.transform.SetParent(content.transform, false);
        RectTransform iconRect = iconGo.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.sizeDelta = new Vector2(48f, 48f);
        iconRect.anchoredPosition = new Vector2(0f, 0f);
        Image leadingIcon = iconGo.AddComponent<Image>();
        leadingIcon.raycastTarget = false;
        leadingIcon.preserveAspect = true;

        TextMeshProUGUI primary = CloneText(textTemplate, content.transform, rowName + "_Primary");
        RectTransform primaryRect = primary.rectTransform;
        primaryRect.anchorMin = new Vector2(0f, 0.48f);
        primaryRect.anchorMax = new Vector2(1f, 1f);
        primaryRect.offsetMin = new Vector2(64f, 0f);
        primaryRect.offsetMax = Vector2.zero;
        primary.alignment = TextAlignmentOptions.Left;
        primary.fontSize = Mathf.Max(primary.fontSize * 0.80f, 18f);
        primary.enableWordWrapping = false;
        primary.overflowMode = TextOverflowModes.Ellipsis;
        primary.raycastTarget = false;

        TextMeshProUGUI secondary = CloneText(textTemplate, content.transform, rowName + "_Secondary");
        RectTransform secondaryRect = secondary.rectTransform;
        secondaryRect.anchorMin = new Vector2(0f, 0f);
        secondaryRect.anchorMax = new Vector2(1f, 0.50f);
        secondaryRect.offsetMin = new Vector2(64f, 0f);
        secondaryRect.offsetMax = Vector2.zero;
        secondary.alignment = TextAlignmentOptions.Left;
        secondary.fontSize = Mathf.Max(secondary.fontSize * 0.58f, 14f);
        secondary.enableWordWrapping = false;
        secondary.overflowMode = TextOverflowModes.Ellipsis;
        secondary.raycastTarget = false;
        secondary.color = new Color(0.84f, 0.84f, 0.84f, 0.92f);

        RuntimeSelectionRow row = rowGo.AddComponent<RuntimeSelectionRow>();
        row.Button = button;
        row.Background = background;
        row.LeadingIcon = leadingIcon;
        row.PrimaryText = primary;
        row.SecondaryText = secondary;
        row.SelectedIndicator = selectedGo;
        row.SetTexts(primaryText, secondaryText);
        row.SetLeadingSprite(leadingSprite);
        row.SetSelected(selected);
        row.SetInteractable(interactable);

        rowGo.SetActive(true);
        return row;
    }

    private static TextMeshProUGUI CloneText(TextMeshProUGUI template, Transform parent, string name)
    {
        TextMeshProUGUI text;
        if (template != null)
        {
            text = UnityEngine.Object.Instantiate(template, parent, false);
            text.name = name;
        }
        else
        {
            GameObject go = new(name);
            go.transform.SetParent(parent, false);
            text = go.AddComponent<TextMeshProUGUI>();
        }

        text.gameObject.SetActive(true);
        Color c = text.color;
        c.a = 1f;
        text.color = c;
        return text;
    }

    private static ColorBlock ResolveColorBlock(CommonButtonWidget template)
    {
        if (template?.button != null)
        {
            return template.button.colors;
        }

        return new ColorBlock
        {
            normalColor = Color.white,
            highlightedColor = new Color(1f, 1f, 0.85f, 1f),
            pressedColor = new Color(0.88f, 0.88f, 0.88f, 1f),
            selectedColor = new Color(1f, 1f, 0.85f, 1f),
            disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.65f),
            colorMultiplier = 1f,
            fadeDuration = 0.08f,
        };
    }

    private static void SetButtonLabel(Button button, string label)
    {
        if (button == null)
        {
            return;
        }

        TextMeshProUGUI tmp = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp != null)
        {
            tmp.text = label ?? string.Empty;
            tmp.alignment = TextAlignmentOptions.Center;
            Color c = tmp.color;
            c.a = 1f;
            tmp.color = c;
        }
    }

    private static void ResetButtonClick(Button button)
    {
        if (button == null)
        {
            return;
        }

        button.onClick = new Button.ButtonClickedEvent();
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

    private static RectTransform TryFindCommonAncestorRect(RectTransform a, RectTransform b)
    {
        if (a == null || b == null)
        {
            return null;
        }

        System.Collections.Generic.HashSet<Transform> ancestors = new();
        Transform current = a;
        while (current != null)
        {
            ancestors.Add(current);
            current = current.parent;
        }

        current = b;
        while (current != null)
        {
            if (ancestors.Contains(current))
            {
                return current as RectTransform;
            }

            current = current.parent;
        }

        return null;
    }

    private static void CopyRectTransform(RectTransform destination, RectTransform source)
    {
        if (destination == null || source == null)
        {
            return;
        }

        destination.anchorMin = source.anchorMin;
        destination.anchorMax = source.anchorMax;
        destination.pivot = source.pivot;
        destination.anchoredPosition = source.anchoredPosition;
        destination.sizeDelta = source.sizeDelta;
        destination.offsetMin = source.offsetMin;
        destination.offsetMax = source.offsetMax;
        destination.localScale = source.localScale;
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
}
