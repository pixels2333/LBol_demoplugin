# task

- [ ] 确认范围与验收（已按用户需求更新）
  - [ ] SaveSync 的目标：客户端“真实改写局内进度”（地图节点/站点/房间/怪物信息尽力一致）。
  - [ ] SaveSlot：使用“槽位编号”（int）。
  - [ ] 场景：虚拟局域网联机（LAN），可随时请求，不做防刷（后续需要再加）。
  - [ ] PlayerId 口径：使用 `NetworkIdentityTracker` 的 playerId 作为主键。

- [ ] 补齐消息链路（必须）
  - [ ] `networkplugin/Network/Client/NetworkClient.cs`：把 `SaveSyncRequest/SaveSyncResponse/QuickSaveSync` 纳入 `IsGameEvent`。
  - [ ] `networkplugin/Network/Server/NetworkServer.cs`：把上述类型纳入 `IsGameEvent`。
  - [ ] `networkplugin/Network/Server/NetworkServer.cs`：为 SaveSyncRequest/Response 增加“仅定向路由”（参考 FullStateSyncRequest/Response）。
  - [ ] `networkplugin/Network/Server/RelayServer.cs`：把上述类型纳入 IsGameEvent，并在 Relay 路由中支持 Request->host / Response->target。
  - [ ] `networkplugin/Network/Messages/MessagePriorities.cs`：为 SaveSyncRequest/Response/QuickSaveSync 设置优先级（建议 High 或 Critical）。

- [ ] 完善 SaveLoadSyncPatch 的数据契约（满足真实恢复）
  - [ ] `networkplugin/Patch/Network/SaveLoadSyncPatch.cs`：统一使用 `NetworkMessageTypes.*` 常量。
  - [ ] `networkplugin/Patch/Network/SaveLoadSyncPatch.cs`：Request/Response 中的 SaveSlot 改为 int（槽位编号）。
  - [ ] `networkplugin/Patch/Network/SaveLoadSyncPatch.cs`：SaveSyncResponse 必须附带：
        - [ ] `FullSnapshot`（优先 `ReconnectionManager.CreateFullSnapshot()`，后续补齐 Enemy/Map 填充）。
        - [ ] `GameRunSaveDataBytesBase64`（关键：`SaveDataHelper.SerializeGameRun` -> Base64）。
  - [ ] `networkplugin/Patch/Network/SaveLoadSyncPatch.cs`：尽量附带 `LastKnownEventIndex`（用于 MissedEvents 追赶）。

- [ ] 实现 TODO：ApplyHostSaveSync（真实改写 + 战斗补偿）
  - [ ] 新增“恢复器/应用器”组件（建议放在 `networkplugin/Network/SaveLoadSync/`）：
    - [ ] 输入：SaveSlot(int)、GameRunSaveDataBytesBase64、FullStateSnapshot、MissedEvents(optional)
    - [ ] 输出：
      - [ ] 调用 `GameMaster.RestoreGameRun(GameRunSaveData)` 重建地图/站点/房间
      - [ ] 更新 `RemoteNetworkPlayer`（玩家显示态）
      - [ ] 在战斗中时尽力应用 EnemyUnit HP/状态/意图（依赖 SpawnId 或索引对齐）
  - [ ] `SaveSyncManager.ApplyHostSaveSync`：解析 payload -> 调用恢复器/应用器；按 Timestamp 做幂等。
  - [ ] 若包含 MissedEvents：使用 `NetworkClient.InjectLocalGameEvent` 回放（复用 MidGameJoin 的过滤策略）。
  - [ ] 处理“已在局内”场景：先退出当前局内再 Restore（以实现阶段验证可用退出路径为准）。

- [ ] 验证与回归
  - [ ] `dotnet build networkplugin/NetWorkPlugin.csproj -c Release` 通过。
  - [ ] 手动联机验证：
  - 任意时刻请求 SaveSync：客户端收到 SaveSyncResponse（日志可见）。
  - 客户端进度被重建：地图节点/站点/房间状态与主机一致。
  - 战斗中怪物 HP 等能被尽力对齐（至少不崩溃，日志可诊断）。
  - 无广播风暴：Request/Response 仅单播到目标。

- [ ] 文档与知识库同步
  - [ ] `helloagents/wiki/modules/` 增补 SaveLoadSync/SaveSync 的协议说明（若已有对应模块文档则更新）。
  - [ ] `helloagents/CHANGELOG.md` 记录变更。

