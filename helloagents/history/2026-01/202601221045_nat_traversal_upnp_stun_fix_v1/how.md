# HOW - NatTraversal 实现方案

## 总体策略

- 连接场景：大部分为虚拟局域网（VPN/LAN）直连“房主 IP”，因此优先保证本地端点/固定端口与协议兼容。
- UPnP：按“不支持”为默认路径实现（失败即回退，不影响联机主流程）。
- STUN：作为可选增强；在 VPN/LAN 直连时并非必需。
- 序列化：采用自定义 `JsonConverter<IPEndPoint>`，并通过属性级标注，避免全局 `JsonSerializerOptions` 侵入。
- Token：改为带过期时间的 token（不做强安全签名也可），避免 ticks 等值比较。

## 方案细节

### 1) UPnP 端口映射

候选库：

- `Open.NAT`（常用，支持 UPnP IGD + NAT-PMP）。

设计：

- 在 `NatTraversal` 内封装一个 `UpnpClient`（或静态 lazy），实现：
  - `CheckUpnpAvailability()`：Discover IGD，返回 bool。
  - `EnableUpnpMapping(internalPort, externalPort, description)`：
    - Discover -> CreatePortMap(UDP)
    - 记录 `_upnpEnabled` 与映射信息（便于删除/报告）
  - `DisableUpnpMapping(port)`：
    - DeletePortMap(UDP)

回退：

- 发现失败/映射失败：返回 `Success=false`，写入 `ErrorMessage`，并保持 `_upnpEnabled=false`。

默认行为：

- 按用户反馈，主流程假设 UPnP 大概率不可用，因此：
  - `CheckUpnpAvailability()` 返回 false 时仅记录 Debug/Info，不作为错误；
  - UI/日志提示以“可选加速功能不可用”表述，避免误导。

### 2) STUN 探测

优先级：

- 无第三方库时，直接实现 STUN Binding（RFC 5389 的最小子集）。

最小实现点：

- 构造 Binding Request（Type=0x0001）
- 随机 Transaction ID（12 bytes）
- 通过 UDP 发送到 STUN server
- 解析响应：
  - 优先解析 `XOR-MAPPED-ADDRESS`
  - 兜底解析 `MAPPED-ADDRESS`

多服务器策略（现有 `DetectNatTypeMultiple()`）：

- 并发请求 2-3 个 STUN；
- 若返回公网端口在不同 server 下变化较大，标记 `NatType.Symmetric`（弱推断）。

说明：

- 在 VPN/LAN 直连房主 IP 的场景下，STUN 的价值主要是“排查/诊断”和“未来扩展”，不是刚需。

### 3) NatInfo 序列化/反序列化

问题：`System.Text.Json` 默认无法序列化 `IPEndPoint`。

选择实现（按用户偏好）：

- 自定义 `JsonConverter<IPEndPoint>`，字符串格式 `ip:port`。
- 通过属性级标注（例如 `[JsonConverter(typeof(IPEndPointJsonConverter))]`）避免要求全局 `JsonSerializerOptions`。

原因：

- LiteNetLib 的传输最终是字符串（JSON），端点用字符串承载最稳定。
- 服务端 `RelayServer` 直接 `JsonSerializer.Deserialize<NatTraversal.NatInfo>`，属性级 Converter 可以确保两端默认序列化路径一致。

### 4) Connection Token 机制

问题：当前 `ValidateConnectionToken` 使用 `parts[3] == DateTime.Now.Ticks.ToString()`，必定失败。

修复方向：

- token 内容：`peerId|ip|port|issuedAtTicks|nonce`（或 json）
- 校验：
  - Base64 解码成功
  - `peerId` 一致
  - `issuedAtTicks` 距离当前时间 <= TTL（例如 60s/5min）
  - `nonce` 可选（防重放）

安全增强（可选）：

- 使用 HMAC-SHA256 签名 token（需要密钥来源：配置/随机会话密钥），避免客户端伪造。

本次默认：

- 由于“熟人局域网联机”且风险可接受，优先实现 TTL 与格式校验；HMAC 作为后续增强点。

### 5) DetectLocalConnectivity 的异步问题

现状：方法同步返回，但内部 `Task.Run` 更新 `SupportsUPnP`，调用方立即读会看到默认值。

修复策略：

- 保持 API 不变：在返回前同步等待 `CheckUpnpAvailability()`（带超时）
  - 或新增 `DetectLocalConnectivityAsync`，原方法仅做同步局部端口绑定。

端口策略：

- 端口为固定且需要用户输入：
  - `DetectLocalConnectivity(listenPort)` 应以传入端口为准，绑定失败时给出明确错误；
  - 不建议在此处自动随机端口（会破坏“用户输入固定端口”的体验）。

## 验收点

- 能在 Unity/Mono 环境编译（netstandard2.1）。
- UPnP：有路由器支持时可成功映射端口；不支持时返回明确失败原因。
- STUN：可获得公网端点（至少 `PublicEndPoint` 字段可用）。
- NatInfo：可被 `RelayServer` 反序列化并缓存。
