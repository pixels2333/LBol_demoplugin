using NetworkPlugin.Network.Event;

namespace NetworkPlugin.Core;

/// <summary>
/// 同步管理器接口 - 定义游戏状态同步的核心功能
/// 用于依赖注入和测试隔离
/// </summary>
public interface ISynchronizationManager
{
    /// <summary>
    /// 处理游戏事件的主要入口点
    /// 将本地游戏事件同步到网络
    /// </summary>
    /// <param name="gameEvent">需要处理的游戏事件对象</param>
    void SyncGameEventToNetwork(GameEvent gameEvent);

    /// <summary>
    /// 接收并处理来自网络的远程事件
    /// 将网络传输的事件数据应用到本地游戏状态
    /// </summary>
    /// <param name="eventData">来自网络的原始事件数据</param>
    void ProcessEventFromNetwork(object eventData);

    /// <summary>
    /// 发送卡牌使用事件
    /// 同步卡牌基本信息、法力消耗和目标选择等卡牌使用相关的状态
    /// </summary>
    /// <param name="cardId">使用的卡牌唯一标识符</param>
    /// <param name="cardName">卡牌显示名称</param>
    /// <param name="cardType">卡牌类型（攻击/技能/能力牌等）</param>
    /// <param name="manaCost">法力消耗数组[红,蓝,绿,白]</param>
    /// <param name="targetSelector">目标选择器字符串描述</param>
    /// <param name="playerState">使用卡牌时的玩家状态快照</param>
    void SendCardPlayEvent(string cardId, string cardName, string cardType,
        int[] manaCost, string targetSelector, object playerState);

    /// <summary>
    /// 发送法力消耗事件
    /// 同步法力变化给远程玩家，保持法力状态的一致性
    /// </summary>
    /// <param name="manaBefore">消耗前的法力值数组[红,蓝,绿,白]</param>
    /// <param name="manaConsumed">消耗的法力值数组[红,蓝,绿,白]</param>
    /// <param name="source">法力消耗的来源描述</param>
    void SendManaConsumeEvent(int[] manaBefore, int[] manaConsumed, string source);

    /// <summary>
    /// 发送篝火选项事件
    /// 同步篝火点的选择和操作给远程玩家，协调多人游戏的决策
    /// </summary>
    /// <param name="eventType">篝火事件类型（如休息、强化、升级等）</param>
    /// <param name="optionData">选项的详细数据和参数</param>
    /// <param name="playerState">选择时的玩家状态快照</param>
    void SendGapStationEvent(string eventType, object optionData, object playerState);

    /// <summary>
    /// 请求完整状态同步
    /// 用于新玩家加入游戏或断线重连时获取完整的游戏状态
    /// </summary>
    void RequestFullSync();

    /// <summary>
    /// 处理网络连接恢复事件
    /// 当网络重新连接可用时，处理队列中的待处理事件并请求状态同步
    /// </summary>
    void OnConnectionRestored();

    /// <summary>
    /// 处理网络连接丢失事件
    /// 当网络连接断开时，切换到离线模式并通知其他玩家
    /// </summary>
    void OnConnectionLost();

    /// <summary>
    /// 获取同步统计信息
    /// 返回同步管理器的运行状态和性能指标，用于调试和系统监控
    /// </summary>
    /// <returns>包含同步统计数据的对象</returns>
    object GetSyncStatistics();

    /// <summary>
    /// 获取远程事件缓冲区统计信息
    /// 用于调试和监控有序事件处理的状态
    /// </summary>
    /// <returns>包含缓冲区统计信息的对象</returns>
    object GetEventBufferStatistics();

    /// <summary>
    /// 底层的网络发送方法
    /// 负责实际的事件数据传输和网络通信
    /// </summary>
    /// <param name="gameEvent">要发送的游戏事件</param>
    void SendGameEvent(GameEvent gameEvent);
}
