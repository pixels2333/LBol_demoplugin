using System;
using LBoL.Presentation.Units;
using HarmonyLib;
using UnityEngine;
using MyFirstPlugin.Loader;
using System.IO;
using Spine.Unity;
using BepInEx.Logging;

namespace MyFirstPlugin.Patch;

/// <summary>
/// UnitView.SetAnimation方法的Harmony补丁类
/// 用于拦截和修改游戏中的单位动画设置行为
/// 当特定单位（如Koishi）设置动画时，自动加载自定义的Spine动画资源
/// </summary>
[HarmonyPatch]
public class Viewer_Loadspine_Patch
{
    /// <summary>
    /// 日志源，用于记录补丁执行过程中的信息和错误
    /// </summary>
    internal static ManualLogSource Logger;

    /// <summary>
    /// UnitView.SetAnimation方法的后置补丁
    /// 在原始的SetAnimation方法执行完成后被调用
    /// 用于检查是否需要加载自定义Spine动画并执行相应的动画替换逻辑
    /// </summary>
    /// <param name="__instance">被补丁方法的UnitView实例（Harmony自动注入）</param>
    /// <param name="order">动画播放顺序参数</param>
    /// <param name="speed">动画播放速度参数</param>
    /// <param name="stop">是否停止当前动画参数</param>
    [HarmonyPatch(typeof(UnitView), "SetAnimation")]
    [HarmonyPostfix]
    public static void OnSetAnimationPostfix(UnitView __instance, string order, float speed, bool stop)
    {
        try
        {
            // 记录补丁开始执行的日志信息
            Logger.LogInfo($"补丁开始执行: {__instance?.name}");

            // 验证UnitView实例是否有效
            if (__instance == null)
            {
                Logger.LogWarning("[SpineViewer Patch] UnitView is null. Skipping SetAnimation patch.");
                return;
            }

            // 使用Harmony的Traverse工具获取私有字段_modelName
            var modelName = Traverse.Create(__instance).Field("_modelName").GetValue<string>();

            // 检查是否为目标单位类型（Koishi）
            if (modelName != "Koishi")
            {
                Logger.LogWarning($"[SpineViewer Patch] UnitView '{__instance.name}' is not a Koishi model. Skipping SetAnimation patch.");
                return;
            }

            Logger.LogInfo($"[SpineViewer Patch] UnitView '{__instance.name}' SetAnimation called with order '{order}', speed {speed}, stop {stop}.");

            // 获取SkeletonAnimation组件
            // 使用Traverse访问私有字段spineSkeleton而不是使用GetComponent
            var skeletonAnimation = Traverse.Create(__instance).Field("spineSkeleton").GetValue<SkeletonAnimation>();

            if (skeletonAnimation == null)
            {
                Logger.LogWarning($"UnitView '{__instance.name}' is marked as SpineViewer but has no SkeletonAnimation component.");
                return;
            }

            // 调用SpineLoader加载自定义动画资源
            // 注意：这里的路径是硬编码的，实际项目中应该使用配置文件或相对路径
            SpineLoader.LoadSpineAnimation(skeletonAnimation,
                                     @"F:\thunderbolt mods\TouhouLostBranchOfLegend\profiles\Default\BepInEx\plugins\koishi514\MyFirstPlugin\Resource\marisa\marisa.json",
                                     @"F:\thunderbolt mods\TouhouLostBranchOfLegend\profiles\Default\BepInEx\plugins\koishi514\MyFirstPlugin\Resource\marisa\marisa.atlas",
                                     "huiyin.png");

            // 设置要播放的目标动画参数
            const string targetAnimationName = "idle"; // 目标动画名称
            const int trackIndex = 0; // 动画轨道索引（0通常为主动画轨道）
            const bool loopAnimation = true; // 是否循环播放动画

            // 验证SkeletonAnimation组件是否已正确初始化
            if (skeletonAnimation.AnimationState != null && skeletonAnimation.SkeletonDataAsset != null)
            {
                // 在加载的Spine数据中查找目标动画片段
                var animation = skeletonAnimation.SkeletonDataAsset.GetSkeletonData(true).FindAnimation(targetAnimationName);

                if (animation != null)
                {
                    // 清除当前轨道上的动画，避免动画混合冲突
                    skeletonAnimation.AnimationState.ClearTrack(trackIndex);

                    // 设置并开始播放目标动画
                    skeletonAnimation.AnimationState.SetAnimation(trackIndex, animation, loopAnimation);

                    Debug.Log($"[SpineViewer Patch] Set Spine animation '{targetAnimationName}' on track {trackIndex} for UnitView '{__instance.name}' (originally called with order '{order}').");

                    // 可选：立即更新一次，让动画跳到第一帧
                    // skeletonAnimation.Update(0f);
                }
                else
                {
                    // 如果找不到目标动画，记录警告信息
                    Debug.LogWarning($"[SpineViewer Patch] Spine animation '{targetAnimationName}' not found in the loaded data for UnitView '{__instance.name}'. Cannot set animation.");

                    // 可选：清除轨道或设置默认动画
                    // skeletonAnimation.AnimationState.ClearTrack(trackIndex);
                }
            }
            else
            {
                // 如果SkeletonAnimation组件未正确初始化，记录错误信息
                Debug.LogError($"[SpineViewer Patch] SkeletonAnimation component not initialized or missing SkeletonDataAsset for UnitView '{__instance.name}'. Cannot set animation.");
            }

            // 关于访问私有变量的说明：
            // 如果需要访问UnitView实例的私有变量（如_hasBlink、AllAnimationsNames等），
            // 可以使用Harmony的Traverse工具。例如：
            // bool hasBlink = Traverse.Create(__instance).Field("_hasBlink").GetValue<bool>();
            // System.Collections.Generic.List<string> animationNames = Traverse.Create(__instance).Field("AllAnimationsNames").GetValue<System.Collections.Generic.List<string>>();
            // 但在当前的逻辑中，不需要访问这些私有变量。
        }
        catch (Exception ex)
        {
            // 捕获并记录补丁执行过程中的异常
            Debug.LogError($"补丁执行出错: {ex}");
        }
    }
}