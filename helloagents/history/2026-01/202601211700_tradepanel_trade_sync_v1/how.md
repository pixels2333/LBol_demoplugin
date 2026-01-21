# TradePanel TODO: Trade Sync v1 - HOW (executed)

- 新增 Trade v1 消息类型：`OnTradeStartRequest/OnTradeOfferUpdateRequest/OnTradeConfirmRequest/OnTradeCancelRequest/OnTradeSnapshotRequest/OnTradeStateUpdate`。
- 新增 `TradeSyncPatch`：Host 维护会话状态并广播；Host 自己不收广播则本地补 Apply。
- `TradePanel` 订阅 `TradeSyncPatch.OnTradeStateUpdated` 刷新 UI，并在 Completed 时本地落地交易。
- 交易落地期间抑制部分 deck 同步补丁以避免冗余广播。

