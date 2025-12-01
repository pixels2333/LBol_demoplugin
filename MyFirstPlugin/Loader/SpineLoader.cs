using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using Spine.Unity;
using UnityEngine;

namespace MyFirstPlugin.Loader;

/// <summary>
/// Spine动画加载器类
/// 负责从文件系统或Resources目录加载Spine骨骼动画
/// 支持完整的Spine资源加载流程，包括JSON骨骼数据、Atlas图集和纹理贴图
/// </summary>
public class SpineLoader
{
    /// <summary>
    /// 日志源，用于记录加载过程中的信息和错误
    /// </summary>
    internal static ManualLogSource Logger;

    /// <summary>
    /// 从指定文件路径加载Spine动画
    /// 支持完整的资源加载流程，包括着色器查找、纹理加载、材质创建和骨骼数据初始化
    /// </summary>
    /// <param name="animator">目标SkeletonAnimation组件</param>
    /// <param name="jsonPath">Spine骨骼数据文件(.json)的完整路径</param>
    /// <param name="atlasPath">Spine图集文件(.atlas)的完整路径</param>
    /// <param name="texturepath">纹理图片文件的路径（可以是相对路径）</param>
    public static void LoadSpineAnimation(SkeletonAnimation animator, string jsonPath, string atlasPath, string texturepath)
    {
        // 记录开始加载的日志信息
        Logger.LogInfo($"加载Spine动画: JSON路径: {jsonPath}, Atlas路径: {atlasPath}");

        // 验证动画组件是否有效
        if (animator == null)
        {
            Logger.LogError("原动画组件为空");
            return;
        }

        // 清除现有的动画数据，准备加载新的动画
        animator.skeletonDataAsset = null;
        animator.Initialize(false); // 初始化时不加载骨骼数据，避免加载默认动画

        // 验证必需的文件是否存在
        if (!File.Exists(jsonPath) || !File.Exists(atlasPath))
        {
            Logger.LogError($"文件不存在: JSON: {jsonPath} 或 Atlas: {atlasPath}");
            return;
        }

        // 查找并配置Spine专用的着色器
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

        // 加载Atlas文本文件，用于解析纹理引用
        TextAsset atlasTextAsset = new(File.ReadAllText(atlasPath));
        string atlasText = atlasTextAsset.text;
        string[] atlasLines = atlasText.Split('\n');

        Logger.LogInfo($"Atlas文件行数: {atlasLines.Length}");

        // 输出Atlas文件的前几行内容用于调试
        for (int i = 0; i < Math.Min(5, atlasLines.Length); i++)
        {
            Logger.LogInfo($"Atlas行 {i}: {atlasLines[i]}");
        }

        // 创建材质列表，用于存储纹理材质
        List<Material> materials = [];

        // 解析纹理文件路径并加载纹理
        string currentPageName = null;
        string atlasDir = Path.GetDirectoryName(atlasPath);
        string imageName = Path.GetFileName(texturepath);

        // 验证并提取纹理文件名
        currentPageName = string.IsNullOrEmpty(imageName)
            ? null
            : (imageName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                ? imageName
                : null);

        Logger.LogInfo($"当前图片纹理名称: {currentPageName}");
        string texturePath = Path.Combine(atlasDir, currentPageName);
        Logger.LogInfo($"尝试加载纹理图片: {texturePath}");

        try
        {
            if (File.Exists(texturePath))
            {
                // 读取纹理文件数据
                byte[] textureData = File.ReadAllBytes(texturePath);
                Texture2D texture = new(2, 2); // 创建临时纹理，尺寸会在加载时自动重置

                if (texture.LoadImage(textureData))
                {
                    // 设置纹理名称并创建材质
                    texture.name = Path.GetFileNameWithoutExtension(currentPageName);
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
        catch (Exception ex)
        {
            Logger.LogError($"加载纹理时发生异常: {texturePath}, 错误: {ex.Message}\n{ex.StackTrace}");
        }

        // 如果没有找到任何纹理，创建一个默认材质作为后备
        if (materials.Count == 0)
        {
            Logger.LogWarning("没有找到任何纹理，使用默认材质");
            materials.Add(new Material(shader));
        }

        // 创建Spine Atlas资源实例
        var atlasAsset = SpineAtlasAsset.CreateRuntimeInstance(atlasTextAsset, [.. materials], true);
        if (atlasAsset == null)
        {
            Logger.LogError("创建Atlas资源失败");
            return;
        }

        // 加载Spine骨骼数据
        TextAsset jsonTextAsset = new(File.ReadAllText(jsonPath));
        var skeletonDataAsset = SkeletonDataAsset.CreateRuntimeInstance(jsonTextAsset, atlasAsset, true);

        // 将加载的骨骼数据应用到动画组件
        animator.skeletonDataAsset = skeletonDataAsset;
        animator.Initialize(true); // 重新初始化并加载骨骼数据

        // 验证初始化是否成功
        if (animator.Skeleton == null)
        {
            Logger.LogError("初始化后Skeleton仍为null");
            return;
        }

        Logger.LogInfo("Spine动画加载并初始化成功");
    }

    /// <summary>
    /// 从Unity Resources目录加载Spine动画
    /// 用于加载预打包在Resources目录中的Spine资源
    /// </summary>
    /// <param name="animator">目标SkeletonAnimation组件</param>
    /// <param name="jsonResourcePath">Resources目录中的骨骼数据资源路径</param>
    /// <param name="atlasResourcePath">Resources目录中的图集资源路径</param>
    public void LoadSpineAnimationFromResources(SkeletonAnimation animator, string jsonResourcePath, string atlasResourcePath)
    {
        // 从Resources目录加载骨骼数据资源
        var skeletonDataAsset = Resources.Load<SkeletonDataAsset>(jsonResourcePath);

        if (skeletonDataAsset == null)
        {
            Logger.LogError($"无法加载Spine数据: {jsonResourcePath}");
            return;
        }

        // 直接应用预配置的骨骼数据到动画组件
        animator.skeletonDataAsset = skeletonDataAsset;
        animator.Initialize(true);
    }
}