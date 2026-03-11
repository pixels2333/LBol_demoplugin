# Changelog

本文文件记录项目所有重要变更（Keep a Changelog 风格）。

## [Unreleased]

### 重构
- **[networkplugin]**: 第二批防御性代码简化——移除 `networkplugin/` 中大量不必要的 try-catch、冗余 null 守卫与过度防御性封装，共涉及 14 个文件：
  - `TradeSyncPatch`、`ExitGamePatch`、`NetworkIdentityTracker`：`TryGetClient/TryGetNetworkClient/TryGetNetworkManager` 改为表达式体。
  - `AiDefaultMimicLocalAnimationPatch`：`IsEnabled/IsNetworkConnected` 改为表达式体；`Postfix` 中内层 `Singleton<GameDirector>.Instance?.PlayerUnitView` try-catch 删除（null-conditional 已足够）。
  - `OtherPlayersOverlayPlayerStore`：`ShouldInjectTradeDebugPlayers/IsVirtualAiDefaultEnabled` 改为表达式体。
  - `EnemySpawnSyncPatch`、`RoomStateSyncPatch`：3 个独立 Traverse try-catch 合并为 1 个带注释的 try 块。
  - `MapCatchUpOrchestrator`：`stages[2]?.AsNormalFinal()` / `stages[3]?.AsTrueEndFinal()` 外层 try-catch 删除（null-conditional 安全）。
  - `OtherPlayersOverlayViewRegistry`：删除 `unit.Initialize()`、`BoxCollider.enabled`、`SelectorCollider.enabled`、`SnapshotRemoteCharacterUnitViews`、`SetRemoteCharacterTargetingEnabled`、`TryGetRemoteCharacterUnitView`、`SetSelectorColliderEnabled` 七处无必要 try-catch；保留 `StartCoroutine`、`Traverse._circleCollider` 反射、`TryGetPointedRemotePlayer` 的 Camera/Raycast try-catch（有合理抛出可能）。
  - `TradePanel`：
    - 双 `SetButtonText` try-catch 改为直接调用。
    - `onClick` 匿名 lambda 内静默 catch 改为带 `Plugin.Log.LogError` 的错误日志。
    - `Destroy(_offerEditorRoot)` try-catch 删除（Destroy 安全）。
    - `_moneyValueBaseFontSize = _moneyValueText.fontSize` try-catch 删除。
    - `_offerEditorRoot.transform.SetAsLastSibling()` → `_offerEditorRoot?.transform.SetAsLastSibling()`。
    - `Destroy(child/t.gameObject)` 循环体内 try-catch 删除（已有 null 检查）。
    - `LayoutRebuilder.ForceRebuildLayoutImmediate` try-catch 删除（null 守卫已在外层）。
    - `TryGetOwnedMoney` 中 `GameRun?.Money ?? 0` try-catch 删除。
    - `GetComponent<TextMeshProUGUI>` 两处 try-catch 改为直接 block + null 检查。
    - `_offerActionsRoot.SetActive(...)` → `_offerActionsRoot?.SetActive(...)`。
    - `NormalizeButtonWidget` 外层整体 try-catch 删除；fallback 分支 3 行单独 try-catch 合并为 1 行。
    - `PreferSingleButtonWidget` 及 `TryPickButtonTemplate` 的 `GetComponentsInChildren<Button/Transform>` try-catch 删除（返回数组，不抛）。
    - `DisableCursorBehaviours` 内层 `behaviour.enabled = false` try-catch 删除（外层 try 仍保留）。
  - `TradePanelRuntimeFactory`：`p.gameObject.SetActive(false)`、`Destroy(p.gameObject)`、`Destroy(b.gameObject)`、`Destroy(img)`、`slotWidget.enabled = false` 以及 `GetComponentsInChildren<Button/Transform>` 的 try-catch 全部删除。
  - 保留的有理依据 try-catch：`NetworkIdentityTracker.EnsureSubscribed` 事件订阅/取消订阅（竞态保护）；`AiDefaultMimicLocalAnimationPatch.Postfix` 外层（`remote.PlayAnimation` 可能 MRE）；`NormalizeButtonWidget` 的 Traverse 反射 try；`DisableCursorBehaviours/DisableTooltipBehaviours` 外层（Unity 已销毁对象访问）；`TryGetOwnedMoney` 和 `TryGetPointedRemotePlayer` 各自的 Reflection/Raycast 段。
  - 验证：`get_errors` 扫描 `networkplugin/` 全目录，0 errors。


- **[UiObjectQueryPlugin]**: 新增独立 BepInEx 调试插件 `UiObjectQueryPlugin`，通过本地 HTTP `GET /health` 与 `POST /query` 按 `GameObject.name` 精确查询运行时对象，并返回 `Transform`/`RectTransform` 位置、缩放、旋转和布局属性。
	- 验证：`dotnet build UiObjectQueryPlugin/UiObjectQueryPlugin.csproj -v minimal` 通过（1 个仓库级 `Mono.Cecil` 版本冲突警告，0 errors）。
	- 文档：新增 `helloagents/modules/ui-object-query-plugin.md` 并同步 `modules/_index.md`。
	- 方案：已归档至 `helloagents/archive/2026-03/202603111012_ui-object-query-plugin/`。

