# 任务清单: fix_endturn_gameevent_routing

目录: `helloagents/plan/{YYYYMMDDHHMM}_fix_endturn_gameevent_routing/`

> **@status:** completed | 2026-01-28 21:32

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
总任务: 6
已完成: 3
完成率: 50%
```

---

## 任务列表

### 1. {阶段/模块名称}

- [√] 1.1 在 `networkplugin/Network/Server/NetworkServer.cs` 的 `IsGameEvent(string messageType)` 中将 `EndTurnRequest/EndTurnStatus/EndTurnConfirm/CardStateChanged` 视为 GameEvent
  - 验证: 本地复现时服务器不再打印 `未知系统消息类型: EndTurn*`，且回合可正常结束

- [√] 1.2 在 `networkplugin/Network/Server/RelayServer.cs` 的 `IsGameEvent(string messageType)` 中将 `EndTurnRequest/EndTurnStatus/EndTurnConfirm/CardStateChanged` 视为 GameEvent
  - 依赖: 1.1

### 2. 验证与回归

- [√] 2.1 运行一次 `dotnet build` 确认编译通过
  - 验证: 构建成功无错误

- [ ] 2.2 手动验证 Host 单人结束回合逻辑（对照日志）
  - 验证: 不再出现 `未知系统消息类型: EndTurn*`，回合正常推进

- [ ] 2.3 （可选）验证 Relay 模式下 EndTurn 与 CardStateChanged 不被误判
  - 验证: Relay 端日志无 unknown type，事件可转发

- [√] 2.4 记录本次修复到 `helloagents/CHANGELOG.md`
  - 验证: CHANGELOG 有一条修复记录（含模块与影响说明）

---

## 执行备注

> 执行过程中的重要记录

| 任务 | 状态 | 备注 |
|------|------|------|
