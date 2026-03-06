# 任务清单: networkplugin-architecture-consolidation

目录: `helloagents/archive/2026-03/202603060931_networkplugin-architecture-consolidation/`

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
总任务: 22
已完成: 20
已跳过: 0
待确认: 2
完成率: 90.9%
收敛率: 90.9%
```

---

## 任务列表

### 1. HostServer 双路径收敛（P0）

- [√] 1.1 清理 `NetworkServer` 未接线旧监听器注册入口
  - 文件: `networkplugin/Network/Server/NetworkServer.cs`
  - 结果: 已删除 `RegisterCoreEvents()`、`RegisterEvents()` 及其对应旧监听器残留
  - 验证: 静态搜索已不再命中这两个私有方法定义

- [√] 1.2 收敛 `HandleSystemMessage` 为单一路径
  - 文件: `networkplugin/Network/Server/NetworkServer.cs`
  - 结果: 已删除 `HandleSystemMessage(NetPeer, string, NetPacketReader)`，保留 JSON 主路径
  - 验证: `HandleSystemMessage(PlayerSession, ...)` 继续作为基类回调入口

- [√] 1.3 收敛 `HandleGameEvent` 为单一路径
  - 文件: `networkplugin/Network/Server/NetworkServer.cs`
  - 结果: 已删除 `HandleGameEvent(NetPeer, string, NetDataReader)`，保留 JSON 主路径
  - 验证: `HandleGameEvent(PlayerSession, ...)` 正常转发到唯一主实现

- [√] 1.4 合并玩家清单与欢迎包生成逻辑
  - 文件: `networkplugin/Network/Server/NetworkServer.cs`
  - 结果: 已移除 `HandlePlayerJoined(NetPeer, NetDataReader)`、`HandleGetSelfRequest(NetPeer, NetDataReader)`、`HandleUpdatePlayerLocation(NetPeer, NetDataReader)` 等重复实现
  - 验证: `Welcome` / `PlayerListUpdate` / `PlayerJoined` 只剩一份主实现

- [?] 1.5 回归 DirectMessage 与 FullSync/RoomState 定向路由
  - 文件: `networkplugin/Network/Server/NetworkServer.cs`
  - 结果: 已补充一次性 `[RouteProbe]` 日志，覆盖 `DirectMessage/*`、`FullStateSync*`、`RoomState*` 路由首包可观测性
  - 说明: 缺少双端运行环境，仍需 Host 直连手工联机回归确认最终行为

### 2. `SynchronizationManager` 生命周期统一（P0）

- [√] 2.1 移除 `SynchronizationManager` 静态单例入口
  - 文件: `networkplugin/Core/SynchronizationManager.cs`
  - 结果: 已删除 `_instance`、`Instance`，并改为 DI 可构造
  - 验证: 全局静态搜索不再命中 `SynchronizationManager.Instance`

- [√] 2.2 调整同步管理器 DI 注册为“具体类 + 接口别名同实例”
  - 文件: `networkplugin/Plugin.cs`
  - 结果: 已注册 concrete，并将 `ISynchronizationManager` 绑定到同一实例
  - 验证: 注册路径明确为 `GetRequiredService<SynchronizationManager>()`

- [√] 2.3 将 `NetworkClient` 改为注入同步管理器实例
  - 文件: `networkplugin/Network/Client/NetworkClient.cs`
  - 结果: 已替换连接恢复、断开连接、接收游戏事件三处静态调用
  - 验证: `PeerConnectedEvent`、`PeerDisconnectedEvent`、`HandleGameEvent(...)` 全部改为实例调用

- [?] 2.4 回归 Patch 侧同步管理器解析一致性
  - 文件: `networkplugin/Patch/Actions/ApplyStatusEffectAction_Patch.cs`
  - 文件: `networkplugin/Patch/Actions/DamageAction_Patch.cs`
  - 文件: `networkplugin/Patch/Actions/PlayCardAction_Patch.cs`
  - 文件: `networkplugin/Plugin.cs`
  - 结果: 已补充 `[SyncProbe] StartupWiring` 与 `PatchResolve[...]` 一次性日志，覆盖启动期 wiring 和三个 Patch 取服务路径
  - 说明: 仍需在游戏内触发对应 Patch 场景并读取日志做最终运行时确认

### 3. `OtherPlayersOverlayPatch` 拆层（P1）

- [√] 3.1 提取网络事件桥接层
  - 文件: `networkplugin/Patch/UI/OtherPlayersOverlay/OtherPlayersOverlayEventBridge.cs`
  - 结果: 已迁出网络客户端解析、订阅、Welcome/PlayerListUpdate/PlayerJoined 等事件桥接逻辑
  - 验证: 构建通过，`OtherPlayersOverlayPatch` 仍可通过局部类访问共享字段与工具方法

- [√] 3.2 提取玩家缓存与位置查询层
  - 文件: `networkplugin/Patch/UI/OtherPlayersOverlay/OtherPlayersOverlayPlayerStore.cs`
  - 结果: 已迁出 `_players` 快照、自身玩家解析、位置查询与名称映射逻辑
  - 验证: `TradePanel`/`MoodEffectSyncPatch` 依赖的静态查询接口保持不变

- [√] 3.3 提取远端视图注册表层
  - 文件: `networkplugin/Patch/UI/OtherPlayersOverlay/OtherPlayersOverlayViewRegistry.cs`
  - 结果: 已迁出远端角色视图、地图图标、目标选择辅助与视图查找逻辑
  - 验证: `RemoteCardUsePatch`/`PlayerTargeterPatch`/`AiDefaultMimicLocalAnimationPatch` 的静态调用面保持兼容

- [√] 3.4 瘦身原 Patch 入口文件
  - 文件: `networkplugin/Patch/UI/OtherPlayersOverlayPatch.cs`
  - 结果: 已改为 `partial` 门面，保留 Harmony 入口与对外静态 API，仅删除迁出的职责实现
  - 验证: `dotnet build networkplugin/NetWorkPlugin.csproj -v minimal` 通过（289 warnings，0 errors）

### 4. `RemoteCardUsePatch` 拆层（P1）

- [√] 4.1 提取发送载荷构建逻辑
  - 文件: `networkplugin/Patch/Network/RemoteCardUsePatch.cs`
  - 结果: 主文件保留发送链路、`BuildActionBlueprint` 与公共 JSON helper，不再承载接收/执行实现。

- [√] 4.2 提取事件订阅与接收桥接逻辑
  - 文件: `networkplugin/Patch/Network/RemoteCardUsePatch.ReceiveBridge.cs`
  - 结果: 已迁出订阅钩子、Welcome/OnRemoteCardUse/OnRemoteCardResolved 事件桥接与播放动画逻辑。

- [√] 4.3 提取回放执行与状态落地逻辑
  - 文件: `networkplugin/Patch/Network/RemoteCardUsePatch.Execution.cs`
  - 结果: 已迁出远程回放执行、Resolved 广播与状态落地逻辑。

- [√] 4.4 保留并标注 ReversePatch 白名单入口
  - 文件: `networkplugin/Patch/Network/RemoteCardUsePatch.cs`
  - 结果: `Card_GetActions_Original` ReversePatch 入口保留且仍为白名单桩方法。

### 5. 系统消息常量收口（已补跑完成）

- [√] 5.1 补齐并统一系统消息常量定义
  - 文件: `networkplugin/Network/Messages/NetworkMessageTypes.cs`
  - 结果: 已补齐 `DirectMessage`、`UpdatePlayerLocation`、`Reconnect_*`、房间管理与通用 `Error` 等系统消息常量。

- [√] 5.2 替换客户端高频系统消息字面量
  - 文件: `networkplugin/Network/Client/NetworkClient.cs`
  - 结果: 已将 `GetSelf_RESPONSE` 响应判定切换为 `NetworkMessageTypes` 常量比较，客户端系统响应入口不再依赖字面量。

- [√] 5.3 替换 Host/Relay 系统消息字面量
  - 文件: `networkplugin/Network/Server/NetworkServer.cs`
  - 文件: `networkplugin/Network/Server/RelayServer.cs`
  - 结果: 已将 `Welcome`、`PlayerJoined`、`PlayerListUpdate`、`DirectMessage`、`Reconnect_*`、房间管理等高频系统消息统一改为 `NetworkMessageTypes` 常量。

### 6. 文档同步与验证（P2）

- [√] 6.1 更新知识库记录
  - 文件: `helloagents/CHANGELOG.md`
  - 文件: `helloagents/INDEX.md`
  - 文件: `helloagents/archive/_index.md`
  - 结果: 已回写执行记录、归档索引与元数据

- [√] 6.2 执行构建验证
  - 文件: `networkplugin/NetWorkPlugin.csproj`
  - 结果: `dotnet build networkplugin/NetWorkPlugin.csproj -v minimal` 通过
  - 验证: 256 warnings / 0 errors

---

## 执行备注

| 任务 | 状态 | 备注 |
|------|------|------|
| 1.x | completed | Host 主路径已完成收敛，旧监听器与旧 reader 入口已清理 |
| 2.x | completed/uncertain | 生命周期统一已落地，并新增 `SyncProbe` 可观测闭环；最终同实例确认仍需游戏内手工验证 |
| 3.x | completed | Overlay 已按“事件桥接 / 玩家缓存 / 视图注册表”三层拆出，并保留静态门面兼容外部调用 |
| 4.x | completed | RemoteCardUse 已按“发送门面 / 接收桥接 / 回放执行”拆层，`partial` 兼容面保持稳定 |
| 5.x | completed | 系统消息常量治理已补跑完成，Client/Host/Relay 高频系统消息已统一收口到 `NetworkMessageTypes` |
| 6.x | completed | 知识库与构建验证已完成，方案已归档 |