### 重构
- **[networkplugin]**: 简化 `RemoteCardUsePatch` — 移除 `__instance == null` 冗余守卫；`BattleController`、`MoneyCost`、`senderCharacterId` 三处从 null-conditional 表达式上层的 try-catch 全部删除（null-conditional 本身不会抛）；`Guid.NewGuid()` try-catch 删除（不可能抛）；`RemoteOnlyActions` 中 `card.PendingManaUsage`/`PendingTarget`/`KickerPlaying` 两处赋值组删除防御包装；`CardType` switch 表达式臂上的逐行注释清理。
- **[networkplugin]**: 简化 `EnemyIntentReceivePatch` — 移除 `__instance == null` Harmony 守卫；`TryGetNetworkClient` 改为表达式体；去掉 `EnsureSubscribed` 中 `-=` 取消订阅周围无意义的 try-catch。
- **[networkplugin]**: 简化 `MainMenuMultiplayerEntryPatch` — `TryGetNetworkClient` 改为表达式体。
- **[networkplugin]**: 简化 `TradeDetailDialog` — `OnHiding` 中两个 `SetActive` try-catch 改为 `?.`；`OnCancel` picker 关闭块去掉外层 try-catch；`ApplyCompletedTradeAndClose` 两处 `try { Hide(); } catch {}` 改为直接 `Hide()`；`OnConfirmClick`/`OnCancelClick`/`ChangeMoney`/`ShowCardPicker`/`ShowExhibitPicker` 中 `AudioManager` 调用和 `CurrentGameRun?.Money` null-conditional 访问上的 try-catch 全部删除。
- **[networkplugin]**: 简化 `TradeDetailDialogRuntimeFactory` — `GetComponentsInChildren<Button>()` 和 `GetComponentsInChildren<Transform>()` 上的 try-catch 删除（Unity 该方法返回空数组，不抛、不返回 null）。
- **[networkplugin]**: 移除 `BattleController_Patch`（×2）和 `JoinerStageIndexAlignPatch`、`SpawnedEnemyManager` 中冗余的 `__instance == null` Harmony 守卫。
- **[networkplugin]**: 简化 `Plugin.cs` — `TryLogAssemblyFingerprint` 中 `asm.Location` 单行 try-catch 内联为直接赋值；`new FileInfo(asmPath)` 保留 try-catch 但移入非空快速路径中。
- 验证：`dotnet build networkplugin/NetWorkPlugin.csproj --no-restore` 通过（0 errors）。


- **[networkplugin]**: 简化 `OtherPlayersOverlayEventBridge` — `TryGetNetworkClient` 改为表达式体，合并 `EnsureSubscribed` 中两个独立的 try-catch（取消订阅不再静默吞异常）。
- **[networkplugin]**: 简化 `TradePanel` — `SendTradeEvent` 移除空体 try-catch；`SetupTradeSession` 静默 catch 改为错误日志；`UpdateUIStrings`/`SetButtonText` 移除不必要的 try-catch；`TryShowTradeDetailDialog` 静默 catch 改为日志；`IsLocalDebugTradeAllowed`/`TryEnsureNetworkConnected`/`TryShowTopMessage` 改为直接调用；`ResetTradeData` overlay 块移除无实际作用的 try-catch；清理 `AddCardToTrade`/`RemoveCardFromTrade`/`UpdateTradeSlot` 中逐行解释性注释。
- **[networkplugin]**: 简化 `CardUtils` — `GetHandCardsInfo`/`GetDrawDeckInfo` 移除外层 try-catch（null 检查不会抛，`GetCardInfo` 已有内部保护）。
- **[networkplugin]**: 简化 `SynchronizationManager` — `TryNormalizeNetworkEvent` 移除外层 try-catch 并补 null 快速返回；`DescribePayloadHead200` 嵌套 try-catch 压缩为单层；`ResolvePlayerName` 移除无意义的 catch 块。

### UI
- **[networkplugin]**: 重构 `TradeDetailDialog` 交易详情界面，采用分栏布局并优化列表显示。
- **[networkplugin]**: 引入紧凑型 `ListItem` 样式（44px 高度），替代原本笨重的按钮列表占位符。
- **[networkplugin]**: 优化卡牌与遗物选择器，移除冗余背景并改用网格布局 (Grid Layout)。
- **[networkplugin]**: 修正按钮颜色逻辑，移除确认/添加按钮的错误红色样式。
- **[networkplugin]**: 调整报价编辑器（Offer Editor）按钮缩放比例，提升布局平衡感。
- **[networkplugin]**: 升级 UI 版本至 `v6`，确保修改实时生效。

### Docs
- **[helloagents]**: 执行 `~upgrade` 清洗 `INDEX.md`、`context.md`、`modules/_index.md` 与 `archive/_index.md`，并将 `project.md` / `wiki/` / `history/` 明确标注为 legacy 迁移来源。
- **[helloagents]**: 继续执行 `~upgrade` 第二阶段收口：新增 `modules/lbol.md`，并在 `INDEX.md` 中补充 legacy → 标准结构映射表，方便后续清理遗留文档。
- **[helloagents]**: 继续执行 `~upgrade` 第三阶段软清理：将 `project.md`、`wiki/*` 与 `history/index.md` 收口为跳转桩，消除重复维护内容但保留旧路径兼容。
- **[helloagents]**: 更新方案包 `plan/202602071900_trade-partner-picker-centered-ui`：聚焦 TradePanel 内 partner picker，使用游戏 UI 资源并以居中窗口弹层展示玩家列表。
- **[helloagents]**: 新增方案包 `plan/202603060931_networkplugin-architecture-consolidation`：精确规划 `NetworkServer` 双路径收敛、`SynchronizationManager` 生命周期统一、`OtherPlayersOverlayPatch`/`RemoteCardUsePatch` 拆层以及系统消息常量治理。
- **[networkplugin]**: 新增 `networkplugin/NETWORK_ROUTE_REGRESSION_CHECKLIST.md`，固定 `FullStateSync*` / `RoomState*` 在 Host/Relay 下的请求、响应、`DirectMessage` 包裹与异常路径回归步骤。
- **[networkplugin]**: 新增 `networkplugin/MULTIPLAYER_REGRESSION_CHECKLIST.md`，固定房间生命周期、战斗一致性、功能链路与连接恢复回归步骤，并绑定 `GapOptionsSyncPatch` / `RemoteCardUsePatch` 风险检查。
- **[helloagents]**: 同步 `plan/202603061435_networkplugin-roadmap-open-items`、`networkplugin/PLANNING_ROADMAP.md` 与 `helloagents/modules/networkplugin.md`，完成 `8.1/8.2/8.3` 立项评估结论（成就同步暂不立项；观战模式需前置重构；调试/性能能力并入现有诊断体系）。
- **[networkplugin]**: 统一 `networkplugin` / `lbol` 侧的 GapOptions 术语，将历史旧命名收敛到 `GapOptions*`，避免与 `GapStation` / `GapOptionsPanel` 实际代码概念冲突。

