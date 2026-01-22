# 任务清单: tradepanel_trade_sync_v2

目录: `helloagents/plan/无_tradepanel_trade_sync_v2/`

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

## 任务清单（设计态；你确认“开始实施方案”后才会改代码）

### T1. 收敛单机 TODO 的语义
- [√] 仅联机可用：未连接时禁用/提示（并避免执行单机交易分支）。
- [√] 更新 `TradePanel.ExecuteTrade()` / `OnConfirmTrade()` 的分支与提示语，确保未联网无法触发本地落地。

### T2. TradeSyncPatch v2：请求去重与乱序保护
- [√] 在 `TradeSyncPatch` Host 侧加入 RequestId 去重（per trade 滑动窗口）。
- [-] 可选：对 Timestamp 做基本窗口校验（防极端乱序）。

### T3. TradeSyncPatch v2：权限/参与者校验
- [√] StartTrade：拒绝无效参与者（空/相同/不在房间）。
- [√] Offer/Confirm/Cancel/Snapshot：拒绝非参与者。
- [√] 复用 `networkplugin/Network` 的玩家列表来源（优先用 NetworkManager；次选仅做 IsParticipant 校验）。

### T4. Offer 合法性校验（按你确认的取舍）
- [√] L1（必须）：结构校验
  - 卡牌/道具：CardId 非空、数量<=Max、去重
  - 金币：>=0（可选限制上限）
  - Exhibit：ExhibitId 非空、去重
- [√] 不做 Host 侧 InstanceId 归属校验。

### T5. Completed 落地（严格失败语义）
- [√] 卡牌/道具：移除自己报价使用 InstanceId 精确查找；找不到即失败（不做模糊匹配）。
- [√] 金币：出金币使用 `ConsumeMoney`；不足即失败。
- [√] Exhibit：按 ExhibitId 在 `Player.Exhibits` 中查找并 `LoseExhibit`；找不到/不可失去/重复获得即失败。
- [√] 确保交易落地期间所有 deck 同步补丁都尊重 `TradeSyncPatch.IsApplyingTrade`。

### T5.1 Exhibit 可交易过滤（Losable + 黑名单）
- [√] 默认仅允许 `LosableType == Losable` 的 Exhibit 进入报价。
- [√] 增加“不可交易 Exhibit 黑名单”：黑名单命中则禁止交易（即使 Losable 也禁止）。
- [√] 黑名单实现方式：枚举值名 = `Exhibit.Id`（你已确认 Exhibit.Id 为纯英文字母）。

### T6. 扩展协议为多物品交易
- [√] 扩展 `TradeSessionState` 与消息 payload：支持 Money 与 Exhibit 列表。
- [√] TradePanel 发送 OfferUpdate 时带上金币/Exhibit/卡牌(含 Tool) 的报价。
- [√] UI 展示对方的金币与 Exhibit。

### T7. UI：选择玩家（你已确认需要）
- [√] 新增选择 Player2 的 UI 流程（从房间玩家列表中选择）。
- [√] 选择后发起 `RequestStartTrade`，并刷新双方名称显示。

### T8. UI Patch 一致性
- [√] 统一 GapOptions 与 ShopTradeIcon 的“TradePanel 不可用”提示与判定逻辑。

### T9. 验证点（实施后执行）
- [ ] 两端交易：offer(A)、offer(B)、confirm(A)、confirm(B) -> completed，双方 deck 对齐。
- [ ] Offer 后再改 Offer：确认应被重置。
- [ ] Cancel：双方 UI 关闭/状态提示一致。
- [ ] Snapshot：中途打开面板能拉到 lastKnown 并刷新。
- [ ] 幂等：重复发同一 RequestId 不应重复推进状态。
- [ ] 金币交易：A 出 X，B 出 Y，完成后双方 Money 变化正确。
- [ ] Exhibit 交易：双方各给 1 件 exhibit，完成后双方 exhibit 列表变化正确。

## 预计涉及文件
- `networkplugin/UI/Panels/TradePanel.cs`
- `networkplugin/UI/Panels/TradePayload.cs`
- `networkplugin/Patch/Network/TradeSyncPatch.cs`
- `networkplugin/Patch/UI/GapOptionsPanel_Patch.cs`
- `networkplugin/Patch/UI/ShopTradeIconPatch.cs`
- （可选）`helloagents/wiki/modules/networkplugin.md`（补充 trade 章节）

---

## 执行备注

> 执行过程中的重要记录

| 任务 | 状态 | 备注 |
|------|------|------|
