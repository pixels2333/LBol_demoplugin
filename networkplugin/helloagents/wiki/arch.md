# 架构设计

## 总体架构

```mermaid
flowchart TD
    Plugin[Plugin.cs (BepInEx)] --> DI[DI Container]
    DI --> NetClient[INetworkClient / NetworkClient]
    DI --> NetMgr[INetworkManager / NetworkManager]
    DI --> SyncMgr[ISynchronizationManager / SynchronizationManager]
    DI --> Authority[HostAuthorityManager]
    DI --> Reconn[ReconnectionManager]
    DI --> Join[MidGameJoinManager]

    SyncMgr --> Patches[Harmony Patches (Patch/*)]
    Patches --> Game[LBoL Runtime]

    NetClient <--> Server[NetworkServer / RelayServer]
```

## 技术栈
- 网络：LiteNetLib（客户端/服务器/中继）
- 序列化：`System.Text.Json`（多处以 JSON 字符串作为 payload）
- 运行时补丁：HarmonyX
- 插件框架：BepInEx
- DI：Microsoft.Extensions.DependencyInjection

## 核心流程：事件同步

```mermaid
sequenceDiagram
    participant Game as LBoL
    participant Patch as Harmony Patch
    participant Sync as SyncManager
    participant Client as NetworkClient
    participant Server as NetworkServer
    participant Remote as Remote Client

    Game->>Patch: 触发关键逻辑
    Patch->>Sync: 构造并提交 GameEvent/JSON payload
    Sync->>Client: SendGameEventData(type, data)
    Client->>Server: LiteNetLib 消息
    Server->>Remote: 广播/转发
    Remote->>Sync: ProcessEventFromNetwork(data)
    Sync->>Game: 应用远端状态/行为（回放/对齐）
```

## 核心流程：房主权威（可选路径）
> 适用于“需要房主仲裁”的敏感操作（例如关键回合推进、交易/复活/存档等）。

```mermaid
sequenceDiagram
    participant Local as Local Client
    participant Auth as HostAuthorityManager
    participant Net as INetworkClient
    participant Host as Host Client
    participant Game as LBoL

    Local->>Auth: 提交 ClientRequest(ActionType, Params)
    Auth->>Net: 转发到房主/广播（按实现）
    Host->>Auth: ValidateAndExecuteRequest()
    Auth->>Game: ExecuteAuthoritativeAction()
    Auth->>Net: BroadcastActionToClients()
    Net-->>Local: 接收权威结果并应用
```

## 核心流程：全量同步 / 重连
> 用于断线重连或状态漂移时的“对齐”。

```mermaid
sequenceDiagram
    participant Client as Client
    participant Reconn as ReconnectionManager
    participant Host as Host/Server
    participant Sync as SynchronizationManager

    Client->>Reconn: RequestReconnection(token)
    Reconn->>Host: 请求 FullStateSnapshot + MissedEvents
    Host-->>Reconn: FullStateSnapshot + MissedEvents
    Reconn->>Sync: 应用快照/回放事件
    Sync-->>Client: 恢复联机状态
```

## 核心流程：中途加入（MidGameJoin）
> 用于“战斗/局内”新玩家加入的追赶（Catch-up）与 AI 代打等策略（按配置开关启用）。

```mermaid
sequenceDiagram
    participant New as New Player
    participant Join as MidGameJoinManager
    participant Host as Host
    participant Sync as SynchronizationManager

    New->>Join: RequestJoin(roomId)
    Join->>Host: NotifyHostOfJoinRequest()
    Host-->>Join: ApproveJoin(token)
    Join->>Host: RequestFullStateSync()
    Host-->>Join: FullStateSnapshot + CatchUpEvents
    Join->>Sync: 应用快照/回放事件
```

## 重大架构决策（索引）
- ADR 建议记录在每个变更方案包的 `how.md` 中，并在此处维护索引。

