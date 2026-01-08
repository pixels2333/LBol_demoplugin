# API 手册

本文记录项目内部关键接口与主要调用路径（用于调用约定与模块解耦）。具体实现以代码为准。

> 约定：补丁/模块通过 `Network/ModService.cs` 的 `ModService.ServiceProvider` 获取依赖（`GetService<T>()`），并在联机逻辑前检查 `INetworkClient.IsConnected`。

## 1) INetworkClient
位置：`Network/Client/INetworkClient.cs`

- 连接与生命周期：`Start()`、`ConnectToServer(host, port)`、`PollEvents()`、`Stop()`
- 发送：
  - `SendGameEventData(eventType, eventData)`（事件通道）
  - `SendRequest(header, data)`（请求/响应通道，payload 常为 JSON 字符串）
- 连接配置：`SetConnectionTimeout(seconds)`、`EnableAutoReconnect(...)`
- 查询：`GetConnectionStats()`、`GetSelf()`
- 属性：`IsConnected`

默认实现：
- `Network/Client/NetworkClient.cs`：LiteNetLib + `System.Text.Json` 序列化/反序列化。

## 2) INetworkManager
位置：`Network/Client/INetworkManager.cs`

- 玩家管理：`RegisterPlayer(player)`、`RemovePlayer(id)`
- 查询：`GetSelf()`、`GetPlayer(id)`、`GetPlayerByPeerId(peerId)`、`GetAllPlayers()`、`GetPlayerCount()`
- 属性：`IsConnected`（默认实现为“玩家数>0”）

相关数据模型：
- `Network/PlayerEntity.cs`：本地/远端玩家的统一状态实体（HP、法力、位置、是否房主/在线等）。

## 3) ISynchronizationManager
位置：`Core/ISynchronizationManager.cs`

- 主入口：`SyncGameEventToNetwork(gameEvent)`、`ProcessEventFromNetwork(eventData)`
- 常用同步：`SendCardPlayEvent(...)`、`SendManaConsumeEvent(...)`、`SendGapStationEvent(...)`、`SendGameEvent(...)`
- 全量同步：`RequestFullSync()`
- 连接事件：`OnConnectionLost()`、`OnConnectionRestored()`
- 统计：`GetSyncStatistics()`、`GetEventBufferStatistics()`

默认实现：
- `Core/SynchronizationManager.cs`：事件队列、远端事件缓冲（防乱序/重复）、状态缓存与过期清理。

## 4) INetworkPlayer
位置：`Network/NetworkPlayer/INetworkPlayer.cs`

- 职责：代表“本地玩家”的联机身份与同步状态（与 `PlayerEntity` 共同构成玩家层）。

## 5) HostAuthorityManager（房主权威）
位置：`Authority/HostAuthorityManager.cs`

- 职责：处理“需要房主仲裁”的请求；校验请求格式/权限/冲突；生成并广播权威动作；提供重连快照。

## 6) MidGameJoinManager / ReconnectionManager
位置：
- `Network/MidGameJoin/MidGameJoinManager.cs`
- `Network/Reconnection/ReconnectionManager.cs`

- MidGameJoin：局内加入申请、审批、快照追赶、AI 代打等（按配置）。
- Reconnection：心跳/断线检测、事件历史、周期快照、重连快照下发等。

## 7) NetworkMessageTypes（消息类型常量）
位置：`Network/Messages/NetworkMessageTypes.cs`

- 定义各类同步消息的 `eventType` 常量（系统、卡牌、法力/能量、战斗、地图、存档等）。
- `MessageCategories` 给出分类集合，便于按类别过滤/优先级处理。
- `Network/Messages/MessagePriorities.cs` 提供 messageType → priority 的映射（心跳/全量同步/聊天等）。

## 8) Patch 编写约定（对外“API”）
- Patch 入口统一放在 `Patch/`，按功能分类（`Patch/Network/`、`Patch/UI/` 等）。
- Patch 必须做到：
  - 单机不影响：联机判断失败时 `return true`（继续原方法）或 Postfix 不修改结果。
  - DI 安全：`ModService.ServiceProvider` 可能为 null，取服务时必须 `try/catch` 或判空。
  - 序列化稳定：网络负载尽量使用可 JSON 序列化的匿名对象/DTO。

