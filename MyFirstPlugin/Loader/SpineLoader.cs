namespace MyFirstPlugin.Loader;

using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using Spine.Unity;
using UnityEngine;


public class SpineLoader
{
    // 从指定路径加载Spine动画

    internal static ManualLogSource Logger;
    public static void LoadSpineAnimation(SkeletonAnimation animator, string jsonPath, string atlasPath)
    {
        Logger.LogInfo($"加载Spine动画: JSON路径: {jsonPath}, Atlas路径: {atlasPath}");
        if (animator == null)
        {
            Logger.LogError("原动画组件为空");
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
        // 查找Spine Shader
        var shader = Shader.Find("Spine/Skeleton");
        if (shader == null)
        {
            Logger.LogError("找不到Spine/Skeleton着色器，尝试使用默认着色器");
            shader = Shader.Find("Sprites/Default");

            if (shader == null)
            {
                Logger.LogError("无法找到可用着色器，无法加载Spine动画");
                return;
            }
        }
        // 加载Atlas资源
        TextAsset atlasTextAsset = new(File.ReadAllText(atlasPath));
        // 解析Atlas文本来查找引用的图片文件
        string atlasText = atlasTextAsset.text;
        string[] atlasLines = atlasText.Split('\n');

        // 创建材质列表
        List<Material> materials = [];

        // 尝试找到并加载Atlas引用的所有纹理
        string currentPageName = null;
        string atlasDir = Path.GetDirectoryName(atlasPath);
        foreach (string line in atlasLines)
        {
            // Atlas格式中的纹理页通常是独立一行的文件名
            if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith(" ") && line.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                currentPageName = line.Trim();
                string texturePath = Path.Combine(atlasDir, currentPageName);
                Logger.LogInfo($"尝试加载纹理: {texturePath}");

                if (File.Exists(texturePath))
                {
                    // 加载纹理
                    byte[] textureData = File.ReadAllBytes(texturePath);
                    Texture2D texture = new(2, 2); // 尺寸会在加载时重置
                    if (texture.LoadImage(textureData))
                    {
                        texture.name = currentPageName;
                        Material mat = new(shader)
                        {
                            name = currentPageName + " Material",
                            mainTexture = texture
                        };
                        materials.Add(mat);
                        Logger.LogInfo($"成功加载纹理: {currentPageName}");
                    }
                    else
                    {
                        Logger.LogError($"无法加载纹理: {texturePath}");
                    }
                }
                else
                {
                    Logger.LogError($"纹理文件不存在: {texturePath}");
                }
            }
        }

        // 如果没有找到任何材质，添加一个默认材质
        if (materials.Count == 0)
        {
            Logger.LogWarning("没有找到任何纹理，使用默认材质");
            materials.Add(new Material(shader));
        }

        // 创建Atlas资源
        var atlasAsset = SpineAtlasAsset.CreateRuntimeInstance(atlasTextAsset, [.. materials], true);
        if (atlasAsset == null)
        {
            Logger.LogError("创建Atlas资源失败");
            return;
        }

        // 加载JSON骨骼数据
        TextAsset jsonTextAsset = new(File.ReadAllText(jsonPath));
        var skeletonDataAsset = SkeletonDataAsset.CreateRuntimeInstance(jsonTextAsset, atlasAsset, true);
        // 应用到动画组件
        animator.skeletonDataAsset = skeletonDataAsset;
        animator.Initialize(true);
        // 验证初始化结果
        if (animator.Skeleton == null)
        {
            Logger.LogError("初始化后Skeleton仍为null");
            return;
        }

        Logger.LogInfo("Spine动画加载并初始化成功");
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
