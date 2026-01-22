# BattleController Sync Proposal

## 元信息
- 方案包名称: 202601221037_battlecontroller_sync
- 创建日期: 2026-01-22
- 类型: implementation
- 相关文件:
  - networkplugin/Patch/BattleController_Patch.cs
  - lbol/LBoL.Core/Battle/BattleController.cs
  - networkplugin/Network/Client/INetworkClient.cs
  - networkplugin/Network/Messages/NetworkMessageTypes.cs

## 1. 需求

### 背景
当前 `networkplugin/Patch/BattleController_Patch.cs` 通过 Harmony 监听 `BattleController` 的若干方法，在本地战斗状态变化后向联机层发送 JSON 事件。

现状问题(基于代码事实):
- `BattleController.Damage` 的真实签名为 `internal DamageInfo Damage(Unit source, Unit target, DamageInfo info, GameEntity actionSource)`。
  - 现有补丁签名为 `Damage_Postfix(BattleController __instance, DamageInfo damageinfo, Unit target)`，参数不匹配，存在补丁无法生效或取到错误参数的风险。
- `BattleController.Heal` 的真实签名为 `internal int Heal(Unit target, int healValue)`。
  - 现有补丁签名为 `Heal_Postfix(BattleController __instance, Unit target)`，参数不匹配。
- `TryAddStatusEffect` / `RemoveStatusEffect` 的真实签名为:
  - `internal StatusEffectAddResult? TryAddStatusEffect(Unit target, StatusEffect effect)`
  - `internal bool RemoveStatusEffect(Unit target, StatusEffect effect)`
  - 现有补丁缺少 `StatusEffect effect` 参数，且会对 `target` 进行 Traverse 访问私有字段 `_statusEffects`，但未对 `target == null` 或 `statusEffects == null` 做保护。
- 事件命名未统一:
  - 仓库里已存在大量 `NetworkMessageTypes.*` 常量。
  - 本文件当前硬编码了 `OnPlayerDamage` / `UpdateAfterTryAddStatusEffects` / `UpdateAfterTryRemoveStatusEffects` / `UpdateHealthAfterHeal`。
  - 同时，其他补丁(例如 `EnemyIntentSyncPatch`)明确提到对战斗事件使用 `Battle*` 前缀以走 GameEvent 通道。

### 目标
1) 修正补丁方法签名，使其稳定命中 `BattleController` 的真实方法。
2) 明确“只同步本地玩家”的判定方式，并避免回环/重复广播。
3) 统一事件通道与命名(至少保证不会被 server 当作未知消息丢弃)，并在代码中用 TODO 标注待对齐协议处。
4) 完善 TODO 注释: 把"缺什么信息"、"为什么这么做"、"后续扩展点"写清楚。

### 约束条件
- 不改变现有网络底层: 使用 `INetworkClient.SendRequest<T>` / `SendGameEventData` 的既有能力。
- 不假设服务器支持新事件，必须在 proposal 中明确需要你确认/提供的协议。
- 尽量保持补丁不会影响单机流程: 网络不可用时应快速返回。

### 验收标准
- 编译通过。
- Harmony 补丁能正确命中 `BattleController.Damage/Heal/TryAddStatusEffect/RemoveStatusEffect` (通过签名对齐与最少日志验证)。
- 事件发送前具备一致的连接判定与最小空值保护。
- TODO 注释从"随手记"升级为"可执行的下一步"(每条 TODO 都能指向具体动作/文件/协议项)。

## 2. 方案

### 2.1 钩子点与数据源(以代码为准)
- `BattleController.Damage(Unit source, Unit target, DamageInfo info, GameEntity actionSource)`
  - 返回值为 `DamageInfo damageInfo = target.TakeDamage(info)`。
  - 适合在 Postfix 使用 `__result` 获取最终结算后的伤害信息，避免只看到输入 `info`。
- `BattleController.Heal(Unit target, int healValue)`
  - 返回值为 `int` (实际治疗量)，适合在 Postfix 使用 `__result`。
- `TryAddStatusEffect(Unit target, StatusEffect effect)` / `RemoveStatusEffect(Unit target, StatusEffect effect)`
  - 可在 Postfix 同步 "谁" + "增/减了什么" + "变更后状态列表"。

