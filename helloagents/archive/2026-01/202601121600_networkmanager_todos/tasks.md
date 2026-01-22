# 任务清单: networkmanager_todos

目录: `helloagents/plan/无_networkmanager_todos/`

---

## 任务状态符号说明

| 符号 | 状态 | 说明 |
|------|------|------|
| `[ ]` | pending | 待执行 |
| `[√]` | completed | 已完成 |
| `[X]` | failed | 执行失败 |
| `[-]` | skipped | 已跳过 |
| `[?]` | uncertain | 待确认 |

---

## 执行状态
```yaml
总任务: (已归档)
已完成: (参考原任务列表)
完成率: (参考原任务列表)
```

---

## 任务列表

目录: `helloagents/plan/202601121600_networkmanager_todos/`

---

## 1. networkplugin/Network/Client
- [√] 1.1 在 `networkplugin/Network/Client/NetworkManager.cs` 中清理已实现但残留的 TODO（`GetPlayer/GetSelf/RegisterPlayer/RemovePlayer`），验证 why.md#需求-完成-networkmanager-todo---场景-getplayer-按-playerid-查询
- [√] 1.2 在 `networkplugin/Network/Client/NetworkManager.cs` 中实现 `UpdatePlayerInfo(object playerInfo)` 为兼容入口（复用 `TryGetJsonElement/UpdateSinglePlayer/UpdatePlayersFromArray`），验证 why.md#需求-完成-networkmanager-todo---场景-updateplayerinfo-接收玩家信息更新，依赖任务1.1
  > 备注: 原签名为 `dynamic`，但项目未引用 `Microsoft.CSharp` 时使用 dynamic 会触发编译错误 `CS0656`；改为 `object` 避免引入新依赖。

## 2. 验证
- [X] 2.1 执行 `dotnet build simplesolution.sln`，确保编译通过，依赖任务1.2
  > 备注: 当前仓库整体解法构建失败（与本次改动无直接关联，错误集中在其他项目/引用缺失）；已验证 `dotnet build networkplugin/NetWorkPlugin.csproj -c Release` 通过。

## 3. 安全检查
- [√] 3.1 执行安全检查（输入验证、异常处理、潜在 DoS：JSON 解析、线程安全），依赖任务1.2

## 4. 文档更新
- [√] 4.1 如本次变更影响对外理解，更新 `helloagents/wiki/modules/networkplugin.md` 中对玩家身份/玩家列表来源的说明，依赖任务1.2

## 5. 测试
- [?] 5.1 手动场景验证: 连接/断开/收到 PlayerListUpdate 时 `GetAllPlayers/GetPlayerCount/GetSelf` 行为一致，验证点: 不崩溃、玩家列表随事件变化、Self 不被 RemovePlayer 移除，依赖任务2.1
  > 备注: 需要在游戏内联机环境手动验证；本地仅完成 `networkplugin/NetWorkPlugin.csproj` 构建验证。

---

## 执行备注

> 执行过程中的重要记录

| 任务 | 状态 | 备注 |
|------|------|------|