### 变更
- **[networkplugin]**: 执行方案包 `archive/2026-03/202603060931_networkplugin-architecture-consolidation` 的收敛范围批次（`NetworkServer` + `SynchronizationManager`）。
	- `NetworkServer.cs`：删除未接线旧监听器注册入口，以及旧 `NetPacketReader/NetDataReader` 双路径处理实现，只保留 `BaseGameServer -> JSON` 主链。
	- `SynchronizationManager.cs`：删除静态 `Instance` 入口，改为 DI 可构造并延迟解析 `INetworkClient`。
	- `Plugin.cs`：改为 `SynchronizationManager` concrete + `ISynchronizationManager` 别名同实例注册。
	- `NetworkClient.cs`：注入 `ISynchronizationManager`，替换连接恢复、断线、网络事件回放三处静态调用。
	- 验证：`dotnet build networkplugin/NetWorkPlugin.csproj -v minimal` 通过（289 warnings，0 errors）。
- **[networkplugin]**: 继续执行方案包 `archive/2026-03/202603060931_networkplugin-architecture-consolidation` 的 Overlay 拆层批次（`OtherPlayersOverlayPatch`）。
	- `OtherPlayersOverlayPatch.cs`：改为 `partial` 门面，保留现有静态查询/视图访问 API，避免影响 `TradePanel`、`PlayerTargeterPatch`、`RemoteCardUsePatch`、`MoodEffectSyncPatch` 等现有调用方。
	- `Patch/UI/OtherPlayersOverlay/OtherPlayersOverlayEventBridge.cs`：提取网络客户端解析、订阅与玩家列表事件桥接逻辑。
	- `Patch/UI/OtherPlayersOverlay/OtherPlayersOverlayPlayerStore.cs`：提取 `_players` 缓存、自身玩家定位、快照与名称解析逻辑。
	- `Patch/UI/OtherPlayersOverlay/OtherPlayersOverlayViewRegistry.cs`：提取远端角色视图注册、地图图标、目标选择辅助逻辑。
	- 验证：`dotnet build networkplugin/NetWorkPlugin.csproj -v minimal` 通过（289 warnings，0 errors）。
- **[networkplugin]**: 继续执行同一方案包的 `RemoteCardUsePatch` 拆层批次。
	- `RemoteCardUsePatch.cs`：改为 `partial` 发送门面，保留 Harmony 入口、`Card_GetActions_Original` ReversePatch 白名单桩与公共 JSON helper。
	- `Patch/Network/RemoteCardUsePatch.ReceiveBridge.cs`：提取订阅钩子、网络事件桥接与动画预播放逻辑。
	- `Patch/Network/RemoteCardUsePatch.Execution.cs`：提取远程回放执行、Resolved 广播与状态落地逻辑。
	- 验证：`dotnet build networkplugin/NetWorkPlugin.csproj -v minimal` 通过（exit 0）。
- **[networkplugin]**: 继续执行同一方案包的 `5.x` 系统消息常量收口批次。
	- `NetworkMessageTypes.cs`：补齐 `DirectMessage`、`UpdatePlayerLocation`、`Reconnect_*`、房间管理与 `Error` 等高频系统消息常量。
	- `NetworkClient.cs`：将 `GetSelf_RESPONSE` 系统响应判定切换为 `NetworkMessageTypes` 常量比较。
	- `NetworkServer.cs` / `RelayServer.cs`：将 `Welcome`、`PlayerJoined`、`PlayerListUpdate`、`DirectMessage`、`Reconnect_*`、房间管理与通用错误等高频系统消息字面量统一改为常量引用。
	- 验证：`dotnet build networkplugin/NetWorkPlugin.csproj -v minimal` 通过（256 warnings，0 errors）。
- **[networkplugin]**: 继续执行同一方案包的剩余可执行收尾项（运行时可观测探针）。
	- `Plugin.cs`：新增 `SyncProbe` 启动期一致性日志（`StartupWiring`），校验 `SynchronizationManager` concrete/alias 同实例以及 `NetworkClient` 注入一致性。
	- `Patch/Actions/ApplyStatusEffectAction_Patch.cs`、`DamageAction_Patch.cs`、`PlayCardAction_Patch.cs`：接入 `SyncProbe` 的 `PatchResolve[...]` 一次性日志，确认 Patch 侧服务解析路径。
	- `Network/Server/NetworkServer.cs`：新增 `RouteProbe` 一次性日志，覆盖 `DirectMessage/*`、`FullStateSync*`、`RoomState*` 路由首包，便于 Host/直连联机回归定位。
	- 验证：`dotnet build networkplugin/NetWorkPlugin.csproj -v minimal` 通过（256 warnings，0 errors）。
