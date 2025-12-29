using System;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core.Battle;
using LBoL.Presentation.UI.Panels;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.Patch.UI;

/// <summary>
/// 回合结束计时器补丁（仿照 Together in Spire 的 EndTurnTimerPatches）
/// 主要功能：
/// 1) 在联机且处于玩家回合时倒计时
/// 2) 倒计时归零后自动触发“结束回合”
/// 3) 在结束回合按钮上渲染一个颜色条提示剩余时间（绿 -> 红）
/// </summary>
public class EndTurnTimerPatch
{
    private static IServiceProvider serviceProvider => ModService.ServiceProvider; // 统一的 DI 入口，用于取网络客户端等服务

    /// <summary>
    /// 回合时限（秒）。设为 0 或负数表示禁用。
    /// 说明：当前仓库里尚未看到“房间/联机设置里的回合时限”，先用常量占位，后续可接入配置或房间规则。
    /// </summary>
    public static float TurnTimeLimitSeconds { get; set; } = 60f; // 默认 60 秒，可在运行时改写

    /// <summary>
    /// 当前回合剩余时间（秒）
    /// </summary>
    public static float CurrentTimer { get; private set; } // 记录当前剩余秒数

    private const string OverlayObjectName = "NetworkPlugin_EndTurnTimerOverlay"; // 覆盖条节点名称，便于定位/复用
    private static Sprite _whiteSprite; // 缓存的 1x1 白色精灵
    private static Texture2D _whiteTexture; // 缓存的纹理，避免重复创建

    /// <summary>
    /// 重置计时器（在玩家回合开始时调用）
    /// </summary>
    public static void ResetTimer()
    {
        CurrentTimer = Mathf.Max(0f, TurnTimeLimitSeconds); // 不允许负值；0 代表关闭倒计时
    }

