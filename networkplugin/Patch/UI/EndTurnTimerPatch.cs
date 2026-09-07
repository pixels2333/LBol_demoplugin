using System;
using HarmonyLib;
using LBoL.Core.Battle;
using LBoL.Presentation.UI.Panels;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.Patch.UI;

public class EndTurnTimerPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

        public static float TurnTimeLimitSeconds { get; set; } = 60f;

        public static float CurrentTimer { get; private set; }

    private const string OverlayObjectName = "NetworkPlugin_EndTurnTimerOverlay";
    private static Sprite _whiteSprite;
    private static Texture2D _whiteTexture;

        public static void ResetTimer()
    {
        CurrentTimer = Mathf.Max(0f, TurnTimeLimitSeconds);
    }

        private static bool IsNetworkConnected()
        => ServiceProvider?.GetService<INetworkClient>()?.IsConnected == true;

        private static BattleController GetBattle(PlayBoard playBoard)
    {
        try
        {
            return Traverse.Create(playBoard).Property("Battle").GetValue<BattleController>();
        }
        catch
        {
            return null;
        }
    }

        private static Button GetEndTurnButton(PlayBoard playBoard)
    {
        try
        {
            return Traverse.Create(playBoard).Field("endTurnButton").GetValue<Button>();
        }
        catch
        {
            return null;
        }
    }

        private static Sprite GetWhiteSprite()
    {
        if (_whiteSprite != null)
        {
            return _whiteSprite;
        }

        _whiteTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _whiteTexture.SetPixel(0, 0, Color.white);
        _whiteTexture.Apply(false, true);

        _whiteSprite = Sprite.Create(_whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return _whiteSprite;
    }

        private static Image EnsureOverlay(Button endTurnButton)
    {
        Transform overlayTransform = endTurnButton.transform.Find(OverlayObjectName);
        if (overlayTransform != null)
        {
            return overlayTransform.GetComponent<Image>();
        }

        GameObject go = new(OverlayObjectName);
        go.transform.SetParent(endTurnButton.transform, false);

        RectTransform rectTransform = go.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image image = go.AddComponent<Image>();
        image.sprite = GetWhiteSprite();
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillOrigin = (int)Image.OriginHorizontal.Left;
        image.fillAmount = 1f;
        image.raycastTarget = false;

        go.transform.SetAsFirstSibling();

        return image;
    }

        private static void HideOverlay(Button endTurnButton)
    {
        Transform overlayTransform = endTurnButton.transform.Find(OverlayObjectName);
        overlayTransform?.gameObject.SetActive(false);
    }

        private static void UpdateOverlay(Button endTurnButton, float remainingPercent)
    {
        var overlay = EnsureOverlay(endTurnButton);
        if (overlay == null)
        {
            return;
        }

        overlay.gameObject.SetActive(true);

        remainingPercent = Mathf.Clamp01(remainingPercent);
        overlay.fillAmount = remainingPercent;

        float red = 1f - remainingPercent;
        float green = remainingPercent;
        overlay.color = new Color(red, green, 0f, 0.60f);
    }

        [HarmonyPatch(typeof(BattleController), "StartPlayerTurn")]
    [HarmonyPostfix]
    public static void BattleController_StartPlayerTurn_Postfix(BattleController __instance)
    {
        if (!IsNetworkConnected())
        {
            return;
        }

        if (TurnTimeLimitSeconds <= 0f)
        {
            return;
        }

        ResetTimer();
    }

        [HarmonyPatch(typeof(PlayBoard), "Update")]
    [HarmonyPostfix]
    public static void PlayBoard_Update_Postfix(PlayBoard __instance)
    {
        try
        {

            if (TurnTimeLimitSeconds <= 0f || !IsNetworkConnected())
            {
                var btn0 = GetEndTurnButton(__instance);
                if (btn0 != null)
                {
                    HideOverlay(btn0);
                }
                return;
            }

            var endTurnButton = GetEndTurnButton(__instance);
            if (endTurnButton == null)
            {
                return;
            }

            if (!endTurnButton.gameObject.activeInHierarchy || !endTurnButton.interactable)
            {
                HideOverlay(endTurnButton);
                return;
            }

            BattleController battle = GetBattle(__instance);
            if (battle == null || !battle.IsWaitingPlayerInput)
            {

                HideOverlay(endTurnButton);
                return;
            }

            if (CurrentTimer <= 0f || CurrentTimer > TurnTimeLimitSeconds + 0.01f)
            {
                ResetTimer();
            }

            CurrentTimer -= Time.unscaledDeltaTime;

            float remainingPercent = CurrentTimer / TurnTimeLimitSeconds;
            UpdateOverlay(endTurnButton, remainingPercent);

            if (CurrentTimer < 0f)
            {

                __instance.HandleEndTurnFromKey();
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EndTurnTimerPatch] PlayBoard_Update_Postfix 错误: {ex}");
        }
    }
}
