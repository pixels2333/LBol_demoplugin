# 项目技术约定

## 技术栈
- 运行时/平台: Unity 游戏（LBoL.exe），BepInEx 5 插件
- 语言: C#（TargetFramework: netstandard2.1）
- 运行时补丁: HarmonyX (2.14.0)
- 网络: LiteNetLib (0.8.3.1)
- 依赖注入: Microsoft.Extensions.DependencyInjection

## 版本来源
- 程序包版本: `NetWorkPlugin.csproj` 的 `<Version>`
- 插件对外版本: `PluginInfo.PLUGIN_VERSION`

## 代码约定
- 命名: 类型/方法/属性使用 PascalCase；私有字段使用 camelCase
- 日志: 通过 `Plugin.Logger`（BepInEx `ManualLogSource`）输出
- Patch: Harmony Patch 类集中在 `Patch/` 目录，按功能分模块

## 构建与输出
- 项目文件: `NetWorkPlugin.csproj`
- 常用命令:
  - `dotnet build -c Release`
 - 备注: 若构建失败，通常是项目内存在待修复的编译错误/缺失依赖（先修复再验证补丁）。

## 本地部署（运行在游戏内）
> 以 BepInEx 5 为例，最终以你的 LBoL 安装与插件加载方式为准。

- 构建产物：通常为 `bin/Release/netstandard2.1/NetworkPlugin.dll`（或 Debug 路径）
- 放置位置：`<LBoL>/BepInEx/plugins/NetworkPlugin/NetworkPlugin.dll`
- 日志：BepInEx 控制台或 `BepInEx/LogOutput.log`（使用 `Plugin.Logger` 输出）

## 知识库入口（SSOT）
- 概览：`wiki/overview.md`
- 架构：`wiki/arch.md`
- API：`wiki/api.md`
- 数据模型：`wiki/data.md`
- 模块文档：`wiki/modules/*.md`

## 兼容性与依赖
- 关键 NuGet 依赖: BepInEx.Core, HarmonyX, LiteNetLib, UnityEngine
- 游戏进程: `LBoL.exe`