- **[networkplugin]**: 执行方案包 `plan/202603051930_networkplugin-review-optimization-closure` 的 5.5/5.6/6.1~6.4/7.5 收尾批次。
	- 消息治理：`DebutBonusSyncPatch`、`RemoteCardUsePatch`、`GameResultSyncPatch` 关键消息切换到 `NetworkMessageTypes` 常量；新增 `OnDebutBonusRolled` / `OnGameRunResult` 常量。
	- 结构清理：移除未接入的 `MessageCategories`，避免“定义但不使用”结构。
	- NAT 收口：`NatTraversal` 明确 UPnP 语义状态（`DisabledByConfig` / `UnsupportedOrUnavailable` / `AvailableButNotImplemented`），并统一 `[NATTraversal][UPnP]/[STUN]` 日志口径。
	- 配置对齐：`ConfigManager.Sync`/`SyncConfiguration` 新增 `EnableNatDetection` 与 `EnableUpnpExperimental`；`NetworkStatusIndicator` 增加 NAT/UPnP 状态展示。
	- 遗留清理：删除 `ConfigManager.FeatureToggles.cs` 与 `ConfigManager.Performance.cs` 的 `#if false` 历史参考块。
	- 验收范围调整：聊天功能移出当前方案验收范围；`GapOptions` 与敌人链路手工双端回归因无测试环境顺延。
	- 验证：`dotnet build networkplugin/NetWorkPlugin.csproj` 通过（289 warnings，0 errors）。
- **[networkplugin]**: 执行方案包 `plan/202603051930_networkplugin-review-optimization-closure` 的 4.4~5.4（玩家模型映射补齐 + 三端事件判定同源收敛）。
	- `INetworkPlayer` 新增 `stage` 字段；`LocalNetworkPlayer/RemoteNetworkPlayer` 实现并贯通 `NetworkManager.UpdateSinglePlayer` 的 Stage 映射。
	- `OtherPlayersOverlayPatch` 修复 `TryGetJsonElement` 的 string payload 解析断点，并将 `Welcome/PlayerListUpdate/PlayerJoined/PlayerLeft/HostChanged` 切换为 `NetworkMessageTypes` 常量引用。
	- `NetworkMessageTypes` 新增统一判定入口 `IsGameEvent(messageType, route)`（Client/HostServer/Relay），并补充 Chat/Battle/Room/Trade 分组。
	- `NetworkClient/NetworkServer/RelayServer` 的 `IsGameEvent` 改为委托同源规则，消除三端规则漂移。
	- 验证：Problems 视图无错误；关键符号引用链完整（`IsGameEvent`/`stage`/`TryGetJsonElement`）。
- **[networkplugin]**: 执行方案包 `plan/202603051930_networkplugin-review-optimization-closure` 的 3.4~3.6（敌人状态/意图/回放收敛，保留 1 个迭代兼容桥）。
	- `NetworkMessageTypes` 新增 `BattleEnemyIntentChanged`、`BattleEnemyStateChanged` 常量，并将 `EnemyStateUpdate` 显式定义为兼容桥事件。
	- `EnemySyncPatch` 改为 Host 权威发送：主发 `BattleEnemyStateChanged`，兼容镜像 `EnemyStateUpdate`。
	- 新增 `EnemyStateReceivePatch`：客机接收 `BattleEnemyStateChanged/EnemyStateUpdate`，按 `SpawnId` 主键定位并保留 `RootIndex+Id` 兜底匹配，落地 HP/Block/Shield。
	- `EnemyIntentSyncPatch` 与 `EnemyIntentReceivePatch` 收敛为常量事件名；接收侧升级为 SpawnId 主键 + 兼容键双写双查。
	- `NetworkClient/NetworkServer/RelayServer` 的 `IsGameEvent` 显式纳入敌人状态/意图新旧事件名。
	- `MidGameJoinManager.ShouldReplayEventType` 改为显式白名单（含 `BattleEnemySpawned/BattleEnemyIntentChanged/BattleEnemyStateChanged` 与旧名兼容）。
	- 构建验证：`dotnet build networkplugin/NetWorkPlugin.csproj` 通过（289 warnings，无新增错误）。
- **[networkplugin]**: 执行方案包 `plan/202603051930_networkplugin-review-optimization-closure` 的 3.1~3.3（敌人出生单入口收敛，保留 1 个迭代兼容桥）。
	- `NetworkMessageTypes` 新增 `BattleEnemySpawned` 主路径常量；`EnemySpawned` 标注为兼容桥事件。
	- `SpawnedEnemyManager` 发送侧改为主发 `BattleEnemySpawned`，并兼容镜像发送 `EnemySpawned`（仅过渡期保留）。
	- `EnemySpawnSyncPatch` 接收侧统一为 `BattleEnemySpawned/EnemySpawned` 双入口共用同一落地处理。
	- `NetworkClient/NetworkServer/RelayServer` 的 `IsGameEvent` 显式纳入 `BattleEnemySpawned`，降低对前缀匹配的隐式依赖。
	- 构建验证：`dotnet build networkplugin/NetWorkPlugin.csproj` 通过（289 warnings，无新增错误）。
- **[networkplugin]**: 执行方案包 `plan/202603051930_networkplugin-review-optimization-closure` 的 2.5（GapOptions 中途加入追赶最小补充）。
	- `RoomStateSnapshot` 新增 `GapOptionsEvents`（最近 8 条）事件摘要字段，支持 Joiner 最小追赶载荷。
	- `GapOptionsSyncPatch` 广播 payload 新增 `RoomKey`；新增房间级事件缓存与 `MergeCatchupGapOptionsEvents`（仅缓存+ActionId 去重标记，不强行执行游戏动作）。
	- `RoomSyncManager` 在 Request/Upload/Response 链路透传 `GapOptionsEvents`，客户端收到 `RoomStateResponse` 后执行追赶合并。
	- `RoomStateSyncPatch.BuildSnapshot` 附带当前房间 GapOptions 事件摘要。
	- 构建验证：`dotnet build networkplugin/NetWorkPlugin.csproj` 通过（warning 从 290 降至 289，无新增错误）。
