# Multiplayer Regression Checklist

## 目标

为 `networkplugin` 当前主链路建立一套可重复执行的多人回归基线，覆盖房间生命周期、战斗一致性、功能链路与连接恢复，并把 `GapOptionsSyncPatch` / `RemoteCardUsePatch` 这两条高风险链路挂入固定检查面。

## 与其他清单的关系

- 路由层定向转发与异常路径：见 `networkplugin/NETWORK_ROUTE_REGRESSION_CHECKLIST.md`
- 本文聚焦“功能是否落地到状态 / UI / 玩家体验”

## 回归前准备

1. 预备至少一组 Host + Client。
2. 涉及 Relay 时，增加一组 Relay + Host + Client。
3. 打开以下日志关键字：
  - `[GapOptionsSync]`
   - `[RouteProbe]`
   - `[RemoteCardUse]`
   - `[Reconnect]`
4. 默认从空房间开始，避免旧状态残留影响观察。

## 1. 房间生命周期

| 用例 | 步骤 | 预期结果 |
|------|------|----------|
| 建房 | Host 创建房间并进入初始节点 | Host 成为房主；玩家列表只含自己；欢迎消息返回有效 `PlayerId` |
| 入房 | Client 加入房间 | 双端都刷新玩家列表；房主标记稳定；房间状态不串房 |
| 离房 | Client 主动离开或断开 | 另一端玩家列表移除该玩家；房主不误切换 |
| 主机切换 | 旧 Host 断开，仅保留其他在线玩家 | 新 Host 被广播；玩家列表中的 `IsHost` 唯一且一致 |

## 2. 战斗一致性

### 2.1 回合 / 能量 / 敌人状态

| 用例 | 步骤 | 预期结果 |
|------|------|----------|
| 回合结束 | 一端结束回合 | 另一端收到回合推进；不出现按钮卡死或战斗软锁 |
| 能量变化 | 使用消耗法力的卡 | 双端能量值一致；不会重复扣费 |
| 敌人状态 | 对敌造成伤害 / 施加状态 | 双端敌人 HP / Block / Intent 同步；无重复广播 |

### 2.2 远端出牌（`RemoteCardUsePatch`）

| 用例 | 步骤 | 预期结果 |
|------|------|----------|
| 正常对队友出牌 | A 选择 `RemotePlayerProxyEnemy` 作为目标并出牌 | 本地只播放最小动画；目标端收到并落地表现；无本地双重结算 |
| 未连接回退 | 断开网络后再次对队友出牌 | 弹出“未连接，无法对队友出牌”；卡牌/法力/金钱走 `FailFallbackActions` 回退 |
| Resolved 去重 | 同一 `RequestId`/旧 `ResolveSeq` 重放 | 旧消息被忽略，不发生状态回滚 |
| ReversePatch 风险 | 覆盖一次正常出牌与一次异常/取消路径 | 不出现 `Harmony reverse patch stub` 异常；若出现，阻断发布并回查 `Card_GetActions_Original` 路径 |

## 3. 功能链路

### 3.1 交易 / 复活 / 地图推进

| 用例 | 步骤 | 预期结果 |
|------|------|----------|
| 交易 | 双方报价、确认、完成 | 双端资产一致；失败路径可回退到 `Open` |
| 复活 | 在 Gap 中发起复活 | 成功后目标端落地复活；发起端扣费；失败时给出提示 |
| 地图推进 | 进入节点、战斗后结算、下一阶段 | `RoomState` / checkpoint 能正确跟进；地图状态不回退 |

### 3.2 GapOptions（`GapOptionsSyncPatch`）

| 用例 | 步骤 | 预期结果 |
|------|------|----------|
| 升级 / 删卡 / 入 Gap / 喝茶 | 双端各触发一次 | 同一 `ActionId` 不重复处理；日志能看到一次有效收包 |
| 重复收包 | 人为重放同一 `ActionId` | `ProcessedActionIds` 去重生效，不重复表现 |
| 房间缓存 | 同房间连续触发多次 GapOptions 事件 | `GetRecentGapOptionsEvents` 可返回最近事件，旧事件按上限淘汰 |
| 表现落地检查 | 触发一次 GapOptions 事件后观察 UI / 游戏结果 | 不允许只记日志不落状态；若出现“仅缓存未表现”，记录为阻断问题 |

## 4. 连接恢复

| 用例 | 步骤 | 预期结果 |
|------|------|----------|
| 断线重连 | Client 断线后在窗口期内重连 | `Reconnect_REQUEST/RESPONSE` 成功；玩家重新绑定原 `PlayerId` |
| 中途加入 | 新玩家在房间进行中加入 | Joiner 收到 `FullSnapshot` 并完成基本 catch-up |
| FullSnapshot 追赶 | Joiner 或重连端拉取快照 | 房间状态、地图进度、战斗关键状态恢复到房主权威结果 |
| GapOptions 追赶 | Joiner / 重连后读取房间缓存 GapOptions 事件 | `MergeCatchupGapOptionsEvents` 只缓存并去重，不重复执行旧动作 |
| RoomState 追赶 | 进入节点后主动请求 `RoomState` | 请求 / 响应路径与状态落地一致，不串发给非目标客户端 |

## 风险备注

- `RemoteCardUsePatch.Card_GetActions_Original(...)` 是 Harmony reverse patch 白名单桩，允许方法体保留 `throw new NotImplementedException(...)`；真正的发布风险不是“存在这个桩”，而是回归时是否错误走到了桩体。
- `GapOptionsSyncPatch.MergeCatchupGapOptionsEvents(...)` 当前设计是“只缓存，不重放动作”；因此回归必须显式区分“状态缓存成功”和“UI / 游戏表现正确落地”这两个层级。

## 本轮基线

- 已完成首轮代码收敛：`NetworkServer` 统一受控路由、`ChatConsole` 名称入口统一、聊天旧 payload 显示回退补齐。
- 已形成两份固定清单：
  - `networkplugin/NETWORK_ROUTE_REGRESSION_CHECKLIST.md`
  - `networkplugin/MULTIPLAYER_REGRESSION_CHECKLIST.md`
- 构建验证：`dotnet build networkplugin/NetWorkPlugin.csproj -v minimal` 成功（257 warnings / 0 errors）。