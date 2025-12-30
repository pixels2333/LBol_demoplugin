using System;
using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.GapOptions;
using LBoL.Core.Stations;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Configuration;
using NetworkPlugin.Core;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.UI.Panels;
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
            // 创建自定义交易选项对象，包含ID、中文名称和描述
            object tradeOption = CreateCustomGapOption("Trade", "交易", "与其他玩家交易卡牌、道具、金币等物品");

            // 使用Traverse工具获取GapOptionsPanel的私有字段
            Traverse traverse = Traverse.Create(panel);
            Transform optionsLayout = traverse.Field("optionsLayout").GetValue<Transform>(); // UI布局容器，用于放置选项widget
            GapOptionWidget template = traverse.Field("template").GetValue<GapOptionWidget>(); // 选项widget模板，用于实例化新的选项
            AssociationList<GapOptionType, Sprite> spriteTable = traverse.Field("spriteTable").GetValue<AssociationList<GapOptionType, Sprite>>(); // 选项类型与图标的映射表
            List<GapOptionWidget> _options = traverse.Field("_options").GetValue<List<GapOptionWidget>>(); // 当前显示的选项widget列表

            // 验证字段获取成功并提取值
            if (optionsLayout != null &&
                template != null &&
                spriteTable != null &&
                _options != null)
            {
                // 基于模板创建交易选项widget实例，并将其添加到布局中
                GapOptionWidget tradeWidget = UnityEngine.Object.Instantiate(template, optionsLayout);
                tradeWidget.Parent = panel; // 设置父面板引用

                // 设置widget的显示信息和图标
                try
                {
                    Traverse.Create(tradeWidget).Method("SetOption").GetValue(tradeOption, GetTradeSprite());
                }
                catch (Exception ex)
                {
                    Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] SetOption错误: {ex.Message}");
                }

                // 将新创建的widget添加到选项列表中
                _options.Add(tradeWidget);

                // 计算widget的目标位置
                int optionIndex = gapStation.GapOptions.Count; // 获取当前选项数量作为索引
                Vector3 optionPos = GetDefaultOptionPos(panel) + GetOptionPadding(panel) * optionIndex; // 基础位置 + 间距偏移

                // 创建入场动画：从左侧4000像素处滑入，持续1秒，使用OutCubic缓动曲线
                tradeWidget.transform.DOLocalMove(optionPos, 1f, false)
                    .From(optionPos - new Vector3(4000f, 0f, 0f), true, false)
                    .SetEase(DG.Tweening.Ease.OutCubic);

                // 将widget设置为同级节点中的第一个，确保正确的渲染顺序
                tradeWidget.transform.SetAsFirstSibling();

                // 记录添加成功的日志
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
            object resurrectOption = CreateCustomGapOption("Resurrect", "复活", "复活已死亡的队友");

            // 使用Traverse工具获取必要字段
            var traverse = Traverse.Create(panel);
            Transform optionsLayout = traverse.Field("optionsLayout").GetValue<Transform>();
            GapOptionWidget template = traverse.Field("template").GetValue<GapOptionWidget>();
            AssociationList<GapOptionType, Sprite> spriteTable = traverse.Field("spriteTable").GetValue<AssociationList<GapOptionType, Sprite>>();
            List<GapOptionWidget> _options = traverse.Field("_options").GetValue<List<GapOptionWidget>>();

            if (optionsLayout != null &&
                template != null &&
                spriteTable != null &&
                _options != null)
            {
                // 创建复活选项widget
                GapOptionWidget resurrectWidget = UnityEngine.Object.Instantiate(template, optionsLayout);
                resurrectWidget.Parent = panel;

                // 设置选项信息
                try
                {
                    Traverse.Create(resurrectWidget).Method("SetOption").GetValue(resurrectOption, GetResurrectSprite());
                }
                catch (Exception ex)
                {
                    Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] SetOption错误: {ex.Message}");
                }

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
            // 检查是否为自定义交易选项
            if (IsCustomTradeOption(option))
            {
                try
                {
                    Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 处理交易选项");
                    TradePanel tradePanel = GetOrCreateTradePanel();
                    if (tradePanel != null)
                    {
                        Traverse.Create(__instance).Method("StartCoroutine").GetValue(tradePanel.ShowTradeAsync(new TradePayload()));
                    }
                    Traverse.Create(__instance).Method("SelectedAndHide").GetValue();
                }
                catch (Exception ex)
                {
                    Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] 处理交易选项错误: {ex.Message}");
                }
                return false; // 阻止原始方法执行
            }

            // 检查是否为自定义复活选项
            if (IsCustomResurrectOption(option))
            {
                try
                {
                    Plugin.Logger?.LogInfo("[GapOptionsPanel_Patch] 处理复活选项");
                    ResurrectPanel resurrectPanel = GetOrCreateResurrectPanel();
                    if (resurrectPanel != null)
                    {
                        Traverse.Create(__instance).Method("StartCoroutine").GetValue(resurrectPanel.ShowResurrectAsync(new ResurrectPayload()));
                    }
                    Traverse.Create(__instance).Method("SelectedAndHide").GetValue();
                }
                catch (Exception ex)
                {
                    Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] 处理复活选项错误: {ex.Message}");
                }
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
            return Traverse.Create(panel).Field("defaultOptionPos").GetValue<Vector3>();
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
            return Traverse.Create(panel).Field("optionPadding").GetValue<Vector3>();
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
    /// 获取或创建交易面板
    /// </summary>
    private static TradePanel GetOrCreateTradePanel()
    {
        try
        {
            // 尝试从UI管理器获取现有面板
            var panel = Traverse.CreateWithType(typeof(UiManager).FullName).Method("GetPanel").GetValue<TradePanel>();
            if (panel != null)
                return panel;

            // 如果没有找到，创建新的面板
            var tradePanelGO = new GameObject("TradePanel");
            return tradePanelGO.AddComponent<TradePanel>();
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
    private static ResurrectPanel GetOrCreateResurrectPanel()
    {
        try
        {
            // 尝试从UI管理器获取现有面板
            var panel = Traverse.CreateWithType(typeof(UiManager).FullName).Method("GetPanel").GetValue<ResurrectPanel>();
            if (panel != null)
                return panel;

            // 如果没有找到，创建新的面板
            var resurrectPanelGO = new GameObject("ResurrectPanel");
            return resurrectPanelGO.AddComponent<ResurrectPanel>();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GapOptionsPanel_Patch] GetOrCreateResurrectPanel错误: {ex.Message}");
            return null;
        }
    }

    #endregion
}
