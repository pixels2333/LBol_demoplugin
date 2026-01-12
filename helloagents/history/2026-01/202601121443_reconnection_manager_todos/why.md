# 变更提案: ReconnectionManager TODO 补全与断线重连打通

## 需求背景
当前项目已具备服务端侧的会话恢复能力（`ReconnectToken` + `Reconnect_REQUEST/RESPONSE`，Host/Relay 两种服务器均已实现），但 `networkplugin/Network/Reconnection/ReconnectionManager.cs` 仍处于“框架 + TODO”状态，且与现有的客户端连接事件、全量状态同步/追赶链路未完全对齐，导致断线后的体验仍不完整（重连后缺少可靠的状态恢复与事件追赶触发点）。

本变更目标是在尽量不破坏现有协议/结构的前提下：
- 补齐 `ReconnectionManager` 的 TODO（快照、事件历史、心跳/超时、令牌生成等）
- 把它与现有 `NetworkServer/RelayServer` 的重连令牌机制、以及客户端侧 `SynchronizationManager` 的“重连后全量同步请求”串起来，形成可验证的最小闭环。

## 变更内容
1. 补齐 `ReconnectionManager` 内的快照生成、快照存储限制、事件历史维护、令牌生成等 TODO，实现可被服务器侧调用的“重连数据准备”能力。
2. 打通重连后的同步链路：客户端在重连成功后触发（或继续沿用）全量同步请求；服务端能够响应并下发全量快照与必要的追赶事件。
3. 统一 Host/Relay 两种模式下的行为：断线标记、会话恢复、通知广播、以及重连后数据下发的消息类型/负载结构尽量一致。

## 影响范围
- **模块:** `networkplugin/Network/Reconnection/`、`networkplugin/Network/Server/`、`networkplugin/Network/Client/`、`networkplugin/Core/`
- **文件:**
  - `networkplugin/Network/Reconnection/ReconnectionManager.cs`
  - `networkplugin/Network/Client/NetworkClient.cs`（识别/接收重连与全量同步响应）
  - `networkplugin/Network/Server/NetworkServer.cs`（Host 模式：重连/全量同步请求处理）
  - `networkplugin/Network/Server/RelayServer.cs`（Relay 模式：重连/全量同步请求处理）
  - `networkplugin/Core/SynchronizationManager.cs`（重连后全量同步触发/消费）
  - `networkplugin/Network/Messages/NetworkMessageTypes.cs`（如需补齐消息白名单/优先级）
- **API:** 网络消息类型（仅在必要时新增/补齐；优先保持兼容）
- **数据:** 无持久化数据结构变更（以内存快照/事件历史为主）

## 核心场景

### 需求: Host 模式断线重连
**模块:** `NetworkServer` + `ReconnectionManager`
客户端掉线后在宽限期内重连，恢复到原 `PlayerId` 会话，并能触发状态恢复。

#### 场景: 客户端掉线后重连
前置条件：Host 已启动本地服务器，至少 2 名玩家在局内；客户端断网或主动断开后重新连接。
- 预期结果：服务端接受 `Reconnect_REQUEST` 并回复成功；`PlayerId` 不变；其他玩家收到“该玩家已重连”的通知；重连玩家收到全量状态/必要追赶事件。

### 需求: Relay 模式断线重连
**模块:** `RelayServer` + `ReconnectionManager`
Relay 多房间模式下，断线重连后玩家仍归属原房间并恢复会话。

#### 场景: 房间内客户端掉线后重连
前置条件：玩家已加入房间且房间处于游戏中；客户端掉线后在宽限期内重连。
- 预期结果：会话恢复成功，玩家仍在原 `roomId` 作用域；其他玩家看到该玩家重新上线；重连玩家收到快照与追赶事件。

### 需求: 重连后的全量同步与追赶
**模块:** `SynchronizationManager` + `ReconnectionManager`
重连后确保本地状态与房主权威状态一致，并补齐断线期间的关键事件。

#### 场景: 重连后自动发起 FullSync
前置条件：客户端完成重连并收到会话恢复成功响应。
- 预期结果：客户端自动发起 `FullStateSyncRequest`（或等效机制）；服务端回应 `FullStateSyncResponse`，包含快照与事件索引；客户端应用快照并从 `lastKnownEventIndex` 起追赶事件。

## 风险评估
- **风险:** 协议兼容性破坏（新增/改动消息类型导致旧逻辑无法处理）。
  - **缓解:** 优先复用既有消息管道与常量；新增消息加入白名单/判定分支且保持字段向后兼容。
- **风险:** 断线窗口/并发时序问题（重连与广播交错、快照与事件索引不一致）。
  - **缓解:** 以服务端会话恢复成功作为唯一“重连完成”判定点；快照与事件索引同源生成；共享结构加锁。
- **风险:** 安全性（重连令牌可预测、重连请求伪造）。
  - **缓解:** 使用 `RandomNumberGenerator` 生成高熵 token；服务端严格校验 token + 过期窗口；避免把任意 `object` 直接反序列化为可执行类型。

