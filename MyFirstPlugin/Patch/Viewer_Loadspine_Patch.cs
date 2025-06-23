namespace MyFirstPlugin.Patch
{
    using System;
    using LBoL.Presentation.Units;
    using HarmonyLib; // 确保导入 Harmony 的命名空间
    using UnityEngine; // 确保导入 Unity 的命名空间
    using MyFirstPlugin.Loader;
    using System.IO;
    using Spine.Unity;
    using BepInEx.Logging;

    [HarmonyPatch]
    public class Viewer_Loadspine_Patch
    {

        // public static SpineLoader spineLoader;
        internal static ManualLogSource Logger ;
        
        // 假设 SpineLoader 类在某个命名空间下，如果不在，请根据实际情况添加 using 或移除

        // 假设 UnitView 和 Unit 类是可访问的

        [HarmonyPatch(typeof(UnitView), "SetAnimation")]
        [HarmonyPostfix]
        // 注入原始方法的参数，以便在 Postfix 中使用或了解上下文
        // Harmony 会通过参数名称（区分大小写）进行匹配
        public static void OnSetAnimationPostfix(UnitView __instance, string order, float speed, bool stop)
        {
            try
            {
                Logger.LogInfo($"补丁开始执行: {__instance?.name}");
                // 检查 __instance 是否为空，然后检查是否是特定的单位类型
                // 使用 'is' 模式匹配可以同时检查 null 和类型/属性
                if (__instance == null)
                {
                    Logger.LogWarning($"[SpineViewer Patch] UnitView is null. Skipping SetAnimation patch.");
                    return;
                }

                var modelName = Traverse.Create(__instance).Field("_modelname").GetValue<string>();
                if (modelName != "Koishi")
                {
                    // 如果不是目标单位类型，直接返回，不执行后续逻辑
                    Logger.LogWarning($"[SpineViewer Patch] UnitView '{__instance.name}' is not a Koishi model. Skipping SetAnimation patch.");
                    return;
                }

                Logger.LogInfo($"[SpineViewer Patch] UnitView '{__instance.name}' SetAnimation called with order '{order}', speed {speed}, stop {stop}.");
                // 获取 SkeletonAnimation 组件
                // 原方法中的 SpineLoaded 可能意味着这个组件是否存在或已被初始化
                // 依赖 GetComponent 是获取它的标准方式
                var skeletonAnimation = __instance.GetComponent<SkeletonAnimation>();
                if (skeletonAnimation == null)
                {
                    // 对于 SpineViewer 类型但没有 SkeletonAnimation 的情况，可能需要警告
                    // 这可能意味着 SpineLoaded 为 false，或者 UnitView 的设置有问题
                    Debug.LogWarning($"UnitView '{__instance.name}'  is marked as SpineViewer but has no SkeletonAnimation component.");
                    return;
                }

                // --- 动画加载和设置逻辑 ---
                // 注意：在每次调用 SetAnimation 时都创建新的 SpineLoader 并加载资源是非常低效的。
                // 理想情况下，Spine 数据应该在 UnitView 初始化时或 SpineLoaded 变为 true 时加载一次。
                // 为了遵循您原有的结构，这里保留了加载调用，但请注意性能问题。
                // 更好的做法是：
                // 1. 在 UnitView 初始化时加载资源。
                // 2. 在 Postfix 中只负责设置动画。


                // 调用加载方法。
                // 注意：SpineLoader.LoadSpineAnimationFromResources 的实际签名和行为未知。
                // 我们假设它能够正确加载动画和图集，并将 SkeletonDataAsset 和 AtlasAsset
                // 分配给 skeletonAnimation 组件，并且可能调用 skeletonAnimation.Initialize(true);
                // 您可能需要根据实际的 SpineLoader 类调整这里的调用或后续步骤。

                SpineLoader.LoadSpineAnimation(skeletonAnimation,
                                        Path.Combine(Application.streamingAssetsPath, "MyFirstPlugin/Resource/baize/baize.json"),
                                        Path.Combine(Application.streamingAssetsPath, "MyFirstPlugin/Resource/baize/baize.atlas"));


                // --- 设置加载后的特定动画来播放 ---
                // 加载资源只是准备数据，还需要告诉 Spine 运行时播放哪个动画片段。
                // 假设您想播放加载数据中的一个名为 "idle" 的动画片段（或者您想要的任何片段）
                const string targetAnimationName = "idle"; // <--- 替换为您希望播放的 Spine 动画片段的实际名称
                const int trackIndex = 0; // 通常使用轨道 0 来播放主动画
                const bool loopAnimation = true; // <--- 设置为 true 如果希望动画循环，false 如果是单次播放

                // 在尝试设置动画之前，检查 SkeletonAnimation 组件是否已初始化并且有 Spine 数据
                if (skeletonAnimation.AnimationState != null && skeletonAnimation.SkeletonDataAsset != null)
                {
                    // 查找加载的数据中是否存在目标动画片段
                    var animation = skeletonAnimation.SkeletonDataAsset.GetSkeletonData(true).FindAnimation(targetAnimationName);

                    if (animation != null)
                    {
                        // 清除当前轨道上的任何动画（可选，但常用于避免混合问题）
                        skeletonAnimation.AnimationState.ClearTrack(trackIndex);
                        // 设置并开始播放目标动画
                        skeletonAnimation.AnimationState.SetAnimation(trackIndex, animation, loopAnimation);

                        Debug.Log($"[SpineViewer Patch] Set Spine animation '{targetAnimationName}' on track {trackIndex} for UnitView '{__instance.name}' (originally called with order '{order}').");

                        // 可选：立即更新一次，让动画跳到第一帧
                        // skeletonAnimation.Update(0f);
                    }
                    else
                    {
                        Debug.LogWarning($"[SpineViewer Patch] Spine animation '{targetAnimationName}' not found in the loaded data for UnitView '{__instance.name}'. Cannot set animation.");
                        // 如果找不到目标动画，可以选择清除轨道或设置一个默认/错误动画
                        // skeletonAnimation.AnimationState.ClearTrack(trackIndex);
                    }
                }
                else
                {
                    Debug.LogError($"[SpineViewer Patch] SkeletonAnimation component not initialized or missing SkeletonDataAsset for UnitView '{__instance.name}'. Cannot set animation.");
                }

                // --- 关于访问私有变量 (__hasBlink, AllAnimationsNames 等) ---
                // 如果你需要在 Postfix 中读取原方法所在的 UnitView 实例的私有变量，
                // 你可以使用 Harmony 的 Traverse 工具。例如：
                // bool hasBlink = Traverse.Create(__instance).Field("_hasBlink").GetValue<bool>();
                // System.Collections.Generic.List<string> animationNames = Traverse.Create(__instance).Field("AllAnimationsNames").GetValue<System.Collections.Generic.List<string>>();
                // 但在当前的逻辑中，似乎不需要访问这些私有变量。
            }
            catch (Exception ex)
            {
                Debug.LogError($"补丁执行出错: {ex}");
            }

        }
    }
}

