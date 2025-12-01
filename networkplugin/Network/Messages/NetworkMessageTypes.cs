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

        // === 回合同步消息 ===

        /// <summary>
        /// 回合开始
        /// </summary>
        public const string OnTurnStart = "OnTurnStart";

        /// <summary>
        /// 回合结束
        /// </summary>
        public const string OnTurnEnd = "OnTurnEnd";

        /// <summary>
        /// 战斗开始
        /// </summary>
        public const string OnBattleStart = "OnBattleStart";

        /// <summary>
        /// 战斗结束
        /// </summary>
        public const string OnBattleEnd = "OnBattleEnd";

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
        /// 商店购买事件
        /// </summary>
        public const string OnShopPurchase = "OnShopPurchase";

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
        /// 药水使用事件
        /// </summary>
        public const string OnPotionUsed = "OnPotionUsed";

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

    /// <summary>
    /// 消息优先级配置
    /// </summary>
    public static class MessagePriorities
    {
        private static readonly Dictionary<string, MessagePriority> _priorities = new Dictionary<string, MessagePriority>
        {
            // 系统消息 - 高优先级
            [NetworkMessageTypes.Heartbeat] = MessagePriority.High,
            [NetworkMessageTypes.HeartbeatResponse] = MessagePriority.High,
            [NetworkMessageTypes.PlayerJoined] = MessagePriority.High,
            [NetworkMessageTypes.PlayerLeft] = MessagePriority.High,
            [NetworkMessageTypes.HostChanged] = MessagePriority.High,

            // 游戏同步消息 - 正常优先级
            [NetworkMessageTypes.OnCardPlayStart] = MessagePriority.Normal,
            [NetworkMessageTypes.OnCardPlayComplete] = MessagePriority.Normal,
            [NetworkMessageTypes.ManaConsumeStarted] = MessagePriority.Normal,
            [NetworkMessageTypes.ManaConsumeCompleted] = MessagePriority.Normal,

            // 状态管理消息 - 高优先级
            [NetworkMessageTypes.StateSyncRequest] = MessagePriority.High,
            [NetworkMessageTypes.StateSyncResponse] = MessagePriority.High,
            [NetworkMessageTypes.FullStateSyncRequest] = MessagePriority.Critical,
            [NetworkMessageTypes.FullStateSyncResponse] = MessagePriority.Critical,

            // 聊天消息 - 低优先级
            [NetworkMessageTypes.ChatMessage] = MessagePriority.Low
        };

        /// <summary>
        /// 获取消息优先级
        /// </summary>
        /// <param name="messageType">消息类型</param>
        /// <returns>消息优先级</returns>
        public static MessagePriority GetPriority(string messageType)
        {
            // 尝试从优先级字典中查找指定消息类型的优先级
            return _priorities.TryGetValue(messageType, out var priority)
                ? priority  // 如果找到则返回对应的优先级
                : MessagePriority.Normal; // 如果未找到则返回默认的正常优先级
        }
    }
}