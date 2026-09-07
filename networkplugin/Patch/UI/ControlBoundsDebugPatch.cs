using System.Collections.Generic;
using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation.Units;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.Patch.UI;

[HarmonyPatch(typeof(GameDirector), "Update")]
public static class ControlBoundsDebugPatch
{
    private static bool _lastEnabledState;
    private static float _lastScanTime;
    private const float ScanInterval = 2f;
    private static readonly HashSet<Graphic> _trackedGraphics = new();

    [HarmonyPostfix]
    public static void Postfix()
    {
        bool currentEnabled = Plugin.ConfigManager?.DebugShowControlBounds?.Value ?? false;

        if (currentEnabled != _lastEnabledState)
        {
            if (!currentEnabled)
            {
                ClearAllOutlines();
            }
            _lastEnabledState = currentEnabled;
        }

        if (!currentEnabled)
        {
            return;
        }

        float now = Time.realtimeSinceStartup;
        if (now - _lastScanTime < ScanInterval)
        {
            return;
        }
        _lastScanTime = now;

        ScanAndAddOutlines();
    }

    private static void ScanAndAddOutlines()
    {
        Graphic[] allGraphics = Object.FindObjectsOfType<Graphic>(true);
        foreach (Graphic graphic in allGraphics)
        {
            if (graphic == null || graphic.gameObject == null)
            {
                continue;
            }

            if (_trackedGraphics.Contains(graphic))
            {
                continue;
            }

            if (graphic.gameObject.GetComponent<Outline>() != null)
            {
                continue;
            }

            Outline outline = graphic.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.red;
            outline.effectDistance = new Vector2(5, 5);
            _trackedGraphics.Add(graphic);
        }
    }

    private static void ClearAllOutlines()
    {
        foreach (Graphic graphic in _trackedGraphics)
        {
            if (graphic == null || graphic.gameObject == null)
            {
                continue;
            }

            Outline outline = graphic.gameObject.GetComponent<Outline>();
            if (outline != null)
            {
                Object.Destroy(outline);
            }
        }

        _trackedGraphics.Clear();
    }
}
