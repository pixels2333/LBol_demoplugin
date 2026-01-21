# 事件/对话同步 v1 - How

## 设计总览
保持权威模型（B：允许非 Host 触发，但 Host 仲裁）：
- 非 Host 允许触发“事件开始/对话开始”的上报（例如进入某个事件对话），但不允许在本地确认选项推进。
- Host 是唯一会确认并广播 `OnEventSelection` 的一端（投票模式除外：所有人只投票，不直接推进）。
- 非 Host 只负责“应用 Host 的最终选择”，以 Host 广播为准，避免分叉。

中途加入边界（按需求定义）：
- 玩家中途加入后回到其上次退出时的地图节点；进入节点对应关卡以及关卡内同步不在本方案范围内。

复用现有基础设施：
- `INetworkClient.OnGameEventReceived` 作为统一订阅/分发入口。
- `NetworkMessageTypes` 已包含所需消息键。

## 关键改进点（实施口径）

### 1) 统一入站 payload 解析与校验
- 事件/对话消息以 JSON object 为准，按消息类型提取必需字段（`EventId`/`Options`/`OptionId`/`OptionIndex` 等）。
- 允许 `OptionId` 缺失时用 `OptionIndex` 作为补偿，并在本地 phase/options 存在时映射回 `OptionId`。

### 2) 确定性的本地落地（apply）路径
- 收到 `OnEventSelection`：按 `eventId` 写入 pending。
- 仅在 `VnPanel` 运行且处于 `DialogOptionsPhase` 时落地。
- 幂等去重：同一 `eventId` 若已应用同一 `OptionId`，直接清理 pending。
- 落地后清理 pending，避免重复推进。

### 3) 断线重连/中途加入追赶（v1 最小可用）
- Host 缓存当前 `eventId` 的最近 options 与最近确认 selection。
- Host 在 options 阶段周期性重发 `OnDialogOptions`（包含 `OptionId`），便于重连/中途加入追赶。
- Host 在 `PlayerJoined/Welcome/PlayerListUpdate` 到来时限频重发最小快照：`OnEventStart` + `OnDialogOptions` +（如存在）`OnEventSelection`。

### 4) 投票模式完善
- 仅手动注册 `eventId` 才启用投票模式。
- 非 Host 收到 `OnEventVotingResult` 仅用于诊断/等待后续 `OnEventSelection`，真正推进仍以 `OnEventSelection` 为准。

## 验证口径
- 编译通过：`dotnet build networkplugin/NetWorkPlugin.csproj -c Release`
- 手工验证：Host/Client Adventure 对话选择一致；投票事件一致；options 阶段断线重连后能追赶。
