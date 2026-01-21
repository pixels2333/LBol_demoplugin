# 复活面板同步 v1 - How

## 总体链路
- 数据源：`DeathRegistry` 汇总 `OnPlayerDeathStatusChanged`，供 Gap UI 展示。
- UI 发起：`ResurrectPanel` 选择目标（用 PlayerId），发送 `OnResurrectRequest`。
- Host 仲裁：`ResurrectSyncPatch` 仅 Host 处理请求，校验目标仍在 `DeathRegistry`，计算 `ResurrectionHp = clamp(Cost/2, 1, TargetMaxHp)`，广播 `OnPlayerResurrected`；失败广播 `OnResurrectFailed`。
- 客户端落地：所有人接收广播，但只有 `TargetPlayerId == selfId` 的客户端执行 `DeathPatches.ResurrectPlayer(..., hp)`。
- 扣费：发起者（`RequesterPlayerId == selfId`）在成功回调后 `ConsumeMoney(cost)`；失败提示即等价退款。

## 关键实现点
- PlayerId：服务端分配并在 Welcome/PlayerListUpdate 中下发，客户端通过 `NetworkIdentityTracker` 获取 selfId。
- 防回环：目标端从广播落地复活时将 `DeathPatches.SuppressNetworkSync = true`，避免复活再触发网络同步。
- Host 不会收到自身广播：`DeathPatches` 在发送死亡/复活同步时同时更新本地 `DeathRegistry`，确保 Host 也能看到“死者列表”。

## 复用点（避免冗余）
- `INetworkClient.SendGameEventData(...)` / `OnGameEventReceived`。
- `NetworkIdentityTracker` 维护 PlayerId/Host 标志。
- 既有 `DeathPatches` 的复活落地入口。
