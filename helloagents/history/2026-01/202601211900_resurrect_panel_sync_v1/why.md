# 复活面板同步 v1 - Why

## 背景
`networkplugin/UI/Panels/ResurrectPanel.cs` 最初存在“发送复活网络事件”的 TODO，导致复活只能在本地 UI 层完成，无法让联机房间内其他客户端一致。
同时项目已有“假死/复活”补丁（`DeathPatches`/`DeathStateSyncPatch`）与网络身份追踪（`NetworkIdentityTracker`），适合复用而不是重新设计。

## 目标
1. 完成 Gap 场景的“复活他人”联机闭环：发起者请求 -> Host 广播结果 -> 目标本人落地复活。
2. 使用服务端分配的 PlayerId 作为唯一标识（`INetworkPlayer.playerId` / `NetworkIdentityTracker.GetSelfPlayerId()`）。
3. 费用规则：复活后目标 HP = Cost/2；扣费归属为发起者；失败提示（等价退款/延迟扣费）。
4. 避免冗余：复用 `#file:Network` 的 GameEvent 通道与订阅机制。

## 非目标
- v1 不做完整的权威经济系统对账（仅在成功广播后由发起者扣费）。
- v1 不扩展到战斗中复活，仅限 Gap 面板。

## 成功标准
- Gap 复活请求可被 Host 处理并广播；目标玩家客户端可落地复活；发起者成功后扣费、失败提示。
- 不产生回环广播（从网络落地复活会抑制再次同步）。
