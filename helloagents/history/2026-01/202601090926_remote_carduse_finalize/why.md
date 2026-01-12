# 变更提案: 远端目标出牌链路收尾

## 需求背景
当前已实现“远端队友作为代理目标出牌”，并采用“目标端结算 + OnRemoteCardResolved 广播状态快照”的方式同步结果。

仍需补齐三个可靠性/一致性点：
1. **失败回退**：未连接/发送失败时需要阻止或回退，避免资源被消耗或卡牌异常流转。
2. **Resolved 防回滚**：OnRemoteCardResolved 乱序到达会导致远端状态回滚，需要丢弃旧包。
3. **动画一致性**：所有客户端都应看到施法者（包括本地玩家在他人视角）播放施法动作。

## 变更内容
1. 未连接时在 `BattleController.RequestUseCard` 入口直接阻止对远端队友出牌并提示。
2. 发送失败时在 `Card.GetActions` 拦截内 best-effort 回退（提示、尝试退还法力、尝试把卡移回手牌/弃牌堆）。
3. `OnRemoteCardResolved` 增加 `ResolveSeq`，接收端按 `ResolveSeq` 优先、`Timestamp` 兜底，并结合 `RequestId` 去重，丢弃旧包防止状态回滚。
4. `OnRemoteCardUse` 接收端播放施法者动画；发送端补目标预动画（因服务端不会回显给发送者）。

## 影响范围
- `networkplugin/Patch/Network/RemoteCardUsePatch.cs`

## 风险评估
- 回退是 best-effort：不同出牌路径（Play/Use）在不同阶段移动卡牌，回退无法保证 100% 复原；但在 RequestUseCard 入口阻止未连接场景可覆盖主要路径。

