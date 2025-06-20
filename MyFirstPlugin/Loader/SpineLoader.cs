namespace MyFirstPlugin.Loader;

using System.IO;
using HarmonyLib;
using Spine.Unity;
using UnityEngine;

[HarmonyPatch]
public  class SpineLoader : MonoBehaviour
{
    // 从指定路径加载Spine动画
    public void LoadSpineAnimation(SkeletonAnimation animator, string jsonPath, string atlasPath)
    {
        if (animator == null)
        {
            Debug.LogError("动画组件为空");
            return;
        }

        // 检查文件是否存在
        if (!File.Exists(jsonPath) || !File.Exists(atlasPath))
        {
            Debug.LogError($"文件不存在: JSON: {jsonPath} 或 Atlas: {atlasPath}");
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
            Debug.LogError($"无法加载Spine数据: {jsonResourcePath}");
            return;
        }

        // 应用到动画组件
        animator.skeletonDataAsset = skeletonDataAsset;
        animator.Initialize(true);
    }
}
