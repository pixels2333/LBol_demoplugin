using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using NetworkPlugin.UI.Panels;
using NetworkPlugin.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Factories;

internal static class ResurrectPanelRuntimeFactory
{
    private const string RuntimeRootName = "NetworkPlugin_ResurrectPanel";
    private const string RuntimeUiVersion = "2026-03-24-ui-v2";

    internal static ResurrectPanel GetOrCreate(Transform preferredParent)
    {
        try
        {
            ResurrectPanel[] existingPanels = UnityEngine.Object.FindObjectsByType<ResurrectPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (existingPanels.Length > 0)
            {
                ResurrectPanel currentRuntime = existingPanels.FirstOrDefault(IsCurrentRuntimePanel);
                if (currentRuntime != null)
                {
                    return currentRuntime;
                }

                foreach (ResurrectPanel panel in existingPanels)
                {
                    if (panel != null && IsRuntimeCreatedPanel(panel))
                    {
                        UnityEngine.Object.Destroy(panel.gameObject);
                    }
                }
            }

            if (!UiManager.IsInitialized)
            {
                Plugin.Logger?.LogWarning("[ResurrectPanelRuntimeFactory] UiManager 尚未初始化。");
                return null;
            }

            CommonButtonWidget confirmTemplate = TryPickButtonTemplate(preferConfirm: true);
            CommonButtonWidget cancelTemplate = TryPickButtonTemplate(preferConfirm: false) ?? confirmTemplate;

            Transform parent = preferredParent ?? UiManager.Instance?.transform;
            if (parent == null)
            {
                Canvas canvas = UnityEngine.Object.FindObjectOfType<Canvas>(true);
                parent = canvas?.transform;
            }

            GameObject root = new GameObject(RuntimeRootName);
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

            GameObject dialogPrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            if (dialogPrefab == null)
            {
                Plugin.Logger?.LogWarning("[ResurrectPanelRuntimeFactory] 未找到 MessageDialog 资源: UI/Dialogs/MessageDialog");
                UnityEngine.Object.Destroy(root);
                return null;
            }

            GameObject frame = UnityEngine.Object.Instantiate(dialogPrefab, root.transform, false);
            frame.name = "ResurrectFrame";

            RectTransform frameRect = frame.GetComponent<RectTransform>();
            if (frameRect != null)
            {
                ConfigureAnchors(frameRect, new Vector2(0.06f, 0.06f), new Vector2(0.94f, 0.94f));
            }

            MessageDialog dialog = frame.GetComponentInChildren<MessageDialog>(true);
            if (dialog == null)
            {
                Plugin.Logger?.LogWarning("[ResurrectPanelRuntimeFactory] MessageDialog 组件为空。");
                UnityEngine.Object.Destroy(root);
                return null;
            }

            TextMeshProUGUI mainText = GetDialogField<TextMeshProUGUI>(dialog, "mainText");
            TextMeshProUGUI subText = GetDialogField<TextMeshProUGUI>(dialog, "subText");
            Button singleConfirm = GetDialogField<Button>(dialog, "singleConfirmButton");
            Button frameConfirm = GetDialogField<Button>(dialog, "confirmButton");
            Button frameCancel = GetDialogField<Button>(dialog, "cancelButton");

            if (mainText != null)
            {
                mainText.text = "治疗";
                mainText.alignment = TextAlignmentOptions.Center;
                mainText.raycastTarget = false;
            }

            if (subText != null)
            {
                subText.text = string.Empty;
                subText.alignment = TextAlignmentOptions.Center;
                subText.raycastTarget = false;
                Color c = subText.color;
                c.a = 0f;
                subText.color = c;
            }

            HideDialogButton(singleConfirm);
            HideDialogButton(frameConfirm);
            HideDialogButton(frameCancel);

            dialog.enabled = false;

            TextMeshProUGUI textTemplate = mainText ?? subText ?? TryPickTextTemplate();
            if (textTemplate == null)
            {
                Plugin.Logger?.LogWarning("[ResurrectPanelRuntimeFactory] 治疗面板文本模板为空。");
                UnityEngine.Object.Destroy(root);
                return null;
            }

            TextMeshProUGUI status = CloneTextOrCreate(textTemplate, root.transform, "Status");
            status.text = "请选择要治疗的玩家";
            status.alignment = TextAlignmentOptions.Center;
            status.fontSize = Mathf.Max(status.fontSize, 22f);
            ConfigureAnchors(status.rectTransform, new Vector2(0.12f, 0.80f), new Vector2(0.88f, 0.88f));

            RectTransform panelRect = TryFindCommonAncestorRect(mainText?.rectTransform, subText?.rectTransform)
                                      ?? frameRect
                                      ?? rootRect;

            RectTransform listContent;
            DeadPlayerEntryWidget entryTemplate;

            if (TryAttachHistoryListWithRecordRow(panelRect, subText?.rectTransform, out ScrollRect listScrollRect, out RectTransform historyListContent, out RecordRow rowTemplate))
            {
                DeadPlayerEntryWidget historyTemplate = CreateEntryTemplateFromRecordRow(historyListContent, rowTemplate);
                if (historyTemplate != null)
                {
                    listScrollRect.gameObject.SetActive(true);
                    rowTemplate?.gameObject.SetActive(false);
                    listContent = historyListContent;
                    entryTemplate = historyTemplate;
                }
                else
                {
                    Plugin.Logger?.LogWarning("[ResurrectPanelRuntimeFactory] RecordRow 模板转换失败，回退到简化列表。");
                    if (!TryCreateFallbackSimpleList(root.transform, textTemplate, out listContent, out entryTemplate))
                    {
                        UnityEngine.Object.Destroy(root);
                        return null;
                    }
                }
            }
            else
            {
                Plugin.Logger?.LogWarning("[ResurrectPanelRuntimeFactory] 无法挂载 HistoryPanel 列表，回退到简化列表。");
                if (!TryCreateFallbackSimpleList(root.transform, textTemplate, out listContent, out entryTemplate))
                {
                    UnityEngine.Object.Destroy(root);
                    return null;
                }
            }

            TextMeshProUGUI costText = CloneTextOrCreate(textTemplate, root.transform, "CostText");
            costText.text = string.Empty;
            costText.alignment = TextAlignmentOptions.Center;
            costText.fontSize = Mathf.Max(costText.fontSize, 18f);
            ConfigureAnchors(costText.rectTransform, new Vector2(0.12f, 0.22f), new Vector2(0.88f, 0.28f));

            TextMeshProUGUI hintText = CloneTextOrCreate(textTemplate, root.transform, "HintText");
            hintText.text = "效果：回复目标 20% 最大生命值";
            hintText.alignment = TextAlignmentOptions.Center;
            hintText.fontSize = Mathf.Max(hintText.fontSize, 16f);
            ConfigureAnchors(hintText.rectTransform, new Vector2(0.12f, 0.18f), new Vector2(0.88f, 0.22f));

            CommonButtonWidget confirmButton = null;
            CommonButtonWidget cancelButton = null;

            if (confirmTemplate != null)
            {
                confirmButton = UnityEngine.Object.Instantiate(confirmTemplate, root.transform, false);
                confirmButton.name = "TreatConfirmButton";
                DisableExtraButtons(confirmButton);
                ConfigureAnchors(confirmButton.GetComponent<RectTransform>(), new Vector2(0.22f, 0.08f), new Vector2(0.48f, 0.16f));
            }

            if (cancelTemplate != null)
            {
                cancelButton = UnityEngine.Object.Instantiate(cancelTemplate, root.transform, false);
                cancelButton.name = "TreatCancelButton";
                DisableExtraButtons(cancelButton);
                ConfigureAnchors(cancelButton.GetComponent<RectTransform>(), new Vector2(0.52f, 0.08f), new Vector2(0.78f, 0.16f));
            }

            if (confirmButton == null && cancelTemplate != null)
            {
                confirmButton = UnityEngine.Object.Instantiate(cancelTemplate, root.transform, false);
                confirmButton.name = "TreatConfirmButton";
                DisableExtraButtons(confirmButton);
                ConfigureAnchors(confirmButton.GetComponent<RectTransform>(), new Vector2(0.22f, 0.08f), new Vector2(0.48f, 0.16f));
            }

            if (cancelButton == null && confirmTemplate != null)
            {
                cancelButton = UnityEngine.Object.Instantiate(confirmTemplate, root.transform, false);
                cancelButton.name = "TreatCancelButton";
                DisableExtraButtons(cancelButton);
                ConfigureAnchors(cancelButton.GetComponent<RectTransform>(), new Vector2(0.52f, 0.08f), new Vector2(0.78f, 0.16f));
            }

            if (confirmButton == null || cancelButton == null)
            {
                Plugin.Logger?.LogWarning("[ResurrectPanelRuntimeFactory] 无法解析确认/取消按钮组件。");
                UnityEngine.Object.Destroy(root);
                return null;
            }

            confirmButton.gameObject.SetActive(true);
            cancelButton.gameObject.SetActive(true);
            SetButtonLabel(confirmButton, "治疗");
            SetButtonLabel(cancelButton, "取消");

            ResurrectPanel panelRuntime = root.AddComponent<ResurrectPanel>();
            panelRuntime.BindRuntimeUi(
                listContent,
                entryTemplate,
                confirmButton,
                cancelButton,
                status,
                costText,
                hintText,
                canvasGroup);

            ResurrectPanelRuntimeMarker marker = root.AddComponent<ResurrectPanelRuntimeMarker>();
            marker.Version = RuntimeUiVersion;

            root.SetActive(true);
            root.SetActive(false);
            return panelRuntime;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogWarning($"[ResurrectPanelRuntimeFactory] 创建运行时治疗面板失败: {ex}");
            return null;
        }
    }

