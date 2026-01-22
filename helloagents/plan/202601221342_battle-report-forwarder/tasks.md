# 任务清单: battle_report_forwarder

目录: `helloagents/plan/202601221342_battle-report-forwarder/`

---

## 任务状态符号说明

| 符号 | 状态 | 说明 |
|------|------|------|
| `[ ]` | pending | 待执行 |
| `[√]` | completed | 已完成 |
| `[X]` | failed | 执行失败 |
| `[-]` | skipped | 已跳过 |
| `[?]` | uncertain | 待确认 |

---

## 执行状态
```yaml
总任务: 3
已完成: 3
完成率: 100%
```

---

## 任务列表

### 1. Network 事件转发补丁

- [√] 1.1 在 `networkplugin/Patch/Network/BattleReportForwardPatch.cs` 中实现 Host 侧 `BattlePlayer*Report -> BattlePlayer*Broadcast` 转发
  - 验证: Host 侧收到 `*Report` 时调用 `SendGameEventData(broadcastType, payload)`；仅处理 `BattlePlayer*Report`，忽略 `*Broadcast`

- [√] 1.2 在 `networkplugin/Patch/BattleController_Patch.cs` 中将 TODO 注释更新为 NOTE，指向已实现的 Host 转发补丁
  - 依赖: 1.1

### 2. 构建验证

- [√] 2.1 构建验证：`dotnet build networkplugin/NetWorkPlugin.csproj -c Release`
  - 结果: 通过（存在历史 warning，不影响本变更）

---

## 执行备注

> 执行过程中的重要记录

| 任务 | 状态 | 备注 |
|------|------|------|
| 1.1 | completed | 事件映射使用 `NetworkMessageTypes` 常量；payload 原样透传并做轻量去重 |
| 1.2 | completed | TODO 迁移为 NOTE，避免重复实现 |
| 2.1 | completed | 构建通过，输出 DLL: `networkplugin/bin/Release/netstandard2.1/NetworkPlugin.dll` |
