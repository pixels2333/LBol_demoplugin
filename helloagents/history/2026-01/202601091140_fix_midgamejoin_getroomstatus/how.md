# 技术设计: 修复 MidGameJoinManager 编译错误（GetRoomStatus 缺失）

## 构建错误清单（本次问题）
- `Network/MidGameJoin/MidGameJoinManager.cs`: CS0103 - 当前上下文中不存在名称 `GetRoomStatus`。

## 推荐修复方案（最小变更）
### 方案1（最小改动-推荐）：改用已存在方法
- 将 `ApproveJoin(...)` 内 `GetRoomStatus(request.RoomId)` 改为 `GetRoomInfo(request.RoomId)`。
- 理由：
  - 当前文件中只有这一处引用 `GetRoomStatus`，且同文件已存在 `GetRoomInfo`（同样返回 `RoomStatus?`）。
  - 避免增加额外别名方法，减少未来维护歧义。

### 方案2（备选）：补齐 `GetRoomStatus` 作为别名
- 新增 `private RoomStatus? GetRoomStatus(string roomId) => GetRoomInfo(roomId);`
- 仅在希望保留 `GetRoomStatus` 命名时采用。

## 风险与回滚
- 风险：极低；本次仅为引用修复。
- 回滚：若后续需要保留 `GetRoomStatus` 命名，可改用“方案2”在本文件补齐别名。

## 验证
- `dotnet build .\networkplugin\NetWorkPlugin.csproj -c Debug --no-restore`

