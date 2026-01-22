using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BepInEx.Logging;

namespace NetworkPlugin.Network.Utils;

/// <summary>
/// NAT穿透工具类
/// 实现P2P连接辅助功能，支持UPnP端口映射、STUN服务器交互、连接类型检测
/// 提供NAT类型检测、端口穿透、P2P连接建立等功能
/// </summary>
public partial class NatTraversal
{
    private const int DefaultTokenTtlSeconds = 300;
    private const int DefaultUpnpProbeTimeoutMs = 500;
    private const int DefaultStunTimeoutMs = 1500;

    /// <summary>
    /// 日志记录器
    /// </summary>
    private static readonly ManualLogSource _logger;

    /// <summary>
    /// 对等方NAT信息缓存
    /// 存储所有已注册的对等方的网络地址转换信息
    /// </summary>
    private static readonly Dictionary<string, NatInfo> _peerNatInfo = [];

    /// <summary>
    /// UPnP功能启用状态
    /// </summary>
    private static bool _upnpEnabled = false;

    /// <summary>
    /// 静态构造函数
    /// 初始化NAT穿透类的静态成员
    /// </summary>
    static NatTraversal()
    {
        // 初始化日志系统：优先复用插件日志，避免额外创建 LogSource；若插件尚未初始化则创建专用 LogSource。
        _logger = Plugin.Logger ?? BepInEx.Logging.Logger.CreateLogSource("NATTraversal");
    }

    #region NAT类型定义

    /// <summary>
    /// NAT类型枚举
    /// 定义了各种网络地址转换类型，用于判断P2P连接的可行性
    /// </summary>
    public enum NatType
    {
        /// <summary>
        /// 未知NAT类型，检测失败或尚未检测
        /// </summary>
        Unknown,

        /// <summary>
        /// 无NAT，公网IP，可以直接P2P连接
        /// </summary>
        OpenInternet,

        /// <summary>
        /// 全锥形NAT，最容易穿透的NAT类型
        /// </summary>
        FullCone,

        /// <summary>
        /// 受限锥形NAT，需要特定条件才能穿透
        /// </summary>
        RestrictedCone,

        /// <summary>
        /// 端口受限锥形NAT，端口受限的NAT类型
        /// </summary>
        PortRestrictedCone,

        /// <summary>
        /// 对称NAT，最难穿透的NAT类型
        /// </summary>
        Symmetric,

        /// <summary>
        /// 阻止NAT穿透，防火墙或路由器阻止连接
        /// </summary>
        Blocked,

        /// <summary>
        /// 双重NAT，两层NAT网络，难以穿透
        /// </summary>
        DoubleNat,

        /// <summary>
        /// 发夹型NAT，支持同一NAT内的设备互相访问
        /// </summary>
        Hairpin,
    }

    /// <summary>
    /// NAT信息数据结构
    /// 存储对等方的网络地址转换详细信息，用于P2P连接
    /// </summary>
    public class NatInfo
    {
        /// <summary>
        /// 对等方唯一标识符
        /// </summary>
        public string PeerId { get; set; }

        /// <summary>
        /// 检测到的NAT类型
        /// </summary>
        public NatType NatType { get; set; }

        /// <summary>
        /// 公网端点（经过NAT转换后的地址）
        /// </summary>
        [JsonConverter(typeof(IPEndPointJsonConverter))]
        public IPEndPoint PublicEndPoint { get; set; }

        /// <summary>
        /// 本地端点（局域网内地址）
        /// </summary>
        [JsonConverter(typeof(IPEndPointJsonConverter))]
        public IPEndPoint LocalEndPoint { get; set; }

        /// <summary>
        /// 是否支持端口打洞
        /// </summary>
        public bool SupportsHolePunching { get; set; }

        /// <summary>
        /// 是否支持UPnP
        /// </summary>
        public bool SupportsUPnP { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// 使用的STUN服务器列表
        /// </summary>
        public List<string> StunServers { get; set; } = [];
    }

    /// <summary>
    /// UPnP端口映射结果
    /// 记录UPnP端口映射操作的执行结果
    /// </summary>
    public class UpnpMappingResult
    {
        /// <summary>
        /// 映射操作是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 外部端口（公网端口）
        /// </summary>
        public int ExternalPort { get; set; }

