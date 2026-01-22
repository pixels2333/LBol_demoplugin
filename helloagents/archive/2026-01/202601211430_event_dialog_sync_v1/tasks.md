# 任务清单: event_dialog_sync_v1

目录: `helloagents/plan/无_event_dialog_sync_v1/`

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
总任务: (已归档)
已完成: (参考原任务列表)
完成率: (参考原任务列表)
```

---

## 任务列表

状态标记: [ ] 待做, [~] 进行中, [√] 已完成, [-] 跳过

## 阶段 B：确定性落地（apply）
- [√] `OnEventSelection`：pending + apply + 幂等去重 + 清理 pending。
- [√] `OnDialogOptions`：接收端缓存 options；用于 OptionIndex->OptionId 映射补偿。

## 阶段 C：断线重连/中途加入追赶（v1 最小可用）
- [√] Host：缓存最近 options/selection，并在 options 阶段周期性重发 options。
- [√] Host：在 `PlayerJoined/Welcome/PlayerListUpdate` 到来时限频重发最小快照。

## 阶段 D：投票模式打磨
- [√] 投票启用策略：仅手动注册 `eventId`。
- [√] 非 Host：接收 `OnEventVotingResult` 保持为“等待 selection 推进”的诊断路径。

## 阶段 E：验证
- [√] `dotnet build networkplugin/NetWorkPlugin.csproj -c Release`
- [-] `mvn -B test/verify`：当前 bash 环境未安装 `mvn`，无法执行。

## 备注 / 约束
- [√] 允许非 Host 上报事件开始（权威模型 B），但最终选择仍由 Host 仲裁并广播。
- [√] 中途加入只关注地图节点恢复边界；进入关卡/关卡内同步由其他模块负责。

---

## 执行备注

> 执行过程中的重要记录

| 任务 | 状态 | 备注 |
|------|------|------|
