# MIGRATED

本方案包在 HelloAGENTS 流程中已执行并归档，统一以知识库为准：
`helloagents/history/2026-01/202601211700_tradepanel_trade_sync_v1/how.md`
- `PlayerAId` / `PlayerBId`: 参与者网络 PlayerId。
- `OfferedCardsA` / `OfferedCardsB`: 数组，每项包含：
  - `CardId`（配置Id）
  - `InstanceId`（实例Id）
  - （可选）`Upgraded` / `UpgradeCounter`（若仅靠 InstanceId 不稳定，则补充）
- `MaxTradeSlots`
- `State`: `Open|Confirming|Completed|Canceled|Failed`
- `Reason`（失败原因）
- `Timestamp`

去重/顺序：
- v1 先以 `Timestamp` + `RequestId` 做幂等处理。
- Host 端维护 `TradeId -> TradeSessionState` 内存态，必要时超时清理。

## 4. 卡组落地策略（解决 TODO#1）
关键问题：LBoL 原生只有一个 `GameRunController` 的 BaseDeck；联机 MOD 里多玩家可能通过“远程玩家镜像/虚拟牌库”实现。

v1 的落地策略分两种可选路径（需要你确认哪一种符合当前联机架构）：

A) 多玩家“各自卡组”存在（推荐）
- 每个玩家有独立的 deck 数据源（可能不在 `GameRunController.BaseDeck`）。
- 交易落地：
  - 从 `PlayerA` 的 deck remove 指定 `InstanceId`
  - 往 `PlayerB` 的 deck add 对应 card（可能是复制/迁移）
- 需要明确：远程玩家 deck 的存储与 API（例如某个 `PlayerEntity` / `RemoteNetworkPlayer` / `CardLibrary`）。

B) 单卡组共享（保守兜底）
- 所有玩家共享 `GameRunController.BaseDeck`。
- 交易本质只是 UI 玩法，不改变真实所有权；只做“双方一致确认”后执行一次本地 deck 变动（等价于单人选择卡牌交换）。

无论 A/B，网络协议层一致；差异只在“ApplyTradeResult()”的落地实现。

## 5. UI 与补丁完善（你提到的“完善补丁方法”）
现有 `GetOrCreateTradePanel()` 直接 AddComponent 的方式不可用（序列化字段为空）。
建议的实施顺序：
1) 优先从 `UiManager` 获取现有面板实例（如果游戏/Mod 已注册 prefab）。
2) 如果没有 prefab：实现一个最小 runtime builder（复用 `ResurrectPanel` 现有做法/模板）：
   - 复用游戏 UI 的 `CommonButtonWidget`、`TextMeshProUGUI`，并创建最小 slot 列表。
   - 或者临时降级：交易只走“无 UI”的流程（命令/按钮），直到 prefab 完成。

## 6. 订阅模型（解决 TODO#2 的发送与接收）
参考 `ResurrectSyncPatch`：
- 新增 `TradeSyncPatch`（放在 `networkplugin/Patch/Network/`）
- `EnsureSubscribed(INetworkClient)`：挂 `client.OnGameEventReceived += ...`
- 在 UI 打开交易面板时确保订阅（类似 `ResurrectPanel` 在 OnShown 里 EnsureSubscribed）。
- 发送事件使用 `INetworkClient.SendGameEventData(eventType, payload)`。

## 7. 失败与边界情况
- 任意一方取消：Host 广播 `OnTradeCancel`，双方关闭面板并解锁交互。
- 超时：Host 超时清理会话并广播 cancel。
- 断线重连：
  - `ISynchronizationManager.OnConnectionRestored()` 或相关 hook 触发时，客户端向 Host 请求 `OnTradeSnapshot`。
  - Host 回发当前 Trade 会话快照。

## 8. 需要你确认的信息（实施前必须回答）
1) 当前联机是否是“多玩家各自卡组”(A) 还是“共享卡组”(B)？
2) Trade v1 是否只交换“卡牌”，还是要包含“金币/展品/道具”？
3) 交易的参与者如何选择？（当前 UI 有 Player1/Player2 名称字段，但没有选择逻辑）
