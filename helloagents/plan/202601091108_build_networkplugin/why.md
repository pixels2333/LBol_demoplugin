# 变更提案: 修复 networkplugin 构建错误

## 需求背景
- 当前仅构建 `networkplugin/NetWorkPlugin.csproj`（`dotnet build -c Debug --no-restore`）失败：11 个编译错误（另有 34 个警告）。
- 目标：先把 `networkplugin` 恢复到可编译状态，后续再逐步处理 warning/功能实现。

## 变更内容
1. 消除重复类型定义：`NetworkPlugin.Network.MidGameJoin.MidGameJoinConfig` 在两个文件重复定义。
2. 消除重复成员定义：`NetworkPlugin.Configuration.ConfigManager` 的多个配置项在不同 partial 文件中重复声明。
3. 修复 Harmony Patch 的可访问性不一致：public 方法参数/`out` 使用了 private 类型。

## 影响范围
- 仅影响 `networkplugin` 模块编译（不涉及发布/运行时行为的目标变更）。

## 风险评估
- 风险：移除/迁移配置项定义时可能遗漏某个配置项绑定。
- 缓解：以“只删重复、保留唯一来源”为原则；修复后重新 `dotnet build` 验证。
