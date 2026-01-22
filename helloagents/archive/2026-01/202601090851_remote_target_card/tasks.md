# 任务清单: remote_target_card

目录: `helloagents/plan/无_remote_target_card/`

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

- [√] 1) 失败回退：未连接阻止 + 发送失败 best-effort 回退
- [√] 2) Resolved 防回滚：ResolveSeq + RequestId 去重 + Timestamp 兜底
- [√] 3) 动画一致性：远端看到本地玩家施法动作 + 发送端补目标预动画

---

## 执行备注

> 执行过程中的重要记录

| 任务 | 状态 | 备注 |
|------|------|------|
