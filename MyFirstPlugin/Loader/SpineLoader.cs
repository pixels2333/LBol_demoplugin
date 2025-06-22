namespace MyFirstPlugin.Loader;

using System.IO;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using Spine.Unity;
using UnityEngine;


public class SpineLoader
{
    // 从指定路径加载Spine动画

    internal static new ManualLogSource Logger;
    public static void LoadSpineAnimation(SkeletonAnimation animator, string jsonPath, string atlasPath)
    {
        Logger.LogInfo($"加载Spine动画: JSON路径: {jsonPath}, Atlas路径: {atlasPath}");
        if (animator == null)
        {
            Logger.LogError("动画组件为空");
            return;
        }
        //清除动画文件
        animator.skeletonDataAsset = null;
        animator.Initialize(false);// 初始化时不加载骨骼数据

        // 检查文件是否存在
        if (!File.Exists(jsonPath) || !File.Exists(atlasPath))
        {
            Logger.LogError($"文件不存在: JSON: {jsonPath} 或 Atlas: {atlasPath}");
            return;
        }

        // 加载Atlas资源
        TextAsset atlasTextAsset = new(File.ReadAllText(atlasPath));
        var atlasAsset = SpineAtlasAsset.CreateRuntimeInstance(atlasTextAsset,
            [new Material(Shader.Find("Spine/Skeleton"))], true);

        // 加载JSON骨骼数据
        TextAsset jsonTextAsset = new(File.ReadAllText(jsonPath));
        var skeletonDataAsset = SkeletonDataAsset.CreateRuntimeInstance(jsonTextAsset, atlasAsset, true);

        // 应用到动画组件
        animator.skeletonDataAsset = skeletonDataAsset;
        animator.Initialize(true);
    }

    // 从Resources目录加载Spine动画
    public void LoadSpineAnimationFromResources(SkeletonAnimation animator, string jsonResourcePath, string atlasResourcePath)
    {
        // 从Resources加载资源
        var skeletonDataAsset = Resources.Load<SkeletonDataAsset>(jsonResourcePath);

        if (skeletonDataAsset == null)
        {
            Logger.LogError($"无法加载Spine数据: {jsonResourcePath}");
            return;
        }

        // 应用到动画组件
        animator.skeletonDataAsset = skeletonDataAsset;
        animator.Initialize(true);
    }
}
