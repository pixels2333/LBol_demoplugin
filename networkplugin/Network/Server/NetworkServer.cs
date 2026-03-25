using System;

using System.Threading;
using BepInEx.Logging;
using LiteNetLib; // 假设使用的库是 LiteNetLib
using LiteNetLib.Utils;
//TODO:这个地方用了日志系统和依赖注入,如果使用了分离服务器,需要修改日志系统和依赖注入
namespace NetworkPlugin.Network.Server;

/// <summary>
/// LiteNetLib 服务端实现，负责处理客户端连接请求、事件分发和服务器生命周期管理。
/// </summary>
public class NetworkServer
{
    private EventBasedNetListener _listener;
    private NetManager _netManager;
    private int _port;
    private int _maxConnections;
    private string _connectionKey;

    private readonly ManualLogSource _logger;

    /// <summary>
    /// 初始化服务器并注册网络事件处理程序。
    /// </summary>
    /// <param name="port">服务器监听端口。</param>
    /// <param name="maxConnections">最大允许连接数。</param>
    /// <param name="connectionKey">客户端连接时使用的验证密钥。</param>
    /// <param name="logger">BepInEx 日志记录器。</param>
    public NetworkServer(int port, int maxConnections, string connectionKey, ManualLogSource logger)
    {
        _port = port;
        _maxConnections = maxConnections;
        _connectionKey = connectionKey;
        _listener = new EventBasedNetListener();
        _logger = logger;
        _netManager = new NetManager(_listener);
        RegisterEvents();
    }

    /// <summary>
    /// 注册所有网络事件，包括连接请求、连接建立、断开和数据接收。
    /// </summary>
    private void RegisterEvents()
    {
        _listener.ConnectionRequestEvent += request =>
        {
            if (_netManager.PeersCount < _maxConnections)
            {
                // 注意：在实际应用中，检查密钥应该更安全，这里只是简单比较
                // 如果需要从DataReader中读取密钥，可以这样：request.Data.GetString()
                if (request.Data.GetString(_connectionKey.Length) == _connectionKey)
                {
                    request.AcceptIfKey(_connectionKey);
                    Console.WriteLine($"[Server] Accepted connection from {request.RemoteEndPoint}");
                }
                else
                {
                    request.Reject();
                    Console.WriteLine($"[Server] Rejected connection from {request.RemoteEndPoint} due to invalid key.");
                }
            }
            else
            {
                request.Reject();
                Console.WriteLine($"[Server] Rejected connection from {request.RemoteEndPoint}: Max connections reached.");
            }
        };

        _listener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine($"[Server] Client connected: {peer.EndPoint}");
            // 可以选择在这里发送欢迎消息

        };

        _listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            Console.WriteLine($"[Server] Client disconnected: {peer.EndPoint}, Reason: {disconnectInfo.Reason}");
        };

        _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {
            try
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server] Error processing data from {fromPeer.EndPoint}: {ex.Message}");
            }
            finally
            {
                dataReader.Recycle();
            }
        };
    }

    /// <summary>
    /// 启动服务器并开始监听指定端口。
    /// </summary>
    public void Start()
    {
        _netManager.Start(_port);
        Console.WriteLine($"[Server] Server started on port {_port}.");
    }

    /// <summary>
    /// 轮询并处理所有挂起的网络事件。
    /// </summary>
    public void PollEvents()
    {
        _netManager.PollEvents();
    }

    /// <summary>
    /// 停止服务器并断开所有连接。
    /// </summary>
    public void Stop()
    {
        _netManager.Stop();
        Console.WriteLine("[Server] Server stopped.");
    }

   
}

