# 任务清单: unify_server_core_two_modes

目录: `helloagents/plan/无_unify_server_core_two_modes/`

---

## 任务状态符号说明

| 符号 | 状态 | 说明 |
|------|------|------|
| `[ ]` | pending | 待执行 |
| `[√]` | completed | 已完成 |
| `[X]` | failed | 执行失败 |
| `[-]` | skipped | 已跳过 |
| `[?]` | uncertain | 待确认 |

---

## 执行状态
```yaml
总任务: (已归档)
已完成: (参考原任务列表)
完成率: (参考原任务列表)
```

---

## 任务列表

﻿# 任务清单: 抽取 ServerCore + 双模式服务器（Host/Relay）

目录: `helloagents/plan/202601091556_unify_server_core_two_modes/`

---

## 1. ServerCore（公共内核）
- [√] 1.1 新增 `networkplugin/Network/Server/Core/`：定义 `ServerCore`、`ServerOptions`、`IServerMessageCodec`、基础事件回调（验证 why.md#需求-房主直连-场景-基础连通）。
- [√] 1.2 在 core 内实现心跳/超时与会话索引（复用 `PlayerSession`），并提供可选 `RunLoop/Poll`（验证 why.md#需求-断线重连-场景-心跳超时）。
- [√] 1.3 引入“消息分发管线”：优先级队列 + 队列上限 + 丢弃策略（复用 `networkplugin/Network/Messages/MessagePriorities.cs`）（验证 why.md#需求-房主直连-场景-高优先级系统消息）。

## 2. Host 模式（保持 NetworkServer 语义）
- [√] 2.1 将 `networkplugin/Network/Server/NetworkServer.cs` 迁移为基于 `ServerCore` 的 Host 模式实现（对外 API/消息语义保持不变：`PlayerListUpdate/PlayerJoined/HostChanged/GetSelf_REQUEST/UpdatePlayerLocation`）。
- [-] 2.2 将 Host 权限模型抽象为可复用的 `IAuthorizationPolicy`（Host/成员），并在敏感消息处理处强制校验（验证 why.md#需求-房主直连-场景-踢人/控制类操作）。
  > 备注: 当前敏感操作主要通过现有逻辑判断（例如 Relay 的 KickPlayer 仅 Host 可用），尚未抽象统一 `IAuthorizationPolicy` 接口。

## 3. Relay 模式（房间/中继）
- [√] 3.1 清理 server/client 连接抽象混用：将 `networkplugin/Network/Messages/NetworkConnection.cs` 改为 server 侧可用（基于 `NetPeer` 发送），并修正 `networkplugin/Network/Room/NetworkRoom.cs` 的依赖（验证 why.md#需求-多房间与中继转发-场景-广播可用）。
- [√] 3.2 将 `networkplugin/Network/Server/RelayServer.cs` 迁移为基于 `ServerCore` 的 Relay 模式，并把房间相关 handler（Create/Join/Leave/RoomMessage/DirectMessage）迁移到模块/服务中（验证 why.md#需求-多房间与中继转发）。
- [√] 3.3 Relay 模式补齐 Host 协议兼容：在房间作用域实现 `PlayerListUpdate` 广播、响应 `GetSelf_REQUEST`、处理 `UpdatePlayerLocation` 并按房间广播（验证 why.md#需求-房主直连 与 #需求-多房间与中继转发 的兼容）。

## 4. 断线重连（Server 支撑）
- [√] 4.1 定义 `ReconnectToken` 生成/校验与会话恢复接口；补齐 Relay/Host 两种模式的最小恢复流程（与 `networkplugin/Network/Reconnection/ReconnectionManager.cs` 对齐）（验证 why.md#需求-断线重连）。

## 5. NAT/打洞协助（Relay 优先）
- [√] 5.1 定义 NAT 信息交换消息类型与数据结构，复用 `networkplugin/Network/Utils/NatTraversal.cs`（验证 why.md#需求-NAT-打洞协助-场景）。
- [√] 5.2 让 Relay 模式在 `RelayServerConfig.EnableNatPunchthrough` 开启时提供“候选端点交换/打洞协调”，并支持失败降级到 Relay 转发。
  > 备注: 当前配置类 `RelayServerConfig` 未接入 `RelayServer` 构造流程，NAT 消息处理按“可用即启用”实现；如需按配置开关，需补齐 ConfigManager 绑定与注入。

## 6. 安全检查
- [√] 6.1 输入验证与限流检查（按 G9）：消息大小、频率、JSON 反序列化边界、权限校验、避免新增未受控反射/动态类型反序列化输入面。

## 7. 文档更新
- [√] 7.1 更新 `helloagents/wiki/arch.md`：补充 ServerCore + 双模式架构与 ADR 索引。

## 8. 验证
- [√] 8.1 执行 `dotnet build .\networkplugin\NetWorkPlugin.csproj -c Debug --no-restore`。
- [ ] 8.2 手动验证：Host 模式局域网两人联机 + Relay 模式创建/加入房间 + 关键事件（PlayerListUpdate/HostChanged/UpdatePlayerLocation）。

---

## 执行备注

> 执行过程中的重要记录

| 任务 | 状态 | 备注 |
|------|------|------|
