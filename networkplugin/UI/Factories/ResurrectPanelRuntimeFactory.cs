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
    private const string RuntimeUiVersion = "2026-04-19-runtime-selection-list-v17";

    internal static ResurrectPanel GetOrCreate(Transform preferredParent)
    {
        try
        {
            ResurrectPanel[] existingPanels = UnityEngine.Object.FindObjectsByType<ResurrectPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (existingPanels.Length > 0)
            {
                ResurrectPanel currentRuntime = existingPanels.FirstOrDefault(panel =>
                {
                    if (panel == null) return false;
                    ResurrectPanelRuntimeMarker marker = panel.GetComponent<ResurrectPanelRuntimeMarker>();
                    if (marker == null) return false;
                    return string.Equals(marker.Version, RuntimeUiVersion, StringComparison.Ordinal);
                });
                if (currentRuntime != null)
                {
                    return currentRuntime;
                }

                foreach (ResurrectPanel panel in existingPanels)
                {
                    if (panel != null)
                    {
                        bool isRuntimeCreated = panel.GetComponent<ResurrectPanelRuntimeMarker>() != null
                            || string.Equals(panel.gameObject.name, RuntimeRootName, StringComparison.Ordinal);
                        if (isRuntimeCreated)
                        {
                            UnityEngine.Object.Destroy(panel.gameObject);
                        }
                    }
                }
            }

            if (!UiManager.IsInitialized)
            {
                Plugin.Logger?.LogWarning("[ResurrectPanelRuntimeFactory] UiManager 尚未初始化。");
                return null;
            }

            GapSharedPanelTemplateFactory.GapRuntimePanelTemplate scaffold = GapSharedPanelTemplateFactory.Create(
                preferredParent,
                RuntimeRootName,
                "治疗",
                "请选择要治疗的玩家",
                "治疗",
                "取消");
            if (scaffold == null)
            {
                Plugin.Logger?.LogWarning("[ResurrectPanelRuntimeFactory] 无法创建共享治疗模板。");
                return null;
            }

            RectTransform listContent;
            TextMeshProUGUI textTemplate = scaffold.TextTemplate;

            // 按 TradePartnerPicker 结构：列表挂在 dialog panelRect，滚动区域优先复用 subText 的 Rect。
            Transform framePanelXform = scaffold.Root.transform.Find(RuntimeRootName + "_Frame")
                ?? (Transform)scaffold.ContentRoot;

            RectTransform frameRect = framePanelXform as RectTransform;
            if (frameRect != null)
            {
                frameRect.anchorMin = Vector2.zero;
                frameRect.anchorMax = Vector2.one;
                frameRect.offsetMin = Vector2.zero;
                frameRect.offsetMax = Vector2.zero;
            }

            MessageDialog dialog = framePanelXform != null
                ? framePanelXform.GetComponentInChildren<MessageDialog>(true)
                : null;

            TextMeshProUGUI frameMainText = GetDialogField<TextMeshProUGUI>(dialog, "mainText");
            TextMeshProUGUI frameSubText = GetDialogField<TextMeshProUGUI>(dialog, "subText");
            Button frameCancel = GetDialogField<Button>(dialog, "cancelButton");

            RectTransform subTextRect = frameSubText?.rectTransform;
            if (frameMainText != null)
            {
                frameMainText.text = "请选择要治疗的玩家";
                frameMainText.alignment = TextAlignmentOptions.Center;
                frameMainText.raycastTarget = false;
                Color c = frameMainText.color;
                c.a = 1f;
                frameMainText.color = c;
                frameMainText.gameObject.SetActive(false);
                textTemplate = frameMainText;
            }

            if (frameSubText != null)
            {
                frameSubText.text = string.Empty;
                frameSubText.raycastTarget = false;
                frameSubText.alignment = TextAlignmentOptions.Center;
                Color c = frameSubText.color;
                c.a = 0f;
                frameSubText.color = c;
                frameSubText.gameObject.SetActive(true);
            }

            if (scaffold.TitleText != null)
            {
                scaffold.TitleText.gameObject.SetActive(false);
            }

            if (scaffold.StatusText != null)
            {
                scaffold.StatusText.gameObject.SetActive(false);
            }

            RectTransform panelRect;
            {
                RectTransform __a = frameMainText?.rectTransform;
                RectTransform __b = frameCancel?.GetComponent<RectTransform>();
                RectTransform __result = null;
                if (__a != null && __b != null)
                {
                    System.Collections.Generic.HashSet<Transform> ancestors = new System.Collections.Generic.HashSet<Transform>();
                    Transform current = __a;
                    while (current != null)
                    {
                        ancestors.Add(current);
                        current = current.parent;
                    }
                    current = __b;
                    while (current != null)
                    {
                        if (ancestors.Contains(current))
                        {
                            __result = current as RectTransform;
                            break;
                        }
                        current = current.parent;
                    }
                }
                panelRect = __result;
            }
            panelRect = panelRect
                ?? frameMainText?.rectTransform.parent as RectTransform
                ?? frameRect
                ?? (RectTransform)scaffold.ContentRoot;

            listContent = null;

            // 按 TradePanel.PartnerPicker 模式手动创建 ScrollRect，完全控制锚点和布局
            {
                GameObject scrollGo = new GameObject("PlayersScroll");
                scrollGo.transform.SetParent(panelRect, false);
                scrollGo.transform.SetAsLastSibling();

                var scrollRt = scrollGo.AddComponent<RectTransform>();
                scrollRt.anchorMin = new Vector2(0.20f, 0.35f);
                scrollRt.anchorMax = new Vector2(0.80f, 0.65f);
                scrollRt.offsetMin = Vector2.zero;
                scrollRt.offsetMax = Vector2.zero;

                var scrollImg = scrollGo.AddComponent<Image>();
                scrollImg.color = new Color(0f, 0f, 0f, 0f);
                scrollImg.raycastTarget = true;

                var scrollRect = scrollGo.AddComponent<ScrollRect>();
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
                scrollRect.movementType = ScrollRect.MovementType.Clamped;
                scrollRect.scrollSensitivity = 24f;

                GameObject viewport = new GameObject("Viewport");
                viewport.transform.SetParent(scrollGo.transform, false);
                var viewportRt = viewport.AddComponent<RectTransform>();
                viewportRt.anchorMin = Vector2.zero;
                viewportRt.anchorMax = Vector2.one;
                viewportRt.offsetMin = Vector2.zero;
                viewportRt.offsetMax = Vector2.zero;
                viewport.AddComponent<RectMask2D>();

                GameObject contentGo = new GameObject("Content");
                contentGo.transform.SetParent(viewport.transform, false);
                var contentRt = contentGo.AddComponent<RectTransform>();
                contentRt.anchorMin = new Vector2(0f, 1f);
                contentRt.anchorMax = new Vector2(1f, 1f);
                contentRt.pivot = new Vector2(0.5f, 1f);
                contentRt.sizeDelta = new Vector2(0f, 0f);

                var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
                vlg.childAlignment = TextAnchor.UpperCenter;
                vlg.spacing = 10f;
                vlg.padding = new RectOffset(10, 10, 4, 4);
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;

                var csf = contentGo.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                scrollRect.viewport = viewportRt;
                scrollRect.content = contentRt;

                listContent = contentRt;
            }

            if (frameCancel != null)
            {
                frameCancel.transform.SetAsLastSibling();
            }

            // 标题放在滚动区域上方，明确使用白色确保可见
            TextMeshProUGUI runtimeStatusText = GapSharedPanelTemplateFactory.CloneTextOrCreate(textTemplate, panelRect, "StatusText");
            runtimeStatusText.text = "请选择要治疗的玩家";
            runtimeStatusText.alignment = TextAlignmentOptions.Center;
            runtimeStatusText.fontSize = Mathf.Max(28f, runtimeStatusText.fontSize * 0.80f);
            runtimeStatusText.color = new Color(1f, 1f, 1f, 1f);
            GapSharedPanelTemplateFactory.ConfigureAnchors(runtimeStatusText.rectTransform, new Vector2(0.20f, 0.67f), new Vector2(0.80f, 0.73f));
            runtimeStatusText.transform.SetAsLastSibling();

            if (frameCancel != null)
            {
                frameCancel.transform.SetAsLastSibling();
            }

            ResurrectPanel panelRuntime = scaffold.Root.AddComponent<ResurrectPanel>();
            panelRuntime.BindRuntimeUi(
                listContent,
                textTemplate,
                scaffold.ConfirmButton,
                scaffold.CancelButton,
                runtimeStatusText,
                scaffold.CanvasGroup);

            ResurrectPanelRuntimeMarker marker = scaffold.Root.AddComponent<ResurrectPanelRuntimeMarker>();
            marker.Version = RuntimeUiVersion;

            scaffold.Root.SetActive(true);
            scaffold.Root.SetActive(false);
            return panelRuntime;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogWarning($"[ResurrectPanelRuntimeFactory] 创建运行时治疗面板失败: {ex}");
            return null;
        }
    }

    private static bool TryCreateFallbackSimpleList(Transform root, TextMeshProUGUI textTemplate, CommonButtonWidget buttonTemplate, out RectTransform listContent, out DeadPlayerEntryWidget entryTemplate)
    {
        listContent = null;
        entryTemplate = null;

        try
        {
            GameObject entriesGo = new GameObject("FallbackEntriesRoot");
            entriesGo.transform.SetParent(root, false);
            RectTransform entriesRect = entriesGo.AddComponent<RectTransform>();
            ConfigureAnchors(entriesRect, new Vector2(0.06f, 0.24f), new Vector2(0.94f, 0.98f));

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

            DeadPlayerEntryWidget template = CreateSimpleEntryTemplate(entriesRect, textTemplate, buttonTemplate);
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

    private static DeadPlayerEntryWidget CreateSimpleEntryTemplate(RectTransform parent, TextMeshProUGUI textTemplate, CommonButtonWidget buttonTemplate)
    {
        try
        {
            GameObject entryGo;
            Button button;
            if (buttonTemplate != null)
            {
                // 直接创建对象，避免克隆模板上的意外组件
                entryGo = new GameObject("DeadPlayerEntryTemplate");
                entryGo.transform.SetParent(parent, false);
                entryGo.transform.localScale = Vector3.one;

                Image bg = entryGo.AddComponent<Image>();
                if (buttonTemplate.button != null && buttonTemplate.button.targetGraphic is Image templateImg)
                {
                    bg.sprite = templateImg.sprite;
                    bg.type = templateImg.type;
                    bg.color = templateImg.color;
                }
                else
                {
                    bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                }

                button = entryGo.AddComponent<Button>();
                button.targetGraphic = bg;
            }
            else
            {
                entryGo = new GameObject("DeadPlayerEntryTemplate");
                entryGo.transform.SetParent(parent, false);

                Image bg = entryGo.AddComponent<Image>();
                bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

                button = entryGo.AddComponent<Button>();
                button.targetGraphic = bg;
            }

            RectTransform entryRect = entryGo.GetComponent<RectTransform>();
            if (entryRect == null)
            {
                entryRect = entryGo.AddComponent<RectTransform>();
            }

            entryRect.sizeDelta = new Vector2(0f, 72f);
            if (button == null)
            {
                return null;
            }

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

            // 直接创建对象，避免克隆模板上的意外组件
            GameObject rowGo = new GameObject("DeadPlayerEntryTemplate");
            rowGo.transform.SetParent(parent, false);
            rowGo.transform.localScale = Vector3.one;

            var templateRow = rowGo.AddComponent<RecordRow>();
            var rowRt = rowGo.AddComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0f, 0.5f);
            rowRt.anchorMax = new Vector2(1f, 0.5f);
            rowRt.pivot = new Vector2(0.5f, 0.5f);
            rowRt.sizeDelta = new Vector2(0f, 72f);

            // cover（全屏遮罩，用于 hover 效果）
            GameObject coverGo = new GameObject("Cover");
            coverGo.transform.SetParent(rowGo.transform, false);
            coverGo.transform.localScale = Vector3.one;
            var cover = coverGo.AddComponent<Image>();
            cover.color = new Color(0f, 0f, 0f, 0.5f);
            var coverRt = coverGo.AddComponent<RectTransform>();
            coverRt.anchorMin = Vector2.zero;
            coverRt.anchorMax = Vector2.one;
            coverRt.offsetMin = Vector2.zero;
            coverRt.offsetMax = Vector2.zero;

            // nameText (gameResultText)
            GameObject nameGo = new GameObject("Name");
            nameGo.transform.SetParent(rowGo.transform, false);
            nameGo.transform.localScale = Vector3.one;
            var nameText = nameGo.AddComponent<TextMeshProUGUI>();
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.raycastTarget = false;
            var nameRt = nameGo.AddComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0.02f, 0.3f);
            nameRt.anchorMax = new Vector2(0.55f, 0.7f);
            nameRt.offsetMin = Vector2.zero;
            nameRt.offsetMax = Vector2.zero;

            // infoText (difficultyText)
            GameObject infoGo = new GameObject("Info");
            infoGo.transform.SetParent(rowGo.transform, false);
            infoGo.transform.localScale = Vector3.one;
            var infoText = infoGo.AddComponent<TextMeshProUGUI>();
            infoText.alignment = TextAlignmentOptions.Right;
            infoText.raycastTarget = false;
            var infoRt = infoGo.AddComponent<RectTransform>();
            infoRt.anchorMin = new Vector2(0.6f, 0.3f);
            infoRt.anchorMax = new Vector2(0.98f, 0.7f);
            infoRt.offsetMin = Vector2.zero;
            infoRt.offsetMax = Vector2.zero;

            // selectedIndicator
            GameObject selectedGo = new GameObject("SelectedIndicator");
            selectedGo.transform.SetParent(rowGo.transform, false);
            selectedGo.transform.localScale = Vector3.one;
            var selectedImg = selectedGo.AddComponent<Image>();
            selectedImg.color = new Color(1f, 1f, 1f, 0.3f);
            var selectedRt = selectedGo.AddComponent<RectTransform>();
            selectedRt.anchorMin = Vector2.zero;
            selectedRt.anchorMax = Vector2.one;
            selectedRt.offsetMin = Vector2.zero;
            selectedRt.offsetMax = Vector2.zero;
            selectedGo.SetActive(false);

            // avatarImage（隐藏）
            GameObject avatarGo = new GameObject("Avatar");
            avatarGo.transform.SetParent(rowGo.transform, false);
            avatarGo.transform.localScale = Vector3.one;
            var avatarImage = avatarGo.AddComponent<Image>();
            avatarImage.preserveAspect = true;
            var avatarRt = avatarGo.AddComponent<RectTransform>();
            avatarRt.anchorMin = new Vector2(0f, 0.5f);
            avatarRt.anchorMax = new Vector2(0f, 0.5f);
            avatarRt.pivot = new Vector2(0f, 0.5f);
            avatarRt.sizeDelta = new Vector2(48f, 48f);
            avatarGo.SetActive(false);

            // exhibitIcon（隐藏）
            GameObject exIconGo = new GameObject("ExhibitIcon");
            exIconGo.transform.SetParent(rowGo.transform, false);
            exIconGo.transform.localScale = Vector3.one;
            var exhibitIcon = exIconGo.AddComponent<Image>();
            exhibitIcon.preserveAspect = true;
            var exIconRt = exIconGo.AddComponent<RectTransform>();
            exIconRt.anchorMin = new Vector2(1f, 0.5f);
            exIconRt.anchorMax = new Vector2(1f, 0.5f);
            exIconRt.pivot = new Vector2(1f, 0.5f);
            exIconRt.sizeDelta = new Vector2(32f, 32f);
            exIconGo.SetActive(false);

            // timestampText（隐藏）
            GameObject tsGo = new GameObject("Timestamp");
            tsGo.transform.SetParent(rowGo.transform, false);
            tsGo.transform.localScale = Vector3.one;
            var timestampText = tsGo.AddComponent<TextMeshProUGUI>();
            timestampText.alignment = TextAlignmentOptions.Right;
            timestampText.raycastTarget = false;
            var tsRt = tsGo.AddComponent<RectTransform>();
            tsRt.anchorMin = new Vector2(0.7f, 0f);
            tsRt.anchorMax = new Vector2(1f, 0.3f);
            tsRt.offsetMin = Vector2.zero;
            tsRt.offsetMax = Vector2.zero;
            tsGo.SetActive(false);

            // Button
            Button button = rowGo.AddComponent<Button>();
            button.targetGraphic = cover;

            // 使用 Traverse 设置私有字段
            var rowTraverse = Traverse.Create(templateRow);
            rowTraverse.Field("cover").SetValue(cover);
            rowTraverse.Field("root").SetValue(rowRt);
            rowTraverse.Field("gameResultText").SetValue(nameText);
            rowTraverse.Field("difficultyText").SetValue(infoText);
            rowTraverse.Field("timestampText").SetValue(timestampText);
            rowTraverse.Field("avatarImage").SetValue(avatarImage);
            rowTraverse.Field("exhibitIcon").SetValue(exhibitIcon);
            rowTraverse.Field("selectedIndicator").SetValue(selectedGo);

            DeadPlayerEntryWidget widget = rowGo.GetComponent<DeadPlayerEntryWidget>();
            if (widget == null)
            {
                widget = rowGo.AddComponent<DeadPlayerEntryWidget>();
            }

            widget.button = button;
            Traverse.Create(widget).Field("root").SetValue(rowRt);
            Traverse.Create(widget).Field("playerName").SetValue(nameText);
            Traverse.Create(widget).Field("playerInfo").SetValue(infoText);
            Traverse.Create(widget).Field("selectedIndicator").SetValue(selectedImg);

            rowGo.SetActive(false);
            return widget;
        }
        catch
        {
            return null;
        }
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
