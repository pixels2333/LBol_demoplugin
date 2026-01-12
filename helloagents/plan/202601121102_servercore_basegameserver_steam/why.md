# 变更提案: 抽取 BaseGameServer（面向未来 Steam 联机）

## 需求背景
当前存在两套服务端实现：
- Host/直连：`networkplugin/Network/Server/NetworkServer.cs`
- Relay/中继：`networkplugin/Network/Server/RelayServer.cs`

两者基于同一底层网络事件模型（`ServerCore` 的 PeerConnected/PeerDisconnected/PeerLatencyUpdated/MessageReceived），但在会话管理、心跳、重连窗口、日志与部分系统消息上存在重复实现与差异化演进的风险。

后续如果接入 Steam 联机（SteamNetworkingSockets / Steam P2P / SDR），现有实现会面临两类成本：
1. **传输层替换成本高**：大量代码直接依赖 LiteNetLib 类型（`NetPeer/DeliveryMethod/NetDataWriter` 等）。
2. **业务语义难以复用**：Host/Relay 的“会话/消息分流/重连/心跳”等业务骨架并未形成清晰可复用层。

## 变更内容
1. 抽取 `BaseGameServer`（或 `GameServerBase`）：统一“会话/心跳/重连/消息分流”的业务骨架。
2. 为 `ServerCore` 引入最小接口（例如 `IServerCore`）：让业务层只依赖“事件 + Start/Stop/Poll”能力，为未来 Steam 实现替换/并行实现创造空间。
3. 以**渐进方式**减少 LiteNetLib 类型外溢：先将“核心业务流程”收敛到基类与组件内，再逐步抽象 peer/transport。

## 影响范围
- **模块:** `networkplugin/Network/Server`, `networkplugin/Network/Server/Core`, `networkplugin/Network/Room`, `networkplugin/Network/Messages`
- **文件:** `NetworkServer.cs`, `RelayServer.cs`, `PlayerSession.cs`（可能）, `ServerCore.cs`（可能新增接口/适配）
- **API:** 不改变现有消息 Type/字段（以兼容客户端与历史联机行为为约束）
- **数据:** 无持久化数据变更

## 核心场景

### 需求: 复用服务端通用业务骨架
**模块:** networkplugin/Network/Server
将重复的 session/心跳/重连/消息分流逻辑抽取，避免 Host/Relay 双线维护导致的行为漂移。

#### 场景: 玩家连接与 Welcome
Host/Relay 均能在连接时创建 `PlayerSession`、生成 `ReconnectToken`、发送 Welcome（字段可按各自模式保持兼容）。
- 预期结果：重复代码显著减少；新字段/新消息只需在一个位置实现。

#### 场景: 心跳与超时
- 预期结果：两种模式都遵循一致的 `LastHeartbeat/LastMessageAt` 更新与响应；避免某一侧遗漏导致掉线/卡顿。

#### 场景: 断线重连窗口
- 预期结果：两种模式的 token 校验与窗口判定一致；业务层统一暴露 `Reconnect_REQUEST/RESPONSE`（或保持已有消息类型）。

### 需求: 为未来 Steam 联机预留传输层替换点
**模块:** networkplugin/Network/Server/Core
业务层只依赖 `IServerCore`（事件与生命周期），未来可新增 `SteamServerCore` 实现同一接口。

#### 场景: 以 SteamTransport 替换 LiteNetLib
- 预期结果：新增 Steam 传输实现时，Host/Relay 的业务代码基本不动；仅替换/注入 core 实现与少量 peer/发送适配。

## 风险评估
- **风险:** 迁移过程中可能引入兼容性回归（消息字段、路由策略、线程模型差异）。
- **缓解:** 采用分阶段迁移；保持消息协议不变；增加最小“录包/回放”或关键路径手动联机回归清单；对 Host（主线程 Poll）与 Relay（后台线程）分别验证。

