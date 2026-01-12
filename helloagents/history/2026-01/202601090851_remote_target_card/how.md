# 技术设计: 远端队友代理目标出牌同步增强

## 关键链路
1. 目标选择：远端队友在 UI 上被鼠标指向时返回 `RemotePlayerProxyEnemy`，满足 `TargetType.SingleEnemy` 的目标系统。
2. 发送端出牌：在 `Card.GetActions` 拦截到目标为 `RemotePlayerProxyEnemy` 时，生成动作蓝图并发送 `OnRemoteCardUse`，本地只做动画不结算。
3. 全员播动画：所有客户端收到 `OnRemoteCardUse` 先播放施法者/受击动画；目标端再执行结算。
4. 目标端结算：按动作蓝图在目标端执行，并发送 `OnRemoteCardResolved`（状态快照）。
5. 全员更新：所有客户端收到 `OnRemoteCardResolved` 更新目标远端 UnitView 的 HP/格挡/护盾/状态。

## 失败回退
- 未连接：在 `BattleController.RequestUseCard` 入口直接阻止（保证主要出牌路径不扣费）。
- 发送失败：在 `Card.GetActions` 拦截内 best-effort 回退（提示、退法力、回收卡牌）。

## 乱序丢弃
- `OnRemoteCardResolved`：增加 `ResolveSeq`（目标端本地单调递增）。
- 接收端按 per-target 维护 last seq/timestamp，并用 `RequestId` 去重，丢弃旧包避免状态回滚。

