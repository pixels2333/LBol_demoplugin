# 变更提案: 抽取 ServerCore + 双模式服务器（Host/Relay）

## 需求背景
- 目前存在两套服务器实现：`NetworkServer`（房主直连/局域网）与 `RelayServer`（房间/中继/打洞辅助）。两者在“连接管理/会话/收发消息/心跳”等基础能力上重叠，但协议与功能分叉，维护成本高。
- 现状：`RelayServer` 具备房间、中继转发、配置化、线程/锁等“服务化”能力；但 `NetworkServer` 具备客户端/补丁已依赖的游戏层协议（例如 `PlayerListUpdate`、`GetSelf_REQUEST`、`UpdatePlayerLocation`、`OnGameEventReceived` 等）。
- 目标：在不牺牲“房主=权威（Host/Client）”模式可用性的前提下，把房间/中继转发/断线重连/打洞/权限/消息优先级等能力沉淀为可复用模块，形成“公共内核 + 两种模式”。

## 变更内容
1. 抽取公共内核 `ServerCore`：统一 LiteNetLib 生命周期、连接鉴权、会话管理、心跳/超时、消息编解码与分发管线。
2. 保留两种运行模式：
   - Host 模式：等价于现有 `NetworkServer` 的语义（房主直连、单房间隐式、兼容现有消息类型/事件上抛）。
   - Relay 模式：等价于现有 `RelayServer` 的语义（多房间、中继转发、NAT 协调），并补齐 Host 模式依赖的系统消息以实现协议兼容（按房间作用域）。
3. 将高级能力模块化下沉：
   - 房间/中继转发（Relay 模式启用；Host 模式为“单隐式房间”）
   - 断线重连（Token + 会话恢复钩子，配合现有 `ReconnectionManager`）
   - NAT/打洞协调（NAT 信息交换/Token 校验/可选开启）
   - 权限与角色（Host/管理员/成员；限制踢人、开局等敏感操作）
   - 消息优先级与限流（复用 `MessagePriorities`，支持高优先级抢占/队列上限）

## 影响范围
- 模块：`networkplugin/Network/Server/`、`networkplugin/Network/Room/`、`networkplugin/Network/Messages/`（可能需要拆分 client/server 侧的连接抽象）。
- 协议：以“兼容现有客户端/补丁”为第一原则，新增能力通过新消息类型或 capability 协商渐进引入。

## 核心场景
### 需求 1: 房主直连（局域网/同网段）
- Host 启动 Host 模式服务端；客机直连；能正常收到 `PlayerListUpdate/HostChanged/PlayerJoined` 等事件；位置同步与 GetSelf 可用。

### 需求 2: 多房间与中继转发
- 客户端连接 Relay；创建/加入房间；房间内广播/定向消息正常；房主权限按“房间 Host”判定。

### 需求 3: NAT/打洞协助 + P2P 优先
- Relay 用于交换 NAT 信息/候选端点；优先尝试 P2P；失败时降级走 Relay 转发。

### 需求 4: 断线重连
- 客机掉线后在配置窗口内重连，能恢复会话并补齐必要状态/事件。

## 风险评估
- 风险：协议兼容性破坏（客户端补丁依赖的消息缺失/字段变化）。
  - 缓解：以消息类型/字段“向后兼容”为约束；引入 adapter 层；逐步替换而非一次性重写。
- 风险：并发与线程模型复杂化（锁竞争/死锁/时序 bug）。
  - 缓解：优先单线程 `PollEvents()`；仅在需要后台服务化时启用线程，并把共享状态改为“事件队列 + 单线程消费”。
- 风险：安全输入面扩大（来自网络的 JSON/字符串负载）。
  - 缓解：限制消息大小/频率；严格校验消息类型与权限；避免对 `object` 直接反序列化为任意类型。
