# 变更提案: networkplugin-architecture-consolidation

## 元信息
```yaml
类型: 架构优化
方案类型: implementation
优先级: P0
状态: 已归档（按分批收敛完成）
创建: 2026-03-06
完成: 2026-03-06
复杂度: 轻量迭代
来源: `~plan` / `~exec` - 基于 networkplugin 项目 review 结果生成并执行
```

---

## 0. 执行前范围确认

```yaml
用户对齐结果:
  - 第一批执行 `NetworkServer` 双路径收敛
  - 第一批执行 `SynchronizationManager` 生命周期统一
  - 第二批执行 `OtherPlayersOverlayPatch` 三文件拆层，并保留现有静态门面
  - 第三批执行 `RemoteCardUsePatch` 三文件拆层，并保留 ReversePatch 白名单入口
  - 第四批执行 `NetworkMessageTypes` + `NetworkClient`/`NetworkServer`/`RelayServer` 的系统消息常量收口
移出当前方案:
  - `PLANNING_ROADMAP.md` 扩展回写
执行结果:
  - 代码修改已完成
  - 已补充 `SyncProbe` / `RouteProbe` 运行时探针，覆盖剩余待确认项的首包可观测性
  - `dotnet build networkplugin/NetWorkPlugin.csproj -v minimal` 通过
  - 构建结果: 256 warnings / 0 errors
```

---

## 1. 需求

### 背景
对 `networkplugin/` 的审查确认，当前最高优先级问题集中在两个会直接放大维护成本的结构点：

1. `networkplugin/Network/Server/NetworkServer.cs` 同时保留旧 LiteNetLib 监听器路径与新的 `BaseGameServer`/`ServerCore` 路径，存在两套 `HandleSystemMessage` / `HandleGameEvent` / `PlayerJoined` / `PlayerList` 处理分支。
2. `networkplugin/Core/SynchronizationManager.cs` 混用静态单例 `SynchronizationManager.Instance` 与 DI 单例 `ISynchronizationManager`，而 `networkplugin/Network/Client/NetworkClient.cs` 仍直接调用静态实例，容易导致生命周期分叉。

### 目标
1. 将 Host/直连服务端链路收敛到一条可维护路径，避免修复只覆盖旧实现或新实现的一半。
2. 统一 `SynchronizationManager` 生命周期，确保网络回调、Patch 调用和 DI 容器拿到的是同一个同步管理器实例。
3. 将 `OtherPlayersOverlayPatch` 拆为职责清晰的局部文件，同时不破坏现有静态调用面。
4. 将 Client/Host/Relay 高频系统消息字面量收口到 `NetworkMessageTypes`，降低三端协议漂移风险。
5. 产出一份和实际执行范围一致、可归档的方案记录。

### 非目标
- 不重写 `networkplugin` 整体网络架构。
- 不在本方案内引入新联机玩法或新 UI 功能。
- 不在本轮扩展 `PLANNING_ROADMAP.md` 路线图文档。

### 约束条件
```yaml
兼容性约束:
  - Host/Join 基础联机流程不能回退
  - 现有 `BaseGameServer` / `ServerCore` 主链保持继续使用
范围约束:
  - 优先收敛重复实现，不额外引入无必要抽象层
验证约束:
  - 至少通过 `dotnet build networkplugin/NetWorkPlugin.csproj`
文档约束:
  - 同步更新 `helloagents/CHANGELOG.md`、`helloagents/INDEX.md` 与归档索引
```

### 验收标准
- [√] `networkplugin/Network/Server/NetworkServer.cs` 不再保留未接线的 `RegisterCoreEvents()` / `RegisterEvents()` 和双份系统/游戏消息主逻辑。
- [√] `networkplugin/Core/SynchronizationManager.cs` 只保留一种生命周期模型，`networkplugin/Network/Client/NetworkClient.cs` 不再调用 `SynchronizationManager.Instance`。
- [√] `networkplugin/Plugin.cs` 通过“具体类单例 + 接口别名”注册同步管理器。
- [√] `networkplugin/Network/Messages/NetworkMessageTypes.cs` 已补齐系统消息常量，`NetworkClient` / `NetworkServer` / `RelayServer` 的高频系统消息字面量已收口到常量引用。
- [√] `dotnet build networkplugin/NetWorkPlugin.csproj -v minimal` 通过。
- [?] 游戏内 Host/直连链路的 `DirectMessage` / `FullStateSync` / `RoomState` 单播行为仍需联机手工回归确认（已补充 `RouteProbe` 可观测日志）。

---

## 2. 方案总览

### 2.1 执行分期