    private static bool TryCreateFallbackSimpleList(Transform root, TextMeshProUGUI textTemplate, out RectTransform listContent, out DeadPlayerEntryWidget entryTemplate)
    {
        listContent = null;
        entryTemplate = null;

        try
        {
            GameObject entriesGo = new GameObject("FallbackEntriesRoot");
            entriesGo.transform.SetParent(root, false);
            RectTransform entriesRect = entriesGo.AddComponent<RectTransform>();
            ConfigureAnchors(entriesRect, new Vector2(0.14f, 0.30f), new Vector2(0.86f, 0.78f));

            VerticalLayoutGroup entriesLayout = entriesGo.AddComponent<VerticalLayoutGroup>();
            entriesLayout.childAlignment = TextAnchor.UpperCenter;
            entriesLayout.childControlWidth = true;
            entriesLayout.childControlHeight = false;
            entriesLayout.childForceExpandWidth = true;
            entriesLayout.childForceExpandHeight = false;
            entriesLayout.spacing = 8f;
            entriesLayout.padding = new RectOffset(0, 0, 0, 0);

            ContentSizeFitter entriesFitter = entriesGo.AddComponent<ContentSizeFitter>();
            entriesFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            DeadPlayerEntryWidget template = CreateSimpleEntryTemplate(entriesRect, textTemplate);
            if (template == null)
            {
                return false;
            }

            listContent = entriesRect;
            entryTemplate = template;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static DeadPlayerEntryWidget CreateSimpleEntryTemplate(RectTransform parent, TextMeshProUGUI textTemplate)
    {
        try
        {
            GameObject entryGo = new GameObject("DeadPlayerEntryTemplate");
            entryGo.transform.SetParent(parent, false);

            RectTransform entryRect = entryGo.AddComponent<RectTransform>();
            entryRect.sizeDelta = new Vector2(0f, 72f);

            Image bg = entryGo.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            Button button = entryGo.AddComponent<Button>();
            button.targetGraphic = bg;

            DeadPlayerEntryWidget widget = entryGo.AddComponent<DeadPlayerEntryWidget>();
            widget.button = button;
            Traverse.Create(widget).Field("root").SetValue(entryGo.transform);

            TextMeshProUGUI nameText = CloneTextOrCreate(textTemplate, entryGo.transform, "PlayerName");
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.fontSize = Mathf.Max(nameText.fontSize, 18f);
            ConfigureAnchors(nameText.rectTransform, new Vector2(0.04f, 0.52f), new Vector2(0.96f, 0.96f));

            TextMeshProUGUI infoText = CloneTextOrCreate(textTemplate, entryGo.transform, "PlayerInfo");
            infoText.alignment = TextAlignmentOptions.Left;
            infoText.fontSize = Mathf.Max(infoText.fontSize, 14f);
            ConfigureAnchors(infoText.rectTransform, new Vector2(0.04f, 0.10f), new Vector2(0.96f, 0.56f));

            GameObject indicatorGo = new GameObject("SelectedIndicator");
            indicatorGo.transform.SetParent(entryGo.transform, false);
            RectTransform indicatorRect = indicatorGo.AddComponent<RectTransform>();
            ConfigureAnchors(indicatorRect, new Vector2(0.0f, 0f), new Vector2(0.02f, 1f));
            Image indicator = indicatorGo.AddComponent<Image>();
            indicator.color = new Color(0.4f, 0.8f, 1f, 0.95f);
            indicatorGo.SetActive(false);

            Traverse.Create(widget).Field("playerName").SetValue(nameText);
            Traverse.Create(widget).Field("playerInfo").SetValue(infoText);
            Traverse.Create(widget).Field("selectedIndicator").SetValue(indicator);

            entryGo.SetActive(false);
            return widget;
        }
        catch
        {
            return null;
        }
    }

    private static DeadPlayerEntryWidget CreateEntryTemplateFromRecordRow(Transform parent, RecordRow rowTemplate)
    {
        try
        {
            if (parent == null || rowTemplate == null)
            {
                return null;
            }

            RecordRow templateRow = UnityEngine.Object.Instantiate(rowTemplate, parent, false);
            templateRow.name = "DeadPlayerEntryTemplate";

            TextMeshProUGUI nameText = GetPrivateFieldValue<TextMeshProUGUI>(templateRow, "gameResultText");
            TextMeshProUGUI infoText = GetPrivateFieldValue<TextMeshProUGUI>(templateRow, "difficultyText");
            TextMeshProUGUI timestampText = GetPrivateFieldValue<TextMeshProUGUI>(templateRow, "timestampText");
            Image avatarImage = GetPrivateFieldValue<Image>(templateRow, "avatarImage");
            Image exhibitIcon = GetPrivateFieldValue<Image>(templateRow, "exhibitIcon");
            GameObject selectedGo = GetPrivateFieldValue<GameObject>(templateRow, "selectedIndicator");
            Image selectedIndicator = selectedGo?.GetComponent<Image>();

            avatarImage?.gameObject.SetActive(false);
            exhibitIcon?.gameObject.SetActive(false);
            timestampText?.gameObject.SetActive(false);

            Button button = templateRow.GetComponent<Button>();
            if (button == null)
            {
                button = templateRow.gameObject.AddComponent<Button>();
            }

            Image cover = GetPrivateFieldValue<Image>(templateRow, "cover");
            if (cover != null)
            {
                button.targetGraphic = cover;
            }

            DeadPlayerEntryWidget widget = templateRow.gameObject.GetComponent<DeadPlayerEntryWidget>();
            if (widget == null)
            {
                widget = templateRow.gameObject.AddComponent<DeadPlayerEntryWidget>();
            }

            widget.button = button;

            RectTransform rowRoot = GetPrivateFieldValue<RectTransform>(templateRow, "root") ?? templateRow.GetComponent<RectTransform>();
            Traverse.Create(widget).Field("root").SetValue(rowRoot);
            Traverse.Create(widget).Field("playerName").SetValue(nameText);
            Traverse.Create(widget).Field("playerInfo").SetValue(infoText);
            Traverse.Create(widget).Field("selectedIndicator").SetValue(selectedIndicator);

            templateRow.gameObject.SetActive(false);
            return widget;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsRuntimeCreatedPanel(ResurrectPanel panel)
    {
        if (panel == null)
        {
            return false;
        }

        if (panel.GetComponent<ResurrectPanelRuntimeMarker>() != null)
        {
            return true;
        }

        return string.Equals(panel.gameObject.name, RuntimeRootName, StringComparison.Ordinal);
    }

    private static bool IsCurrentRuntimePanel(ResurrectPanel panel)
    {
        if (panel == null)
        {
            return false;
        }

        ResurrectPanelRuntimeMarker marker = panel.GetComponent<ResurrectPanelRuntimeMarker>();
        if (marker == null)
        {
            return false;
        }

        return string.Equals(marker.Version, RuntimeUiVersion, StringComparison.Ordinal);
    }

    private static bool TryAttachHistoryListWithRecordRow(
        RectTransform dialogPanelRect,
        RectTransform placeholderRect,
        out ScrollRect listScrollRect,
        out RectTransform listContent,
        out RecordRow recordRowTemplate)
    {
        listScrollRect = null;
        listContent = null;
        recordRowTemplate = null;

        GameObject historyInstance = null;
        try
        {
            GameObject historyPrefab = Resources.Load<GameObject>("UI/Panels/HistoryPanel");
            if (historyPrefab == null)
            {
                HistoryPanel sceneHistory = UnityEngine.Object.FindObjectsByType<HistoryPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault();

                if (sceneHistory != null)
                {
                    historyPrefab = sceneHistory.gameObject;
                }
            }

            if (historyPrefab == null)
            {
                Plugin.Logger?.LogWarning("[ResurrectPanelRuntimeFactory] 未找到 HistoryPanel 资源或场景实例。");
                return false;
            }

            historyInstance = UnityEngine.Object.Instantiate(historyPrefab);
            historyInstance.SetActive(false);

            HistoryPanel historyPanel = historyInstance.GetComponentInChildren<HistoryPanel>(true);
            if (historyPanel == null)
            {
                Plugin.Logger?.LogWarning("[ResurrectPanelRuntimeFactory] HistoryPanel 组件为空。");
                return false;
            }

            listScrollRect = GetPrivateFieldValue<ScrollRect>(historyPanel, "listScrollRect");
            listContent = GetPrivateFieldValue<RectTransform>(historyPanel, "listContent");
            recordRowTemplate = GetPrivateFieldValue<RecordRow>(historyPanel, "recordRowTemplate");

            if (listScrollRect == null || listContent == null || recordRowTemplate == null)
            {
                Plugin.Logger?.LogWarning("[ResurrectPanelRuntimeFactory] HistoryPanel 私有字段解析失败(listScrollRect/listContent/recordRowTemplate)。");
                return false;
            }

            listScrollRect.transform.SetParent(dialogPanelRect, false);

            RectTransform scrollRect = listScrollRect.GetComponent<RectTransform>();
            if (scrollRect != null)
            {
                if (placeholderRect != null)
                {
                    CopyRectTransform(scrollRect, placeholderRect);
                }
                else
                {
                    scrollRect.anchorMin = new Vector2(0.06f, 0.20f);
                    scrollRect.anchorMax = new Vector2(0.94f, 0.78f);
                    scrollRect.offsetMin = Vector2.zero;
                    scrollRect.offsetMax = Vector2.zero;
                }
            }

            UnityEngine.Object.Destroy(historyInstance);
            historyInstance = null;
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            try
            {
                if (historyInstance != null)
                {
                    UnityEngine.Object.Destroy(historyInstance);
                }
            }
            catch
            {
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

                    CommonButtonWidget widget = button?.GetComponentInParent<CommonButtonWidget>(true);
                    if (widget != null)
                    {
                        return widget;
                    }
                }
            }

            return UnityEngine.Object.FindObjectsByType<CommonButtonWidget>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(w => w != null && !IsUnderRuntimeResurrectPanel(w.transform));
        }
        catch
        {
            return null;
        }
    }

    private static TextMeshProUGUI TryPickTextTemplate()
    {
        try
        {
            GameObject dialogPrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            if (dialogPrefab != null)
            {
                TextMeshProUGUI prefabTmp = dialogPrefab.GetComponentInChildren<TextMeshProUGUI>(true);
                if (prefabTmp != null)
                {
                    return prefabTmp;
                }
            }

            return UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(t => t != null && !IsUnderRuntimeResurrectPanel(t.transform));
        }
        catch
        {
            return null;
        }
    }

    private static bool IsUnderRuntimeResurrectPanel(Transform transform)
    {
        Transform current = transform;
        while (current != null)
        {
            if (string.Equals(current.name, RuntimeRootName, StringComparison.Ordinal) || current.GetComponent<ResurrectPanelRuntimeMarker>() != null)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
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

    private static RectTransform TryFindCommonAncestorRect(RectTransform a, RectTransform b)
    {
        if (a == null || b == null)
        {
            return null;
        }

        System.Collections.Generic.HashSet<Transform> ancestors = new System.Collections.Generic.HashSet<Transform>();
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

    private static T GetPrivateFieldValue<T>(object target, string fieldName) where T : class
    {
        if (target == null || string.IsNullOrWhiteSpace(fieldName))
        {
            return null;
        }

        Type type = target.GetType();
        while (type != null)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                return field.GetValue(target) as T;
            }

            type = type.BaseType;
        }

        return null;
    }

    private static TextMeshProUGUI CloneTextOrCreate(TextMeshProUGUI template, Transform parent, string name)
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

    private static void ConfigureAnchors(RectTransform rect, Vector2 min, Vector2 max)
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

    private static void SetButtonLabel(CommonButtonWidget buttonWidget, string label)
    {
        TextMeshProUGUI tmp = buttonWidget?.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp == null)
        {
            return;
        }

        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    private static void DisableExtraButtons(CommonButtonWidget widget)
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

    private static T GetDialogField<T>(MessageDialog dialog, string fieldName) where T : class
    {
        if (dialog == null || string.IsNullOrWhiteSpace(fieldName))
        {
            return null;
        }

        FieldInfo field = typeof(MessageDialog).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
        {
            return null;
        }

        return field.GetValue(dialog) as T;
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

    internal sealed class ResurrectPanelRuntimeMarker : MonoBehaviour
    {
        public string Version;
    }
}