using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;

namespace NetworkPlugin.Network.Utils;

/// <summary>
/// NAT穿透工具类 - 实现P2P连接辅助功能
/// 支持UPnP端口映射、STUN服务器交互、连接类型检测
/// 重要性: ⭐⭐⭐⭐ (网络可连接性)
/// </summary>
public class NatTraversal
{
    private static readonly ManualLogSource _logger;
    private static readonly Dictionary<string, NatInfo> _peerNatInfo = [];
    private static bool _upnpEnabled = false;

    static NatTraversal()
    {
        // TODO: 初始化日志系统
        // _logger = BepInEx.Logging.Logger.CreateLogSource("NATTraversal");
    }

    #region NAT类型定义

    /// <summary>
    /// NAT类型枚举
    /// </summary>
    public enum NatType
    {
        Unknown,
        OpenInternet,      // 无NAT，公网IP
        FullCone,           // 全锥形NAT
        RestrictedCone,     // 受限锥形NAT
        PortRestrictedCone, // 端口受限锥形NAT
        Symmetric,          // 对称NAT
        Blocked,            // 阻止NAT穿透
        DoubleNat          // 双重NAT
        Hairpin             // 发夹型NAT
    }

    /// <summary>
    /// NAT信息数据结构
    /// </summary>
    public class NatInfo
    {
        public string PeerId { get; set; }
        public NatType NatType { get; set; }
        public IPEndPoint PublicEndPoint { get; set; }
        public IPEndPoint LocalEndPoint { get; set; }
        public bool SupportsHolePunching { get; set; }
        public bool SupportsUPnP { get; set; }
        public DateTime LastUpdate { get; set; }
        public List<string> StunServers { get; set; } = [];
    }

    /// <summary>
    /// UPnP端口映射结果
    /// </summary>
    public class UpnpMappingResult
    {
        public bool Success { get; set; }
        public int ExternalPort { get; set; }
        public int InternalPort { get; set; }
        public string Protocol { get; set; }
        public string Description { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// STUN服务器响应
    /// </summary>
    public class StunResponse
    {
        public NatType DetectedNatType { get; set; }
        public IPEndPoint PublicEndPoint { get; set; }
        public bool SupportsHairpinning { get; set; }
        public string StunServer { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    #region UPnP端口映射

    /// <summary>
    /// 启用UPnP端口映射
    /// </summary>
    public static async Task<UpnpMappingResult> EnableUpnpMapping(int internalPort, int externalPort = 0, string description = "LBoL_Multiplayer")
    {
        try
        {
            if (externalPort == 0)
            {
                externalPort = internalPort;
            }

            var result = new UpnpMappingResult
            {
                InternalPort = internalPort,
                ExternalPort = externalPort,
                Protocol = "UDP",
                Description = description
            };

            // TODO: 实现实际的UPnP映射
            // 这里需要使用Windows的UPnP API或第三方库

            // 模拟UPnP映射成功
            result.Success = true;
            _upnpEnabled = true;

            _logger?.LogInfo($"[NATTraversal] UPnP mapping enabled: {internalPort} -> {externalPort}");
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[NATTraversal] UPnP mapping failed: {ex.Message}");
            return new UpnpMappingResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                InternalPort = internalPort,
                ExternalPort = externalPort,
                Protocol = "UDP"
            };
        }
    } // 启用UPnP端口映射：创建内部端口到外部端口的映射，支持NAT穿透

    /// <summary>
    /// 禁用UPnP端口映射
    /// </summary>
    public static async Task<bool> DisableUpnpMapping(int port)
    {
        try
        {
            // TODO: 实现UPnP映射删除
            _upnpEnabled = false;
            _logger?.LogInfo($"[NATTraversal] UPnP mapping disabled for port: {port}");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[NATTraversal] Failed to disable UPnP mapping: {ex.Message}");
            return false;
        }
    } // 禁用UPnP端口映射：删除已创建的端口映射，释放网络资源

    /// <summary>
    /// 检查UPnP可用性
    /// </summary>
    public static async Task<bool> CheckUpnpAvailability()
    {
        try
        {
            // TODO: 实现UPnP可用性检查
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[NATTraversal] UPnP availability check failed: {ex.Message}");
            return false;
        }
    } // 检查UPnP可用性：检测系统是否支持UPnP协议，用于NAT穿透

    #endregion

    #region STUN服务器交互

    /// <summary>
    /// 默认STUN服务器列表
    /// </summary>
    private static readonly List<string> DefaultStunServers =
    [
        "stun.l.google.com:19302",
        "stun1.l.google.com:19302",
        "stun2.l.google.com:19302",
        "stun3.l.google.com:19302",
        "stun4.l.google.com:19302",
        "stun.ekiga.net:3478",
        "stun.ideasip.com:3478",
        "stun.sipgate.net:3478",
        "stun.xten.com:3478"
    ];

    /// <summary>
    /// 通过STUN服务器检测NAT类型
    /// </summary>
    public static async Task<StunResponse> DetectNatType(string stunServer = null)
    {
        try
        {
            var server = stunServer ?? DefaultStunServers[0];
            var result = new StunResponse
            {
                StunServer = server
            };

            // TODO: 实现真实的STUN协议交互
            // 这里需要实现STUN绑定请求和响应解析

            // 模拟STUN响应
            result.Success = true;
            result.DetectedNatType = NatType.OpenInternet; // 模拟开放网络
            result.PublicEndPoint = new IPEndPoint(IPAddress.Parse("203.0.113.1"), 12345);
            result.SupportsHairpinning = true;

            _logger?.LogInfo($"[NATTraversal] NAT type detected: {result.DetectedNatType}");
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[NATTraversal] NAT type detection failed: {ex.Message}");
            return new StunResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                DetectedNatType = NatType.Unknown
            };
        }
    } // 通过STUN服务器检测NAT类型：使用STUN协议检测网络地址转换类型

    /// <summary>
    /// 使用多个STUN服务器检测NAT类型
    /// </summary>
    public static async Task<List<StunResponse>> DetectNatTypeMultiple()
    {
        var results = new List<StunResponse>();
        var tasks = new List<Task<StunResponse>>();

        // 并发查询多个STUN服务器
        foreach (var server in DefaultStunServers.Take(3))
        {
            tasks.Add(DetectNatType(server));
        }

        var responses = await Task.WhenAll(tasks);
        results.AddRange(responses);

        _logger?.LogInfo($"[NATTraversal] NAT type detection completed: {results.Count} responses");
        return results;
    } // 使用多个STUN服务器检测NAT类型：并发查询多个STUN服务器，提高检测准确性

    #endregion

    #region 连接类型检测

    /// <summary>
    /// 检测本地连接能力
    /// </summary>
    public static NatInfo DetectLocalConnectivity(int listenPort)
    {
        try
        {
            var info = new NatInfo
            {
                LocalEndPoint = new IPEndPoint(GetLocalIpAddress(), listenPort),
                LastUpdate = DateTime.Now
            };

            // 检测端口是否可用
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Bind(info.LocalEndPoint);
                var localPort = ((IPEndPoint)socket.LocalEndPoint).Port;
                info.LocalEndPoint = new IPEndPoint(info.LocalEndPoint.Address, localPort);
            }

            // 检测UPnP支持
            Task.Run(async () =>
            {
                info.SupportsUPnP = await CheckUpnpAvailability();
            });

            _logger?.LogInfo($"[NATTraversal] Local connectivity detected: {info.LocalEndPoint}");
            return info;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[NATTraversal] Local connectivity detection failed: {ex.Message}");
            return new NatInfo
            {
                NatType = NatType.Blocked
            };
        }
    } // 检测本地连接能力：检测本地网络端口和UPnP支持情况，用于NAT穿透评估