#### Phase A（P0）: 生命周期与服务端主路径收敛
- 统一 `SynchronizationManager` 生命周期。
- 清理 `NetworkServer` 双路径残留，保留 `BaseGameServer` 主链。

#### Phase B（后续补跑，已执行）
- 系统消息常量收口。

### 2.2 实际影响范围
```yaml
涉及模块:
  - Network/Server
  - Core
  - Network/Client
  - Patch/UI
  - Patch/Network
  - Root(Plugin)
  - Docs(helloagents)
实际变更文件: 19
新增文件: 5
```

### 2.3 风险评估

| 风险 | 等级 | 影响 | 缓解策略 |
|------|------|------|----------|
| `NetworkServer` 去重时误删仍在运行路径上的逻辑 | 高 | Host 模式联机异常 | 以 `BaseGameServer` 回调链为基准，只删除未接线旧实现 |
| `SynchronizationManager` 生命周期统一后出现构造循环 | 高 | 客户端初始化失败 | 改为 DI 构造 + 延迟解析 `INetworkClient`，避免构造期回环 |
| 缺少游戏内双端环境 | 中 | 无法完成运行时行为验收 | 先用构建和静态引用校验兜底，并在任务清单中显式保留手工回归项 |

---

## 3. 文件级改动设计与执行结果

### 3.1 HostServer 双路径收敛（已执行）

| 文件位置 | 实际改动 |
|----------|----------|
| `networkplugin/Network/Server/NetworkServer.cs` | 删除未接线旧监听器注册入口 `RegisterCoreEvents()`、`RegisterEvents()` 及其配套旧处理路径。 |
| `networkplugin/Network/Server/NetworkServer.cs` | 删除 `HandleSystemMessage(NetPeer, string, NetPacketReader)`、`HandleGameEvent(NetPeer, string, NetDataReader)` 两套旧入口，只保留 JSON 主路径。 |
| `networkplugin/Network/Server/NetworkServer.cs` | 删除重复的 `HandlePlayerJoined(NetPeer, NetDataReader)`、`HandleGetSelfRequest(NetPeer, NetDataReader)`、`HandleUpdatePlayerLocation(NetPeer, NetDataReader)`，只保留一份主实现。 |
| `networkplugin/Network/Server/NetworkServer.cs` | 保留 `HandleDirectMessage(...)`、`RouteFullStateSync*`、`RouteRoomState*`、`SendRawJsonToPeer(...)`，统一挂在现有 `BaseGameServer -> JSON payload` 主链下。 |
| `networkplugin/Network/Server/NetworkServer.cs` | 新增一次性 `RouteProbe` 日志，记录 `DirectMessage/*`、`FullStateSync*`、`RoomState*` 路由首包，降低手工联机回归定位成本。 |

### 3.2 `SynchronizationManager` 生命周期统一（已执行）

| 文件位置 | 实际改动 |
|----------|----------|
| `networkplugin/Core/SynchronizationManager.cs` | 删除 `_instance` 与 `Instance` 静态访问入口；将构造函数改为 DI 可构造形式；取消构造期立即解析 `INetworkClient`。 |
| `networkplugin/Plugin.cs` | 改为注册 `SynchronizationManager` concrete，并将 `ISynchronizationManager` 绑定到同一实例；新增 `StartupWiring` / `PatchResolve` 一次性 `SyncProbe` 日志。 |
| `networkplugin/Network/Client/NetworkClient.cs` | 构造函数注入 `ISynchronizationManager`，替换连接恢复、断开连接、接收游戏事件三处静态调用。 |
| `networkplugin/Patch/Actions/*.cs` | 保持现有 `GetService<ISynchronizationManager>()` 取法不变，继续与 DI 主路径对齐；并接入 `PatchResolve` 运行时探针。 |

### 3.3 `OtherPlayersOverlayPatch` 拆层（已执行）

| 文件位置 | 实际改动 |
|----------|----------|
| `networkplugin/Patch/UI/OtherPlayersOverlayPatch.cs` | 改为 `partial` 门面类，保留对外静态 API，删除已迁出的缓存/事件桥接/视图注册实现。 |
| `networkplugin/Patch/UI/OtherPlayersOverlay/OtherPlayersOverlayEventBridge.cs` | 提取 `INetworkClient` 解析、订阅、消息桥接、玩家列表负载解析逻辑。 |
| `networkplugin/Patch/UI/OtherPlayersOverlay/OtherPlayersOverlayPlayerStore.cs` | 提取玩家缓存、自身位置查询、快照导出、名字到玩家 ID 解析逻辑。 |
| `networkplugin/Patch/UI/OtherPlayersOverlay/OtherPlayersOverlayViewRegistry.cs` | 提取远端角色视图查找、注册、地图图标与目标选择辅助逻辑。 |