- **[networkplugin]**: 执行方案包 `plan/202603051930_networkplugin-review-optimization-closure` 的幽灵方法治理批次。
	- 清理插件入口无用字段 `Plugin.netWorkPlayer`，并删除无引用重复 DTO `Network/NetworkPlayer/dto/NetWorkPlayer.cs`。
	- 移除无调用接口/实现占位：`GetMyself`、`UpdateStance`、`NetworkManager.UpdatePlayerInfo(object)`。
	- 删除 `ShopTradeIconPatch` 中 3 个无调用私有方法，并标注 `RemoteCardUsePatch.Card_GetActions_Original` 为 ReversePatch 白名单保留项。
	- 构建验证：`dotnet build networkplugin/NetWorkPlugin.csproj` 通过（存在既有 warning，无新增错误）。
- **[networkplugin]**: 执行方案包 `plan/202603051930_networkplugin-review-optimization-closure` 的聊天协议统一批次。
	- 统一聊天模型到 `Chat/ChatMessage.cs`，主字段收敛为 `PlayerName`，并兼容读取旧 `username` 负载。
	- 删除 `UI/Components/ChatUI.cs` 内重复 `ChatMessage/ChatMessageType` 定义，改为复用 `NetworkPlugin.Chat` 命名空间模型。
	- 修复聊天消息淡出计时：由哈希构造时间改为显式记录消息创建时间，避免淡出跳变。
	- `ChatConsole` 发送改为 `NetworkMessageTypes.ChatMessage` 常量；`SynchronizationManager` 接收侧名字解析统一为 `PlayerName` 优先并兼容旧字段。
	- 构建验证：`dotnet build networkplugin/NetWorkPlugin.csproj` 通过（存在既有 warning，无新增错误）。
- **[networkplugin]**: 进入方案包 `plan/202603061435_networkplugin-roadmap-open-items` 的首批执行。
	- `Network/Server/NetworkServer.cs`：新增 `TryRouteControlledMessage(...)`、统一 `FullStateSync*` / `RoomState*` 在 GameEvent/SystemMessage/DirectMessage 内层的受控路由，并移除旧的 NetPeer 包装转调维护点。
	- `Utils/GameStateUtils.cs`：新增 `GetCurrentPlayerName()` 统一名称解析入口。
	- `Chat/ChatConsole.cs` / `Chat/ChatMessage.cs`：本地发送名称解析改为走 `GameStateUtils.GetCurrentPlayerName()`；历史 `username/UserName` payload 新增兼容反序列化与显示回退。
	- `Network/Utils/NatTraversal.cs` / `UI/Components/NetworkStatusIndicator.cs`：NAT 方向明确为“STUN + UPnP 语义展示”；新增统一摘要/策略文案，并删除 `NetworkStatusIndicator` 中旧的 `_upnpEnabled` / `_natType` 并行状态源。
	- `Network/NetworkPlayer/NetWorkPlayer.cs` / `Network/NetworkPlayer/dto/README.md`：保留 `username/location_X/location_Y` 等 legacy 线协议字段不变，同时新增 PascalCase 运行时别名层，并为 `dto/` 目录补充用途说明。
	- `Patch/UI/OtherPlayersOverlay/OtherPlayersOverlayPlayerStore.cs` / `UI/Panels/TradePanel.cs` / `Patch/DeathPatches.cs` / `Patch/Network/ResurrectSyncPatch.cs`：新增 `OtherPlayersOverlayPatch.ResolveDisplayName(...)` 作为统一显示名入口，收敛交易、复活登记、Overlay 与地图图标的名字来源，并把本地玩家兜底固定到 `GameStateUtils.GetCurrentPlayerName()`。
	- 验证：`dotnet build networkplugin/NetWorkPlugin.csproj -v minimal` 通过（257 warnings，0 errors）。

### Fixed
- **[networkplugin]**: 收紧 `ShopTradeIconPatch` 的商店回归保护：在注入交易按钮前保存 `CardService` / `ReturnButton` 原生容器的 `RectTransform` 快照，并在隐藏或异常清理时完整恢复，避免商店布局残留偏移。
- **[networkplugin]**: 收敛 `ShopTradeIconPatch` 的商店交易按钮文案定位逻辑，改为优先解析主标题 `TMP_Text`，仅在异常层级时回退到 legacy `Text` 并打印 hierarchy。
	- 方案: [202603061348_networkplugin-todo-consolidation](archive/2026-03/202603061348_networkplugin-todo-consolidation/)
	- 决策: networkplugin-todo-consolidation#D001(目标节点解析替代全量覆盖), networkplugin-todo-consolidation#D002(层级日志改为条件诊断)
	- 验证: `dotnet build networkplugin/NetWorkPlugin.csproj -v minimal` 通过（257 warnings，0 errors）。