    /// <summary>
    /// 判断当前是否已经连接到联机服务。
    /// </summary>
    private static bool IsNetworkConnected()
    {
        try
        {
            INetworkClient networkClient = serviceProvider?.GetService<INetworkClient>(); // 从 DI 拉取网络客户端
            return networkClient?.IsConnected == true; // 安全判空后返回连接状态
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 通过反射获取当前 PlayBoard 上的战斗实例。
    /// </summary>
    private static BattleController GetBattle(PlayBoard playBoard)
    {
        try
        {
            return Traverse.Create(playBoard).Property("Battle").GetValue<BattleController>(); // Harmony Traverse 访问私有属性
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 通过反射获取结束回合按钮。
    /// </summary>
    private static Button GetEndTurnButton(PlayBoard playBoard)
    {
        try
        {
            return Traverse.Create(playBoard).Field("endTurnButton").GetValue<Button>(); // 反射获取结束回合按钮
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 创建/缓存 1x1 的白色精灵，作为填充条底图。
    /// </summary>
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

    /// <summary>
    /// 确保结束回合按钮上存在填充覆盖条，并返回对应的 Image。
    /// </summary>
    private static Image EnsureOverlay(Button endTurnButton)
    {
        Transform overlayTransform = endTurnButton.transform.Find(OverlayObjectName); // 查找并复用已有覆盖条
        if (overlayTransform != null)
        {
            return overlayTransform.GetComponent<Image>(); // 已存在则直接复用
        }

        GameObject go = new(OverlayObjectName);
        go.transform.SetParent(endTurnButton.transform, false); // 挂在按钮下，继承布局

        RectTransform rectTransform = go.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero; // 左下对齐
        rectTransform.anchorMax = Vector2.one;  // 右上对齐
        rectTransform.offsetMin = Vector2.zero; // 占满父节点
        rectTransform.offsetMax = Vector2.zero;

        var image = go.AddComponent<Image>();
        image.sprite = GetWhiteSprite(); // 1x1 白贴图作为填充底图
        image.type = Image.Type.Filled; // 开启填充模式
        image.fillMethod = Image.FillMethod.Horizontal; // 水平填充
        image.fillOrigin = (int)Image.OriginHorizontal.Left; // 从左到右
        image.fillAmount = 1f; // 初始满格
        image.raycastTarget = false; // 不阻挡点击

        // 放在最底层：不遮挡按钮上的文字/特效，但会覆盖按钮底图（作为“剩余时间底色”）。
        go.transform.SetAsFirstSibling();

        return image;
    }

    /// <summary>
    /// 隐藏覆盖条，防止在不应显示时干扰 UI。
    /// </summary>
    private static void HideOverlay(Button endTurnButton)
    {
        Transform overlayTransform = endTurnButton.transform.Find(OverlayObjectName); // 找到就直接隐藏
        if (overlayTransform != null)
        {
            overlayTransform.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 根据剩余百分比更新覆盖条的填充量与颜色。
    /// </summary>
    private static void UpdateOverlay(Button endTurnButton, float remainingPercent)
    {
        var overlay = EnsureOverlay(endTurnButton); // 确保覆盖条存在
        if (overlay == null)
        {
            return;
        }

        overlay.gameObject.SetActive(true);

        remainingPercent = Mathf.Clamp01(remainingPercent); // 限制到 [0,1]
        overlay.fillAmount = remainingPercent; // 更新填充比例

        float red = 1f - remainingPercent; // 剩余越低红色越高
        float green = remainingPercent;    // 剩余越高绿色越高
        overlay.color = new Color(red, green, 0f, 0.60f); // 半透明叠加
    }

    /// <summary>
    /// 玩家回合开始时重置计时器
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "StartPlayerTurn")]
    [HarmonyPostfix]
    public static void BattleController_StartPlayerTurn_Postfix(BattleController __instance)
    {
        try
        {
            if (!IsNetworkConnected()) // 仅在联网时启用倒计时逻辑
            {
                return;
            }

            if (TurnTimeLimitSeconds <= 0f) // 配置为 0/负数则跳过
            {
                return;
            }

            ResetTimer();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EndTurnTimerPatch] BattleController_StartPlayerTurn_Postfix 错误: {ex}");
        }
    }

    /// <summary>
    /// 每帧更新计时器并在必要时自动结束回合
    /// </summary>
    [HarmonyPatch(typeof(PlayBoard), "Update")]
    [HarmonyPostfix]
    public static void PlayBoard_Update_Postfix(PlayBoard __instance)
    {
        try
        {
            // 未开启倒计时或未联网时直接退出，并隐藏覆盖条。
            if (TurnTimeLimitSeconds <= 0f || !IsNetworkConnected()) // 未启用或未联网时关闭覆盖条并退出
            {
                var btn0 = GetEndTurnButton(__instance);
                if (btn0 != null)
                {
                    HideOverlay(btn0);
                }
                return;
            }

            var endTurnButton = GetEndTurnButton(__instance); // 获取 UI 按钮
            if (endTurnButton == null)
            {
                return;
            }

            // 按钮不可见或不可交互时不计时。
            // 只在按钮可见且可交互时计时（与参考补丁里的 __instance.enabled 思路一致）
            if (!endTurnButton.gameObject.activeInHierarchy || !endTurnButton.interactable) // 按钮不可用时不显示计时
            {
                HideOverlay(endTurnButton);
                return;
            }

            BattleController battle = GetBattle(__instance); // 拿到当前战斗
            if (battle == null || !battle.IsWaitingPlayerInput) // 非玩家输入阶段不显示
            {
                // 非玩家输入阶段时暂停显示倒计时。
                HideOverlay(endTurnButton);
                return;
            }

            // 兜底：如果因为“中途加入/热重载”等原因没有走到回合开始补丁，则在首次需要计时时初始化一次
            if (CurrentTimer <= 0f || CurrentTimer > TurnTimeLimitSeconds + 0.01f) // 兜底重置异常的计时值
            {
                ResetTimer();
            }

            // 使用未缩放时间，保证游戏暂停或加速时计时一致。
            CurrentTimer -= Time.unscaledDeltaTime; // 不受时间缩放影响的减法

            // 以剩余百分比更新 UI，同时推进渐变色。
            float remainingPercent = CurrentTimer / TurnTimeLimitSeconds; // 计算剩余比例
            UpdateOverlay(endTurnButton, remainingPercent); // 更新进度与颜色

            if (CurrentTimer < 0f) // 时间耗尽后结束回合
            {
                // 倒计时耗尽后模拟按键触发结束回合。
                __instance.HandleEndTurnFromKey();
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EndTurnTimerPatch] PlayBoard_Update_Postfix 错误: {ex}");
        }
    }
}
