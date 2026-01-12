# 变更提案: MidGameJoinManager TODO 落地

## 需求背景
`networkplugin/Network/MidGameJoin/MidGameJoinManager.cs` 当前仍包含多处 TODO 与 Stub（房间信息获取、请求/批准/执行加入、进度/补偿计算、追赶事件回放、网络事件订阅等），导致“中途加入”能力无法端到端工作，也难以与现有的断线重连（`ReconnectionManager`）与状态同步（`SynchronizationManager`）能力复用。

这是一个局域网/虚拟局域网联机 Mod。本变更按“Mod 可用优先”的标准：优先把 MidGameJoin 跑通（能用、好调试、尽量少改动面），不追求严格的服务端权威与对抗性安全。

## 变更内容
1. 完成 `MidGameJoinManager` 的 TODO 与 Stub，实现房间事件订阅、请求/批准/执行加入、进度/补偿、事件追赶回放等关键逻辑。
2. 复用现有网络能力（尤其是 Relay 的 `DirectMessage`）实现点对点的加入请求/响应与 FullSync 请求/响应，避免把服务端路由 TODO 作为前置依赖。
3. 与 `ReconnectionManager` 的能力“能复用就复用”：优先复用快照/事件历史；若集成成本过高则先做最小可用实现（以可跑通为目标）。

## 影响范围
- **模块:** `networkplugin/Network/MidGameJoin`、`networkplugin/Network/Reconnection`、`networkplugin/Core`、`networkplugin/Plugin`
- **文件(预期):**
  - `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs`
  - `networkplugin/Plugin.cs`
  - `networkplugin/Network/Reconnection/ReconnectionManager.cs`（如需对外暴露快照/事件接口）
  - `networkplugin/Core/SynchronizationManager.cs`（用于 FullSyncResponse 落地/事件回放）
- **API:** 新增/补齐 MidGameJoin 与 FullSync 的消息载荷结构（基于 `DirectMessage` 包装）
- **数据:** 无（仅内存态队列/缓存）

## 核心场景

### 需求: 初始化订阅与状态维护
**模块:** networkplugin

#### 场景: MidGameJoinManager 随插件启动初始化
条件描述：插件 Awake 后服务注册完成；网络可能未连接/稍后连接。
- 预期结果：`MidGameJoinManager.Initialize()` 能安全订阅 `INetworkClient.OnGameEventReceived`，并在断线重连后保持可用（幂等、可重复调用）。

### 需求: 中途加入请求与审批
**模块:** networkplugin

#### 场景: Joiner 进入房间后申请加入进行中的对局
条件描述：Joiner 已 `JoinRoom` 成功进入同一房间；房间已开始游戏；Joiner 仍未获得对局状态。
- 预期结果：Joiner 发送 `MidGameJoinRequest` 给 Host；Host 记录请求并能在超时/拒绝时给出明确原因；通过则返回 `JoinToken + BootstrappedState`。

#### 场景: Host 权限校验与限流
条件描述：非 Host 试图批准请求；或同房间请求数超过 `MaxJoinRequestsPerRoom`。
- 预期结果：请求被拒绝并返回可诊断错误；不会污染 Host 端的 pending 队列。

### 需求: 执行加入与完整同步
**模块:** networkplugin

#### 场景: Joiner 使用 JoinToken 执行加入
条件描述：Joiner 已收到 Host 下发的 JoinToken 与 BootstrappedState。
- 预期结果：先进行快速落地（FastSync/本地基础状态），再向 Host 发起 FullSync 请求；Joiner 收到 FullSyncResponse 后完成状态落地并进入可操作状态。

### 需求: 进度计算与补偿生成
**模块:** networkplugin

#### 场景: 根据对局进度给 Joiner 生成起始资源
条件描述：Host 能获得当前 `FullStateSnapshot`（含 `GameStateSnapshot`）。
- 预期结果：`CalculateGameProgress` 与卡牌/宝物/药水补偿逻辑可配置、可测试、不会生成非法内容（空ID/负数数量/超过上限）。

### 需求: 追赶事件回放
**模块:** networkplugin

#### 场景: Joiner 在 FullSnapshot 落地后回放 MissedEvents
条件描述：Host 在 FullSyncResponse 中携带 MissedEvents；Joiner 尚未处理这些事件。
- 预期结果：Joiner 按顺序应用事件（失败可降级/停止回放并提示），避免把控制消息（FullSyncRequest/Response）写入回放队列造成循环。

## 风险评估
- **风险:** MidGameJoin 与 Reconnection/Sync 的口径不一致导致状态分叉/偶发不同步。
  - **缓解:** 优先走“FullSnapshot + 少量 MissedEvents”这条最直观路径；不强行覆盖所有事件类型，先保证常见场景可用。
- **风险:** 大快照/长事件回放带来卡顿或调试困难。
  - **缓解:** `CatchUpBatchSize` 分批回放；失败时允许降级为“只落地快照、跳过回放”，并输出可诊断日志。
- **风险:** JoinToken/RequestId 仅作为流程协作标识，缺乏“对抗性安全”防护。
  - **缓解:** 本项目为局域网/虚拟局域网联机 Mod 场景，安全威胁模型较弱；仍建议保留超时与单次消费，主要用于避免误操作/重复触发导致的逻辑污染与资源泄露。