- **[networkplugin]**: 修复服务器端 GameEvent 分类遗漏导致的回合结束卡死：将 `EndTurnRequest/EndTurnStatus/EndTurnConfirm` 与 `CardStateChanged` 归类为 GameEvent，避免被当作未知系统消息丢弃（影响 Host/Relay）。
- **[networkplugin]**: 修复客户端点击结束回合后掉线与按钮卡死：补齐 `EndTurn*`/`CardStateChanged` 的 GameEvent 分类；在 `PollEvents()` 中周期发送 `Heartbeat` 保活；断线时强制恢复 EndTurn 按钮可点击。
- **[networkplugin]**: 修复部分场景下结束回合后意外断线回主菜单：`GameMaster.QuitGame` 在联机中可能被内部流程触发；改为拦截并忽略该调用（仅记录告警+调用栈），避免误触发“断开联机并返回主菜单”。
- **[networkplugin]**: 修复结束回合“偶发不推进”的软锁：确认到达时战斗可能尚未进入 `IsWaitingPlayerInput`（动画/结算中），原逻辑只尝试一次导致永远不放行；改为主线程上短暂延迟重试并设置超时兜底（避免永久锁手）。
- **[networkplugin]**: 改善商店交易入口布局：将“交易”按钮插入到商店底部按钮条中，位于“卡牌服务”与“关闭商店”之间，并对相邻按钮做缩放以避免遮挡。
- **[networkplugin]**: 交易面板改为运行时克隆游戏 UI 模板创建（背景/按钮/TMP），并移除 `AddComponent<TradePanel>` 的裸创建回退，避免序列化字段缺失导致空引用。
- **[networkplugin]**: TradePanel partner picker 弹层改为克隆 `UI/Dialogs/MessageDialog` 作为遮罩/窗口框架（替换运行时纯色遮罩与 Outline 边框），列表仍使用 CommonButtonWidget 行样式以保持风格统一。
- **[networkplugin]**: TradePanel partner picker 禁用 fallback/兜底策略：列表滚动区与行模板直接复用 `UI/Panels/HistoryPanel` 的 `ScrollRect + RecordRow`，空列表提示复用 `MessageDialog` 的 subText，确保可见控件均为游戏 UI 资源。
- **[networkplugin]**: 修复 TradePanel partner picker 候选“点不了”：点击检测改为优先使用 Unity InputSystem 的 `Mouse`（兼容禁用 legacy `UnityEngine.Input` 的环境），并在命中判定中遍历候选项全部 `Graphic`，避免根 Rect 为 0 导致无法选中。
- **[networkplugin]**: 改善 TradePanel partner picker 在“严格同节点”筛选下的空列表体验：打开时若尚未同步到自身位置，则显示“正在同步位置信息...”并在 1.0 秒内自动刷新一次；同时增加“刷新”按钮用于手动重试（并在位置仍缺失时重新触发一次性等待刷新）。
- **[networkplugin]**: 修复状态效果同步日志刷错：部分状态效果 `HasLevel=false`，读取 `StatusEffect.Level` 会抛 `has no level`；改为仅在 `HasLevel` 时读取并将 Level 作为可空字段输出。
- **[networkplugin]**: 避免网络事件缓冲区因同一 tick 重复 key 导致的异常（SortedList duplicate key）。
- **[networkplugin]**: 修复 CardStateChanged 负载序列化失败（ManaGroup 自引用）——卡牌快照改为发送 `CostText`。
- **[networkplugin]**: 限流/去重高频同步事件：`UpdatePlayerLocation` 与 `OnMoodEffectStateSync`，减少刷屏与重复发送。
- **[networkplugin]**: 为 `FullStateSyncRequest` 增加 2 秒节流并附带 `RequestId`，降低重连路径重复请求。
- **[networkplugin]**: 修复 TradePanel 运行时 UI “矩形块”观感：`TradePanelRuntimeFactory` 的面板背景与交易槽位背景改为使用游戏内 `Adventure` 切片背景（`ResourcesHelper.LoadUiBackground("Adventure")` + `Image.Type.Sliced`），并用白色 tint 让纹理可见。
- **[networkplugin]**: TradePanel 交易槽位 UI 改为直接克隆游戏内 `CommonButtonWidget` 按钮模板并挂载 `TradeSlotWidget`，确保槽位外观使用原生按钮素材而非运行时纯色矩形。
- **[networkplugin]**: TradePanel 移除全屏半透明背景色块：改为透明 raycast blocker，仅使用 `UI/Dialogs/MessageDialog` 的游戏框架提供视觉；同时 `TradeSlotWidget` 选中态不再把按钮底图染成纯色块（优先仅调整 alpha），避免再次出现“纯色矩形”。
- **[networkplugin]**: 增强“是否加载了新 DLL”的可观测性：启动时输出插件程序集路径、最后写入时间、大小与 FNV64 指纹，便于排查“没有任何变化”是否为部署路径/版本不一致。
- **[networkplugin]**: TradePanel 状态文案 `Trade.WaitingForItems` 缺失本地化时回退为中文，避免 Unity Log 刷屏并更直观地看到状态变化。
- **[tooling]**: `copy_networkplugin_dll.ps1` 支持自动探测目标 Mods 目录（从 BepInEx/ModLBoL 日志推断），并在复制前后输出源/目标的元信息与 SHA256（可选复制支持库）。
- **[networkplugin]**: TradePanel 报价编辑弹层（`EnsureOfferEditorOverlay()`）改为优先从 `UI/Dialogs/MessageDialog` 预制体提取 TMP/按钮模板并克隆，按钮会禁用多余 Button/Tooltip 行为；同时为容器增加 `Adventure` 切片背景，确保报价编辑区域整体观感为游戏原生 UI。

### 变更
- **[networkplugin]**: 网络日志中文化与参数化：发送/接收日志附带 payload 指纹（FNV-1a 64）与关键字段摘要，便于区分“同一事件重复发送”与“不同事件”。
- **[networkplugin]**: 脱敏敏感字段：`joinToken` 不再出现在日志中（仅记录“已脱敏”）。

### 变更
- 局内地图进度同步（inrun-map-progress-sync）：移除 HostSaveTransfer 存档分片传输链路，改为 FullStateSnapshot 聚焦 seeds + MapState（主机权威）。
	- `networkplugin/Network/Messages/NetworkMessageTypes.cs`: 删除 `OnHostSaveTransferStart/Chunk/End`。
	- `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs`: 删除 HostSaveTransfer 相关 handler/发送逻辑，FullStateSyncResponse 不再包含 host save bytes。
	- `networkplugin/Network/Snapshot/MapStateSnapshot.cs`: 扩充地图进度与提交点字段 `NodeStates`、`ClearedNodes`、`LastCheckpointId`、`LastCheckpointAtUtcTicks`（保留旧字段兼容）。
	- `networkplugin/Network/Reconnection/ReconnectionManager.cs`: 新增 `MarkMapCheckpoint(reason, nodeKey)` 并在 CreateFullSnapshot 中填充 LastCheckpoint*。

### 新增
- 局内地图进度同步（inrun-map-progress-sync）：新增主机侧“关键提交点”Hook（用于中途加入/重连对齐）。
	- `networkplugin/Patch/Network/RoomEntrySyncPatch.cs`: enter_node checkpoint（忽略 forced EnterNode）。
	- `networkplugin/Patch/Actions/TurnAction_Patch.cs`: battle_end checkpoint。
	- `networkplugin/Patch/Network/MapCheckpointSyncPatch.cs`: next_stage / reward_closed / station_finish / shop_after_buying / gap_option_selected checkpoints。

