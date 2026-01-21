# TradePanel TODO: Trade Sync v2 - WHY

## 背景
当前 `networkplugin/UI/Panels/TradePanel.cs` 中仍存在一个 TODO：
- `ExecuteTrade()` 的单机分支里，对玩家2的卡牌只做了本地 `AddDeckCard`，缺少“从对方卡组移除”的真实多玩家卡组操作。

与此同时，联机分支已经基本改造为：
- Host 权威裁决 + 广播状态（`networkplugin/Patch/Network/TradeSyncPatch.cs`）。
- 客户端在交易完成后，仅对“自己本地 GameRun 的 deck”落地（remove 自己报价，create+add 对方报价）。

因此 v2 的目标不是再造一套体系，而是：
1) 明确 TODO 的真实含义与范围（单机演示 vs 联机交易）。
2) 补齐 v1 方案的安全性/一致性/可维护性，完善“补丁方法”（TradeSyncPatch / UI Patch / 校验策略）。
3) 将设计沉淀到方案库，供你调整后再进入实施。

## v2 目标
- 明确并收敛 TradePanel 在“非联机”时的行为：仅联机可用（未连接则禁用并提示）。
- 扩展交易范围：卡牌、道具（Tool 卡）、金币、Exhibit 都可交易。
- 强化状态机规则与错误语义：找不到/不满足前置条件则直接视为失败（不做模糊匹配）。
- 复用 `networkplugin/Network` 现有通用设施（身份、玩家列表、消息类型等），避免冗余实现。

## 成功标准
- 联机：两端(或多端旁观)看到一致的 TradeSessionState；参与者能正常 offer/confirm/cancel；完成后双方 deck 对齐。
- 安全：Host 能拒绝明显非法请求（如非参与者发起、Offer 数量超限、重复/异常请求）。
- 维护：TradeSyncPatch 的职责边界清晰；TradePanel 的 TODO 不再“指向不存在的联机能力”。

## 约束（由你确认）
- 仅联机可用；不提供“旁观者发起交易”（仅参与者可发起与确认）。
- 不在 Host 侧校验 `InstanceId` 是否属于请求者；但客户端落地仍会用 `InstanceId` 做“移除自己报价”的精确定位。
- 不做模糊匹配：移除卡/道具、移除 exhibit、扣金币任一失败，视为交易失败（并在 UI 给出明确提示）。
