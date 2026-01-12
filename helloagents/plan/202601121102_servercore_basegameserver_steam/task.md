# 任务清单: 抽取 BaseGameServer（面向未来 Steam 联机）

目录: `helloagents/plan/202601121102_servercore_basegameserver_steam/`

---

## 1. 业务骨架抽取（BaseGameServer）
- [√] 1.1 在 `networkplugin/Network/Server/` 新增 `BaseGameServer`（或同义命名），统一 core 事件注册与消息分流，验证 why.md#核心场景-需求-复用服务端通用业务骨架-场景-玩家连接与-welcome
- [-] 1.2 抽取 `SessionManager`（或内聚到 Base 中），统一 session 映射、心跳、断线时间记录，验证 why.md#核心场景-需求-复用服务端通用业务骨架-场景-心跳与超时
  > 备注: 已由 `BaseGameServer` 内聚承载（`SessionsByPeer/SessionsByPlayerId/DisconnectedAtByPlayerId` + 统一事件接入），并将 Host 的心跳/会话查找迁移到 Base 映射上。
- [√] 1.3 统一重连处理（token 生成/校验、窗口判定、恢复 peer 映射），验证 why.md#核心场景-需求-复用服务端通用业务骨架-场景-断线重连窗口（Host 已补齐 Base peer->session 映射更新，避免重连后收包找不到 session）

## 2. Host/直连（NetworkServer）迁移
- [√] 2.1 将 `networkplugin/Network/Server/NetworkServer.cs` 迁移到 Base 骨架，保留消息协议与房主语义，验证 why.md#核心场景-需求-复用服务端通用业务骨架-场景-玩家连接与-welcome
- [√] 2.2 清理/隔离旧的 `_listener.*Event` 注册路径（确保只走一条收发链路），验证 why.md#风险评估（构造函数不再注册旧的 core 事件；事件接入统一由 Base 托管）

## 3. Relay/中继（RelayServer）迁移
- [√] 3.1 将 `networkplugin/Network/Server/RelayServer.cs` 迁移到 Base 骨架，保持房间路由与现有 `NetworkRoom` 行为不变，验证 why.md#核心场景-需求-复用服务端通用业务骨架-场景-心跳与超时
- [ ] 3.2 回归验证 `CreateRoom/JoinRoom/LeaveRoom/RoomMessage/DirectMessage/KickPlayer` 与 NAT 消息，验证 why.md#影响范围
  > 备注: 需要手动联机回归验证（无法在仓库内自动化）。

## 4. 为 Steam 联机预留接口
- [√] 4.1 在 `networkplugin/Network/Server/Core/` 定义 `IServerCore`（或等价接口），让 Base/业务层依赖接口而不是 `ServerCore` 具体类型，验证 why.md#核心场景-需求-为未来-steam-联机预留传输层替换点
- [-] 4.2 为未来 `SteamServerCore` 预留最小适配点（Start/Stop/Poll + inbound message event），不引入 Steam 依赖，验证 why.md#核心场景-需求-为未来-steam-联机预留传输层替换点-场景-以-steamtransport-替换-litenetlib（暂未新增 SteamCore 占位实现，避免引入接口/peer 抽象的大规模改动）

## 5. 安全检查
- [√] 5.1 执行安全检查（输入校验、token/密钥日志脱敏、权限控制、潜在 DoS（队列/消息大小）风险规避）
  > 备注: 未发现密钥/token 明文日志；ServerCore 队列大小与每 tick 处理量受 `ServerOptions` 控制；仍建议对超大 payload 做长度上限（后续可选增强）。

## 6. 验证清单
- [ ] 6.1 手动联机验证：Host 直连（2人/3人），断线重连（60秒内/超时），房主迁移（如适用）
- [ ] 6.2 手动联机验证：Relay 创建房间/加入/离开/解散，房间内消息转发，NAT 消息
- [√] 6.3 局部构建验证：`dotnet build .\\networkplugin\\NetWorkPlugin.csproj`