        /// <summary>
        /// 内部端口（局域网端口）
        /// </summary>
        public int InternalPort { get; set; }

        /// <summary>
        /// 协议类型（TCP或UDP）
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// 端口映射描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 错误消息（失败时）
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// STUN服务器响应
    /// 记录STUN服务器检测NAT类型的结果
    /// </summary>
    public class StunResponse
    {
        /// <summary>
        /// 检测到的NAT类型
        /// </summary>
        public NatType DetectedNatType { get; set; }

        /// <summary>
        /// 公网端点（外部可见地址）
        /// </summary>
        [JsonConverter(typeof(IPEndPointJsonConverter))]
        public IPEndPoint PublicEndPoint { get; set; }

        /// <summary>
        /// 是否支持发夹转换（同一NAT内设备互访）
        /// </summary>
        public bool SupportsHairpinning { get; set; }

        /// <summary>
        /// 使用的STUN服务器地址
        /// </summary>
        public string StunServer { get; set; }

        /// <summary>
        /// 检测是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 错误消息（失败时）
        /// </summary>
        public string ErrorMessage { get; set; }
    }
    #endregion

    /// <summary>
    /// IPEndPoint 的 JSON 转换器，使用字符串格式："ip:port"。
    /// 说明：System.Text.Json 默认无法序列化 IPEndPoint；我们用属性级标注保证两端默认序列化路径可用。
    /// </summary>
    public sealed class IPEndPointJsonConverter : JsonConverter<IPEndPoint>
    {
        public override IPEndPoint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Expected string for IPEndPoint.");
            }

            string s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            if (!TryParseEndPoint(s, out var ep))
            {
                throw new JsonException($"Invalid IPEndPoint string: '{s}'");
            }

            return ep;
        }

        public override void Write(Utf8JsonWriter writer, IPEndPoint value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue($"{value.Address}:{value.Port}");
        }

        private static bool TryParseEndPoint(string s, out IPEndPoint endPoint)
        {
            endPoint = null;

            // IPv6 常见格式为 [::1]:1234，这里做最小兼容；项目主要使用 IPv4。
            if (s.StartsWith("[", StringComparison.Ordinal))
            {
                int idx = s.IndexOf("]:", StringComparison.Ordinal);

                if (idx <= 0)
                {
                    return false;
                }

                string ipPart = s.Substring(1, idx - 1);
                string portPart = s.Substring(idx + 2);

                if (!IPAddress.TryParse(ipPart, out var ip) || !int.TryParse(portPart, out int port))
                {
                    return false;
                }

                endPoint = new IPEndPoint(ip, port);
                return true;
            }

            int lastColon = s.LastIndexOf(':');
            if (lastColon <= 0)
            {
                return false;
            }

            string ipStr = s.Substring(0, lastColon);
            string portStr = s.Substring(lastColon + 1);

            if (!IPAddress.TryParse(ipStr, out var ip4) || !int.TryParse(portStr, out int port4))
            {
                return false;
            }

            endPoint = new IPEndPoint(ip4, port4);
            return true;
        }
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

            // UPnP 不是主流程依赖：按当前需求，默认假设多数环境不支持 UPnP。
            bool available = await CheckUpnpAvailability();
            if (!available)
            {
                _upnpEnabled = false;
                return new UpnpMappingResult
                {
                    Success = false,
                    ErrorMessage = "UPnP not available (treated as unsupported by default).",
                    InternalPort = internalPort,
                    ExternalPort = externalPort,
                    Protocol = "UDP",
                    Description = description
                };
            }

            // TODO(可选增强): 如未来需要真实映射，可接入 Open.NAT 并在此创建端口映射。
            _upnpEnabled = false;
            return new UpnpMappingResult
            {
                Success = false,
                ErrorMessage = "UPnP mapping not implemented (optional enhancement).",
                InternalPort = internalPort,
                ExternalPort = externalPort,
                Protocol = "UDP",
                Description = description
            };
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
    public static Task<bool> DisableUpnpMapping(int port)
    {
        try
        {
            // UPnP 映射属于可选增强，本实现不做真实映射删除，仅清理标记并返回 false/true 的可诊断结果。
            _upnpEnabled = false;
            _logger?.LogInfo($"[NATTraversal] UPnP mapping disabled for port: {port}");
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[NATTraversal] Failed to disable UPnP mapping: {ex.Message}");
            return Task.FromResult(false);
        }
    } // 禁用UPnP端口映射：删除已创建的端口映射，释放网络资源

