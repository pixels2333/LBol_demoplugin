# UI 模块

目录：`UI/`

## 职责
- 提供联机相关 UI：连接状态、聊天窗口、交易/复活面板等。
- 将网络状态与统计可视化：Ping、人数、连接状态、系统提示。

## 关键组件
- `UI/Components/NetworkStatusIndicator.cs`：连接状态面板（状态、Ping、人数、重连按钮等）
- `UI/Components/ChatUI.cs`：聊天窗口（发送/接收、消息滚动与淡出）

## 关键面板（节选）
- `UI/Panels/TradePanel.cs` / `TradePayload.cs`：交易面板（按联机同步策略使用）
- `UI/Panels/ResurrectPanel.cs` / `ResurrectPayload.cs`：复活面板（死亡状态与复活流程）
- `UI/Panels/DeathStateManager.cs`：死亡状态 UI 管理

