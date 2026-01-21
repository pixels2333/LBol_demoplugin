# 项目技术约定

## 技术栈
- 语言: C#
- 形态: LBoL Mod / Harmony Patch / LiteNetLib 网络

## 开发约定
- 代码风格: 以项目现有风格为准，优先最小改动。
- Patch 约定: HarmonyPatch 类尽量保持单一职责，避免跨模块耦合。
- 网络事件: 明确事件来源与接收端职责（谁发送/谁结算/谁广播）。

## 测试与验证
- `networkplugin/NetWorkPlugin.csproj` 可通过 `dotnet build` 验证（当前仍有既有 warnings）。
- 全仓库/解决方案的完整构建可能受外部依赖与工程配置影响；涉及运行时行为的改动仍建议进行手动联机验证。

