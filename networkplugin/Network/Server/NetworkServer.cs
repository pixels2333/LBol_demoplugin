using System;

using System.Threading;
using BepInEx.Logging;
using LiteNetLib; // 假设使用的库是 LiteNetLib
using LiteNetLib.Utils;
//TODO:这个地方用了日志系统和依赖注入,如果使用了分离服务器,需要修改日志系统和依赖注入
namespace NetworkPlugin.Network.Server
{
    public class NetworkServer
    {
        private EventBasedNetListener _listener;
        private NetManager _netManager;
        private int _port;
        private int _maxConnections;
        private string _connectionKey;

        private readonly ManualLogSource _logger;

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

        public void Start()
        {
            _netManager.Start(_port);
            Console.WriteLine($"[Server] Server started on port {_port}.");
        }

        public void PollEvents()
        {
            _netManager.PollEvents();
        }

        public void Stop()
        {
            _netManager.Stop();
            Console.WriteLine("[Server] Server stopped.");
        }

       
    }
}

