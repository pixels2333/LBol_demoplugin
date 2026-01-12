# API 手册（网络事件）

本项目的网络同步主要通过 `NetworkMessageTypes` 中定义的事件类型进行广播。

## 远端目标出牌
- `OnRemoteCardUse`: 发送端对远端队友出牌（目标端结算入口）。
- `OnRemoteCardResolved`: 目标端结算完成后广播状态快照（用于所有客户端更新远端玩家显示）。

