using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.Units;
using UnityEngine;

namespace NetworkPlugin.Patch.UI;

public static partial class OtherPlayersOverlayPatch
{
    private static int _lastSceneBindingSignature;
    private static int _mapIconRefreshSkipCounter;
    private const int MapIconRefreshFrameInterval = 10;

    [HarmonyPatch(typeof(GameDirector), nameof(GameDirector.EnterBattle))]
    [HarmonyPostfix]
    private static void GameDirector_EnterBattle_Postfix()
    {
        ResetSceneBoundBindings("GameDirector.EnterBattle");
    }

    [HarmonyPatch(typeof(GameDirector), nameof(GameDirector.LeaveBattle))]
    [HarmonyPostfix]
    private static void GameDirector_LeaveBattle_Postfix()
    {
        ResetSceneBoundBindings("GameDirector.LeaveBattle");
    }

    [HarmonyPatch(typeof(GameDirector), nameof(GameDirector.ClearAll))]
    [HarmonyPostfix]
    private static void GameDirector_ClearAll_Postfix()
    {
        ResetSceneBoundBindings("GameDirector.ClearAll");
    }

    [HarmonyPatch(typeof(MainMenuPanel), "Awake")]
    [HarmonyPostfix]
    private static void MainMenuPanel_Awake_ResetOverlay_Postfix()
    {
        ResetSceneBoundBindings("MainMenuPanel.Awake");
    }

    [HarmonyPatch(typeof(MainMenuPanel), "RefreshProfile")]
    [HarmonyPostfix]
    private static void MainMenuPanel_RefreshProfile_ResetOverlay_Postfix()
    {
        ResetSceneBoundBindings("MainMenuPanel.RefreshProfile");
    }

    [HarmonyPatch(typeof(MapPanel), "Update")]
    [HarmonyPostfix]
    private static void MapPanel_Update_RefreshRemoteIcons_Postfix(MapPanel __instance)
    {
        try
        {
            _mapIconRefreshSkipCounter++;
            if (_mapIconRefreshSkipCounter < MapIconRefreshFrameInterval)
                return;
            _mapIconRefreshSkipCounter = 0;

            if (__instance != null && __instance.IsVisible)
            {
                UpdateMapIcons(__instance);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void EnsureSceneBoundBindingsCurrent()
    {
        int currentSignature = ComputeSceneBindingSignature();
        if (_lastSceneBindingSignature != 0 && _lastSceneBindingSignature != currentSignature)
        {
            ResetSceneBoundBindings($"SceneSignatureChanged:{_lastSceneBindingSignature}->{currentSignature}");
        }

        _lastSceneBindingSignature = currentSignature;
    }

    private static int ComputeSceneBindingSignature()
    {
        unchecked
        {
            int hash = 17;
            GameDirector director = Singleton<GameDirector>.Instance;
            hash = hash * 31 + GetUnityObjectSignature(director);
            hash = hash * 31 + GetUnityObjectSignature(director?.PlayerUnitView);
            hash = hash * 31 + GetUnityObjectSignature(TryGetGameDirectorTransform("unitRoot"));
            hash = hash * 31 + GetUnityObjectSignature(TryGetGameDirectorTransform("playerRoot"));
            hash = hash * 31 + GetUnityObjectSignature(UiManager.Instance);
            hash = hash * 31 + GetUnityObjectSignature(TryGetUiLayerTransform("topLayer"));
            hash = hash * 31 + GetUnityObjectSignature(TryGetUiLayerTransform("topmostLayer"));
            hash = hash * 31 + (IsBattleOverlayActive() ? 1 : 0);
            return hash;
        }
    }

    private static int GetUnityObjectSignature(Object obj)
    {
        return obj != null ? obj.GetInstanceID() : 0;
    }

    private static void ResetSceneBoundBindings(string reason)
    {
        try
        {
            LogOverlayDebug($"ResetSceneBoundBindings: reason={reason}", force: true);

            if (_ui != null)
            {
                if (_ui.Root != null)
                {
                    Object.Destroy(_ui.Root);
                }

                if (_ui.AvatarTemplate != null)
                {
                    Object.Destroy(_ui.AvatarTemplate);
                }

                if (_ui.HealthBarTemplate != null)
                {
                    Object.Destroy(_ui.HealthBarTemplate);
                }

                _ui = null;
            }

            ClearRemoteCharacters();
            ClearMapIcons();
            ResetMapIconLayoutCache();

            _defaultFont = null;
            _missingFontWarningLogged = false;
            _lastOverlayDebugSummary = null;
            _nextOverlayDebugLogTime = 0f;
            _lastSceneBindingSignature = 0;

            MarkOverlayUiDirty();
        }
        catch (System.Exception ex)
        {
            Plugin.Logger?.LogWarning($"[OtherPlayersOverlayPatch] ResetSceneBoundBindings 失败: {reason}, {ex.Message}");
        }
    }
}