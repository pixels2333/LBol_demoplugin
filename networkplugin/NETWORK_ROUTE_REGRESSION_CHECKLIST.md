# Network Route Regression Checklist

## 目标

固定 `NetworkServer` / `RelayServer` 对 `FullStateSync*` 与 `RoomState*` 的定向路由回归路径，避免 Host/Client/Relay 三端继续靠临时手测判断“看起来能跑”。

## 适用范围

- `networkplugin/Network/Server/NetworkServer.cs`
- `networkplugin/Network/Server/RelayServer.cs`
- `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs`
- `networkplugin/Patch/Network/RoomStateSyncPatch.cs`

## 回归前准备

1. 启动一组 Host + Client；需要覆盖 Relay 时再补一组 Relay + Host + Client。
2. 清空上一轮房间状态，保证 `RouteProbe` 日志能重新出现首包记录。
3. 打开日志过滤，至少关注以下关键词：
   - `[RouteProbe] FullStateSyncRequest`
   - `[RouteProbe] FullStateSyncResponse`
   - `[RouteProbe] RoomStateRequest`
   - `[RouteProbe] RoomStateResponse`
   - `[RouteProbe] DirectMessage/FullStateSyncRequest`
   - `[RouteProbe] DirectMessage/RoomStateRequest`

## Host / 直连固定回归

| 用例 | 触发方式 | 预期结果 | 观察点 |
|------|----------|----------|--------|
| `FullStateSyncRequest` | Joiner 触发中途加入或重连快照请求 | 请求只转发给房主，不广播给其他客户端 | Host 出现 `RouteProbe FullStateSyncRequest`；非目标客户端无对应落地 |
| `FullStateSyncResponse` | 房主回发完整快照 | 响应只单播给请求方 | Joiner 收到快照；其他客户端无 `FullStateSyncResponse` 落地 |
| `RoomStateRequest` | Joiner 进入节点或主动请求房间状态 | 请求只发往房主 | Host 出现 `RouteProbe RoomStateRequest` |
| `RoomStateResponse` | 房主回发房间状态 | 响应仅到 `TargetPlayerId/RequesterId` 指向的客户端 | 非目标客户端状态不变化 |
| `DirectMessage/FullStateSyncRequest` | 通过 `DirectMessage` 包裹 `FullStateSyncRequest` | 仍按房主权威路由，不信任客户端自填目标 | `DirectMessage/FullStateSyncRequest` 与 `FullStateSyncRequest` 各记录一次首包探针 |
| `DirectMessage/RoomStateRequest` | 通过 `DirectMessage` 包裹 `RoomStateRequest` | 仍由受控路由统一转发到房主 | Host 收到请求；未出现客户端互转广播 |

## Relay 固定回归

| 用例 | 触发方式 | 预期结果 | 观察点 |
|------|----------|----------|--------|
| `FullStateSyncRequest` | Joiner 通过 Relay 触发快照请求 | Relay 只把请求转发给房主所在房间 | Relay 日志显示定向转发；其他房间无串房 |
| `FullStateSyncResponse` | 房主回发快照 | Relay 仅单播给请求方 | Joiner 收到快照，房内旁观玩家不落地 |
| `RoomStateRequest/Response` | 节点推进或主动房间状态同步 | Relay 沿用同样的定向路由边界 | 房间状态只在当前房间目标端变化 |

## 异常路径

| 场景 | 预期结果 |
|------|----------|
| 非 Host 发送 `FullStateSyncResponse` / `RoomStateResponse` | 服务端丢弃，不转发给其他客户端 |
| `FullStateSyncRequest.TargetPlayerId != sender.PlayerId` | 服务端丢弃请求 |
| `RoomStateResponse` 缺少 `TargetPlayerId` 且缺少 `RequesterId` | 服务端丢弃响应 |
| `TargetPlayerId` 指向离线玩家 | 服务端丢弃，不抛异常 |
| `DirectMessage` 缺少 `Payload` | 服务端按空对象 `{}` 处理，不应崩溃 |

## 本轮基线

- 代码收敛后，`NetworkServer` 已统一通过 `TryRouteControlledMessage(...)` 处理 `FullStateSync*` / `RoomState*`。
- `HandleGameEvent`、`HandleSystemMessage` 与 `DirectMessage` 内层控制消息现在共享同一条受控路由判定。
- 构建验证：`dotnet build networkplugin/NetWorkPlugin.csproj -v minimal` 成功（257 warnings / 0 errors）。