using System;
using System.Collections.Generic;

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

        /// <summary>
        /// 点对点转发消息
        /// </summary>
        public const string DirectMessage = "DirectMessage";

        /// <summary>
        /// 玩家位置更新
        /// </summary>
        public const string UpdatePlayerLocation = "UpdatePlayerLocation";

        /// <summary>
        /// 重连请求
        /// </summary>
        public const string Reconnect_REQUEST = "Reconnect_REQUEST";

        /// <summary>
        /// 重连响应
        /// </summary>
        public const string Reconnect_RESPONSE = "Reconnect_RESPONSE";

        /// <summary>
        /// 创建房间
        /// </summary>
        public const string CreateRoom = "CreateRoom";

        /// <summary>
        /// 加入房间
        /// </summary>
        public const string JoinRoom = "JoinRoom";

        /// <summary>
        /// 离开房间
        /// </summary>
        public const string LeaveRoom = "LeaveRoom";

        /// <summary>
        /// 房间广播消息
        /// </summary>
        public const string RoomMessage = "RoomMessage";

        /// <summary>
        /// 获取房间列表请求
        /// </summary>
        public const string GetRoomList = "GetRoomList";

        /// <summary>
        /// 房间列表响应
        /// </summary>
        public const string RoomList = "RoomList";

        /// <summary>
        /// 房间创建成功
        /// </summary>
        public const string RoomCreated = "RoomCreated";

        /// <summary>
        /// 房间加入成功
        /// </summary>
        public const string RoomJoined = "RoomJoined";

        /// <summary>
        /// 踢出房间玩家
        /// </summary>
        public const string KickPlayer = "KickPlayer";

        /// <summary>
        /// 通用错误响应
        /// </summary>
        public const string Error = "Error";

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

        /// <summary>远程玩家使用卡牌事件</summary>
        public const string OnRemoteCardUse = "OnRemoteCardUse";
        /// <summary>远程玩家卡牌结算完成事件</summary>
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

        /// <summary>心境效果循环开始事件</summary>
        public const string OnMoodEffectLoopStarted = "OnMoodEffectLoopStarted";
        /// <summary>心境效果循环结束事件</summary>
        public const string OnMoodEffectLoopEnded = "OnMoodEffectLoopEnded";
        /// <summary>心境效果状态同步事件</summary>
        public const string OnMoodEffectStateSync = "OnMoodEffectStateSync";

        // === 玩家状态同步消息 ===

        /// <summary>
        /// 玩家状态更新（按游戏事件广播/转发；必须以 On 开头以避免被服务端按 SystemMessage 处理）。
        /// </summary>
        public const string OnPlayerStateUpdate = "OnPlayerStateUpdate";

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

        /// <summary>结束回合请求</summary>
        public const string EndTurnRequest = "EndTurnRequest";
        /// <summary>结束回合状态</summary>
        public const string EndTurnStatus = "EndTurnStatus";
        /// <summary>结束回合确认</summary>
        public const string EndTurnConfirm = "EndTurnConfirm";

        /// <summary>
        /// 战斗开始
        /// </summary>
        public const string OnBattleStart = "OnBattleStart";

        /// <summary>
        /// 战斗结束
        /// </summary>
        public const string OnBattleEnd = "OnBattleEnd";

        /// <summary>
        /// 战斗内敌人意图变更（Host 权威广播）。
        /// </summary>
        public const string BattleEnemyIntentChanged = "BattleEnemyIntentChanged";

        /// <summary>
        /// 战斗内敌人状态变更（Host 权威广播）。
        /// </summary>
        public const string BattleEnemyStateChanged = "BattleEnemyStateChanged";

        /// <summary>
        /// 战斗内敌人生成（主路径事件）。
        /// </summary>
        public const string BattleEnemySpawned = "BattleEnemySpawned";

        /// <summary>
        /// 敌人状态变更（兼容桥事件，计划在 1 个迭代周期后下线）。
        /// </summary>
        public const string EnemyStateUpdate = "EnemyStateUpdate";

        /// <summary>
        /// 敌人生成（兼容桥事件，计划在 1 个迭代周期后下线）。
        /// </summary>
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

        /// <summary>
        /// 地图节点投票提交（玩家 -> 房主/房间）。
        /// </summary>
        public const string OnMapNodeVoteCast = "OnMapNodeVoteCast";

        /// <summary>
        /// 地图节点投票裁决结果（房主广播）。
        /// </summary>
        public const string OnMapNodeVoteResult = "OnMapNodeVoteResult";

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
        /// 开局奖励（Debut Bonus）随机结果广播。
        /// </summary>
        public const string OnDebutBonusRolled = "OnDebutBonusRolled";

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
        /// Gap 治疗请求：客户端 -> Host。
        /// </summary>
        public const string OnGapHealRequest = "OnGapHealRequest";

        /// <summary>
        /// Gap 治疗失败：Host -> 全体。
        /// </summary>
        public const string OnGapHealFailed = "OnGapHealFailed";

        /// <summary>
        /// Gap 治疗结果：Host -> 全体。
        /// </summary>
        public const string OnGapPlayerHealed = "OnGapPlayerHealed";

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
        /// GapOptions 卡牌升级选择
        /// </summary>
        public const string GapOptionsUpgradeSelected = "GapOptionsUpgradeSelected";

        /// <summary>
        /// GapOptions 卡牌移除选择
        /// </summary>
        public const string GapOptionsRemoveCard = "GapOptionsRemoveCard";

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
        /// 使用工具牌
        /// </summary>
        public const string OnToolCardUsed = "OnToolCardUsed";

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
        /// 本局结算结果（胜利链路）。
        /// </summary>
        public const string OnGameRunResult = "OnGameRunResult";

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

        // === 工具牌同步消息 ===

        /// <summary>
        /// 工具牌获得事件
        /// </summary>
        public const string OnToolCardObtained = "OnToolCardObtained";


        /// <summary>
        /// 工具牌移除事件
        /// </summary>
        public const string OnToolCardRemoved = "OnToolCardRemoved";

        /// <summary>
        /// 工具牌效果应用事件
        /// </summary>
        public const string OnToolCardEffectApplied = "OnToolCardEffectApplied";

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

        /// <summary>
        /// 游戏事件判定路由场景。
        /// </summary>
        public enum GameEventRoute
        {
            /// <summary>客户端视角：仅接收与客户端自身相关的游戏事件。</summary>
            Client,
            /// <summary>主机/服务器视角：处理所有游戏事件的路由和广播。</summary>
            HostServer,
            /// <summary>中继服务器视角：转发游戏事件，不生成权威数据。</summary>
            Relay,
        }

        /// <summary>
        /// 显式声明的游戏事件集合。<br/>
        /// 这些消息类型无论路由场景如何，均被视为 GameEvent 并进入游戏事件通道处理。<br/>
        /// 包括：回合控制事件、战斗事件、卡牌状态变化、Gap 休息点事件、聊天消息等。
        /// </summary>
        private static readonly HashSet<string> ExplicitGameEvents = new(StringComparer.Ordinal)
        {
            EndTurnRequest,           // 结束回合请求
            EndTurnStatus,            // 结束回合状态
            EndTurnConfirm,           // 结束回合确认
            BattleEnemyIntentChanged, // 敌人意图变更
            BattleEnemyStateChanged,  // 敌人状态变更
            BattleEnemySpawned,       // 敌人生成
            EnemyStateUpdate,         // 敌人状态更新（兼容桥事件）
            EnemySpawned,             // 敌人生成（兼容桥事件）
            CardStateChanged,         // 卡牌状态变更
            GapOptionsUpgradeSelected,// Gap 卡牌升级选择
            GapOptionsRemoveCard,     // Gap 卡牌移除选择
            GapStationEntered,        // 进入 Gap 休息点
            DrinkTeaStarted,          // 开始喝茶
            DrinkTeaCompleted,        // 喝茶完成
            ChatMessage,              // 聊天消息
        };

        /// <summary>
        /// 仅在客户端路由场景下被视为 GameEvent 的消息集合。<br/>
        /// 这些消息通常是客户端接收到的状态同步响应、房间信息更新等被动消息。
        /// </summary>
        private static readonly HashSet<string> ClientOnlyGameEvents = new(StringComparer.Ordinal)
        {
            StateSyncResponse,        // 状态同步响应
            FullStateSyncRequest,     // 完整状态同步请求
            FullStateSyncResponse,    // 完整状态同步响应
            RoomStateRequest,         // 房间状态请求
            RoomStateResponse,        // 房间状态响应
            RoomStateUpload,          // 房间状态上传
            RoomStateBroadcast,       // 房间状态广播
            MidGameJoinRequest,       // 中途加入请求
            MidGameJoinResponse,      // 中途加入响应
            Welcome,                  // 欢迎消息
            PlayerJoined,             // 玩家加入
            PlayerLeft,               // 玩家离开
            PlayerListUpdate,         // 玩家列表更新
            HostChanged,              // 房主变更
        };

        /// <summary>
        /// 仅在主机关联（HostServer）路由场景下被视为 GameEvent 的消息集合。<br/>
        /// 这些消息通常是主机向中继或客户端发起的同步请求与响应。
        /// </summary>
        private static readonly HashSet<string> HostServerOnlyGameEvents = new(StringComparer.Ordinal)
        {
            StateSyncRequest,         // 状态同步请求
            FullStateSyncRequest,     // 完整状态同步请求
            FullStateSyncResponse,    // 完整状态同步响应
            RoomStateRequest,         // 房间状态请求
            RoomStateResponse,        // 房间状态响应
            RoomStateUpload,          // 房间状态上传
            RoomStateBroadcast,       // 房间状态广播
        };

        /// <summary>
        /// 仅在中继（Relay）路由场景下被视为 GameEvent 的消息集合。<br/>
        /// 中继主要负责转发，此类消息用于中继自身的状态同步和广播。
        /// </summary>
        private static readonly HashSet<string> RelayOnlyGameEvents = new(StringComparer.Ordinal)
        {
            StateSyncRequest,         // 状态同步请求（中继转发用）
            RoomStateBroadcast,       // 房间状态广播（中继转发用）
        };

        /// <summary>
        /// 统一判定消息是否应进入 GameEvent 通道。<br/>
        /// 判定优先级：<br/>
        /// 1) 显式声明的游戏事件集合（ExplicitGameEvents）→ 是；<br/>
        /// 2) 以 On / Mana / Gap / Battle 开头的消息类型 → 是；<br/>
        /// 3) 按路由场景（Client / HostServer / Relay）分别查各自的白名单集合。
        /// </summary>
        /// <param name="messageType">消息类型字符串。</param>
        /// <param name="route">调用方场景（Client / HostServer / Relay）。</param>
        /// <returns>若消息类型属于 GameEvent 则返回 true；否则返回 false。</returns>
        public static bool IsGameEvent(string messageType, GameEventRoute route)
        {
            if (string.IsNullOrWhiteSpace(messageType))
            {
                return false;
            }

            // 1) 显式声明的游戏事件（无论路由场景均视为 GameEvent）
            if (ExplicitGameEvents.Contains(messageType) ||
                // 2) 按命名约定前缀匹配："On"（事件）、"Mana"（法力）、"Gap"（休息点）、"Battle"（战斗）
                messageType.StartsWith("On", StringComparison.Ordinal) ||
                messageType.StartsWith("Mana", StringComparison.Ordinal) ||
                messageType.StartsWith("Gap", StringComparison.Ordinal) ||
                messageType.StartsWith("Battle", StringComparison.Ordinal))
            {
                return true;
            }

            // 3) 按路由场景查找各自的白名单
            return route switch
            {
                GameEventRoute.Client => ClientOnlyGameEvents.Contains(messageType),
                GameEventRoute.HostServer => HostServerOnlyGameEvents.Contains(messageType),
                GameEventRoute.Relay => RelayOnlyGameEvents.Contains(messageType),
                _ => false,
            };
        }
    }

    /// <summary>
    /// 消息优先级枚举。<br/>
    /// 用于网络传输队列的排序依据，优先级越高越早被发送。
    /// </summary>
    public enum MessagePriority
    {
        /// <summary>低优先级：用于非关键的状态同步、日志等。</summary>
        Low = 0,
        /// <summary>普通优先级：默认级别，用于大多数常规消息。</summary>
        Normal = 1,
        /// <summary>高优先级：用于需要及时送达的消息，如战斗事件。</summary>
        High = 2,
        /// <summary>关键优先级：用于必须确保及时送达的极重要消息，如连接控制。</summary>
        Critical = 3
    }
}
