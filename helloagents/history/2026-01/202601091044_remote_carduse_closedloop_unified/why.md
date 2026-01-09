# 变更提案: 远端队友目标出牌闭环（目标端结算 + 结算后快照广播）

## 需求背景
联机中希望“对远端队友使用可选目标的牌”（典型为 `TargetType.SingleEnemy`）时：
- 发送端不在本地结算（避免双算/状态不一致），而是发送网络事件。
- 所有客户端都能看到施法/受击等动画。
- 被选中的目标玩家（目标端）在自己客户端完成结算，然后把结算后的状态（HP/护盾/格挡/状态等）广播给所有人同步显示。

前提更新：**敌人已被同步且是共享对象**，不存在“那边的敌人”。因此本闭环仅处理“把远端队友当作可选目标”，结算目标固定为目标端自身 `battle.Player`。

## 推荐方案（已执行）
方案（已执行的推荐方案）: “目标端结算 + 结算后快照广播”闭环
- 发送端：选中队友 → 拦截 `Card.GetActions` → 生成动作蓝图 → 发 `OnRemoteCardUse` → 本地播放施法动画（并补目标预动画）
- 接收端（全员）：收到 `OnRemoteCardUse` → 播放施法者/受击动画
- 目标端：按蓝图结算 → 发 `OnRemoteCardResolved`（带 `RequestId + ResolveSeq + Timestamp + 状态快照`）
- 全员：收到 `OnRemoteCardResolved` → 按序/去重丢弃旧包 → 更新目标远端 `UnitView` 状态

## 必须收尾点（可靠性与一致性）
1. **失败回退**：未连接/发送失败时，入口阻断或 best-effort 回退，避免扣费/移卡后卡死。
2. **Resolved 乱序丢弃**：按 `ResolveSeq`/`Timestamp`/`RequestId` 去重与丢弃旧包，避免状态回滚。
3. **施法动画一致**：远端玩家也能看到“本地玩家在他人视角的施法动作”，发送端也能看到目标预动画（因服务端可能不回显给发送者）。

## 影响范围
- `networkplugin/Patch/Network/RemoteCardUsePatch.cs`
- `networkplugin/Patch/UI/PlayerTargeterPatch.cs`
- `networkplugin/Patch/UI/RemotePlayerProxyEnemy.cs`
- `networkplugin/Patch/UI/OtherPlayersOverlayPatch.cs`

## 风险评估
- 动作蓝图覆盖面：当前仅支持部分 Action 类型（伤害/治疗/上状态），遇到更复杂的牌需要扩展蓝图或改为“发送端直接发送最终数值”。
- 回退逻辑 best-effort：发送失败时不能保证 100% 复原卡牌位置/资源，但通过入口阻断可覆盖主要路径。
- 非确定性：若卡牌伤害依赖更多上下文（遗物/被动/临时数值），需要补充序列化字段或改成“发送端算完发结果”。
