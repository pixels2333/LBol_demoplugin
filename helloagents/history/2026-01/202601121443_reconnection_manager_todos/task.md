# 任务清单: ReconnectionManager TODO 补全与断线重连打通

目录: `helloagents/history/2026-01/202601121443_reconnection_manager_todos/`

---

## 1. ReconnectionManager TODO 补全（服务器侧数据管理）
- [√] 1.1 在 `networkplugin/Network/Reconnection/ReconnectionManager.cs` 中补齐 `Initialize/CreateFullSnapshot/SaveSnapshot/CheckHeartbeats/GenerateReconnectToken` 等 TODO，验证 why.md#需求-host-模式断线重连-场景-客户端掉线后重连
- [√] 1.2 在 `networkplugin/Network/Reconnection/ReconnectionManager.cs` 中补齐“事件历史记录与追赶”相关逻辑（`RecordGameEvent/GetMissedEvents` 对齐 `EventIndex`），验证 why.md#需求-重连后的全量同步与追赶-场景-重连后自动发起-fullsync

## 2. Host/Relay 服务器接入重连数据链路
- [-] 2.1 在 `networkplugin/Network/Server/NetworkServer.cs` 中接入断线/重连节点，调用 ReconnectionManager 保存与下发重连数据，验证 why.md#需求-host-模式断线重连-场景-客户端掉线后重连
> 备注: 本次仅补齐 `FullStateSyncRequest/FullStateSyncResponse` 走 GameEvent 通道（IsGameEvent），未额外新增会话断线/重连钩子。
- [-] 2.2 在 `networkplugin/Network/Server/RelayServer.cs` 中接入断线/重连节点，调用 ReconnectionManager 保存与下发重连数据，验证 why.md#需求-relay-模式断线重连-场景-房间内客户端掉线后重连
> 备注: 同 2.1，本次不在 RelayServer 增加额外钩子；先保证 FullSync 消息可广播/订阅。

## 3. 客户端识别并消费 FullSync/重连响应
- [√] 3.1 在 `networkplugin/Network/Client/NetworkClient.cs` 中补齐对 `FullStateSyncResponse` 的识别与上抛（按现有 IsGameEvent/OnGameEventReceived 机制），验证 why.md#需求-重连后的全量同步与追赶-场景-重连后自动发起-fullsync
- [-] 3.2 在 `networkplugin/Core/SynchronizationManager.cs` 中补齐对 `FullStateSyncResponse` 的消费入口（快照应用/事件追赶的最小实现），验证 why.md#需求-重连后的全量同步与追赶-场景-重连后自动发起-fullsync
> 备注: `SynchronizationManager` 当前的 `CreateGameEventFromNetworkData/ApplyRemoteEvent` 仍为 TODO/未实现；直接接入会导致大量异常日志，暂不启用此链路。

## 4. 安全检查
- [√] 4.1 执行安全检查（按G9：输入验证、敏感信息处理、权限控制、EHRB 风险规避）
> 备注: 重连令牌改为 `RandomNumberGenerator.Create().GetBytes` 生成；未引入敏感信息硬编码与不可信反序列化下沉。

## 5. 测试
- [-] 5.1 Host 模式手动联机验证：断线→重连→FullSync 下发与消费（关键点：PlayerId 不变、重连通知、状态一致性）
> 备注: 待定（需要手动联机验证，本轮跳过）。
- [-] 5.2 Relay 模式手动联机验证：房间内断线→重连→仍在房间 + FullSync 下发与消费
> 备注: 待定（需要手动联机验证，本轮跳过）。
