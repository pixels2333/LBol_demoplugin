# 技术设计: 远端目标出牌链路收尾

## 失败回退
### 未连接阻止（主路径）
- 在 `BattleController.RequestUseCard(Card, UnitSelector, ManaGroup, bool)` 前置补丁中检测：
  - `selector.Type == SingleEnemy && selector.SelectedEnemy is RemotePlayerProxyEnemy`
  - `INetworkClient` 未连接
- 命中则 `ShowTopMessage` 提示并 `return false` 阻止出牌流程，避免扣法力/移动卡牌。

### 发送失败回退（best-effort）
- 在 `Card.GetActions` 拦截中发送 `OnRemoteCardUse` 后检查发送是否成功：
  - 若失败：提示，尝试退还法力（仅当卡已进入 PlayArea），并尝试将卡移回手牌（手满则丢弃）。

## OnRemoteCardResolved 防回滚
### 发送端
- 目标端结算完成后广播：
  - `ResolveSeq = Interlocked.Increment(ref _resolvedSeq)`
  - `Timestamp`
  - `RequestId`（沿用 OnRemoteCardUse）
  - 目标端状态快照（Hp/Block/Shield/StatusEffects）

### 接收端
- 按 `TargetPlayerId` 维度维护：
  - `LastResolveSeq`
  - `LastTimestamp`
  - `ProcessedRequestIds`（上限 256，避免内存增长）
- 到达时：
  - `RequestId` 已处理 → 丢弃
  - `ResolveSeq <= LastResolveSeq` → 丢弃
  - 否则更新 last 并应用快照
- 断线时清空上述缓存。

## 动画一致性
- 接收端：`OnRemoteCardUse` 中根据 `SenderPlayerId` 找到远端施法者 `UnitView` 播放动画。
- 发送端：由于服务端不会回显事件给发送者，本地补一次对目标的预动画（hit/spell）。

