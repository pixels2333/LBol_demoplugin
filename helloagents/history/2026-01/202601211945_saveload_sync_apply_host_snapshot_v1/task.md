# task

- [ ] 确认范围与验收（按你最新规则）
  - [ ] 目标不是“统一路线/统一进度”，而是“房间/战斗残局同步 + 已清房领奖励”。
  - [ ] 谁先进入房间/先开战：负责把房间信息上传到主机；主机再同步给其他人。
  - [ ] 客机进入主机未经过的节点：按已同步的随机种子在本地生成。
  - [ ] SaveSlot 若继续沿用：槽位编号从 1 开始算。
  - [ ] 直连与 Relay 都要支持。

- [ ] 补齐消息链路（必须）
  - [ ] 确认/新增 `NetworkMessageTypes` 常量：
    - [ ] `RoomStateRequest`
    - [ ] `RoomStateResponse`
    - [ ] `RoomStateUpload`
    - [ ] （可选）`RoomStateBroadcast`
  - [ ] `networkplugin/Network/Client/NetworkClient.cs`：把上述类型纳入 `IsGameEvent`。
  - [ ] `networkplugin/Network/Server/NetworkServer.cs`：把上述类型纳入 `IsGameEvent`。
  - [ ] `networkplugin/Network/Server/NetworkServer.cs`：为 RoomStateRequest/Response/Upload 增加“主机中枢路由”（Request->host, Upload->host, Response->requester）。
  - [ ] `networkplugin/Network/Server/RelayServer.cs`：同样纳入 IsGameEvent，并在 Relay 路由中支持 Request/Upload->host / Response->target。
  - [ ] `networkplugin/Network/Messages/MessagePriorities.cs`：为房间同步消息设置优先级（建议 High 或 Critical）。

- [ ] 完善房间同步的数据契约（核心）
  - [ ] 定义 RoomKey 口径（从 lbol/ 找稳定来源）：层数/节点索引/坐标/路径等，确保主机与各客户端一致。
  - [ ] 定义 RoomStateSnapshot：
    - [ ] RoomKey、RoomVersion、RoomPhase（未访问/战斗中/战斗结束/奖励已领取）
    - [ ] RoomSeed（用于未同步房间的本地生成）
    - [ ] Enemies 清单（种类/数量/序号 + HP/格挡/护盾/状态/意图等）
    - [ ] RoomOutcome（战斗结束后的奖励结果）
  - [ ] Request/Response/Upload 统一使用 `NetworkMessageTypes.*` 常量。
  - [ ] SaveSlot 若继续沿用：在相关请求中用 int，且从 1 开始。

- [ ] 实现 TODO：应用主机房间快照（房间残局/奖励）
  - [ ] 新增“房间状态中枢”组件（建议放在 `networkplugin/Network/RoomSync/`）：
    - [ ] 主机：缓存 `RoomKey -> RoomStateSnapshot`，维护 RoomVersion。
    - [ ] 主机：处理 Upload（更新缓存）与 Request（返回最新快照）。
  - [ ] 新增“客户端房间应用器”组件：
    - [ ] 进入房间时发送 Request。
    - [ ] 收到 Response：
      - [ ] 未访问：按 RoomSeed/同步 RNG 规则生成。
      - [ ] 战斗中：按 Enemies 清单生成并应用状态（HP/格挡/护盾/状态/意图）。
      - [ ] 战斗结束：跳过开战，直接发放/展示奖励（按 RoomOutcome）。
  - [ ] `SaveSyncManager.ApplyHostSaveSync`：实现为“应用房间快照”的入口（或迁移/重命名为 RoomSyncManager），按 RoomVersion/Timestamp 做幂等。
  - [ ] 先进入者战斗中上报：
    - [ ] 战斗开始上传一次（怪物清单与初始状态）。
    - [ ] 每回合上传一次（建议节流）。
    - [ ] 战斗结束上传一次（RoomOutcome）。

- [ ] 验证与回归
  - [ ] `dotnet build networkplugin/NetWorkPlugin.csproj -c Release` 通过。
  - [ ] 手动联机验证：
  - 进入同一房间：后进入的人能拿到主机快照并应用（战斗中接残局 / 战斗结束直接领奖励）。
  - 进入主机未走过节点：能按同步 RNG 在本地生成（至少不崩溃，日志可诊断）。
  - 直连与 Relay 下都能收发房间同步消息。
  - 无状态风暴：Request/Response/Upload 以单播为主，广播可选且可控。

- [ ] 文档与知识库同步
  - [ ] `helloagents/wiki/modules/` 增补 SaveLoadSync/SaveSync 的协议说明（若已有对应模块文档则更新）。
  - [ ] `helloagents/CHANGELOG.md` 记录变更。

