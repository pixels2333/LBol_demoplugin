# 任务清单: networkplayer_dto_visitingnode_nullfix

目录: `helloagents/plan/无_networkplayer_dto_visitingnode_nullfix/`

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

- [ ] 确认 `dto/NetWorkPlayer` 是否被实际使用（避免修到未使用的类型）。
- [ ] 修复 `networkplugin/Network/NetworkPlayer/dto/NetWorkPlayer.cs` 构造函数：`location_X/location_Y` 对 `VisitingNode` 做空值回退，避免 `NullReferenceException`。
- [ ] （可选）审查并同步修复 `networkplugin/Network/NetworkPlayer/NetWorkPlayer.cs` 中同样的坐标初始化逻辑，避免另一个同名类仍触发空引用。
- [ ] 运行最小验证：编译/测试（优先跑项目现有的 .NET 构建或你常用的验证流程）。
- [ ] 更新知识库（如需要）：在 `helloagents/wiki/modules/` 记录 DTO/实体/SyncVar 分层约定与本次修复。

---

## 执行备注

> 执行过程中的重要记录

| 任务 | 状态 | 备注 |
|------|------|------|
