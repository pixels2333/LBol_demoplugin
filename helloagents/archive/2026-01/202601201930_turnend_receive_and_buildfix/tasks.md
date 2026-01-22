# 任务清单: turnend_receive_and_buildfix

目录: `helloagents/plan/无_turnend_receive_and_buildfix/`

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

- [√] A1. 盘点现有 OnGameEventReceived 订阅模式（参考 EndTurnSyncPatch），确定接收 patch 的放置目录与命名。
- [√] A2. 新增 TurnEnd 接收模块：订阅网络事件并过滤 `OnTurnEnd`。
- [√] A3. 解析 payload 为 TurnEndStateSnapshot（兼容 JsonElement / Dictionary<string,object>）。
- [√] A4. 设计并实现远端玩家状态落地：
      - [√] 先实现最小侵入缓存（playerId/userName -> snapshot）
      - [√] 写入远端玩家对象的基础字段（HP/block/shield/endturn/mana）
- [√] A5. 增加诊断日志：收到/解析失败/落地成功（包含 battleId/round/user 标识）。

- [√] B1. 定位所有 `INetworkPlayer.mana` 引用点并统计。
- [√] B2. 新增 `NetworkPlayerManaCompat`（扩展方法/反射读取）并提供默认值。
- [√] B3. 批量替换 `player.mana` 为兼容方法调用。
- [√] B4. `dotnet build networkplugin/NetWorkPlugin.csproj -c Release` 验证编译通过。

- [√] KB1. 更新知识库 `helloagents/wiki/modules/networkplugin.md`：补充 TurnEnd 快照链路的设计说明与边界。

---

## 执行备注

> 执行过程中的重要记录

| 任务 | 状态 | 备注 |
|------|------|------|