- 局内地图进度同步（inrun-map-progress-sync）：新增客户端侧最小追赶骨架（MapState 消费/应用）。
	- `networkplugin/Network/MidGameJoin/MapCatchUpOrchestrator.cs`: 暂存 FullSnapshot 并在本地 GameRun/地图 UI 就绪后尽力对齐节点状态与路径。
	- `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs`: 收到 FullSnapshot 后写入追赶执行器。
	- `networkplugin/Patch/Map/MapPanelUpdateMapNodesStatusPatch.cs`: 在 MapPanel 刷新时 opportunistic apply pending MapState。
	- `networkplugin/Network/MidGameJoin/MapCatchUpOrchestrator.cs`: 追赶执行改为“可分帧会话 + 预算推进”，避免一次性应用导致 UI 卡顿；应用完成后仍保持 room-state 主动请求逻辑。
	- `networkplugin/Patch/Map/MapPanelUpdateMapNodesStatusPatch.cs`: 增加 `MapPanel.Update` 每帧小预算驱动，并在 `UpdateMapNodesStatus` 中使用更合理的预算，提升追赶收敛速度。

- 局内地图进度同步（inrun-map-progress-sync）：Joiner 开局锁定与 stage 对齐。
	- `networkplugin/Patch/MidGameJoin/JoinerStartGameLockPatch.cs`: Patch `GameMaster.StartGame`，joiner 选角但强制锁定主机 `seed/difficulty/puzzles/mode/stages/debutAdventureType`。
	- `networkplugin/Patch/MidGameJoin/JoinerStageIndexAlignPatch.cs`: Patch `GameRunController.EnterNextStage`，首次进入 stage 前设置 `_stageIndex = hostStageIndex - 1`，加速 `MapSeedUlong` 对齐。

- 局内地图进度同步（inrun-map-progress-sync）：断线重连（回主菜单）最小闭环。
	- `networkplugin/Patch/UI/MainMenuMultiplayerEntryPatch.cs`: Join 时检测本地可继续存档，支持“连接成功后本地 Restore → 向房主追赶 FullSnapshot”。
	- `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs`: 新增 `BeginReconnectAndCatchUp(...)` 与 `TryGetApprovedJoinTokenByRequestId(...)`，避免 UI 线程同步等待。
	- `networkplugin/Plugin.cs`: 增加主线程调度队列（`RunOnMainThread`）与定期 catch-up pump，确保后台线程回调/弹窗安全并提升“不开地图也能追赶”的可靠性。
	- `networkplugin/Network/MidGameJoin/MapCatchUpOrchestrator.cs`: 增加 `PumpMainThread()`；SetPendingSnapshot 变为纯数据写入；MapState 应用成功后主动请求当前 `RoomStateSnapshot`（避免 catch-up 绕开 EnterNode 导致房间状态缺失）。
	- `networkplugin/Patch/Map/MapPanelUpdateMapNodesStatusPatch.cs`: 调整为 Prefix 先 apply pending map state，确保本次地图刷新直接展示对齐结果。
	- `networkplugin/Patch/Network/RoomStateSyncPatch.cs`: 忽略 `forced` 的 `GameMap.EnterNode`（用于路径重建/追赶），避免重建路径时刷 `RoomStateRequest`。
	- `networkplugin/Network/MidGameJoin/MapCatchUpOrchestrator.cs`: 追赶时按 `PathHistory` 逐节点调用 `GameMap.EnterNode(forced=true)` 重建 `_path`，使地图内部 bookkeeping 更一致（4.4 方向）。

- 局内地图进度同步（inrun-map-progress-sync）：战斗意图追赶补齐（Host 权威 + 客户端落地）。
	- `networkplugin/Patch/Network/EnemyIntentSyncPatch.cs`: 限定为 Host 才广播 `BattleEnemyIntentChanged`；并在 `PlayerJoined/Welcome/PlayerListUpdate` 时限频重发“当前战斗所有敌人的意图”，加速 join/reconnect 的 UI 追赶。
	- `networkplugin/Patch/Network/EnemyIntentReceivePatch.cs`: 客户端接收 `BattleEnemyIntentChanged`，按 SpawnId/RootIndex+Id 定位 EnemyUnit，重建 `EnemyUnit.Intentions` 并触发 `NotifyIntentionsChanged()` 以刷新意图 UI。

- 局内地图进度同步（inrun-map-progress-sync）：补齐最小可运行验收材料。
	- `helloagents/plan/202601251728_inrun-map-progress-sync/verify.md`: 手工验收清单（中途加入/战斗意图/回主菜单重连）。
	- `debugtools/VerifyInrunMapProgressSync.cs`: 轻量级仓库不变量校验（无 SaveLoadSyncPatch、无 HostSaveTransfer 常量）。

### 新增
- **[networkplugin]**: 完善交易详情对话框 `TradeDetailDialog`：
    - **逻辑与同步**: 实现完整的报价同步、地理位置过滤（仅显示同节点玩家）、握手确认以及交易完成后卡牌/金币/遗物的严格扣除与增加。
    - **UI 风格沉浸化**: 彻底重构 UI 构建流程，移除“矩形块”原生样式，全面使用游戏内的 `Adventure` 背景素材、`CommonButtonWidget` 按钮素材以及 `RecordCardCell`/`ExhibitWidget` 等原生组件，确保视觉风格与游戏本体高度一致。
    - **交互增强**: 为交易面板添加游戏原生音效（确认/取消/点击），并在列表区域增加半透明装饰底色，提升操作反馈感。
