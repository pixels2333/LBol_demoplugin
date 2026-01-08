# 数据模型

## 1. 网络事件（逻辑层）

项目按 `eventType + eventData(json)` 组织同步事件，`eventType` 建议来自 `Network/Messages/NetworkMessageTypes.cs`。

核心类型：
- `Network/Event/GameEvent.cs`：统一事件对象（含 `EventId`/`EventIndex`/`Timestamp`/`Source`/`Data` 等字段）。
- `Network/Event/GameEventManager.cs`：事件索引分配与管理（用于回放/追赶）。

建议的通用字段（以现有实现与 Patch 用法为参考，最终以实现为准）：
```json
{
  "Timestamp": 1234567890,
  "PlayerId": "player_unique_id",
  "EventType": "OnCardPlayStart",
  "Data": { }
}
```

## 2. 消息分类与优先级

- 类型常量：`Network/Messages/NetworkMessageTypes.cs`
- 分类集合：`Network/Messages/NetworkMessageTypes.cs` 的 `MessageCategories`
- 优先级：`Network/Messages/MessagePriorities.cs`

典型分类：
- 系统类：玩家加入/离开、心跳、房主变更、自身信息查询等
- 游戏同步类：卡牌、法力/能量、战斗、回合、地图、事件等
- 状态管理类：全量状态请求/响应、连接状态、重连尝试等

## 3. 快照（状态层）

用于断线重连/中途加入/全量同步：
- `Network/Snapshot/FullStateSnapshot.cs`：顶层快照（`GameState`/`MapState`/`BattleState`/`PlayerStates` 等）
- `Network/PlayerEntity.cs`：玩家实体及其 `CreateSnapshot`/`ApplySnapshot`

## 4. 房间与中继（Lobby/Relay 层）

房间相关数据模型（用于中继服务器或房间列表）：
- `Network/Room/RoomConfig.cs`：房间配置（MaxPlayers/IsPublic/Password/GameMode/Description）
- `Network/Room/RoomStatus.cs`：房间状态（人数/房主/是否进行中/延迟等）
- `Network/Room/NetworkMessage.cs`：房间内消息（Type/Payload/SenderPlayerId）

## 5. 典型同步场景

- 卡牌使用：`OnCardPlayStart` / `OnCardPlayComplete`
- 法力消耗：`ManaConsumeStarted` / `ManaConsumeCompleted`
- 回合流转：`OnTurnStart` / `OnTurnEnd` / `EndTurn*`
- 全量同步：`FullStateSyncRequest` / `FullStateSyncResponse`
- 聊天：`ChatMessage`