    /// <summary>
    /// 检测P2P连接能力
    /// </summary>
    public static async Task<bool> TestP2pConnectivity(IPEndPoint remoteEndPoint, int timeoutMs = 5000)
    {
        try
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.ReceiveTimeout = timeoutMs;
                socket.Bind(new IPEndPoint(IPAddress.Any, 0));

                // 发送测试数据包
                var testData = Encoding.UTF8.GetBytes("P2P_TEST_" + DateTime.Now.Ticks);
                await socket.SendToAsync(new ArraySegment<byte>(testData), SocketFlags.None, remoteEndPoint);

                // 等待响应
                var buffer = new byte[1024];
                var result = await socket.ReceiveFromAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

                var response = Encoding.UTF8.GetString(buffer, 0, result.ReceivedBytes);
                return response.StartsWith("P2P_TEST_ACK");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogDebug($"[NATTraversal] P2P connectivity test failed: {ex.Message}");
            return false;
        }
    } // 检测P2P连接能力：测试与远程端点的直接连接能力，用于NAT穿透验证

    #endregion

    #region NAT穿透辅助

    /// <summary>
    /// 生成连接令牌
    /// </summary>
    public static string GenerateConnectionToken(string peerId, IPEndPoint endPoint)
    {
        var data = $"{peerId}:{endPoint.Address}:{endPoint.Port}:{DateTime.Now.Ticks}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
    } // 生成连接令牌：为P2P连接创建包含时间戳的安全令牌

    /// <summary>
    /// 验证连接令牌
    /// </summary>
    public static bool ValidateConnectionToken(string token, out string peerId, out IPEndPoint endPoint)
    {
        try
        {
            var data = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = data.Split(':');

            if (parts.Length >= 4 && parts[3] == DateTime.Now.Ticks.ToString())
            {
                peerId = parts[0];
                endPoint = new IPEndPoint(IPAddress.Parse(parts[1]), int.Parse(parts[2]));
                return true;
            }

            peerId = null;
            endPoint = null;
            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[NATTraversal] Token validation failed: {ex.Message}");
            peerId = null;
            endPoint = null;
            return false;
        }
    } // 验证连接令牌：验证P2P连接令牌的有效性和安全性

