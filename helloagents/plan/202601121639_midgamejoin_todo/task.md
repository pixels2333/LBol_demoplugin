# 任务清单: MidGameJoinManager TODO 落地

目录: `helloagents/plan/202601121639_midgamejoin_todo/`

---

## 1. MidGameJoin 核心闭环
- [ ] 1.1 在 `networkplugin/Plugin.cs` 注册并初始化 `MidGameJoinManager`（能启动、不报错），验证 why.md#需求-初始化订阅与状态维护
- [ ] 1.2 在 `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs` 实现 `Initialize()`：订阅必要事件 + 打点日志（可重复调用），验证 why.md#需求-初始化订阅与状态维护
- [ ] 1.3 在 `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs` 用 Relay `DirectMessage` 跑通请求/响应收发（先跑通消息链路），验证 why.md#需求-中途加入请求与审批

## 2. 请求/批准/执行加入流程
- [ ] 2.1 在 `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs` 落地 `RequestJoin()`：校验配置/限流/房间开局状态，生成 RequestId 并发给 Host，验证 why.md#需求-中途加入请求与审批
- [ ] 2.2 在 `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs` 落地 `ApproveJoin()`：Host-only 校验 + 超时清理 + 生成 `BootstrappedState` 并回包 Joiner，验证 why.md#需求-中途加入请求与审批
- [ ] 2.3 在 `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs` 落地 `ExecuteJoin()`：能消费 JoinToken 并完成“快照落地 + 进入可操作状态”，验证 why.md#需求-执行加入与完整同步

## 3. FullSync 与追赶回放
- [ ] 3.1 在 `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs` 实现 `RequestFullStateSync()`：用 `DirectMessage` 向 Host 请求 FullSnapshot 并接收响应，验证 why.md#需求-执行加入与完整同步
- [ ] 3.2 优先复用 `ReconnectionManager` 生成 `FullStateSnapshot` 与 `MissedEvents`（不强求抽象/接口化；能复用即可），验证 why.md#需求-追赶事件回放
- [ ] 3.3 在 `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs` 实现最小回放：能按顺序尝试回放 `MissedEvents`；失败允许降级并打日志，验证 why.md#需求-追赶事件回放

## 4. 进度与补偿
- [ ] 4.1 在 `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs` 实现 `CalculateGameProgress()`（基于 `FullStateSnapshot.GameState`），验证 why.md#需求-进度计算与补偿生成
- [ ] 4.2 在 `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs` 实现 `GenerateStartingCards/GenerateStartingExhibits/GenerateStartingPotions` 并接入 `ApproveJoin()`（受 `EnableCompensation` 控制），验证 why.md#需求-进度计算与补偿生成

## 5. 安全检查（按模板要求，Mod 标准做“最低限度”）
- [ ] 5.1 最低限度检查：空值校验、超时与清理、Host-only 校验、避免 FullSync 控制消息进入回放队列

## 6. 文档更新
- [ ] 6.1 更新 `helloagents/wiki/modules/networkplugin.md`：简单补充 MidGameJoin 的消息类型与基本流程（够用即可）

## 7. 测试
- [ ] 7.1 补充烟测步骤到 `helloagents/plan/202601121639_midgamejoin_todo/how.md`（覆盖批准失败/回放失败降级）
