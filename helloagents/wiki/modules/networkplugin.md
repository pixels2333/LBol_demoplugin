# 模块: networkplugin

## 目的
提供联机同步与 UI 扩展补丁。

## 规范
### 远端队友作为出牌目标（单体目标）
- 通过 `RemotePlayerProxyEnemy : EnemyUnit` 兼容 `UnitSelector.SelectedEnemy` 的类型约束。
- 发送端在 `Card.GetActions` 处拦截：不本地结算，改为发送 `OnRemoteCardUse`。
- 目标端接收后按蓝图结算，再发送 `OnRemoteCardResolved` 广播结算后的状态快照。
- 接收端对 `OnRemoteCardResolved` 做 `ResolveSeq/Timestamp/RequestId` 去重与乱序丢弃，避免状态回滚。

### 玩家身份与玩家列表来源
- 客户端侧“玩家列表/数量”由 `NetworkManager` 维护（轻量缓存），不承担完整权威同步。
- 服务器侧分配的 `PlayerId` / Host 信息由 `NetworkIdentityTracker` 从 GameEvent 提取并缓存。
- `NetworkManager` 在收到 `Welcome/PlayerListUpdate/PlayerJoined/PlayerLeft` 等事件后同步缓存，并可通过 `GetAllPlayers/GetPlayerCount/GetPlayer` 查询。
