using System;
using System.Net;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.NetworkPlayer;

namespace NetworkPlugin.Network.Client;

/// <summary>
/// 网络客户端接口，定义了客户端与服务器通信的基本功能
/// 提供连接管理、数据传输和事件处理的核心方法
/// </summary>
public interface INetworkClient
{
    #region 基本连接管理

    /// <summary>
    /// 启动客户端服务
    /// 初始化网络组件并准备接收连接
    /// </summary>
    void Start();

    /// <summary>
    /// 获取客户端自身信息
    /// </summary>
    INetworkPlayer GetSelf();

    /// <summary>
    /// 连接到指定服务器
    /// </summary>
    /// <param name="host">服务器主机地址</param>
    /// <param name="port">服务器端口</param>
    void ConnectToServer(string host, int port);

    /// <summary>
    /// 轮询网络事件
    /// 处理接收到的数据和网络状态变化
    /// </summary>
    void PollEvents();

    /// <summary>
    /// 停止客户端服务
    /// 清理资源并断开所有连接
    /// </summary>
    void Stop();

    /// <summary>
    /// 获取当前客户端连接是否活跃
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 获取最近一次与服务器断开的具体原因（如 ConnectionRejected、ConnectionFailed、Timeout 等）
    /// </summary>
    string LastDisconnectReason { get; }

    /// <summary>
    /// 客户端是否处于“正在连接”状态。
    /// </summary>
    bool IsConnecting { get; }

    /// <summary>
    /// 当前连接延迟（毫秒）。
    /// </summary>
    int Ping { get; }

    /// <summary>
    /// 本地端点（可能为 null）。
    /// </summary>
    IPEndPoint LocalEndPoint { get; }

    /// <summary>
    /// 远程端点（可能为 null）。
    /// </summary>
    IPEndPoint RemoteEndPoint { get; }

    #endregion

    #region 数据传输方法

    /// <summary>
    /// 发送游戏同步事件（JSON格式），默认不进行本地回环（<c>IncludeSelf = false</c>）。
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="eventData">事件数据</param>
    void SendGameEventData(string eventType, object eventData);

    /// <summary>
    /// 带有选项配置的游戏同步事件发送方法。
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="eventData">事件数据</param>
    /// <param name="options">发送选项（控制是否本地回环、是否点对点发送等）</param>
    void SendGameEventData(string eventType, object eventData, NetworkEventOptions options);

    /// <summary>
    /// 广播权威状态/结算结果给所有玩家（包含本地主线程安全回环派发）。
    /// 适用于：交易结算、加血复活、回合切换、敌人意图、地图事件、种子同步等。
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="eventData">事件数据</param>
    void BroadcastState(string eventType, object eventData);

    /// <summary>
    /// 广播单机操作/输入动作给其他玩家（排除发送者自身回环，防回声）。
    /// 适用于：单机玩家发起的出牌、卡牌交互等（本地已先执行）。
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="eventData">事件数据</param>
    void BroadcastAction(string eventType, object eventData);

    /// <summary>
    /// 向指定玩家点对点定向发送消息。
    /// 若目标玩家是本地玩家自身，将智能转为本地主线程安全回环，不发送网络数据包。
    /// </summary>
    /// <param name="targetPlayerId">目标玩家 ID</param>
    /// <param name="eventType">事件类型</param>
    /// <param name="eventData">事件数据</param>
    void SendDirect(string targetPlayerId, string eventType, object eventData);

    /// <summary>
    /// 发送通用请求到服务器
    /// 支持原始类型和复杂对象的传输
    /// </summary>
    /// <typeparam name="T">请求数据类型</typeparam>
    /// <param name="requestHeader">请求头标识</param>
    /// <param name="requestData">请求数据</param>
    void SendRequest<T>(string requestHeader, T requestData);

    #endregion

    #region 网络事件

    /// <summary>
    /// 当连接到服务器时触发
    /// </summary>
    event Action<string> OnConnected;

    /// <summary>
    /// 当与服务器断开连接时触发
    /// </summary>
    event Action<string, string> OnDisconnected;

    /// <summary>
    /// 当接收到游戏事件时触发
    /// </summary>
    event Action<string, object> OnGameEventReceived;

    /// <summary>
    /// 当接收到服务器响应时触发
    /// </summary>
    event Action<string, object> OnResponseReceived;

    /// <summary>
    /// 当连接状态改变时触发
    /// </summary>
    event Action<bool> OnConnectionStateChanged;

    #endregion

    #region 高级功能

    /// <summary>
    /// 获取当前连接的统计信息
    /// 包括延迟、丢包率等网络质量指标
    /// </summary>
    /// <returns>连接统计信息</returns>
    object GetConnectionStats();

    /// <summary>
    /// 设置连接超时时间
    /// </summary>
    /// <param name="timeoutMs">超时时间（毫秒）</param>
    void SetConnectionTimeout(int timeoutMs);

    /// <summary>
    /// 启用或断开自动重连
    /// </summary>
    /// <param name="enabled">是否启用自动重连</param>
    /// <param name="retryInterval">重试间隔（毫秒）</param>
    void EnableAutoReconnect(bool enabled, int retryInterval = NetworkConstants.ReconnectIntervalMs);

    #endregion
}
