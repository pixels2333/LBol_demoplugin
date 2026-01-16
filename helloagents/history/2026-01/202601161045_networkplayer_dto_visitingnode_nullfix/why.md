# 为什么要做这个修复

## 背景
`networkplugin/Network/NetworkPlayer/dto/NetWorkPlayer.cs` 的构造函数在未确保 `VisitingNode` 被赋值的情况下，直接读取 `VisitingNode.X/Y`。

## 问题
- `VisitingNode` 默认值为 `null`，因此构造函数可能触发 `NullReferenceException`。
- 该异常与是否使用 `SyncVar` 无关，但会导致联机流程在初始化阶段不稳定（例如：玩家数据创建/序列化前就崩溃）。

## 目标与成功标准
- 构造 `NetworkPlugin.Network.NetworkPlayer.dto.NetWorkPlayer` 时不再依赖 `VisitingNode` 非空。
- 不引入 `SyncVar`，保持该类型作为纯 DTO/序列化载体的定位。
- 不改变现有 JSON 字段协议（`location_X`/`location_Y` 仍为整数）。
- 行为可预测：当无法从访问节点取坐标时，坐标回退到稳定的默认值（例如 0）。
