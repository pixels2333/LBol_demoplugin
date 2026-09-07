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

public partial class NatTraversal
{
    private const int DefaultTokenTtlSeconds = 300;
    private const int DefaultUpnpProbeTimeoutMs = 500;
    private const int DefaultStunTimeoutMs = 1500;

        private static readonly ManualLogSource _logger;

        private static readonly object _syncLock = new();

        private static readonly Dictionary<string, NatInfo> _peerNatInfo = [];

        private static volatile bool _upnpEnabled = false;

        private static volatile string _upnpState = "DisabledByConfig";

        private static volatile NatType _lastDetectedNatType = NatType.Unknown;

    public static string UpnpState => _upnpState;
    public static NatType LastDetectedNatType => _lastDetectedNatType;

    public static string GetStatusSummary()
        => $"NAT: {LastDetectedNatType} | UPnP: {UpnpState}";

    public static string GetConnectionStrategySummary()
    {
        return UpnpState switch
        {
            "DisabledByConfig" => "连接策略: STUN 检测 + UPnP 语义展示（配置关闭真实映射）",
            "UnsupportedOrUnavailable" => "连接策略: STUN 检测 + UPnP 语义展示（当前环境不可用）",
            "AvailableButNotImplemented" => "连接策略: STUN 检测 + UPnP 语义展示（检测到可用但未接入真实映射）",
            "Enabled" => "连接策略: STUN 检测 + UPnP 真实映射",
            _ => "连接策略: STUN 检测 + UPnP 状态待确认",
        };
    }

        static NatTraversal()
    {

        _logger = Plugin.Logger ?? BepInEx.Logging.Logger.CreateLogSource("NATTraversal");
    }

    #region NAT类型定义

        public enum NatType
    {
                Unknown,

                OpenInternet,

                FullCone,

                RestrictedCone,

                PortRestrictedCone,

                Symmetric,

                Blocked,

                DoubleNat,

                Hairpin,
    }

        public class NatInfo
    {
                public string PeerId { get; set; }

                public NatType NatType { get; set; }

                [JsonConverter(typeof(IPEndPointJsonConverter))]
        public IPEndPoint PublicEndPoint { get; set; }

                [JsonConverter(typeof(IPEndPointJsonConverter))]
        public IPEndPoint LocalEndPoint { get; set; }

                public bool SupportsHolePunching { get; set; }

                public bool SupportsUPnP { get; set; }

                public DateTime LastUpdate { get; set; }

                public List<string> StunServers { get; set; } = [];
    }

        public class UpnpMappingResult
    {
                public bool Success { get; set; }

                public int ExternalPort { get; set; }

                public int InternalPort { get; set; }

                public string Protocol { get; set; }

                public string Description { get; set; }

                public string ErrorMessage { get; set; }
    }

        public class StunResponse
    {
                public NatType DetectedNatType { get; set; }

                [JsonConverter(typeof(IPEndPointJsonConverter))]
        public IPEndPoint PublicEndPoint { get; set; }

                public bool SupportsHairpinning { get; set; }

                public string StunServer { get; set; }

                public bool Success { get; set; }

                public string ErrorMessage { get; set; }
    }
    #endregion

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