### 2.2 事件通道选择(待确认)
仓库存在两类发送方式:
- `SendGameEventData(eventType, object)` : 会写入 `eventType + json`，属于 GameEvent 通道。
- `SendRequest<T>(header, payload)` : 对复杂对象会写入 `header + json`，属于 Request 通道。

其他补丁中有注释:"Battle* 前缀走 GameEvent 通道"。
本方案建议:
- 对战斗同步优先使用 `SendGameEventData`，并将 eventType 统一成 `Battle*` 前缀，避免被 server 误判/丢弃。
- 如果你确认服务器端只接收 `SendRequest` 的 header 形式，则反过来统一使用 `SendRequest`。

该点需要你确认服务器的路由规则(见"需要你提供的信息")。

### 2.3 本地玩家判定
现文件中用 `battle.Player != playerTarget` 来判定本地玩家。
- 该判定在 LBoL 里通常成立(战斗控制器持有本地 `PlayerUnit Player`)。
- 仍需结合联机模型确认: 你们是否存在"观战"/"远端回放"会在本机创建非本地 PlayerUnit 的情况。

### 2.4 去回环(循环同步)策略
参考 `EnemyIntentSyncPatch.PauseIntentSync` 的设计，本方案建议为玩家同步也提供开关:
- 例如 `public static bool PausePlayerSync { get; set; }`
- 当应用远端同步时(例如重放远端伤害/状态), 临时置 true，避免又触发本地补丁反发。

### 2.5 Payload 结构建议
不强推固定 schema，但建议原则:
- 每个事件都带 `Timestamp`。
- 统一包含 `PlayerId` / `TargetId` / `BattleRound` 等定位字段(如果可获取)。
- 对数值字段尽量发送数值而不是 `ToString()`，减少解析歧义。

### 2.6 风险评估
- Harmony 参数对齐风险(高): 必须按真实签名修正，否则所有逻辑都可能不执行。
- 事件被丢弃风险(中): 需要对齐 server 路由(推荐你提供 server/router 代码或协议文档)。
- 负载大小/频率风险(中): 状态列表每次全量发送可能过大，后续可改为增量(本次先保证正确)。

## 3. 需要你提供的信息(请回复其中关键项即可)
1) 服务器端如何区分 GameEvent vs Request? 对 `Battle*` 前缀是否有特殊路由?
2) 你期望这几个事件的最终名字是什么? 是否希望加入 `NetworkMessageTypes` 常量统一管理?
3) 服务端/客户端对 PlayerId 的定义是什么(使用 `INetworkClient.GetSelf()` 里的哪个字段)?
4) 是否需要同步给"房间内所有人"，还是仅主机权威广播(客户端只上报意图/请求)?
5) 状态效果同步你想要:
   - A: 全量列表(现在做法)
   - B: 增量(只发新增/移除的 effect)
   - C: 两者都要(增量为主，定期全量校验)

## 4. 已确认决策(已落地到代码)

> 说明: 本节以“当前仓库代码 + 最近一次构建验证”为准。

- 事件通道: 优先使用 `INetworkClient.SendGameEventData`。
- 事件命名: 统一集中到 `networkplugin/Network/Messages/NetworkMessageTypes.cs`，并新增 Battle* 常量。
- 权威模型: 主机广播 / 客户端上报 (Report/Broadcast 分离)。
- 状态效果同步: C - 增量为主 + 周期性全量校验。
- 身份定义:
  - `PlayerName`: 使用 LBoL 档案名(如 `Singleton<GameMaster>.Instance.CurrentProfile.Name`)。
  - `PlayerIp`: 仅使用本机局域网 IPv4(无 ServerIP 兜底；无可用 IP 时回退 `0.0.0.0`)。
  - `PlayerId`: `${PlayerName}@${PlayerIp}`，例如 `Alice@192.168.1.23`。

## 5. 当前状态与验证

- 方案包结构校验: `helloagents/plan/202601221037_battlecontroller_sync` 已通过 validate_package 校验(valid=true, executable=true)。
- 构建验证: `networkplugin/NetWorkPlugin.csproj` Release 编译通过(存在 warning，但无 error)。

## 6. 后续可选增强(未在本方案包内继续推进)

- 接收端闭环: 若需要完整实现“客户端上报 -> 主机接收 -> 主机再广播”的链路，可在 Host 侧增加对 Battle*Report 的处理与转发，并使用 `PausePlayerBattleSync` 避免回环。
