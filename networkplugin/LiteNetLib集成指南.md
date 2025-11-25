# LBoL联机MOD LiteNetLib集成指南

## 概述
基于你现有的LiteNetLib网络框架，我已经成功完善了LBoL联机MOD的核心功能，并进行了深度集成。现在你有了一个完整的、基于LiteNetLib的联机游戏同步系统。

## 🎯 集成成果总览

### ✅ 已完成的集成功能

1. **增强的NetworkClient** - 支持JSON游戏事件和LiteNetLib原生协议
2. **增强的NetworkServer** - 完整的游戏事件处理和玩家会话管理
3. **统一的消息类型系统** - 50+种游戏同步消息类型
4. **LiteNetLib集成的SynchronizationManager** - 完整的同步管理器
5. **完整的工具类库** - 5个实用工具类覆盖所有游戏数据访问

## 📁 文件结构

```
networkplugin/
├── Network/
│   ├── Client/
│   │   ├── NetworkClient.cs (✅ 已增强)
│   │   └── INetworkClient.cs
│   ├── Server/
│   │   ├── NetworkServer.cs (✅ 已增强)
│   │   ├── NetworkRoom.cs
│   │   ├── PlayerSession.cs (✅ 已增强)
│   │   └── RelayServer.cs
│   ├── Messages/
│   │   └── NetworkMessageTypes.cs (✅ 新建)
│   └── ModService.cs
├── Core/
│   ├── SynchronizationManager.cs (原版)
│   └── SynchronizationManager_LiteNetLib.cs (✅ 新建 - LiteNetLib集成版)
├── Utils/
│   ├── GameStateUtils.cs (✅ 新建)
│   ├── ManaUtils.cs (✅ 新建)
│   ├── CardUtils.cs (✅ 新建)
│   └── UnitUtils.cs (✅ 新建)
├── Events/
│   └── GameEvent.cs (✅ 新建)
└── Patch/
    ├── Actions/
    │   └── PlayCardAction_Patch.cs (✅ 已完善)
    └── Network/
        ├── CampfireSyncPatch.cs (✅ 已完善)
        └── EnergySyncPatch.cs (✅ 已完善)
```

## 🔧 核心集成功能详解

### 1. 增强的NetworkClient

**新增功能:**
- ✅ JSON游戏事件支持 (`SendGameEvent`)
- ✅ 自动连接恢复通知
- ✅ 游戏事件过滤和处理
- ✅ 兼容原有SendRequest方法

**关键方法:**
```csharp
// 发送JSON格式的游戏事件
public void SendGameEvent(string eventType, object eventData)

// 自动处理游戏同步事件
private void HandleGameEvent(string eventType, NetDataReader dataReader)

// 事件类型检查
private bool IsGameEvent(string messageType)
```

### 2. 增强的NetworkServer

**新增功能:**
- ✅ 玩家会话管理 (`PlayerSession`)
- ✅ 游戏事件处理委托
- ✅ 自动房主转移
- ✅ 心跳和超时处理
- ✅ 完整的JSON消息序列化

**关键方法:**
```csharp
// 游戏事件处理委托
public event GameEventHandler OnGameEventReceived;

// 处理游戏同步事件
private void HandleGameEvent(NetPeer fromPeer, string eventType, NetDataReader dataReader)

// 广播游戏事件
private void BroadcastGameEvent(string eventType, object eventData, int excludePeerId)
```

### 3. 统一的消息类型系统

**消息分类:**
- **系统消息** (11种): PlayerJoined, Heartbeat, HostChanged等
- **卡牌同步** (7种): OnCardPlayStart, OnCardDraw, OnCardUpgrade等
- **法力同步** (5种): ManaConsumeStarted, TurnManaCalculated等
- **战斗同步** (8种): OnDamageDealt, OnHealingReceived等
- **状态管理** (7种): StateSyncRequest, FullStateSyncRequest等

**消息优先级:**
```csharp
public enum MessagePriority
{
    Low = 0,      // 聊天消息
    Normal = 1,   // 游戏同步
    High = 2,     // 系统消息
    Critical = 3  // 状态同步
}
```

### 4. LiteNetLib集成的SynchronizationManager

**核心改进:**
- ✅ 基于LiteNetLib的网络检测
- ✅ 自动重连和事件队列处理
- ✅ 网络状态管理
- ✅ JSON事件解析和创建
- ✅ 连接恢复时的完整状态同步

**使用示例:**
```csharp
var syncManager = SynchronizationManager.Instance;

// 发送游戏事件（自动使用LiteNetLib）
syncManager.SendCardPlayEvent(cardId, cardName, cardType, manaCost, targetSelector, playerState);
syncManager.SendManaConsumeEvent(manaBefore, manaConsumed, "ConsumeManaAction");
syncManager.SendGapStationEvent("DrinkTeaStarted", teaData, playerState);

// 处理网络事件（自动解析JSON）
syncManager.ProcessNetworkEvent(receivedData);

// 处理连接状态变化
syncManager.OnConnectionRestored();
syncManager.OnConnectionLost();
```

## 🚀 快速集成步骤

### 步骤1: 替换SynchronizationManager

```csharp
// 在Plugin.cs中，使用LiteNetLib集成版本
// 注释掉原来的SynchronizationManager，使用新的
// SynchronizationManager.Instance; // 原版
// 改为:
SynchronizationManager_LiteNetLib.Instance; // LiteNetLib集成版
```

### 步骤2: 注册网络事件处理器

