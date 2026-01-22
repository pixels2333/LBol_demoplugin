# 任务清单: resurrect_panel_sync_v1

目录: `helloagents/plan/无_resurrect_panel_sync_v1/`

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

状态标记: [√] 已完成, [-] 已跳过

- [√] 补齐 PlayerId：`INetworkPlayer.playerId` + `LocalNetworkPlayer.playerId`（来自 `NetworkIdentityTracker`）。
- [√] 新增死亡登记册：`DeathRegistry`，由网络事件汇总供 Gap UI 展示。
- [√] 新增复活同步：`ResurrectSyncPatch`（请求/失败/结果）。
- [√] UI 接入：`GapOptionsPanel_Patch` 构造 payload；`ResurrectPanel` 发起请求并在回调中扣费/提示。
- [√] 防回环与 Host 自身广播缺失修复：`DeathPatches` 增加 `SuppressNetworkSync` 并本地更新 `DeathRegistry`。
- [√] 构建验证：`dotnet build networkplugin/NetWorkPlugin.csproj -c Release` 成功。

- [-] 手动双开冒烟：本次未执行（可按需要补充）。

---

## 执行备注

> 执行过程中的重要记录

| 任务 | 状态 | 备注 |
|------|------|------|