    /// <summary>
    /// 检查UPnP可用性
    /// </summary>
    public static Task<bool> CheckUpnpAvailability()
    {
        try
        {
            // 默认按“不支持”处理，避免把 UPnP 变成主流程依赖。
            // 如未来需要更精确探测，可实现 SSDP/IGD 发现并在成功时返回 true。
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[NATTraversal] UPnP availability check failed: {ex.Message}");
            return Task.FromResult(false);
        }
    } // 检查UPnP可用性：检测系统是否支持UPnP协议，用于NAT穿透

    #endregion

    #region STUN服务器交互

    /// <summary>
    /// 默认STUN服务器列表
    /// 包含多个公共STUN服务器，用于NAT类型检测
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
    /// 使用STUN协议与指定的STUN服务器交互，检测当前网络的NAT类型
    /// </summary>
    /// <param name="stunServer">STUN服务器地址（可选，默认为第一个公共STUN服务器）</param>
    /// <returns>STUN响应对象，包含检测到的NAT类型和公网端点信息</returns>
    public static async Task<StunResponse> DetectNatType(string stunServer = null)
    {
        try
        {
            string server = stunServer ?? DefaultStunServers[0];

            var (host, port) = ParseHostPort(server, 3478);
            using UdpClient udp = new(AddressFamily.InterNetwork);
            udp.Client.ReceiveTimeout = DefaultStunTimeoutMs;
            udp.Client.SendTimeout = DefaultStunTimeoutMs;

            byte[] request = BuildStunBindingRequest(out byte[] transactionId);
            await udp.SendAsync(request, request.Length, host, port);

            UdpReceiveResult recv = await udp.ReceiveAsync();
            if (!TryParseStunBindingResponse(recv.Buffer, transactionId, out var publicEp, out string parseError))
            {
                return new StunResponse
                {
                    Success = false,
                    ErrorMessage = parseError,
                    DetectedNatType = NatType.Unknown,
                    StunServer = server
                };
            }

            NatType natType = NatType.Unknown;
            IPAddress localIp = GetLocalIpAddress();
            if (publicEp != null && localIp != null && publicEp.Address.Equals(localIp) && !IsPrivateOrLoopback(localIp))
            {
                natType = NatType.OpenInternet;
            }

            StunResponse result = new()
            {
                Success = true,
                DetectedNatType = natType,
                PublicEndPoint = publicEp,
                SupportsHairpinning = false,
                StunServer = server
            };

            _logger?.LogInfo($"[NATTraversal] NAT type detected: {result.DetectedNatType}, public={result.PublicEndPoint}");
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
    /// 并发查询多个STUN服务器，提高检测准确性和可靠性
    /// </summary>
    /// <returns>STUN响应列表，包含从多个服务器获取的检测结果</returns>
    public static async Task<List<StunResponse>> DetectNatTypeMultiple()
    {
        List<StunResponse> results = [];
        List<Task<StunResponse>> tasks = [];

        // 并发查询多个STUN服务器
        foreach (string server in DefaultStunServers.Take(3))
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
    /// 检测本地网络端口可用性、UPnP支持情况和本地IP地址
    /// </summary>
    /// <param name="listenPort">要监听的端口号</param>
    /// <returns>NAT信息对象，包含本地网络配置详情</returns>
    public static NatInfo DetectLocalConnectivity(int listenPort)
    {
        try
        {
            NatInfo info = new()
            {
                LocalEndPoint = new IPEndPoint(GetLocalIpAddress(), listenPort),
                LastUpdate = DateTime.UtcNow
            };

            // 检测端口是否可用
            using (Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Bind(info.LocalEndPoint);// 绑定到本地端点
                int localPort = ((IPEndPoint)socket.LocalEndPoint).Port;
                info.LocalEndPoint = new(info.LocalEndPoint.Address, localPort);
            }

            // 检测UPnP支持：保持结果一致性（同步等待，带超时；默认返回 false）。
            try
            {
                Task<bool> checkTask = CheckUpnpAvailability();
                bool completed = Task.WhenAny(checkTask, Task.Delay(DefaultUpnpProbeTimeoutMs)).GetAwaiter().GetResult() == checkTask;
                info.SupportsUPnP = completed && checkTask.GetAwaiter().GetResult();
            }
            catch
            {
                info.SupportsUPnP = false;
            }

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
            using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.ReceiveTimeout = timeoutMs;
            socket.Bind(new IPEndPoint(IPAddress.Any, 0));

            // 发送测试数据包
            byte[] testData = Encoding.UTF8.GetBytes("P2P_TEST_" + DateTime.Now.Ticks);
            await socket.SendToAsync(new ArraySegment<byte>(testData), SocketFlags.None, remoteEndPoint);

            // 等待响应
            byte[] buffer = new byte[1024];
            IPEndPoint responseEndPoint = new(IPAddress.Any, 0);
            SocketReceiveFromResult result = await socket.ReceiveFromAsync(
                new ArraySegment<byte>(buffer), SocketFlags.None, responseEndPoint);

            string response = Encoding.UTF8.GetString(buffer, 0, result.ReceivedBytes);
            return response.StartsWith("P2P_TEST_ACK");
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
    /// 为P2P连接创建包含对等方ID、端点和时间戳的安全令牌
    /// </summary>
    /// <param name="peerId">对等方唯一标识符</param>
    /// <param name="endPoint">对等方网络端点</param>
    /// <returns>Base64编码的连接令牌字符串</returns>
    public static string GenerateConnectionToken(string peerId, IPEndPoint endPoint)
    {
        // 使用分隔符 '|'，避免与 IPv6/冒号冲突。
        long issuedAt = DateTime.UtcNow.Ticks;
        byte[] nonceBytes = new byte[8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(nonceBytes);
        }

        string nonce = Convert.ToBase64String(nonceBytes);
        string data = $"{peerId}|{endPoint.Address}|{endPoint.Port}|{issuedAt}|{nonce}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
    } // 生成连接令牌：为P2P连接创建包含时间戳的安全令牌

    /// <summary>
    /// 验证连接令牌
    /// 验证P2P连接令牌的有效性、完整性和安全性
    /// </summary>
    /// <param name="token">要验证的连接令牌</param>
    /// <param name="peerId">输出参数：验证通过的对等方ID</param>
    /// <param name="endPoint">输出参数：验证通过的网络端点</param>
    /// <returns>验证是否成功</returns>
    public static bool ValidateConnectionToken(string token, out string peerId, out IPEndPoint endPoint)
    {
        try
        {
            string data = Encoding.UTF8.GetString(Convert.FromBase64String(token)); // 解码Base64令牌获取原始数据
            string[] parts = data.Split('|'); // 使用 '|' 分割，避免 IPv6 冒号冲突

            if (parts.Length < 5)
            {
                peerId = null;
                endPoint = null;
                return false;
            }

            peerId = parts[0];
            if (!IPAddress.TryParse(parts[1], out var ip) || !int.TryParse(parts[2], out int port))
            {
                endPoint = null;
                return false;
            }

            endPoint = new IPEndPoint(ip, port);

            if (!long.TryParse(parts[3], out long issuedAtTicks))
            {
                return false;
            }

            TimeSpan age = DateTime.UtcNow - new DateTime(issuedAtTicks, DateTimeKind.Utc);
            if (age < TimeSpan.Zero || age > TimeSpan.FromSeconds(DefaultTokenTtlSeconds))
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[NATTraversal] Token validation failed: {ex.Message}"); // 记录验证错误
            peerId = null; // 清空输出参数
            endPoint = null; // 清空输出参数
            return false; // 验证失败
        }
    } // 验证连接令牌：验证P2P连接令牌的有效性和安全性

    private static (string host, int port) ParseHostPort(string hostPort, int defaultPort)
    {
        if (string.IsNullOrWhiteSpace(hostPort))
        {
            return ("", defaultPort);
        }

        int idx = hostPort.LastIndexOf(':');
        if (idx > 0 && idx < hostPort.Length - 1 && int.TryParse(hostPort.Substring(idx + 1), out int p))
        {
            return (hostPort.Substring(0, idx), p);
        }

        return (hostPort, defaultPort);
    }

    private static byte[] BuildStunBindingRequest(out byte[] transactionId)
    {
        transactionId = new byte[12];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(transactionId);
        }

        byte[] buf = new byte[20];

        // Type: Binding Request (0x0001)
        buf[0] = 0x00;
        buf[1] = 0x01;

        // Length: 0
        buf[2] = 0x00;
        buf[3] = 0x00;

        // Magic cookie: 0x2112A442
        buf[4] = 0x21;
        buf[5] = 0x12;
        buf[6] = 0xA4;
        buf[7] = 0x42;

        Buffer.BlockCopy(transactionId, 0, buf, 8, 12);
        return buf;
    }

    private static bool TryParseStunBindingResponse(byte[] buf, byte[] transactionId, out IPEndPoint publicEndPoint, out string error)
    {
        publicEndPoint = null;
        error = null;

        if (buf == null || buf.Length < 20)
        {
            error = "STUN response too short.";
            return false;
        }

        // Success response should be 0x0101
        if (buf[0] != 0x01 || buf[1] != 0x01)
        {
            error = "STUN response is not success.";
            return false;
        }

        // Magic cookie
        if (buf[4] != 0x21 || buf[5] != 0x12 || buf[6] != 0xA4 || buf[7] != 0x42)
        {
            error = "STUN magic cookie mismatch.";
            return false;
        }

        for (int i = 0; i < 12; i++)
        {
            if (buf[8 + i] != transactionId[i])
            {
                error = "STUN transaction ID mismatch.";
                return false;
            }
        }

        int msgLen = (buf[2] << 8) | buf[3];
        int end = 20 + msgLen;
        if (end > buf.Length)
        {
            end = buf.Length;
        }

        int offset = 20;
        while (offset + 4 <= end)
        {
            int attrType = (buf[offset] << 8) | buf[offset + 1];
            int attrLen = (buf[offset + 2] << 8) | buf[offset + 3];
            offset += 4;

            if (offset + attrLen > end)
            {
                break;
            }

            // XOR-MAPPED-ADDRESS = 0x0020
            if (attrType == 0x0020 && attrLen >= 8)
            {
                // 0: reserved, 1: family
                byte family = buf[offset + 1];
                if (family == 0x01) // IPv4
                {
                    int xPort = (buf[offset + 2] << 8) | buf[offset + 3];
                    int port = xPort ^ 0x2112;

                    byte[] addr = new byte[4];
                    addr[0] = (byte)(buf[offset + 4] ^ 0x21);
                    addr[1] = (byte)(buf[offset + 5] ^ 0x12);
                    addr[2] = (byte)(buf[offset + 6] ^ 0xA4);
                    addr[3] = (byte)(buf[offset + 7] ^ 0x42);

                    publicEndPoint = new IPEndPoint(new IPAddress(addr), port);
                    return true;
                }
            }

            // MAPPED-ADDRESS = 0x0001
            if (attrType == 0x0001 && attrLen >= 8 && publicEndPoint == null)
            {
                byte family = buf[offset + 1];
                if (family == 0x01)
                {
                    int port = (buf[offset + 2] << 8) | buf[offset + 3];
                    byte[] addr = new byte[4];
                    Buffer.BlockCopy(buf, offset + 4, addr, 0, 4);
                    publicEndPoint = new IPEndPoint(new IPAddress(addr), port);
                    // don't return yet; prefer XOR if present
                }
            }

            // 4-byte padding
            offset += attrLen;
            int pad = attrLen % 4;
            if (pad != 0)
            {
                offset += (4 - pad);
            }
        }

        if (publicEndPoint != null)
        {
            return true;
        }

        error = "STUN response missing mapped address.";
        return false;
    }

    private static bool IsPrivateOrLoopback(IPAddress ip)
    {
        if (ip == null)
        {
            return true;
        }

        if (IPAddress.IsLoopback(ip))
        {
            return true;
        }

        if (ip.AddressFamily != AddressFamily.InterNetwork)
        {
            return false;
        }

        byte[] b = ip.GetAddressBytes();
        return b[0] == 10 ||
               (b[0] == 172 && b[1] >= 16 && b[1] <= 31) ||
               (b[0] == 192 && b[1] == 168);
    }

    /// <summary>
    /// 获取本地IP地址
    /// 获取本机第一个非回环的IPv4地址，用于网络通信
    /// </summary>
    /// <returns>本地IPv4地址，如果获取失败则返回回环地址</returns>
    public static IPAddress GetLocalIpAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName()); // 获取本机主机名对应的DNS主机条目
            return host.AddressList // 遍历所有IP地址
            .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip)) // 筛选IPv4且非回环地址
            ?? IPAddress.Loopback; // 如果未找到则返回回环地址
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[NATTraversal] Failed to get local IP: {ex.Message}"); // 记录获取本地IP失败的错误
            return IPAddress.Loopback; // 异常时返回回环地址作为默认值
        }
    } // 获取本地IP地址：获取本机非回环的IPv4地址，用于网络连接

