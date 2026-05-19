using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.UI.Factories;
using NetworkPlugin.UI.Panels;
using NetworkPlugin.UI.Payloads;
using NetworkPlugin.UI.Rules;
using NetworkPlugin.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Dialogs;

// 布局工具方法 —— 与 TradeDetailDialog 核心逻辑解耦
public sealed partial class TradeDetailDialog
{
    #region 布局工具

    private static T GetPrivateFieldValue<T>(object target, string fieldName) where T : class
    {
        try
        {
            if (target == null || string.IsNullOrWhiteSpace(fieldName))
            {
                return null;
            }

            var t = target.GetType();
            while (t != null)
            {
                var fi = t.GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (fi != null)
                {
                    return fi.GetValue(target) as T;
                }

                t = t.BaseType;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private void SetRect(RectTransform rt, float minX, float minY, float maxX, float maxY)
    {
        rt.anchorMin = new Vector2(minX, minY);
        rt.anchorMax = new Vector2(maxX, maxY);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void DisableExtraButtons(CommonButtonWidget widget)
    {
        try
        {
            if (widget == null)
            {
                return;
            }

            Button keep = widget.button;
            var buttons = widget.GetComponentsInChildren<Button>(true);
            if (buttons == null || buttons.Length <= 1)
            {
                return;
            }

            foreach (var b in buttons)
            {
                if (b == null || b == keep)
                {
                    continue;
                }

                b.enabled = false;
                b.interactable = false;
            }
        }
        catch
        {
            // ignored
        }
    }

    private void DisableTooltipBehaviours(GameObject root)
    {
        try
        {
            if (root == null)
            {
                return;
            }

            foreach (var behaviour in root.GetComponentsInChildren<Behaviour>(true))
            {
                if (behaviour == null)
                {
                    continue;
                }

                var n = behaviour.GetType().Name;
                if (n != null && n.IndexOf("Tooltip", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    behaviour.enabled = false;
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    #endregion

    #region 内部类型

    private readonly struct ApplyingStateScope : IDisposable
    {
        private readonly TradeDetailDialog _d;
        private readonly bool _prev;

        public ApplyingStateScope(TradeDetailDialog d)
        {
            _d = d;
            _prev = d._isApplyingState;
            d._isApplyingState = true;
        }

        public void Dispose()
        {
            if (_d != null) _d._isApplyingState = _prev;
        }
    }

    #endregion
}
