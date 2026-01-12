# Changelog

本文文件记录项目所有重要变更（Keep a Changelog 风格）。

## [Unreleased]

### 新增
- 远端目标出牌链路增强：未连接阻断/发送失败回退。Resolved 乱序丢弃（ResolveSeq/Timestamp/RequestId）。动画一致性改进。
- 文档：补充并归档“远端队友目标出牌闭环（目标端结算 + 快照广播）”方案包。
- 断线重连：补齐 `ReconnectionManager` TODO，并补齐 `FullStateSyncRequest/FullStateSyncResponse` 走 GameEvent 通道（Host/Relay/Client，待手动联机验证）。
- NetworkManager：清理过期 TODO 注释，并补齐 `UpdatePlayerInfo(object)` 作为兼容入口（复用既有 JSON 更新逻辑）。
- MidGameJoin：落地 `MidGameJoinRequest/Response` + 通过 Relay `DirectMessage` 跑通 FullSync 请求/响应，并加入最小事件追赶回放（失败降级）。

### 修复
- 修复 `MidGameJoinManager` 编译错误：将 `GetRoomStatus(...)` 引用改为 `GetRoomInfo(...)`。