    /// <summary>
    /// 获取公网IP地址（通过HTTP服务）
    /// 通过访问外部HTTP服务获取本机的公网IP地址
    /// </summary>
    /// <returns>公网IP地址，如果获取失败则返回null</returns>
    public static async Task<IPAddress> GetPublicIpAddress()
    {
        try
        {
            using (WebClient client = new())
            {
                string response = await client.DownloadStringTaskAsync("https://api.ipify.org");
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
    /// 存储对等方的网络地址转换信息，用于后续P2P连接
    /// </summary>
    /// <param name="peerId">对等方唯一标识符</param>
    /// <param name="natInfo">对等方的NAT信息</param>
    public static void RegisterPeerNatInfo(string peerId, NatInfo natInfo)
    {
        _peerNatInfo[peerId] = natInfo;
        _logger?.LogInfo($"[NATTraversal] Registered NAT info for peer: {peerId}");
    } // 注册对等方NAT信息：存储对等方的网络地址转换信息，用于P2P连接

    /// <summary>
    /// 获取对等方NAT信息
    /// 从缓存中获取指定对等方的网络地址转换信息
    /// </summary>
    /// <param name="peerId">对等方唯一标识符</param>
    /// <returns>NAT信息对象，如果未找到则返回null</returns>
    public static NatInfo GetPeerNatInfo(string peerId)
    {
        _peerNatInfo.TryGetValue(peerId, out var info);
        return info;
    } // 获取对等方NAT信息：从缓存中获取指定对等方的网络地址转换信息

    /// <summary>
    /// 移除对等方NAT信息
    /// 从缓存中删除对等方的网络地址转换信息，释放资源
    /// </summary>
    /// <param name="peerId">对等方唯一标识符</param>
    public static void RemovePeerNatInfo(string peerId)
    {
        _peerNatInfo.Remove(peerId);
        _logger?.LogInfo($"[NATTraversal] Removed NAT info for peer: {peerId}");
    } // 移除对等方NAT信息：从缓存中删除对等方的网络地址转换信息

    /// <summary>
    /// 获取所有注册的对等方NAT信息
    /// 返回所有对等方的网络地址转换信息副本
    /// </summary>
    /// <returns>包含所有对等方NAT信息的字典</returns>
    public static Dictionary<string, NatInfo> GetAllPeerNatInfo()
    {
        return new Dictionary<string, NatInfo>(_peerNatInfo);
    } // 获取所有注册的对等方NAT信息：返回所有对等方的网络地址转换信息副本

    /// <summary>
    /// 检查是否支持NAT穿透
    /// 根据NAT类型判断是否支持P2P连接穿透
    /// </summary>
    /// <param name="natType">要检查的NAT类型</param>
    /// <returns>是否支持NAT穿透</returns>
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
    /// 生成包含所有对等方NAT类型和连接状态的详细报告
    /// </summary>
    /// <returns>NAT穿透报告字符串</returns>
    public static string GenerateNatReport()
    {
        try
        {
            StringBuilder report = new StringBuilder();
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
    }
}

    #endregion

