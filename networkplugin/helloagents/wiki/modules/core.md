# Core 模块

目录：`Core/`

## 职责
- 同步管理：对外提供 `ISynchronizationManager`
- 事件组织：从 Patch 接收本地行为，按配置筛选/入队，发送到网络层
- 远端应用：接收远端事件，做去重/乱序缓冲与时间戳校验后，回放到本地游戏
- 状态同步：支持增量事件与全量状态请求/响应（断线重连/中途加入时使用）
- 统计与观测：提供同步统计与缓冲统计，便于 UI/日志展示

## 关键文件
- `Core/ISynchronizationManager.cs`
- `Core/SynchronizationManager.cs`

## 关键对象（以实现为准）
- `SynchronizationManager.Instance`：单例入口（通过 `ModService.ServiceProvider` 初始化网络依赖）
- `_eventQueue`：待发送事件队列（本地 → 网络）
- `_remoteEventBuffer`：远端事件缓冲（处理乱序/重复/延迟）
- `_stateCache`：状态缓存（用于快速对齐/回放辅助）

## 常见调用路径
- 本地：`Patch/*` → `ISynchronizationManager.Send*Event(...)` / `SendGameEvent(...)`
- 远端：`INetworkClient` 收到消息 → `ISynchronizationManager.ProcessEventFromNetwork(...)` → 应用到游戏对象

