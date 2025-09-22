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
    public static void LoadSpineAnimation(SkeletonAnimation animator, string jsonPath, string atlasPath, string texturepath)
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
            Logger.LogError("找不到Spine/Skeleton着色器,尝试使用默认着色器");
            shader = Shader.Find("Sprites/Default");

            if (shader == null)
            {
                Logger.LogError("无法找到可用着色器,无法加载Spine动画");
                return;
            }
        }
        else
        {
            Logger.LogInfo("成功找到Spine/Skeleton着色器");
        }
        // 加载Atlas资源
        TextAsset atlasTextAsset = new(File.ReadAllText(atlasPath));
        // 解析Atlas文本来查找引用的图片文件
        string atlasText = atlasTextAsset.text;
        string[] atlasLines = atlasText.Split('\n');
        Logger.LogInfo($"Atlas文件行数: {atlasLines.Length}\n");
        // 输出atlasLines前五个元素
        for (int i = 0; i < Math.Min(5, atlasLines.Length); i++)
        {
            Logger.LogInfo($"Atlas行 {i}: {atlasLines[i]}");
        }
        // bool haslines = string.IsNullOrWhiteSpace(atlasLines[0]) && !atlasLines[0].StartsWith(" ") && atlasLines[0].EndsWith(".png", StringComparison.OrdinalIgnoreCase);
        // if (!haslines)
        // {
        //     Logger.LogError("Atlas文件格式不正确，无法解析纹理页");

        // }
        // 创建材质列表
        List<Material> materials = [];

        // 尝试找到并加载Atlas引用的所有纹理
        string currentPageName = null;
        // TODO: 将参数更改为imagepath，并使用path提取文件名
        string atlasDir = Path.GetDirectoryName(atlasPath);
        string imageName = Path.GetFileName(texturepath);
        currentPageName = string.IsNullOrEmpty(imageName)
            ? null
            : (imageName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                ? imageName
                : null
                );
        
        Logger.LogInfo($"当前图片纹理名称: {currentPageName}\n");
        // string texturePath = Path.Combine(atlasDir, currentPageName);
        Logger.LogInfo($"尝试加载纹理图片: {texturePath}\n");

        try
        {
            if (File.Exists(texturePath))
            {
                // 加载纹理
                byte[] textureData = File.ReadAllBytes(texturePath);
                Texture2D texture = new(2, 2); // 尺寸会在加载时重置
                if (texture.LoadImage(textureData))
                {
                    texture.name = Path.GetFileNameWithoutExtension(currentPageName);
                    Material mat = new(shader)
                    {
                        name = currentPageName + " Material",
                        mainTexture = texture
                    };
                    materials.Add(mat);
                    Logger.LogInfo($"成功加载纹理: {currentPageName}\n");
                }
                else
                {
                    Logger.LogError($"无法加载纹理: {texturePath}\n");
                }
            }
            else
            {
                Logger.LogError($"纹理文件不存在: {texturePath}\n");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"加载纹理时发生异常: {texturePath}, 错误: {ex.Message}\n{ex.StackTrace}");
        }




        // 如果没有找到任何材质，添加一个默认材质
        if (materials.Count == 0)
        {
            Logger.LogWarning("没有找到任何纹理，使用默认材质\n");
            materials.Add(new Material(shader));
        }

        // 创建Atlas资源
        var atlasAsset = SpineAtlasAsset.CreateRuntimeInstance(atlasTextAsset, [.. materials], true);
        if (atlasAsset == null)
        {
            Logger.LogError("创建Atlas资源失败\n");
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
