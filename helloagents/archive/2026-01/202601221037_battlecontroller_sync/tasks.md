# Tasks - BattleController Sync

> **@status:** completed | 2026-01-22 12:03

## 执行状态
- 总任务数: 8
- 完成: 8
- 状态: 100% (8/8)

## 任务列表
- [√] 1. 盘点现有网络事件约定
  - 读取并归纳 `networkplugin/Network/Client/NetworkClient.cs` 的 `SendGameEventData` / `SendRequest` 行为差异
  - 搜索 server/handler 侧对 eventType/header 的路由规则(若 server 不在仓库, 在 proposal 中形成待确认问题)

- [√] 2. 修正 BattleController 补丁签名
  - 更新 `networkplugin/Patch/BattleController_Patch.cs`:
    - Damage: 对齐 `Damage(Unit source, Unit target, DamageInfo info, GameEntity actionSource)`，使用 `__result` 获取最终结算
    - Heal: 对齐 `Heal(Unit target, int healValue)`，使用 `__result` 获取实际治疗
    - TryAddStatusEffect/RemoveStatusEffect: 增加 `StatusEffect effect` 参数，必要时记录 effect 的 DebugName/类型

- [√] 3. 统一连接判定与 ServiceProvider 获取
  - 统一使用 `ModService.ServiceProvider`(避免静态缓存过早初始化)
  - 统一 `client == null || !client.IsConnected` 快速返回

- [√] 4. 本地玩家过滤与去回环开关
  - 在补丁里增加 `PausePlayerSync` 或等价机制
  - 对玩家相关同步限定为 `__instance.Player == targetPlayer`

- [√] 5. 状态效果同步健壮性
  - `target == null` 时直接返回
  - Traverse 访问 `_statusEffects` 失败时不抛异常
  - `statusEffects == null` 时发送空列表或跳过(按你确认的策略)

- [√] 6. 事件命名与通道对齐(先做可配置/可替换)
  - 在代码里集中定义事件名常量/私有 const，避免散落字符串
  - 根据你确认结果选择 `SendGameEventData` vs `SendRequest`

- [√] 7. 补丁 TODO 注释完善
  - 将当前 TODO 改写为"缺什么/去哪里改/验收是什么"
  - 增加对 payload schema 的 TODO(字段名、类型)

- [√] 8. 编译与最小验证
  - 运行现有 `mvn -B test`(如适用) 或检查 .NET build 流程(项目可能是 Unity/插件, 以现有解决方案为准)
  - 使用 `get_errors` 确认 `BattleController_Patch.cs` 无编译错误

## 执行备注
- 该方案包当前以"先修正签名与通道"为第一优先级。
- 一旦你提供 server 路由规则/协议, 我会在 develop 阶段把事件名与 payload schema 完整落地。

## 执行记录
- 2026-01-22: 已完成补丁实现与验证(以仓库代码与构建结果为准)。
  - 已将 `BattleController_Patch` 对齐 LBoL 源码真实签名，并使用 Postfix `__result`。
  - 已统一改为通过 `INetworkClient.SendGameEventData` 发送 Battle* 事件，并集中到 `NetworkMessageTypes` 常量。
  - 已按“主机广播 / 客户端上报”拆分 Report/Broadcast 事件。
  - 状态效果同步已升级为“增量为主 + 周期全量校验”，并移除 Traverse 私有字段依赖。
  - 身份策略已调整为：`PlayerName` 取档案名，`PlayerId = {PlayerName}@{LAN_IP}`；IP 仅使用本机局域网 IPv4，无 ServerIP 兜底。
  - 构建验证：`networkplugin/NetWorkPlugin.csproj` Release 编译通过(有 warning，但无 error)。
