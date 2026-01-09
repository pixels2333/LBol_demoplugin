# 技术设计: 远端队友目标出牌闭环（目标端结算 + 快照广播）

## 总览
### 关键约束
- `TargetType.SingleEnemy` 的目标类型要求目标为 `EnemyUnit`（`UnitSelector.SelectedEnemy`）。
- 联机模型中敌人是共享已同步对象，因此“远端队友目标出牌”仅表示：把远端玩家当作目标，不存在“那边的敌人”。
- 因此本闭环的结算目标固定为目标端自身 `battle.Player`。

### 组件与职责
- `RemotePlayerProxyEnemy : EnemyUnit`
  - 仅作为选目标管线的类型适配器，内部持有 `RemotePlayerId`。
- `PlayerTargeterPatch`
  - 在 `TargetSelector` 中让指向远端队友 `UnitView` 时返回/设置 `RemotePlayerProxyEnemy`。
- `OtherPlayersOverlayPatch`
  - 提供通过 `playerId` 查找远端角色 `UnitView` 的能力，用于播动画与应用快照到 UI。
- `RemoteCardUsePatch`
  - 发送端：拦截 `Card.GetActions`，目标为 `RemotePlayerProxyEnemy` 时仅发送 `OnRemoteCardUse`。
  - 接收端：全员播动画；目标端结算并广播 `OnRemoteCardResolved`；全员按序/去重应用快照。

## 协议设计
### OnRemoteCardUse（使用事件）
- `RequestId`: string
- `Timestamp`: long（ticks）
- `SenderPlayerId`: string
- `TargetPlayerId`: string
- `CardId`: string
- `Upgraded`: bool
- `CardType`: string（用于动画选择）
- `SenderStatusEffects`: []（用于目标端计算 buff/debuff 影响）
- `Actions`: 动作蓝图（JSON array；每项包含 kind + 参数）

### OnRemoteCardResolved（结算快照）
- `RequestId`: string（沿用 use）
- `ResolveSeq`: long（目标端本地单调递增；用于乱序丢弃）
- `Timestamp`: long（兜底排序）
- `TargetPlayerId`: string
- `Hp/MaxHp/Block/Shield`: number
- `StatusEffects`: []（目标端结算后的最终状态）

## 执行流程
1. 发送端选中远端队友：`TargetSelector` 指向远端 `UnitView` → 返回 `RemotePlayerProxyEnemy`。
2. 发送端出牌：拦截 `Card.GetActions` → 生成原始 actions → 转换为动作蓝图 → `SendGameEventData(OnRemoteCardUse)`。
3. 全员播动画：收到 `OnRemoteCardUse` 后，定位 `SenderPlayerId` 的远端 `UnitView` 播放施法动画；定位目标远端 `UnitView` 播放受击/预动画。
4. 发送端补动画：发送端额外补播放目标预动画（避免服务端不回显给发送端）。
5. 目标端结算：若 `TargetPlayerId == selfId`，按蓝图在 `battle.Player` 上执行伤害/治疗/上状态，随后广播 `OnRemoteCardResolved` 快照。
6. 全员应用快照：收到 `OnRemoteCardResolved` 后按 `RequestId/ResolveSeq/Timestamp` 去重与丢弃旧包，更新目标远端 `UnitView` HUD/状态显示。

## 可靠性与一致性设计
### 未连接阻断（主路径）
- 在 `BattleController.RequestUseCard` 前置补丁中，当目标为 `RemotePlayerProxyEnemy` 且网络未连接时 `return false` 阻断并提示，避免扣费/移卡。

### 发送失败回退（best-effort）
- 在 `Card.GetActions` 拦截发送 `OnRemoteCardUse` 后若失败：提示；仅在卡已进入 PlayArea 时尝试返还法力；尝试移回手牌（手满则弃牌）。

### Resolved 防回滚（乱序/重复丢弃）
- 目标端发 `OnRemoteCardResolved` 时携带 `RequestId + ResolveSeq + Timestamp`。
- 全员按 `TargetPlayerId` 维护：`LastResolveSeq`、`LastTimestamp`、`ProcessedRequestIds(上限256)`。
- 到达时：
  - `RequestId` 已处理 → 丢弃
  - `ResolveSeq <= LastResolveSeq` → 丢弃
  - 否则更新 last 并应用快照
- 断线/重连时清空上述缓存，避免新会话误丢弃。

## 安全与性能
- 输入校验：JSON 缺字段/非法值时跳过动画/应用，不能崩溃。
- 去重集合上限：每个 target 的 `RequestId` 集合上限 256；超限清理。

## 验收
- 单机：模拟 `OnRemoteCardUse/Resolved`，验证乱序丢弃与去重不会回滚。
- 联机（推荐）：A 对 B 出牌；A/B/旁观者均看到 A 施法与 B 受击；B 结算后快照同步到所有人；断线重连后不回滚。