        public static async Task<UpnpMappingResult> EnableUpnpMapping(int internalPort, int externalPort = 0, string description = "LBoL_Multiplayer")
    {
        try
        {
            if (Plugin.ConfigManager?.EnableUpnpExperimental?.Value != true)
            {
                _upnpEnabled = false;
                _upnpState = "DisabledByConfig";
                _logger?.LogInfo("[NATTraversal][UPnP] Skipped: disabled by config (EnableUpnpExperimental=false).");
                return new UpnpMappingResult
                {
                    Success = false,
                    ErrorMessage = "UPnP disabled by config (EnableUpnpExperimental=false).",
                    InternalPort = internalPort,
                    ExternalPort = externalPort == 0 ? internalPort : externalPort,
                    Protocol = "UDP",
                    Description = description
                };
            }

            if (externalPort == 0)
            {
                externalPort = internalPort;
            }

            bool available = await CheckUpnpAvailability();
            if (!available)
            {
                _upnpEnabled = false;
                _upnpState = "UnsupportedOrUnavailable";
                _logger?.LogInfo("[NATTraversal][UPnP] Unavailable in current environment.");
                return new UpnpMappingResult
                {
                    Success = false,
                    ErrorMessage = "UPnP unavailable in current environment.",
                    InternalPort = internalPort,
                    ExternalPort = externalPort,
                    Protocol = "UDP",
                    Description = description
                };
            }

            _upnpEnabled = false;
            _upnpState = "AvailableButNotImplemented";
            _logger?.LogInfo("[NATTraversal][UPnP] Available but mapping implementation is not enabled yet.");
            return new UpnpMappingResult
            {
                Success = false,
                ErrorMessage = "UPnP available, but mapping is not implemented.",
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
    }

        public static Task<bool> DisableUpnpMapping(int port)
    {
        try
        {

            _upnpEnabled = false;
            _logger?.LogInfo($"[NATTraversal] UPnP mapping disabled for port: {port}");
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[NATTraversal] Failed to disable UPnP mapping: {ex.Message}");
            return Task.FromResult(false);
        }
    }

        public static Task<bool> CheckUpnpAvailability()
    {
        try
        {
            if (Plugin.ConfigManager?.EnableUpnpExperimental?.Value != true)
            {
                _upnpState = "DisabledByConfig";
                return Task.FromResult(false);
            }

            _upnpState = "UnsupportedOrUnavailable";
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[NATTraversal] UPnP availability check failed: {ex.Message}");
            return Task.FromResult(false);
        }
    }

    #endregion

    #region STUN服务器交互

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

        public static async Task<StunResponse> DetectNatType(string stunServer = null)
    {
        try
        {
            if (Plugin.ConfigManager?.EnableNatDetection?.Value == false)
            {
                _lastDetectedNatType = NatType.Unknown;
                _logger?.LogInfo("[NATTraversal][STUN] Skipped: disabled by config (EnableNatDetection=false).");
                return new StunResponse
                {
                    Success = false,
                    ErrorMessage = "NAT detection disabled by config.",
                    DetectedNatType = NatType.Unknown,
                };
            }

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

            _lastDetectedNatType = result.DetectedNatType;
            _logger?.LogInfo($"[NATTraversal][STUN] NAT type detected: {result.DetectedNatType}, public={result.PublicEndPoint}");
            return result;
        }
        catch (Exception ex)
        {
            _lastDetectedNatType = NatType.Unknown;
            _logger?.LogError($"[NATTraversal][STUN] NAT type detection failed: {ex.Message}");
            return new StunResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                DetectedNatType = NatType.Unknown
            };
        }
    }

        public static async Task<List<StunResponse>> DetectNatTypeMultiple()
    {
        List<StunResponse> results = [];
        List<Task<StunResponse>> tasks = [];

        foreach (string server in DefaultStunServers.Take(3))
        {
            tasks.Add(DetectNatType(server));
        }

        var responses = await Task.WhenAll(tasks);
        results.AddRange(responses);

        _logger?.LogInfo($"[NATTraversal] NAT type detection completed: {results.Count} responses");
        return results;
    }

    #endregion

    #region 连接类型检测

        public static NatInfo DetectLocalConnectivity(int listenPort)
    {
        try
        {
            NatInfo info = new()
            {
                LocalEndPoint = new IPEndPoint(GetLocalIpAddress(), listenPort),
                LastUpdate = DateTime.UtcNow
            };

            using (Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Bind(info.LocalEndPoint);
                int localPort = ((IPEndPoint)socket.LocalEndPoint).Port;
                info.LocalEndPoint = new(info.LocalEndPoint.Address, localPort);
            }

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
    }

        public static async Task<bool> TestP2pConnectivity(IPEndPoint remoteEndPoint, int timeoutMs = 5000)
    {
        try
        {
            using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.ReceiveTimeout = timeoutMs;
            socket.Bind(new IPEndPoint(IPAddress.Any, 0));

            byte[] testData = Encoding.UTF8.GetBytes("P2P_TEST_" + DateTime.Now.Ticks);
            await socket.SendToAsync(new ArraySegment<byte>(testData), SocketFlags.None, remoteEndPoint);

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
    }

    #endregion

    #region NAT穿透辅助

        public static string GenerateConnectionToken(string peerId, IPEndPoint endPoint)
    {

        long issuedAt = DateTime.UtcNow.Ticks;
        byte[] nonceBytes = new byte[8];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(nonceBytes);
        }

        string nonce = Convert.ToBase64String(nonceBytes);
        string data = $"{peerId}|{endPoint.Address}|{endPoint.Port}|{issuedAt}|{nonce}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
    }

        public static bool ValidateConnectionToken(string token, out string peerId, out IPEndPoint endPoint)
    {
        try
        {
            string data = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            string[] parts = data.Split('|');

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
            _logger?.LogError($"[NATTraversal] Token validation failed: {ex.Message}");
            peerId = null;
            endPoint = null;
            return false;
        }
    }

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
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(transactionId);
        }

        byte[] buf = new byte[20];

        buf[0] = 0x00;
        buf[1] = 0x01;

        buf[2] = 0x00;
        buf[3] = 0x00;

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

        if (buf[0] != 0x01 || buf[1] != 0x01)
        {
            error = "STUN response is not success.";
            return false;
        }

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

            if (attrType == 0x0020 && attrLen >= 8)
            {

                byte family = buf[offset + 1];
                if (family == 0x01)
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

            if (attrType == 0x0001 && attrLen >= 8 && publicEndPoint == null)
            {
                byte family = buf[offset + 1];
                if (family == 0x01)
                {
                    int port = (buf[offset + 2] << 8) | buf[offset + 3];
                    byte[] addr = new byte[4];
                    Buffer.BlockCopy(buf, offset + 4, addr, 0, 4);
                    publicEndPoint = new IPEndPoint(new IPAddress(addr), port);

                }
            }

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
    }

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
    }

    #endregion

    #region 管理器方法

        public static void RegisterPeerNatInfo(string peerId, NatInfo natInfo)
    {
        lock (_syncLock)
        {
            _peerNatInfo[peerId] = natInfo;
        }
        _logger?.LogInfo($"[NATTraversal] Registered NAT info for peer: {peerId}");
    }

        public static NatInfo GetPeerNatInfo(string peerId)
    {
        lock (_syncLock)
        {
            _peerNatInfo.TryGetValue(peerId, out var info);
            return info;
        }
    }

        public static void RemovePeerNatInfo(string peerId)
    {
        lock (_syncLock)
        {
            _peerNatInfo.Remove(peerId);
        }
        _logger?.LogInfo($"[NATTraversal] Removed NAT info for peer: {peerId}");
    }

        public static Dictionary<string, NatInfo> GetAllPeerNatInfo()
    {
        lock (_syncLock)
        {
            return new Dictionary<string, NatInfo>(_peerNatInfo);
        }
    }

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
    }

        public static string GenerateNatReport()
    {
        try
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("=== NAT Traversal Report ===");
            report.AppendLine($"UPnP Enabled: {_upnpEnabled}");
            report.AppendLine($"UPnP State: {_upnpState}");
            report.AppendLine($"Last NAT Type: {_lastDetectedNatType}");
            report.AppendLine(GetConnectionStrategySummary());
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
