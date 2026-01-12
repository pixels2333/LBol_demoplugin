# 技术设计: 抽取 BaseGameServer（面向未来 Steam 联机）

## 技术方案

### 核心技术
- C# / .NET（项目既有）
- LiteNetLib（现有传输层）
- `ServerCore`（现有网络核心：鉴权/收发/队列/线程）
- 预留：SteamNetworkingSockets / Steam P2P（未来可选传输层）

### 实现要点
- 用**组合**而非继承：`NetworkServer/RelayServer` 继续“持有 core”，不去继承 `ServerCore`。
- 抽取**业务骨架层**：`BaseGameServer` 统一注册 core 事件、维护 session 映射、处理心跳/重连窗口、完成消息分流。
- 引入最小接口 `IServerCore`：业务层依赖接口，便于未来注入 `SteamServerCore`，并减少 LiteNetLib 类型在业务层扩散。
- 渐进拆分 LiteNetLib 类型：先不改消息协议；优先把 `NetPeer` 的使用集中在“发送适配/连接抽象”边界上。

## 架构设计
```mermaid
flowchart TD
    Client[INetworkClient/客户端] -->|消息| Core[IServerCore 实现<br/>LiteNetLib ServerCore / SteamServerCore]
    Core -->|事件| Base[BaseGameServer<br/>会话/心跳/重连/分流]
    Base --> Host[HostServer(NetworkServer)<br/>全局广播/房主语义]
    Base --> Relay[RelayServer<br/>房间管理/中继转发/NAT]
    Relay --> Rooms[NetworkRoom]
```

## 架构决策 ADR
### ADR-20260112-01: 业务骨架从 ServerCore 中分离
**上下文:** 当前 Host/Relay 都依赖 `ServerCore`，但仍有大量业务重复（session/心跳/重连/分流）。未来接入 Steam 时，若没有业务骨架层，会导致“每种传输层 * 每种服务器模式”组合爆炸。
**决策:** 保持 `ServerCore` 作为“网络引擎层”，新增 `BaseGameServer` 作为“业务骨架层”；业务服务器（Host/Relay）只实现差异化路由策略。
**理由:** 降低耦合、减少重复、便于扩展新的 transport（Steam）而不重写业务逻辑。
**替代方案:** 直接把所有共用逻辑塞进 `ServerCore` → 拒绝原因: 会把网络核心演变成业务大杂烩，难以维护与复用。
**影响:** 需要一次性调整 `NetworkServer/RelayServer` 的结构；短期改动面更大，但长期维护成本显著下降。

## API设计
本变更阶段不修改对外消息协议（Type/字段），以“行为不变 + 结构内聚”为目标。
如果后续需要引入“Steam Lobby/房间列表”等能力，优先以**新增消息类型**实现，不破坏旧客户端兼容。

## 数据模型
无持久化数据变更；新增的抽象/组件仅影响内存状态。

## 安全与性能
- **安全:**
  - 对入站消息 `Type/Payload` 做健壮性校验（长度、JSON 解析失败、未知类型）。
  - 不在日志中打印敏感信息（连接 key、token 全量）。
  - Steam 接入时注意 SteamID 与玩家身份绑定逻辑，避免冒用。
- **性能:**
  - Base 层只做 O(1) 映射与分发，避免在热路径中做全量扫描。
  - 保持 Relay 的队列与后台线程策略（现有 `ServerOptions`），避免改动线程模型导致抖动。

## 测试与部署
- **测试:** 以手动联机回归为主（仓库已知存在编译错误时），覆盖：连接/断线/重连、心跳、Host 广播、Relay 房间创建/加入/离开、房间内消息转发、NAT 消息。
- **部署:** 不新增外部依赖；Steam 传输层仅在未来阶段引入并做可选启用（配置开关/编译条件）。