- 交易同步：新增 `TradeSyncPatch`（Host 权威裁决 + 广播）与 `TradePanel` 联机接入，支持两端报价/确认/取消与完成后各自卡组落地（模型A）；并提供 `OnTradeSnapshotRequest` 用于重连/中途加入的会话状态恢复。
- 交易同步 v2：交易范围扩展为卡牌/道具/金币/Exhibit；增加 Preparing + PrepareResult 握手（本地严格校验并允许失败后回到 Open 重试）；TradePanel 增加交易对象选择 overlay、报价编辑（金币/展品），并在 Completed 阶段严格落地（缺失即失败）。
- 交易入口一致性：GapOptions 与 ShopTradeIcon 统一“交易不可用/未连接/配置禁用/缺少 TradePanel 实例”提示。
- Gap 复活同步：新增 `ResurrectPanel` + `ResurrectSyncPatch` + `DeathRegistry`，实现发起者请求 -> Host 广播 -> 目标本人落地复活；成功后发起者扣费、失败提示（等价退款）。
- 网络身份：补齐 `INetworkPlayer.playerId`（服务端分配并下发的唯一标识），并在 `LocalNetworkPlayer` 中通过 `NetworkIdentityTracker` 提供 selfId。
- 事件/对话同步：为 `EventSyncPatch` 增加最小追赶能力（缓存并周期性广播对话 options、对新加入/欢迎事件触发快照重发），并增强 `OnEventSelection` 的确定性落地（OptionId/Index 映射、幂等去重、落地后清理 pending）。
- 事件/对话同步：权威模型 B 支持（允许非 Host 上报事件开始，但最终选择仍由 Host 仲裁并广播）。
- 回合同步：新增 `OnTurnEnd` 回合结束快照结构与接收落地（缓存 + 更新远端玩家基础字段）。
- 远端目标出牌链路增强：未连接阻断/发送失败回退。Resolved 乱序丢弃（ResolveSeq/Timestamp/RequestId）。动画一致性改进。
- 文档：补充并归档“远端队友目标出牌闭环（目标端结算 + 快照广播）”方案包。
- 断线重连：补齐 `ReconnectionManager` TODO，并补齐 `FullStateSyncRequest/FullStateSyncResponse` 走 GameEvent 通道（Host/Relay/Client，待手动联机验证）。
- NetworkManager：清理过期 TODO 注释，并补齐 `UpdatePlayerInfo(object)` 作为兼容入口（复用既有 JSON 更新逻辑）。
- MidGameJoin：落地 `MidGameJoinRequest/Response` + 通过 Relay `DirectMessage` 跑通 FullSync 请求/响应，并加入最小事件追赶回放（失败降级）。
- FullStateSync：实现 RelayServer 的 FullStateSync 定向路由（请求转发给房主、响应单播给请求方），并在 NetworkServer 增加 DirectMessage 中继以支持 Host/直连模式下的同一链路。
- 房间/战斗残局同步：新增 RoomStateRequest/Response/Upload/Broadcast 消息类型与优先级，并提供主机缓存 + 客户端进入节点请求/战斗中上传的最小闭环；直连与 Relay 均支持定向路由。

### 修复
- 修复战斗同步补丁：`BattleController_Patch` 对齐 `BattleController.Damage/Heal/TryAddStatusEffect/RemoveStatusEffect` 的真实签名，并统一改为走 `SendGameEventData`；按“主机广播/客户端上报”拆分 Battle* 事件；状态效果同步升级为“增量为主 + 周期全量校验”，并移除对私有字段的 Traverse 依赖。
- 修复 `TradeSlotWidget` 多次 SetCard 导致按钮回调累积的问题。
- 修复编译错误：引入 `INetworkPlayer` 的 mana 兼容层并替换直接访问，`networkplugin` 可编译通过。
- 修复 `MidGameJoinManager` 编译错误：将 `GetRoomStatus(...)` 引用改为 `GetRoomInfo(...)`。
- 修复 `NetWorkPlayer` 构造函数潜在空引用：`VisitingNode` 为空时初始化 `location_X/location_Y` 回退为 0。
- 修复 NAT 信息序列化与 token 校验：为 `IPEndPoint` 增加 JSON 转换器（`ip:port`），修复连接 token 的 TTL 校验，并用最小 STUN Binding 探测替换占位实现；UPnP 默认按不可用回退。
- 修复主菜单缺少“多人游戏”入口：增强 `MainMenuMultiplayerEntryPatch` 的模板按钮定位（分层降级），并改为打开自定义遮罩面板 UI（做房主/加入房主/返回），入口默认可见。
- 修复“多人游戏”入口弹窗边界线与打开异常：将上下金色分隔线改为围绕弹窗容器绘制（向中心各内缩 50px），并确保 overlay 置顶显示；同时为创建流程补充异常日志，避免点击无反馈。
- 修复心情特效同步刷屏/失效：移除定时状态广播，改为“心情变化触发状态同步”（开始/结束时发送），并修复接收端对 string JSON payload 的解析与远端视图重建后的状态恢复。
- 修复 Host 默认端口：当 ServerPort 未配置或为非正数时，Host/Join 弹窗与本机服务器启动统一回退到 7777。
- 增强中途加入存档继承可观测性：为房主存档分片传输、客户端接收/解码、落盘与恢复补充关键日志（Plugin.Logger）。

### 文档
- 补齐 `INetworkPlayer` 的 XML 文档注释：解释 `stance/ultimatePower/mana/UpdateLocation/GetMyself` 的网络语义与 TODO 背景。

### [0.10.3] - 2026-02-13
- **[TradePanel]**: 优化报价编辑器（Offer Editor）布局。
  - 减小按钮缩放比例 (0.75x) 以适应行高。
  - 调整行间距与对齐方式，使文字和按钮分布更合理。
  - 强制 UI 版本更新至 `v4` 以刷新运行时实例。
  - 优化了卡牌、金币、展品三行的水平布局。
