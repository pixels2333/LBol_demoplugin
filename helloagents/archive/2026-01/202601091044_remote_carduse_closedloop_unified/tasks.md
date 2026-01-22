# 任务清单: remote_carduse_closedloop_unified

目录: `helloagents/plan/无_remote_carduse_closedloop_unified/`

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

﻿# 任务清单: 远端队友目标出牌闭环（目标端结算 + 快照广播）

目录: `helloagents/plan/202601091044_remote_carduse_closedloop_unified/`

---

## 1. 闭环实现核对（按现有代码对齐/验收）
- [√] 1.1 核对 `RemotePlayerProxyEnemy` + `PlayerTargeterPatch`：远端队友可被 `SingleEnemy` 选中，且不影响原生敌人选中
- [√] 1.2 核对发送端：仅目标为 `RemotePlayerProxyEnemy` 时拦截 `Card.GetActions`，成功发送 `OnRemoteCardUse` 后不进行本地结算
- [√] 1.3 核对全员动画：收到 `OnRemoteCardUse` 能定位施法者/目标 `UnitView` 并播放动画；发送端补播放目标预动画
- [√] 1.4 核对目标端结算：仅 `TargetPlayerId == selfId` 时按蓝图在 `battle.Player` 上结算，并广播 `OnRemoteCardResolved`
- [√] 1.5 核对全员应用快照：`OnRemoteCardResolved` 按 `RequestId/ResolveSeq/Timestamp` 去重、乱序丢弃旧包，并更新目标远端 `UnitView`

## 2. 可靠性加固（必须项）
- [√] 2.1 未连接阻断：在 `BattleController.RequestUseCard` 入口阻断远端队友目标出牌（提示 + 不扣费/不移卡）
- [√] 2.2 发送失败回退：在 `Card.GetActions` 拦截中发送失败时 best-effort 回退（提示/可能返还/回手或弃牌）
- [√] 2.3 断线清缓存：断线/重连时清空 per-target 缓存（seq/timestamp/requestId）

## 3. 安全检查
- [√] 3.1 消息字段校验：缺失/非法值不崩溃
- [√] 3.2 去重结构上限：集合不无限增长

## 4. 文档归档
- [√] 4.1 执行完成后迁移方案包到 `helloagents/history/YYYY-MM/` 并更新 `helloagents/history/index.md`

---

## 执行备注

> 执行过程中的重要记录

| 任务 | 状态 | 备注 |
|------|------|------|
