# Network 模块

目录：`Network/`

## 职责
- 客户端连接与消息发送：`INetworkClient` / `NetworkClient`
- 玩家与身份管理：`INetworkManager`、`INetworkPlayer`、`PlayerEntity`（在线/房主/位置/战斗状态等）
- 服务端/中继：`NetworkServer`、`RelayServer`、房间与会话（Room/Session）
- 状态能力：快照（Snapshot）、断线重连（Reconnection）、中途加入（MidGameJoin）
- 网络工具：NAT Traversal 等（按实现演进）

## 关键子目录
- `Network/Client/`：客户端与玩家管理
- `Network/Server/`：服务器端逻辑（玩家会话、广播、欢迎/心跳等）
- `Network/Room/`：房间模型与房间消息
- `Network/Snapshot/`：全量/局部状态快照模型
- `Network/Reconnection/`：断线检测与重连快照
- `Network/MidGameJoin/`：中途加入流程与追赶策略
- `Network/Event/`：GameEvent 与事件索引管理
- `Network/Utils/`：NAT traversal 等

## 重要类型（节选）
- `Network/Client/NetworkClient.cs`：连接、发送 `SendGameEventData`、请求响应（`SendRequest`）与事件分发
- `Network/Client/NetworkManager.cs`：玩家注册、查询、自身身份
- `Network/Server/NetworkServer.cs`：服务端广播、心跳、欢迎、玩家列表维护
- `Network/Server/RelayServer.cs`：中继服务器（房间创建/加入/踢人/转发/心跳/清理）
- `Network/PlayerEntity.cs`：玩家状态实体（含 IsHost/IsConnected/位置/战斗信息等）
- `Network/Snapshot/FullStateSnapshot.cs`：全量快照顶层 DTO
- `Network/SyncVar.cs`：可观察同步变量（值变化触发事件）