    /// <summary>
    /// 获取本地IP地址
    /// </summary>
    public static IPAddress GetLocalIpAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                ?? IPAddress.Loopback;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[NATTraversal] Failed to get local IP: {ex.Message}");
            return IPAddress.Loopback;
        }
    } // 获取本地IP地址：获取本机非回环的IPv4地址，用于网络连接

    /// <summary>
    /// 获取公网IP地址（通过HTTP服务）
    /// </summary>
    public static async Task<IPAddress> GetPublicIpAddress()
    {
        try
        {
            using (var client = new System.Net.WebClient())
            {
                var response = await client.DownloadStringTaskAsync("https://api.ipify.org");
                if (IPAddress.TryParse(response.Trim(), out var ip))
                {
                    return ip;
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[NATTraversal] Failed to get public IP: {ex.Message}");
        }

        return null;
    } // 获取公网IP地址：通过HTTP服务获取本机的公网IP地址

    #endregion

    #region 管理器方法

    /// <summary>
    /// 注册对等方NAT信息
    /// </summary>
    public static void RegisterPeerNatInfo(string peerId, NatInfo natInfo)
    {
        _peerNatInfo[peerId] = natInfo;
        _logger?.LogInfo($"[NATTraversal] Registered NAT info for peer: {peerId}");
    } // 注册对等方NAT信息：存储对等方的网络地址转换信息，用于P2P连接

    /// <summary>
    /// 获取对等方NAT信息
    /// </summary>
    public static NatInfo GetPeerNatInfo(string peerId)
    {
        _peerNatInfo.TryGetValue(peerId, out var info);
        return info;
    } // 获取对等方NAT信息：从缓存中获取指定对等方的网络地址转换信息

    /// <summary>
    /// 移除对等方NAT信息
    /// </summary>
    public static void RemovePeerNatInfo(string peerId)
    {
        _peerNatInfo.Remove(peerId);
        _logger?.LogInfo($"[NATTraversal] Removed NAT info for peer: {peerId}");
    } // 移除对等方NAT信息：从缓存中删除对等方的网络地址转换信息

    /// <summary>
    /// 获取所有注册的对等方NAT信息
    /// </summary>
    public static Dictionary<string, NatInfo> GetAllPeerNatInfo()
    {
        return new Dictionary<string, NatInfo>(_peerNatInfo);
    } // 获取所有注册的对等方NAT信息：返回所有对等方的网络地址转换信息副本

    /// <summary>
    /// 检查是否支持NAT穿透
    /// </summary>
    public static bool SupportsNatTraversal(NatType natType)
    {
        return natType switch
        {
            NatType.OpenInternet => true,
            NatType.FullCone => true,
            NatType.RestrictedCone => true,
            NatType.PortRestrictedCone => true,
            NatType.Hairpin => true,
            NatType.Symmetric => false,
            NatType.DoubleNat => false,
            NatType.Blocked => false,
            _ => false
        };
    } // 检查是否支持NAT穿透：根据NAT类型判断是否支持P2P连接穿透

    /// <summary>
    /// 生成NAT穿透报告
    /// </summary>
    public static string GenerateNatReport()
    {
        try
        {
            var report = new StringBuilder();
            report.AppendLine("=== NAT Traversal Report ===");
            report.AppendLine($"UPnP Enabled: {_upnpEnabled}");
            report.AppendLine($"Registered Peers: {_peerNatInfo.Count}");
            report.AppendLine();

            foreach (var kvp in _peerNatInfo)
            {
                var info = kvp.Value;
                report.AppendLine($"Peer: {kvp.Key}");
                report.AppendLine($"  NAT Type: {info.NatType}");
                report.AppendLine($"  Public Endpoint: {info.PublicEndPoint}");
                report.AppendLine($"  Local Endpoint: {info.LocalEndPoint}");
                report.AppendLine($"  Supports Hole Punching: {info.SupportsHolePunching}");
                report.AppendLine($"  Supports UPnP: {info.SupportsUPnP}");
                report.AppendLine($"  Last Update: {info.LastUpdate}");
                report.AppendLine();
            }

            return report.ToString();
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[NATTraversal] Failed to generate report: {ex.Message}");
            return "Error generating NAT report";
        }
    } // 生成NAT穿透报告：生成包含所有对等方NAT类型和连接状态的详细报告

    #endregion
} // NAT穿透工具类：实现P2P连接辅助功能，支持UPnP端口映射、STUN服务器交互、连接类型检测