# 技术方案：TODO 注释清理与落地（多轮补充）
目录：`helloagents/plan/202601122206_todo_sweep/`

## 扫描范围
- `networkplugin/**/*.cs`

## 判定规则（无需手动验证 vs 需要手动验证）
- **可直接落地**：不改协议/不依赖真实联机环境；主要是补齐缺失实现、健壮性兜底、路由分流、日志与编译修复；以 `dotnet build` 可验证为准。
- **需要手动验证**：涉及真实联机/回放流程、游戏运行期对象与状态应用、UI 交互、UPnP/STUN 外部网络环境、或 `NetworkPlayer` 相关的大规模重构决策。

## 本批完成（无需手动验证）
- **聊天消息路由补齐**：将 `ChatMessage` 归类为 GameEvent 进行转发/分发，保证 UI 层能通过 `OnGameEventReceived` 接收。
  - `networkplugin/Network/Client/NetworkClient.cs`
  - `networkplugin/Network/Server/NetworkServer.cs`
  - `networkplugin/Network/Server/RelayServer.cs`
- **ChatUI 接收/发送对齐**：订阅 `NetworkClient.OnGameEventReceived` 并解析 `ChatMessage`；发送方 ID/昵称改为从 `NetworkIdentityTracker`/`GameStateUtils` 获取（含反射兜底）。
  - `networkplugin/UI/Components/ChatUI.cs`
- **NATTraversal 日志兜底**：补齐 logger 初始化（避免 `Plugin.Logger` 不可用时 NRE），UPnP/STUN 的真实交互仍保持 TODO（需要外部环境）。
  - `networkplugin/Network/Utils/NatTraversal.cs`
- **Save/Load 与事件同步：最小可用辅助实现**：补齐玩家 ID/Host 判断、最小快照构造（仅诊断/可序列化用途），并移除已实现的 TODO 注释。
  - `networkplugin/Patch/Network/SaveLoadSyncPatch.cs`
  - `networkplugin/Patch/Network/EventSyncPatch.cs`
- **ChatConsole：健壮性与 TODO 清理**：加入历史去重/限长/默认值处理，落地本地玩家信息获取；移除已过期的 TODO 占位区域与非必要 TODO 标记。
  - `networkplugin/Chat/ChatConsole.cs`
  - `networkplugin/Chat/ChatMessage.cs`
- **编译修复**：补齐缺失 `using`（`System.Linq`、`NetworkMessageTypes`），确保 Release 编译通过。

## 跳过项（需要手动验证/外部依赖/大规模重构）
- **UPnP/STUN**：真实端口映射与 STUN 交互依赖外部网络环境与实现库。
  - `networkplugin/Network/Utils/NatTraversal.cs`
- **FullStateSyncRequest（非 DirectMessage 路由）**：涉及服务端/Relay 的完整路由与响应语义，需要联机验证。
  - `networkplugin/Network/Server/NetworkServer.cs`
  - `networkplugin/Network/Server/RelayServer.cs`
- **事件入口点/投票/对话同步**：高度依赖游戏运行时与多人一致性验证。
  - `networkplugin/Patch/Network/EventSyncPatch.cs`
- **存档快照应用**：将 Host 存档状态应用到本地游戏需要游戏内验证。
  - `networkplugin/Patch/Network/SaveLoadSyncPatch.cs`
- **战斗/回合相关同步**：MaxMana 获取、服务端应用到 `INetworkPlayer`、请求补充用户 ID 等，需要联机验证与数据源对齐。
  - `networkplugin/Patch/Actions/TurnAction_Patch.cs`
  - `networkplugin/Patch/BattleController_Patch.cs`
- **UI 面板发送逻辑**：与实际 UI/网络架构耦合，需要场景与联机验证。
  - `networkplugin/UI/Panels/TradePanel.cs`
  - `networkplugin/UI/Panels/ResurrectPanel.cs`
- **NetworkPlayer 的 SyncVar/字段重构**：属于结构性改造，需要明确同步模型与联机验证。
  - `networkplugin/Network/NetworkPlayer/*`
- **私聊（Whisper）**：协议与 UI 交互需要明确约束与验证。
  - `networkplugin/Chat/ChatMessage.cs`
- **AI 角色**：低优先级功能，需联机玩法验证。
  - `networkplugin/Network/MidGameJoin/AIPlayerController.cs`

## 验证
- 编译：`dotnet build networkplugin/NetWorkPlugin.csproj -c Release`（通过，存在历史告警但无新增错误）。
- TODO 扫描：剩余 `TODO` 均归类为“需要手动验证/外部依赖/重构决策”（详见 `task.md` 的跳过清单）。