### 3.4 `RemoteCardUsePatch` 拆层（已执行）

| 文件位置 | 实际改动 |
|----------|----------|
| `networkplugin/Patch/Network/RemoteCardUsePatch.cs` | 改为 `partial` 发送门面，保留 Harmony 入口、`Card_GetActions_Original` ReversePatch 白名单桩与公共 JSON helper。 |
| `networkplugin/Patch/Network/RemoteCardUsePatch.ReceiveBridge.cs` | 提取订阅钩子、Welcome/OnRemoteCardUse/OnRemoteCardResolved 事件桥接与动画预播放逻辑。 |
| `networkplugin/Patch/Network/RemoteCardUsePatch.Execution.cs` | 提取远程回放执行、Resolved 广播、状态落地与反射辅助逻辑。 |

### 3.5 系统消息常量收口（已补跑）

| 文件位置 | 实际改动 |
|----------|----------|
| `networkplugin/Network/Messages/NetworkMessageTypes.cs` | 补齐 `DirectMessage`、`UpdatePlayerLocation`、`Reconnect_*`、`CreateRoom/JoinRoom/LeaveRoom/RoomMessage/GetRoomList/RoomList/RoomCreated/RoomJoined/KickPlayer/Error` 等系统消息常量。 |
| `networkplugin/Network/Client/NetworkClient.cs` | 将 `GetSelf_RESPONSE` 的系统响应判定切换为 `NetworkMessageTypes` 常量比较。 |
| `networkplugin/Network/Server/NetworkServer.cs` | 将 `Welcome`、`HeartbeatResponse`、`PlayerListUpdate`、`HostChanged`、`DirectMessage`、`Reconnect_*`、`GetSelf_*` 等高频系统消息统一改为常量引用。 |
| `networkplugin/Network/Server/RelayServer.cs` | 将房间管理、`Welcome`、`HeartbeatResponse`、`PlayerJoined`、`PlayerListUpdate`、`DirectMessage`、`Reconnect_*` 与通用 `Error` 等高频系统消息统一改为常量引用。 |

### 3.6 剩余移出当前方案的条目

| 条目 | 处理结果 |
|------|----------|
| `PLANNING_ROADMAP.md` 扩展回写 | 未执行，移出当前方案 |

---

## 4. 核心场景

### 场景: Host 服务端主链路去重
**模块**: `Network/Server`
**条件**: `NetworkServer` 继续基于 `BaseGameServer` 工作
**行为**: 删除旧监听器残留，只保留一套系统消息/游戏消息入口
**结果**: Host 模式的修复点与实际运行点一致，不再有“改了一套，线上走另一套”的风险

### 场景: 同步管理器统一单例
**模块**: `Core` + `Network/Client`
**条件**: 容器已完成注册
**行为**: `NetworkClient` 和 Patch 侧均走 DI 单例
**结果**: 连接恢复、断开、网络回放与本地 Patch 同步使用相同状态容器

---

## 5. 技术决策

### networkplugin-architecture-consolidation#D001: `NetworkServer` 以 `BaseGameServer` 回调链为唯一主路径
**日期**: 2026-03-06
**状态**: ✅已执行
**决策**: 保留 `BaseGameServer` 主链，删除旧路径。
**理由**: 代码已明显向 `ServerCore + BaseGameServer` 演进，继续保留旧路径没有长期价值。
**结果**: `NetworkServer.cs` 内重复入口已经清理，静态搜索不再命中旧实现定义。

### networkplugin-architecture-consolidation#D002: `SynchronizationManager` 改为纯 DI 单例
**日期**: 2026-03-06
**状态**: ✅已执行
**决策**: 删除静态入口，统一走 DI。
**理由**: 当前调用点有限，适合一次性收口，且能避免生命周期分叉。
**结果**: `NetworkClient` 不再引用 `SynchronizationManager.Instance`，容器改为 concrete + interface alias 注册。

### networkplugin-architecture-consolidation#D003: `NetworkMessageTypes` 作为 Client/Host/Relay 系统消息常量单一来源
**日期**: 2026-03-06
**状态**: ✅已执行
**决策**: 将 `NetworkClient`、`NetworkServer`、`RelayServer` 中高频系统消息字面量统一收口到 `NetworkMessageTypes`。
**理由**: 避免三端继续漂移，后续协议变更只需在同一常量源与个别路由点维护。
**结果**: `Welcome`、`DirectMessage`、`Reconnect_*`、房间管理与通用错误消息均已改为常量引用。