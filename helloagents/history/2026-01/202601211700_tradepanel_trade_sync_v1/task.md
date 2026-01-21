# TradePanel TODO: Trade Sync v1 - TASK (executed)

## 执行结果

### T1. 定义 Trade 事件类型
- [√] `NetworkMessageTypes` 增加 Trade v1 常量：Start/Offer/Confirm/Cancel/State/Snapshot。

### T2. 实现 `TradeSyncPatch`
- [√] 新增 `networkplugin/Patch/Network/TradeSyncPatch.cs`。
- [√] Host 权威裁决 + 广播，Host 本地补 Apply（服务端排除发送方）。
- [√] Snapshot 请求/响应：`OnTradeSnapshotRequest` -> `OnTradeStateUpdate`。

### T3. TradePanel 接入与落地
- [√] `TradePanel` offer/confirm/cancel 走 `TradeSyncPatch`。
- [√] 模型A：完成后每个客户端只修改自己的 deck（移除自己报价、创建并加入对方报价）。

### T4. 抑制冗余广播
- [√] 交易落地期间抑制 `PotionToolSyncPatch` 的 tool deck 增删广播。

### T5. 打开面板补丁
- [√] 修正 `GapOptionsPanel_Patch.GetOrCreateTradePanel()`：不再裸创建缺少 UI 引用的 TradePanel；优先 GetPanel/FindObject，缺失则提示。

### T6. 验证
- [√] `dotnet build networkplugin/NetWorkPlugin.csproj -c Release` 通过。
- [-] `dotnet build simplesolution.sln` 失败：`lbol/` 源码存在大量语法错误/框架引用不匹配（与本次改动无关）。

