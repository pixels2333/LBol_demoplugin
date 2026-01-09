# 变更提案: 修复 MidGameJoinManager 编译错误（GetRoomStatus 缺失）

## 需求背景
- `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs` 存在编译错误：CS0103 - 当前上下文中不存在名称 `GetRoomStatus`。
- 目标：以最小变更恢复 `networkplugin` 编译通过，避免引入新的运行时行为变更。

## 变更内容
1. 将 `ApproveJoin(...)` 中对 `GetRoomStatus(...)` 的调用改为已存在的 `GetRoomInfo(...)`（两者语义一致：查询房间状态/房间信息）。

## 影响范围
- 模块：`networkplugin`
- 文件：`networkplugin/Network/MidGameJoin/MidGameJoinManager.cs`
- 预期影响：仅影响编译通过；`GetRoomInfo` 当前仍为 stub（返回 `null`），因此运行时逻辑不改变现状。

## 风险评估
- 风险：低（仅修复方法名引用，未改变数据结构与对外接口）。
- 缓解：修复后执行 `dotnet build .\networkplugin\NetWorkPlugin.csproj -c Debug --no-restore` 验证。

