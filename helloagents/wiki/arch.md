# 架构设计

## 总体架构
- 客户端通过 `INetworkClient` 收发游戏事件（GameEvent）。
- 通过 HarmonyPatch 拦截游戏关键行为并发送同步事件。
- 部分“接收端结算”的能力通过“动作蓝图 + 目标端执行 + 结算后快照广播”实现。

## 重大架构决策
| adr_id | title | date | status | affected_modules | details |
|--------|-------|------|--------|------------------|---------|
| ADR-202601090851 | 远端目标出牌采用“目标端结算+快照广播” | 2026-01-09 | 已采纳 | networkplugin | `helloagents/plan/202601090851_remote_target_card/how.md` |
| ADR-202601091556 | 抽取 ServerCore + 双模式服务器（Host/Relay） | 2026-01-09 | 已采纳 | networkplugin | `helloagents/plan/202601091556_unify_server_core_two_modes/how.md` |

## 网络服务器架构（ServerCore + Host/Relay）
- `ServerCore` 负责 LiteNetLib 生命周期、连接鉴权、入站消息队列（优先级/限流）、心跳/超时清理等公共能力。
- Host 模式：`networkplugin/Network/Server/NetworkServer.cs`（保持原有直连语义与消息兼容）。
- Relay 模式：`networkplugin/Network/Server/RelayServer.cs`（多房间/中继转发，并补齐 Host 依赖的系统消息）。