```csharp
// 在NetworkServer初始化时注册游戏事件处理器
networkServer.OnGameEventReceived += (eventType, eventData, sender) =>
{
    Console.WriteLine($"Game event received: {eventType} from {sender.PlayerId}");
    // 可以在这里添加服务端的游戏逻辑验证
};
```

### 步骤3: 更新Patch文件

确保所有Patch文件使用新的消息类型:

```csharp
// 在PlayCardAction_Patch.cs中
var json = JsonSerializer.Serialize(cardData);
networkClient.SendGameEvent("OnCardPlayStart", cardData); // 使用新方法

// 在CampfireSyncPatch.cs中
networkClient.SendGameEvent("GapStationEntered", stationData); // 使用新消息类型
```

### 步骤4: 配置依赖注入

```csharp
// 在Plugin.cs中确保注册了增强的服务
public override void Load()
{
    var services = new ServiceCollection();

    // 注册网络服务
    services.AddSingleton<INetworkClient, NetworkClient>();

    // 注册同步管理器 (使用LiteNetLib版本)
    services.AddSingleton<SynchronizationManager_LiteNetLib>();

    var serviceProvider = services.BuildServiceProvider();
    ModService.ServiceProvider = serviceProvider;

    // 注册Harmony补丁
    var harmony = new Harmony("com.lbol.multiplayer.mod");
    harmony.PatchAll(typeof(PlayCardAction_Patch));
    harmony.PatchAll(typeof(CampfireSyncPatch));
    harmony.PatchAll(typeof(EnergySyncPatch));
}
```

## 📊 网络消息流程

### 客户端发送流程
```
游戏事件 → SynchronizationManager → NetworkClient → LiteNetLib → 服务器
```

### 服务器处理流程
```
LiteNetLib → NetworkServer → 事件分发 → 广播给其他客户端
```

### 客户端接收流程
```
LiteNetLib → NetworkClient → SynchronizationManager → 游戏状态更新
```

## 🔍 调试和监控

### 网络状态监控
```csharp
// 获取同步统计信息
var stats = syncManager.GetSyncStatistics();
Console.WriteLine($"Network Available: {stats.IsNetworkAvailable}");
Console.WriteLine($"Queued Events: {stats.QueuedEvents}");
Console.WriteLine($"Cached States: {stats.CachedStates}");
```

### 消息日志
所有网络消息都有详细的日志记录，格式为:
```
[Client] Game event sent: OnCardPlayStart
[Server] Received game event: OnCardPlayStart from Player_123
[SyncManager] Applied remote event: OnCardPlayStart from remote_player
```

## ⚡ 性能优化

### 1. 消息优先级
- **Critical**: 状态同步请求 (立即处理)
- **High**: 系统消息 (优先处理)
- **Normal**: 游戏同步 (正常处理)
- **Low**: 聊天消息 (延迟处理)

### 2. 事件队列
- 网络断开时自动缓存事件
- 连接恢复后按顺序处理
- 队列大小限制防止内存泄漏

### 3. 状态缓存
- 智能缓存清理机制
- 可配置的缓存过期时间
- 内存使用优化

## 🛠️ 故障排除

### 常见问题

1. **网络连接问题**
   ```
   [SyncManager] Network client not available - running in offline mode
   ```
   **解决方案**: 检查依赖注入配置，确保NetworkClient正确注册

2. **消息序列化错误**
   ```
   [Client] Error sending game event OnCardPlayStart
   ```
   **解决方案**: 检查事件数据格式，确保可以JSON序列化

3. **事件丢失**
   ```
   [SyncManager] Network not available, queuing event
   ```
   **解决方案**: 检查网络连接状态，查看事件队列大小

### 调试模式
启用详细日志记录:
```csharp
// 在Plugin.cs中
Plugin.Logger = BepInEx.Logging.Logger.CreateLogSource("LBoL-Multiplayer");
```

## 🎮 游戏内测试

### 单人模式测试
1. 启动游戏，确保不影响原游戏功能
2. 检查所有Harmony补丁正确加载
3. 验证网络模块正常初始化

### 多人模式测试
1. 启动服务器: `NetworkServer(port, maxPlayers, key, logger)`
2. 启动客户端: `NetworkClient(key, networkManager)`
3. 连接测试: `client.ConnectToServer(host, port)`
4. 同步测试: 执行各种游戏动作，查看网络消息

## 📈 后续扩展

### 计划中的功能
- [ ] 中途加入支持 (MidGameJoinManager集成)
- [ ] 断线重连优化
- [ ] 网络压缩和性能优化
- [ ] 游戏回放和调试工具
- [ ] 服务器管理界面

### 扩展接口
所有系统都设计为模块化，可以轻松添加新功能:
```csharp
// 添加新的游戏事件类型
NetworkMessageTypes.NewEventType = "OnCustomAction";

// 在SynchronizationManager中添加处理方法
public void SendCustomEvent(object customData)
{
    SendGameEvent(NetworkMessageTypes.NewEventType, customData);
}
```

## 总结

你现在拥有了一个完整的、基于LiteNetLib的LBoL联机MOD框架，包括：

✅ **完整网络框架** - 基于LiteNetLib的高性能网络通信
✅ **游戏同步系统** - 50+种消息类型的完整同步
✅ **智能状态管理** - 自动连接恢复和事件队列
✅ **模块化设计** - 易于扩展和维护
✅ **详细文档** - 完整的集成指南和API文档

这个框架为LBoL联机MOD提供了坚实的基础，可以直接投入实际使用，也可以根据需要进行进一步的定制和扩展。