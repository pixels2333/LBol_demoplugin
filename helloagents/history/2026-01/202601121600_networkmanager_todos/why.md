# 变更提案: NetworkManager TODO 清理与补齐

## 需求背景
`networkplugin/Network/Client/NetworkManager.cs` 中存在多处 `TODO` 注释，但代码已在后续重构中部分实现（例如：玩家同步已由 `NetworkIdentityTracker` + `OnGameEventReceived` 事件驱动完成），导致 TODO 与实际实现不一致，容易误导维护与二次开发。

本变更目标是：以当前代码架构为准，清理已完成/过期的 TODO，并补齐仍然缺失但对外可能有价值的能力（例如内部 `UpdatePlayerInfo` 的兼容入口）。

## 变更内容
1. 清理/改写已过期的 TODO（不改变现有行为）：
   - `GetPlayer`：移除不可达的 TODO 占位注释，保留现有输入校验与字典查询逻辑。
   - `GetSelf`：确认当前实现与 `LocalNetworkPlayer.userName`/`NetworkIdentityTracker` 的职责划分一致，移除误导性的旧注释。
   - `RegisterPlayer` / `RemovePlayer`：保留现有线程安全与快照脏标记逻辑，移除“未实现”类注释或改为对当前实现的说明。
2. 补齐仍未实现且可能被内部调用的逻辑：
   - `UpdatePlayerInfo(object playerInfo)`：实现为“兼容入口”，将输入解析为 `JsonElement` 并复用现有 `UpdateSinglePlayer/UpdatePlayersFromArray` 更新逻辑（或在确认无引用后改为删除/标记废弃）。

## 影响范围
- **模块:** `networkplugin/Network/Client`、`networkplugin/Utils`（仅依赖关系说明，不改动 tracker）
- **文件:**
  - `networkplugin/Network/Client/NetworkManager.cs`
  - （可选）若需要对外暴露事件/行为约束：`networkplugin/Network/Client/INetworkManager.cs` 的注释
- **API:** 无对外 API 变更（以“清理注释 + 补齐内部方法”为主）
- **数据:** 无

## 核心场景

### 需求: 完成 NetworkManager TODO
对 `NetworkManager` 中标注为 TODO 的位置进行“与当前架构对齐”的实现/清理。

#### 需求: 完成 NetworkManager TODO - 场景: GetPlayer 按 PlayerId 查询
条件：传入 `id` 为 `null/空白/合法字符串`。
- 预期结果：
  - `null/空白` 返回 `null`；
  - 合法 `id` 返回 `_players` 中对应 `INetworkPlayer` 或 `null`；
  - 不引入不可达代码与误导性 TODO 注释。

#### 需求: 完成 NetworkManager TODO - 场景: GetSelf 获取本地玩家实例
条件：单机/未连接/已连接（已收到 `Welcome` 或 `PlayerListUpdate`）。
- 预期结果：
  - 返回 `_selfPlayer`；
  - 说明“身份来源”以 `NetworkIdentityTracker.GetSelfPlayerId()` + `LocalNetworkPlayer.userName` 为准，而不是旧的 `ClientData.username`/请求式获取。

#### 需求: 完成 NetworkManager TODO - 场景: RegisterPlayer 注册/覆盖玩家
条件：传入 `player` 为 `null`/合法实例；`player.userName` 为空或重复。
- 预期结果：
  - `null` 抛出 `ArgumentNullException`；
  - `userName` 为空时生成稳定的 fallback id（如 `Guid`）；
  - 重复注册时覆盖引用但仅在发生“结构变化”时触发快照脏标记。

#### 需求: 完成 NetworkManager TODO - 场景: RemovePlayer 移除玩家
条件：传入 `id` 为 `null/空白/自身/其他玩家`。
- 预期结果：
  - `null/空白` 直接返回；
  - 不允许移除自身映射；
  - 移除成功后刷新快照。

#### 需求: 完成 NetworkManager TODO - 场景: UpdatePlayerInfo 接收玩家信息更新
条件：传入 `playerInfo` 可能为 `JsonElement`、`string(json)` 或其他动态对象。
- 预期结果：
  - 能解析为 JSON 时复用现有 `UpdateSinglePlayer/UpdatePlayersFromArray` 更新玩家属性（名称、角色、位置等）；
  - 解析失败或字段缺失时安全降级为 no-op，不抛异常影响网络线程；
  - 线程安全（使用现有 `_playersLock`）。

## 风险评估
- **风险:** 清理注释时误删仍在使用的内部入口/行为约束不一致（例如 `UpdatePlayerInfo` 虽未被引用但可能被未来补丁调用）。
- **缓解:** 先全局检索引用点；将 `UpdatePlayerInfo` 实现为薄封装以保持兼容；以 `dotnet build` 验证编译通过；避免更改现有同步逻辑与锁策略。
