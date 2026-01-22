namespace NetworkPlugin.Network.Messages
{
    /// <summary>
    /// LiteNetLib网络消息类型定义
    /// 基于杀戮尖塔联机Mod的消息类型，适配LBoL游戏机制
    /// </summary>
    public static class NetworkMessageTypes
    {
        // === 系统消息 ===

        /// <summary>
        /// 玩家加入游戏
        /// </summary>
        public const string PlayerJoined = "PlayerJoined";

        /// <summary>
        /// 玩家离开游戏
        /// </summary>
        public const string PlayerLeft = "PlayerLeft";

        /// <summary>
        /// 玩家列表更新
        /// </summary>
        public const string PlayerListUpdate = "PlayerListUpdate";

        /// <summary>
        /// 欢迎消息（发送给新连接的玩家）
        /// </summary>
        public const string Welcome = "Welcome";

        /// <summary>
        /// 心跳请求
        /// </summary>
        public const string Heartbeat = "Heartbeat";

        /// <summary>
        /// 心跳响应
        /// </summary>
        public const string HeartbeatResponse = "HeartbeatResponse";

        /// <summary>
        /// 房主变更
        /// </summary>
        public const string HostChanged = "HostChanged";

        /// <summary>
        /// 获取自身信息请求
        /// </summary>
        public const string GetSelf_REQUEST = "GetSelf_REQUEST";

        /// <summary>
        /// 获取自身信息响应
        /// </summary>
        public const string GetSelf_RESPONSE = "GetSelf_RESPONSE";

        // === 卡牌同步消息 ===

        /// <summary>
        /// 卡牌开始使用
        /// </summary>
        public const string OnCardPlayStart = "OnCardPlayStart";

        /// <summary>
        /// 卡牌使用完成
        /// </summary>
        public const string OnCardPlayComplete = "OnCardPlayComplete";

        /// <summary>
        /// 抽牌事件
        /// </summary>
        public const string OnCardDraw = "OnCardDraw";

        /// <summary>
        /// 弃牌事件
        /// </summary>
        public const string OnCardDiscard = "OnCardDiscard";

        /// <summary>
        /// 卡牌放逐事件
        /// </summary>
        public const string OnCardExile = "OnCardExile";

        /// <summary>
        /// 卡牌升级事件
        /// </summary>
        public const string OnCardUpgrade = "OnCardUpgrade";

        /// <summary>
        /// 卡牌移除事件
        /// </summary>
        public const string OnCardRemove = "OnCardRemove";

        public const string OnRemoteCardUse = "OnRemoteCardUse";
        public const string OnRemoteCardResolved = "OnRemoteCardResolved";

        // === 法力/能量同步消息 ===

        /// <summary>
        /// 法力消耗开始
        /// </summary>
        public const string ManaConsumeStarted = "ManaConsumeStarted";

        /// <summary>
        /// 法力消耗完成
        /// </summary>
        public const string ManaConsumeCompleted = "ManaConsumeCompleted";

        /// <summary>
        /// 法力回复事件
        /// </summary>
        public const string ManaRegain = "ManaRegain";

        /// <summary>
        /// 回合法力重置
        /// </summary>
        public const string TurnManaCalculated = "TurnManaCalculated";

        /// <summary>
        /// 最大法力变更
        /// </summary>
        public const string MaxManaChange = "MaxManaChange";

        // === 战斗同步消息 ===

        /// <summary>
        /// 伤害事件
        /// </summary>
        public const string OnDamageDealt = "OnDamageDealt";

        /// <summary>
        /// 受到伤害事件
        /// </summary>
        public const string OnDamageReceived = "OnDamageReceived";

        /// <summary>
        /// 获得护盾事件
        /// </summary>
        public const string OnBlockGained = "OnBlockGained";

        /// <summary>
        /// 获得屏障事件
        /// </summary>
        public const string OnShieldGained = "OnShieldGained";

        /// <summary>
        /// 治疗事件
        /// </summary>
        public const string OnHealingReceived = "OnHealingReceived";

        /// <summary>
        /// 状态效果应用
        /// </summary>
        public const string OnStatusEffectApplied = "OnStatusEffectApplied";

        /// <summary>
        /// 状态效果移除
        /// </summary>
        public const string OnStatusEffectRemoved = "OnStatusEffectRemoved";

        public const string OnMoodEffectLoopStarted = "OnMoodEffectLoopStarted";
        public const string OnMoodEffectLoopEnded = "OnMoodEffectLoopEnded";
        public const string OnMoodEffectStateSync = "OnMoodEffectStateSync";

        // === Battle* (Host 权威广播 / Client 上报) ===

        /// <summary>
        /// 客户端上报：本地玩家受到伤害（发送给 Host/Server 以便仲裁/转发）。
        /// </summary>
        public const string BattlePlayerDamageReport = "BattlePlayerDamageReport";

        /// <summary>
        /// 主机广播：玩家受到伤害（权威端广播给房间内其他客户端）。
        /// </summary>
        public const string BattlePlayerDamageBroadcast = "BattlePlayerDamageBroadcast";

        /// <summary>
        /// 客户端上报：本地玩家治疗事件。
        /// </summary>
        public const string BattlePlayerHealReport = "BattlePlayerHealReport";

        /// <summary>
        /// 主机广播：玩家治疗事件。
        /// </summary>
        public const string BattlePlayerHealBroadcast = "BattlePlayerHealBroadcast";

        /// <summary>
        /// 客户端上报：玩家状态效果变化(增量)。
        /// </summary>
        public const string BattlePlayerStatusEffectsDeltaReport = "BattlePlayerStatusEffectsDeltaReport";

        /// <summary>
        /// 主机广播：玩家状态效果变化(增量)。
        /// </summary>
        public const string BattlePlayerStatusEffectsDeltaBroadcast = "BattlePlayerStatusEffectsDeltaBroadcast";

        /// <summary>
        /// 客户端上报：玩家状态效果全量快照(用于校验/追赶)。
        /// </summary>
        public const string BattlePlayerStatusEffectsFullReport = "BattlePlayerStatusEffectsFullReport";

        /// <summary>
        /// 主机广播：玩家状态效果全量快照(用于校验/追赶)。
        /// </summary>
        public const string BattlePlayerStatusEffectsFullBroadcast = "BattlePlayerStatusEffectsFullBroadcast";

        // === 回合同步消息 ===

        /// <summary>
        /// 回合开始
        /// </summary>
        public const string OnTurnStart = "OnTurnStart";

        /// <summary>
        /// 回合结束
        /// </summary>
        public const string OnTurnEnd = "OnTurnEnd";

        public const string EndTurnRequest = "EndTurnRequest";
        public const string EndTurnStatus = "EndTurnStatus";
        public const string EndTurnConfirm = "EndTurnConfirm";

        /// <summary>
        /// 战斗开始
        /// </summary>
        public const string OnBattleStart = "OnBattleStart";

        /// <summary>
        /// 战斗结束
        /// </summary>
        public const string OnBattleEnd = "OnBattleEnd";

        public const string EnemySpawned = "EnemySpawned";

        // === 地图/节点同步消息 ===

        /// <summary>
        /// 进入地图节点
        /// </summary>
        public const string OnMapNodeEnter = "OnMapNodeEnter";

        /// <summary>
        /// 完成地图节点
        /// </summary>
        public const string OnMapNodeComplete = "OnMapNodeComplete";

        /// <summary>
        /// 地图节点右键标记切换（多人共享标记）
        /// </summary>
        public const string OnMapNodeMarkChanged = "OnMapNodeMarkChanged";

        // === 事件/对话同步消息 ===

        /// <summary>
        /// 事件开始（进入 AdventureStation 或启动事件对话）。
        /// </summary>
        public const string OnEventStart = "OnEventStart";

        /// <summary>
        /// 事件选项确认（权威端广播）。
        /// </summary>
        public const string OnEventSelection = "OnEventSelection";

        /// <summary>
        /// 事件结果同步（可选）。
        /// </summary>
        public const string OnEventResult = "OnEventResult";

        /// <summary>
        /// 对话文本同步（主要用于日志/诊断）。
        /// </summary>
        public const string OnDialogText = "OnDialogText";

        /// <summary>
        /// 对话选项列表同步（用于断线重连/诊断，或跟随推进）。
        /// </summary>
        public const string OnDialogOptions = "OnDialogOptions";

        /// <summary>
        /// 事件投票：投票提交。
        /// </summary>
        public const string OnEventVoteCast = "OnEventVoteCast";

        /// <summary>
        /// 事件投票：结算结果。
        /// </summary>
        public const string OnEventVotingResult = "OnEventVotingResult";

        /// <summary>
        /// Boss 奖励选择同步。
        /// </summary>
        public const string OnBossRewardSelection = "OnBossRewardSelection";

        /// <summary>
        /// 商店事件同步。
        /// </summary>
        public const string OnShopEvent = "OnShopEvent";

        /// <summary>
        /// 宝箱/宝藏事件同步。
        /// </summary>
        public const string OnTreasureEvent = "OnTreasureEvent";

        // === 交易同步消息 ===

        /// <summary>
        /// 交易：发起交易请求（客户端->Host）。
        /// </summary>
        public const string OnTradeStartRequest = "OnTradeStartRequest";

        /// <summary>
        /// 交易：报价变更请求（客户端->Host）。
        /// </summary>
        public const string OnTradeOfferUpdateRequest = "OnTradeOfferUpdateRequest";

        /// <summary>
        /// 交易：确认交易请求（客户端->Host）。
        /// </summary>
        public const string OnTradeConfirmRequest = "OnTradeConfirmRequest";

        /// <summary>
        /// 交易：取消交易请求（客户端->Host）。
        /// </summary>
        public const string OnTradeCancelRequest = "OnTradeCancelRequest";

        /// <summary>
        /// 交易：状态更新（Host->所有参与者广播）。
        /// </summary>
        public const string OnTradeStateUpdate = "OnTradeStateUpdate";

        /// <summary>
        /// 交易：请求会话快照（客户端->Host，用于重连/中途加入）。
        /// </summary>
        public const string OnTradeSnapshotRequest = "OnTradeSnapshotRequest";

        /// <summary>
        /// 交易：提交前校验结果（客户端->Host，用于失败回退到 Open 并允许重试）。
        /// </summary>
        public const string OnTradePrepareResultRequest = "OnTradePrepareResultRequest";

        // === 死亡/复活（Gap）同步消息 ===

        /// <summary>
        /// 玩家死亡状态变化（假死/真死/HP 等）同步。
        /// </summary>
        public const string OnPlayerDeathStatusChanged = "OnPlayerDeathStatusChanged";

        /// <summary>
        /// 复活请求：客户端 -> Host。
        /// </summary>
        public const string OnResurrectRequest = "OnResurrectRequest";

        /// <summary>
        /// 复活失败：Host -> 全体（携带 RequesterPlayerId，用于发起者提示/退款）。
        /// </summary>
        public const string OnResurrectFailed = "OnResurrectFailed";

        /// <summary>
        /// 复活结果：Host -> 全体。
        /// </summary>
        public const string OnPlayerResurrected = "OnPlayerResurrected";

        /// <summary>
        /// 进入休息点（GapStation）
        /// </summary>
        public const string GapStationEntered = "GapStationEntered";

        /// <summary>
        /// 开始喝茶
        /// </summary>
        public const string DrinkTeaStarted = "DrinkTeaStarted";

        /// <summary>
        /// 喝茶完成
        /// </summary>
        public const string DrinkTeaCompleted = "DrinkTeaCompleted";

        /// <summary>
        /// 卡牌升级选择
        /// </summary>
        public const string CampfireUpgradeSelected = "CampfireUpgradeSelected";

        /// <summary>
        /// 卡牌移除选择
        /// </summary>
        public const string CampfireRemoveCard = "CampfireRemoveCard";

        // === 物品/道具同步消息 ===

        /// <summary>
        /// 获得道具（Exhibit）
        /// </summary>
        public const string OnExhibitObtained = "OnExhibitObtained";

        /// <summary>
        /// 移除道具
        /// </summary>
        public const string OnExhibitRemoved = "OnExhibitRemoved";

        /// <summary>
        /// 使用药水
        /// </summary>
        public const string OnPotionUsed = "OnPotionUsed";

        /// <summary>
        /// 商店购买
        /// </summary>
        public const string OnShopPurchase = "OnShopPurchase";

        // === 网络和状态同步消息 ===

        /// <summary>
        /// 连接建立
        /// </summary>
        public const string OnConnectionEstablished = "OnConnectionEstablished";

        /// <summary>
        /// 连接丢失
        /// </summary>
        public const string OnConnectionLost = "OnConnectionLost";

        /// <summary>
        /// 重连尝试
        /// </summary>
        public const string OnReconnectionAttempt = "OnReconnectionAttempt";

        /// <summary>
        /// 状态同步请求
        /// </summary>
        public const string StateSyncRequest = "StateSyncRequest";

        /// <summary>
        /// 状态同步响应
        /// </summary>
        public const string StateSyncResponse = "StateSyncResponse";

        /// <summary>
        /// 完整状态快照请求
        /// </summary>
        public const string FullStateSyncRequest = "FullStateSyncRequest";

        /// <summary>
        /// 完整状态快照响应
        /// </summary>
        public const string FullStateSyncResponse = "FullStateSyncResponse";

        // === 房间/战斗残局同步消息 ===

        /// <summary>
        /// 请求主机返回指定房间（RoomKey）的最新状态快照。
        /// </summary>
        public const string RoomStateRequest = "RoomStateRequest";

        /// <summary>
        /// 主机响应房间状态快照（定向单播给请求方）。
        /// </summary>
        public const string RoomStateResponse = "RoomStateResponse";

        /// <summary>
        /// 进入房间/战斗的权威方上传房间状态给主机（主机作为中枢缓存）。
        /// </summary>
        public const string RoomStateUpload = "RoomStateUpload";

        /// <summary>
        /// （可选）主机把房间状态更新推送给房间内其他玩家。
        /// </summary>
        public const string RoomStateBroadcast = "RoomStateBroadcast";

        // === 游戏控制消息 ===

        /// <summary>
        /// 游戏开始
        /// </summary>
        public const string OnGameStart = "OnGameStart";

        /// <summary>
        /// 游戏结束
        /// </summary>
        public const string OnGameEnd = "OnGameEnd";

        /// <summary>
        /// 游戏保存
        /// </summary>
        public const string OnGameSave = "OnGameSave";

        /// <summary>
        /// 游戏加载
        /// </summary>
        public const string OnGameLoad = "OnGameLoad";

        /// <summary>
        /// 错误消息
        /// </summary>
        public const string OnError = "OnError";

        // === 中途加入相关消息 ===

        /// <summary>
        /// 中途加入请求
        /// </summary>
        public const string MidGameJoinRequest = "MidGameJoinRequest";

        /// <summary>
        /// 中途加入响应
        /// </summary>
        public const string MidGameJoinResponse = "MidGameJoinResponse";

        /// <summary>
        /// 游戏状态数据传输
        /// </summary>
        public const string GameStateTransfer = "GameStateTransfer";

        // === 聊天和UI消息 ===

        /// <summary>
        /// 商店进入事件
        /// </summary>
        public const string OnShopEnter = "OnShopEnter";

        /// <summary>
        /// 商店离开事件
        /// </summary>
        public const string OnShopExit = "OnShopExit";


        /// <summary>
        /// 聊天消息
        /// </summary>
        public const string ChatMessage = "ChatMessage";

        /// <summary>
        /// 玩家准备状态变更
        /// </summary>
        public const string PlayerReadyChanged = "PlayerReadyChanged";

        // === 卡牌库同步消息 ===

        /// <summary>
        /// 手牌状态同步请求
        /// </summary>
        public const string HandSyncRequest = "HandSyncRequest";

        /// <summary>
        /// 手牌状态同步响应
        /// </summary>
        public const string HandSyncResponse = "HandSyncResponse";

        /// <summary>
        /// 牌库状态同步请求
        /// </summary>
        public const string DeckSyncRequest = "DeckSyncRequest";

        /// <summary>
        /// 牌库状态同步响应
        /// </summary>
        public const string DeckSyncResponse = "DeckSyncResponse";

        /// <summary>
        /// 弃牌堆状态同步请求
        /// </summary>
        public const string DiscardSyncRequest = "DiscardSyncRequest";

        /// <summary>
        /// 弃牌堆状态同步响应
        /// </summary>
        public const string DiscardSyncResponse = "DiscardSyncResponse";

        /// <summary>
        /// 牌组操作事件（洗牌、搜索等）
        /// </summary>
        public const string DeckOperation = "DeckOperation";

        /// <summary>
        /// 卡牌状态变更事件
        /// </summary>
        public const string CardStateChanged = "CardStateChanged";

        // === 宝物状态同步消息 ===

        /// <summary>
        /// 宝物激活状态变更事件
        /// </summary>
        public const string ExhibitActivationChanged = "ExhibitActivationChanged";

        /// <summary>
        /// 宝物计数器变更事件
        /// </summary>
        public const string ExhibitCounterChanged = "ExhibitCounterChanged";

        // === 药水/道具同步消息 ===

        /// <summary>
        /// 药水获取事件
        /// </summary>
        public const string OnPotionObtained = "OnPotionObtained";


        /// <summary>
        /// 药水丢弃事件
        /// </summary>
        public const string OnPotionDiscarded = "OnPotionDiscarded";

        /// <summary>
        /// 药水效果应用事件
        /// </summary>
        public const string OnPotionEffectApplied = "OnPotionEffectApplied";

        // === 存档同步消息 ===

        /// <summary>
        /// 存档同步请求
        /// </summary>
        public const string SaveSyncRequest = "SaveSyncRequest";

        /// <summary>
        /// 存档同步响应
        /// </summary>
        public const string SaveSyncResponse = "SaveSyncResponse";

        /// <summary>
        /// 快速存档同步
        /// </summary>
        public const string QuickSaveSync = "QuickSaveSync";

        // === NAT/打洞协助（Relay 优先）===

        /// <summary>
        /// 客户端上报自身 NAT 信息（Relay 服务器存储并用于协助打洞/端点交换）
        /// </summary>
        public const string NatInfoReport = "NatInfoReport";

        /// <summary>
        /// 请求目标玩家的 NAT 候选端点信息（用于 P2P 优先尝试）
        /// </summary>
        public const string NatInfoRequest = "NatInfoRequest";

        /// <summary>
        /// NAT 信息响应（包含目标玩家的 NAT 信息与候选端点）
        /// </summary>
        public const string NatInfoResponse = "NatInfoResponse";

        /// <summary>
        /// 打洞协助的错误响应
        /// </summary>
        public const string NatError = "NatError";
    }

    /// <summary>
    /// 消息类型分类
    /// </summary>
    public static class MessageCategories
    {
        /// <summary>
        /// 系统消息
        /// </summary>
        public static readonly string[] System =
        [
            NetworkMessageTypes.PlayerJoined,
            NetworkMessageTypes.PlayerLeft,
            NetworkMessageTypes.PlayerListUpdate,
            NetworkMessageTypes.Welcome,
            NetworkMessageTypes.Heartbeat,
            NetworkMessageTypes.HeartbeatResponse,
            NetworkMessageTypes.HostChanged,
            NetworkMessageTypes.GetSelf_REQUEST,
            NetworkMessageTypes.GetSelf_RESPONSE
        ];

        /// <summary>
        /// 游戏同步消息
        /// </summary>
        public static readonly string[] GameSync =
        [
            NetworkMessageTypes.OnCardPlayStart,
            NetworkMessageTypes.OnCardPlayComplete,
            NetworkMessageTypes.ManaConsumeStarted,
            NetworkMessageTypes.ManaConsumeCompleted,
            NetworkMessageTypes.OnTurnStart,
            NetworkMessageTypes.OnTurnEnd,
            NetworkMessageTypes.EndTurnRequest,
            NetworkMessageTypes.EndTurnStatus,
            NetworkMessageTypes.EndTurnConfirm,
            NetworkMessageTypes.OnBattleStart,
            NetworkMessageTypes.OnBattleEnd
        ];

        /// <summary>
        /// 状态管理消息
        /// </summary>
        public static readonly string[] StateManagement =
        [
            NetworkMessageTypes.StateSyncRequest,
            NetworkMessageTypes.StateSyncResponse,
            NetworkMessageTypes.FullStateSyncRequest,
            NetworkMessageTypes.FullStateSyncResponse,
            NetworkMessageTypes.OnConnectionEstablished,
            NetworkMessageTypes.OnConnectionLost,
            NetworkMessageTypes.OnReconnectionAttempt
        ];
    }

    /// <summary>
    /// 消息优先级
    /// </summary>
    public enum MessagePriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }
}
