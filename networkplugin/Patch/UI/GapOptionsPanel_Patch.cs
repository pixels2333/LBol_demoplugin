using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.GapOptions;
using LBoL.Core.Stations;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Configuration;
using NetworkPlugin.Core;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using UnityEngine;

namespace NetworkPlugin.Patch.UI;

/// <summary>
/// GapOptionsPanel补丁类
/// 在GapStation UI中添加交易和复活选项
/// </summary>
public class GapOptionsPanel_Patch
{
    /// <summary>
    /// 服务提供者实例
    /// </summary>
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 配置管理器实例
    /// </summary>
    private static ConfigManager ConfigManager => serviceProvider?.GetService<ConfigManager>();

    /// <summary>
    /// 获取同步管理器
    /// </summary>
    private static ISynchronizationManager GetSyncManager()
    {
        try
        {
            return serviceProvider?.GetService<ISynchronizationManager>();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取网络管理器
    /// </summary>
    private static INetworkManager GetNetworkManager()
    {
        try
        {
            return serviceProvider?.GetService<INetworkManager>();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// GapOptionsPanel.OnShowing方法补丁
    /// 在显示Gap选项时添加交易和复活选项
    /// </summary>
    [HarmonyPatch(typeof(GapOptionsPanel), "OnShowing")]
    [HarmonyPostfix]
    public static void OnShowing_Postfix(GapOptionsPanel __instance, GapStation gapStation)
    {
        try
        {
            // 检查是否启用gap功能扩展
            if (ConfigManager?.EnableGapStationExtensions?.Value != true)
                return;

            INetworkManager networkManager = GetNetworkManager();
            if (networkManager == null || !networkManager.IsConnected)
                return;

            // 添加交易选项
            if (ConfigManager?.AllowTrading?.Value == true)
            {
                AddTradeOption(__instance, gapStation);
            }

            // 添加复活选项
            if (ConfigManager?.AllowRevival?.Value == true)
            {
                AddResurrectOption(__instance, gapStation);
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] OnShowing_Postfix错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 添加交易选项
    /// </summary>
    private static void AddTradeOption(GapOptionsPanel panel, GapStation gapStation)
    {
        try
        {
            // 创建交易选项
            var tradeOption = CreateCustomGapOption("Trade", "交易", "与其他玩家交易卡牌、道具、金币等物品");

            // 获取optionsLayout和template字段
            var optionsLayoutField = AccessTools.Field(typeof(GapOptionsPanel), "optionsLayout");
            var templateField = AccessTools.Field(typeof(GapOptionsPanel), "template");
            var spriteTableField = AccessTools.Field(typeof(GapOptionsPanel), "spriteTable");
            var _optionsField = AccessTools.Field(typeof(GapOptionsPanel), "_options");

            if (optionsLayoutField?.GetValue(panel) is Transform optionsLayout &&
                templateField?.GetValue(panel) is GapOptionWidget template &&
                spriteTableField?.GetValue(panel) is AssociationList<GapOptionType, Sprite> spriteTable &&
                _optionsField?.GetValue(panel) is System.Collections.Generic.List<GapOptionWidget> _options)
            {
                // 创建交易选项widget
                var tradeWidget = GameObject.Instantiate(template, optionsLayout);
                tradeWidget.Parent = panel;

                // 设置选项信息
                SetCustomOptionInfo(tradeWidget, tradeOption, GetTradeSprite());

                // 添加到选项列表
                _options.Add(tradeWidget);

                // 调整位置
                int optionIndex = gapStation.GapOptions.Count;
                Vector3 optionPos = GetDefaultOptionPos(panel) + GetOptionPadding(panel) * optionIndex;
                tradeWidget.transform.DOLocalMove(optionPos, 1f, false)
                    .From(optionPos - new Vector3(4000f, 0f, 0f), true, false)
                    .SetEase(DG.Tweening.Ease.OutCubic);
                tradeWidget.transform.SetAsFirstSibling();

                Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 已添加交易选项");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] AddTradeOption错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 添加复活选项
    /// </summary>
    private static void AddResurrectOption(GapOptionsPanel panel, GapStation gapStation)
    {
        try
        {
            // 创建复活选项
            var resurrectOption = CreateCustomGapOption("Resurrect", "复活", "复活已死亡的队友");

            // 获取必要字段
            var optionsLayoutField = AccessTools.Field(typeof(GapOptionsPanel), "optionsLayout");
            var templateField = AccessTools.Field(typeof(GapOptionsPanel), "template");
            var spriteTableField = AccessTools.Field(typeof(GapOptionsPanel), "spriteTable");
            var _optionsField = AccessTools.Field(typeof(GapOptionsPanel), "_options");

            if (optionsLayoutField?.GetValue(panel) is Transform optionsLayout &&
                templateField?.GetValue(panel) is GapOptionWidget template &&
                spriteTableField?.GetValue(panel) is AssociationList<GapOptionType, Sprite> spriteTable &&
                _optionsField?.GetValue(panel) is System.Collections.Generic.List<GapOptionWidget> _options)
            {
                // 创建复活选项widget
                var resurrectWidget = GameObject.Instantiate(template, optionsLayout);
                resurrectWidget.Parent = panel;

                // 设置选项信息
                SetCustomOptionInfo(resurrectWidget, resurrectOption, GetResurrectSprite());

                // 添加到选项列表
                _options.Add(resurrectWidget);

                // 调整位置
                int optionIndex = gapStation.GapOptions.Count + 1;
                Vector3 optionPos = GetDefaultOptionPos(panel) + GetOptionPadding(panel) * optionIndex;
                resurrectWidget.transform.DOLocalMove(optionPos, 1f, false)
                    .From(optionPos - new Vector3(4000f, 0f, 0f), true, false)
                    .SetEase(DG.Tweening.Ease.OutCubic);
                resurrectWidget.transform.SetAsFirstSibling();

                Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 已添加复活选项");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] AddResurrectOption错误: {ex.Message}");
        }
    }

    /// <summary>
    /// GapOptionsPanel.OptionClicked方法补丁
    /// 处理交易和复活选项的点击事件
    /// </summary>
    [HarmonyPatch(typeof(GapOptionsPanel), "OptionClicked")]
    [HarmonyPrefix]
    public static bool OptionClicked_Prefix(GapOptionsPanel __instance, GapOption option)
    {
        try
        {
            // 检查是否为自定义选项
            if (IsCustomTradeOption(option))
            {
                HandleTradeOption(__instance, option);
                return false; // 阻止原始方法执行
            }

            if (IsCustomResurrectOption(option))
            {
                HandleResurrectOption(__instance, option);
                return false; // 阻止原始方法执行
            }

            return true; // 继续执行原始方法
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] OptionClicked_Prefix错误: {ex.Message}");
            return true;
        }
    }

    #region 辅助方法

    /// <summary>
    /// 创建自定义Gap选项
    /// </summary>
    private static object CreateCustomGapOption(string id, string name, string description)
    {
        // 由于不能直接继承GapOption，我们创建一个动态对象来模拟
        var optionType = typeof(GapOption);
        var customOption = new
        {
            Id = id,
            Name = name,
            Description = description,
            IsCustom = true
        };
        return customOption;
    }

    /// <summary>
    /// 设置自定义选项信息
    /// </summary>
    private static void SetCustomOptionInfo(GapOptionWidget widget, object customOption, Sprite sprite)
    {
        try
        {
            // 使用反射设置widget的选项信息
            var optionType = customOption.GetType();
            var nameProperty = optionType.GetProperty("Name");
            var descProperty = optionType.GetProperty("Description");

            if (nameProperty != null && descProperty != null)
            {
                string name = nameProperty.GetValue(customOption)?.ToString() ?? "Custom Option";
                string description = descProperty.GetValue(customOption)?.ToString() ?? "Custom option description";

                // 使用反射设置widget的显示信息
                var setNameMethod = widget.GetType().GetMethod("SetOption");
                if (setNameMethod != null)
                {
                    setNameMethod.Invoke(widget, new object[] { customOption, sprite });
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] SetCustomOptionInfo错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取交易图标
    /// </summary>
    private static Sprite GetTradeSprite()
    {
        // 返回交易相关的图标，这里需要根据实际资源调整
        try
        {
            return Resources.Load<Sprite>("UI/Icons/TradeIcon") ??
                   Resources.Load<Sprite>("UI/Icons/DefaultIcon");
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取复活图标
    /// </summary>
    private static Sprite GetResurrectSprite()
    {
        // 返回复活相关的图标，这里需要根据实际资源调整
        try
        {
            return Resources.Load<Sprite>("UI/Icons/ResurrectIcon") ??
                   Resources.Load<Sprite>("UI/Icons/DefaultIcon");
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取默认选项位置
    /// </summary>
    private static Vector3 GetDefaultOptionPos(GapOptionsPanel panel)
    {
        try
        {
            var field = AccessTools.Field(typeof(GapOptionsPanel), "defaultOptionPos");
            return field?.GetValue(panel) as Vector3? ?? Vector3.zero;
        }
        catch
        {
            return Vector3.zero;
        }
    }

    /// <summary>
    /// 获取选项间距
    /// </summary>
    private static Vector3 GetOptionPadding(GapOptionsPanel panel)
    {
        try
        {
            var field = AccessTools.Field(typeof(GapOptionsPanel), "optionPadding");
            return field?.GetValue(panel) as Vector3? ?? new Vector3(200f, 0f, 0f);
        }
        catch
        {
            return new Vector3(200f, 0f, 0f);
        }
    }

    /// <summary>
    /// 检查是否为自定义交易选项
    /// </summary>
    private static bool IsCustomTradeOption(GapOption option)
    {
        return option?.GetType().GetProperty("Id")?.GetValue(option)?.ToString() == "Trade";
    }

    /// <summary>
    /// 检查是否为自定义复活选项
    /// </summary>
    private static bool IsCustomResurrectOption(GapOption option)
    {
        return option?.GetType().GetProperty("Id")?.GetValue(option)?.ToString() == "Resurrect";
    }

    /// <summary>
    /// 处理交易选项
    /// </summary>
    private static void HandleTradeOption(GapOptionsPanel panel, GapOption option)
    {
        try
        {
            Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 处理交易选项");

            // 打开交易UI面板
            OpenTradeUI(panel);

            // 选择后隐藏面板
            var selectedAndHideMethod = panel.GetType().GetMethod("SelectedAndHide");
            selectedAndHideMethod?.Invoke(panel, null);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] HandleTradeOption错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理复活选项
    /// </summary>
    private static void HandleResurrectOption(GapOptionsPanel panel, GapOption option)
    {
        try
        {
            Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 处理复活选项");

            // 打开复活UI面板
            OpenResurrectUI(panel);

            // 选择后隐藏面板
            var selectedAndHideMethod = panel.GetType().GetMethod("SelectedAndHide");
            selectedAndHideMethod?.Invoke(panel, null);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] HandleResurrectOption错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 打开交易UI
    /// </summary>
    private static void OpenTradeUI(GapOptionsPanel panel)
    {
        try
        {
            // 创建或获取交易面板
            var tradePanel = GetOrCreateTradePanel();
            if (tradePanel != null)
            {
                // 启动交易UI协程
                var startCoroutineMethod = panel.GetType().GetMethod("StartCoroutine");
                startCoroutineMethod?.Invoke(panel, new object[] { tradePanel.ShowTradeUI() });
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] OpenTradeUI错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 打开复活UI
    /// </summary>
    private static void OpenResurrectUI(GapOptionsPanel panel)
    {
        try
        {
            // 创建或获取复活面板
            var resurrectPanel = GetOrCreateResurrectPanel();
            if (resurrectPanel != null)
            {
                // 启动复活UI协程
                var startCoroutineMethod = panel.GetType().GetMethod("StartCoroutine");
                startCoroutineMethod?.Invoke(panel, new object[] { resurrectPanel.ShowResurrectUI() });
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] OpenResurrectUI错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取或创建交易面板
    /// </summary>
    private static NetworkPlugin.UI.Panels.TradePanel GetOrCreateTradePanel()
    {
        try
        {
            // 尝试从UI管理器获取现有面板
            var uiManagerType = Type.GetType("LBoL.Presentation.UiManager, LBoL.Presentation");
            if (uiManagerType != null)
            {
                var getPanelMethod = uiManagerType.GetMethod("GetPanel");
                if (getPanelMethod != null)
                {
                    var panel = getPanelMethod.Invoke(null, new object[] { typeof(NetworkPlugin.UI.Panels.TradePanel) });
                    if (panel is NetworkPlugin.UI.Panels.TradePanel tradePanel)
                        return tradePanel;
                }
            }

            // 如果没有找到，创建新的面板
            var tradePanelGO = new GameObject("TradePanel");
            return tradePanelGO.AddComponent<NetworkPlugin.UI.Panels.TradePanel>();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] GetOrCreateTradePanel错误: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 获取或创建复活面板
    /// </summary>
    private static NetworkPlugin.UI.Panels.ResurrectPanel GetOrCreateResurrectPanel()
    {
        try
        {
            // 尝试从UI管理器获取现有面板
            var uiManagerType = Type.GetType("LBoL.Presentation.UiManager, LBoL.Presentation");
            if (uiManagerType != null)
            {
                var getPanelMethod = uiManagerType.GetMethod("GetPanel");
                if (getPanelMethod != null)
                {
                    var panel = getPanelMethod.Invoke(null, new object[] { typeof(NetworkPlugin.UI.Panels.ResurrectPanel) });
                    if (panel is NetworkPlugin.UI.Panels.ResurrectPanel resurrectPanel)
                        return resurrectPanel;
                }
            }

            // 如果没有找到，创建新的面板
            var resurrectPanelGO = new GameObject("ResurrectPanel");
            return resurrectPanelGO.AddComponent<NetworkPlugin.UI.Panels.ResurrectPanel>();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] GetOrCreateResurrectPanel错误: {ex.Message}");
            return null;
        }
    }

    #endregion
}