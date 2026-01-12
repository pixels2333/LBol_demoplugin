# 技术设计: ReconnectionManager TODO 补全与断线重连打通

## 技术方案
### 核心技术
- C# / LiteNetLib / System.Text.Json
- 现有基础设施复用：`NetworkServer`、`RelayServer`、`PlayerSession`、`ReconnectToken`、`SynchronizationManager`、`GameEventManager`

### 实现要点
- **定位与职责收敛：** 将 `ReconnectionManager` 定位为“服务器侧断线重连数据管理器”，负责：
  - 断线时保存玩家快照（最小必要字段 + 事件索引）
  - 维护有限长度的事件历史用于追赶
  - 重连成功后向重连玩家准备“快照 + missed events”的负载
  - 广播重连通知（通过服务器侧的既有广播通道完成）
- **对齐现有重连机制：**
  - Host/Relay 服务器已具备 `Reconnect_REQUEST` 校验与会话恢复；`ReconnectionManager` 只在“断线记录/重连后数据下发”环节介入，避免重复校验逻辑。
- **对齐现有同步机制：**
  - 客户端侧 `SynchronizationManager.OnConnectionRestored()` 已会触发 `FullStateSyncRequest`；本次补齐服务端对 `FullStateSyncRequest` 的处理与 `FullStateSyncResponse` 的下发，并在客户端补齐对 `FullStateSyncResponse` 的识别与消费。
- **快照生成策略（最小闭环优先）：**
  - 先实现“可用于 UI/基础恢复”的快照字段（生命/护盾/格挡/金币/位置/是否战斗等），其余卡牌/宝物/药水/状态效果保持可扩展点（不在本次强行一次性做完所有细节）。
- **存储与性能：**
  - 玩家快照字典限制容量（按时间戳淘汰最旧）。
  - 事件历史 `SortedList<long, GameEvent>` 已有上限（`MaxHistoryEvents`），继续沿用并确保写入在锁内完成。
- **安全：**
  - `ReconnectToken` 生成使用 `System.Security.Cryptography.RandomNumberGenerator`，替换 `Guid.NewGuid()`。
  - 服务端对请求的 `PlayerId/Token/窗口期` 继续使用现有逻辑；`ReconnectionManager` 不下沉不可信反序列化。

## 架构设计
```mermaid
flowchart TD
    Client[NetworkClient] -->|Reconnect_REQUEST| Server[NetworkServer/RelayServer]
    Server -->|Reconnect_RESPONSE| Client
    Client -->|FullStateSyncRequest| Server
    Server -->|FullStateSyncResponse(snapshot+events)| Client
    Server --> RM[ReconnectionManager]
    RM -->|snapshot/events| Server
```

## 架构决策 ADR
### ADR-202601121443: ReconnectionManager 服务器侧化并与 FullSync 对齐
**上下文:** 现有 `ReconnectionManager.cs` 的 TODO 以“主机权威/中继服务器”为依赖前提，但当前实现未接入任一服务器，且客户端侧已存在“重连后请求 FullSync”的逻辑。
**决策:** 将 `ReconnectionManager` 作为服务器侧组件使用（Host/Relay 共用），在断线与重连成功节点被服务器调用；补齐服务端对 `FullStateSyncRequest` 的响应，客户端补齐 `FullStateSyncResponse` 的识别/消费。
**理由:** 与现有 `ReconnectToken` 校验点一致；不引入额外服务端状态来源；让“重连→FullSync”形成最小闭环。
**替代方案:** 将 ReconnectionManager 做成纯客户端自动重连器 → 拒绝原因: 服务端仍需要快照/追赶数据来源，且当前会话恢复校验已在服务端实现，客户端化会造成职责重复与协议不一致风险上升。
**影响:** 会涉及 3-5 个文件的改动与消息类型白名单调整，需要手动联机验证。

## 安全与性能
- **安全:** token 高熵生成；严格校验 playerId/token/过期窗口；限制快照/事件负载大小（避免异常大 JSON）。
- **性能:** 快照/事件结构使用锁保护；限制内存上限（快照数量、历史事件数量）；避免频繁分配大对象。

## 测试与部署
- **测试:**
  - Host 模式：本机开服 2 客户端连接 → 断线 → 重连 → 观察 PlayerListUpdate/重连通知/FullSync 响应。
  - Relay 模式：加入房间后断线重连 → 验证仍在房间，能收到 FullSync 响应。
- **部署:** 仅代码变更，无额外部署步骤；需在游戏内联机手动验证。

