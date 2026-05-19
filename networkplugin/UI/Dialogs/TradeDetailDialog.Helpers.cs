using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LBoL.Presentation.UI.Dialogs;
using System.Collections.Generic;

namespace NetworkPlugin.UI.Dialogs;

// 纯静态辅助方法 —— 与 TradeDetailDialog 实例状态完全解耦
public sealed partial class TradeDetailDialog
{
    #region UI 辅助方法

    private static T GetDialogField<T>(MessageDialog dialog, string fieldName) where T : class
    {
        if (dialog == null || string.IsNullOrWhiteSpace(fieldName))
        {
            return null;
        }

        var fi = typeof(MessageDialog).GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (fi == null)
        {
            return null;
        }

        return fi.GetValue(dialog) as T;
    }

    private static void SetButtonLabel(Button button, string label)
    {
        if (button == null)
        {
            return;
        }

        var tmp = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp != null)
        {
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
        }
    }

    private static RectTransform TryFindCommonAncestorRect(RectTransform a, RectTransform b)
    {
        if (a == null || b == null)
        {
            return null;
        }

        HashSet<Transform> ancestors = new HashSet<Transform>();
        Transform t = a;
        while (t != null)
        {
            ancestors.Add(t);
            t = t.parent;
        }

        Transform u = b;
        while (u != null)
        {
            if (ancestors.Contains(u))
            {
                return u as RectTransform;
            }

            u = u.parent;
        }

        return null;
    }

    #endregion
}
