# 项目上下文

## 1. 基本信息

```yaml
名称: LBol_demoplugin
描述: LBoL 联机/同步 Mod（Harmony Patch + LiteNetLib）
类型: LBoL Mod
状态: 开发中
```

## 2. 技术上下文

```yaml
语言: C#
框架: Harmony + LiteNetLib
包管理器: .NET SDK
构建工具: dotnet build
```

### 主要依赖
| 依赖 | 版本 | 用途 |
|------|------|------|
| Harmony | (repo) | Patch 框架 |
| LiteNetLib | (repo) | 网络传输 |

## 3. 项目概述

### 核心功能
- 联机同步：网络消息/事件通道（GameEvent）
- Harmony 补丁：战斗/事件/交易等关键交互同步

### 项目边界
```yaml
范围内:
  - 让 LBoL 支持多人联机并同步关键状态
范围外:
  - 不承诺兼容所有第三方 Mod；不做云存档/账号体系
```

## 4. 开发约定

### 代码规范
```yaml
命名风格: 与现有代码一致
文件命名: 与现有目录结构一致
目录组织: 与现有目录结构一致
```

### 错误处理
```yaml
错误码格式: 以日志为主（必要时 JSON 结构）
日志级别: Info/Warn/Error
```

### 测试要求
```yaml
测试框架: (未统一)；可用构建验证代替
覆盖率要求: (未设定)
测试文件位置: (无固定路径)
```

### Git规范
```yaml
分支策略: (未约束)
提交格式: (未约束)
```

## 5. 当前约束（源自历史决策）

> 这些是当前生效的技术约束，详细决策过程见对应方案包

| 约束 | 原因 | 决策来源 |
|------|------|---------|
| 约束 | 原因 | 决策来源 |
|------|------|----------|
| 远端队友目标出牌：目标端结算 + 快照广播 | 动画一致且避免重复结算 | archive/2026-01/202601090851_remote_target_card/proposal.md#D001 |
| ServerCore 双模式（Host/Relay） | 复用核心并统一消息链路 | archive/2026-01/202601091556_unify_server_core_two_modes/proposal.md#D001 |


## 6. 已知技术债务（可选）

> 主动识别的需要未来处理的技术问题

| 债务描述 | 优先级 | 来源 | 建议处理时机 |
|---------|--------|------|-------------|
| 无 | P0/P1/P2 | 无 | 无 |

---

## 附录A：旧版 project.md（原文保留）

# 项目技术约定

## 技术栈
- 语言: C#
- 形态: LBoL Mod / Harmony Patch / LiteNetLib 网络

## 开发约定
- 代码风格: 以项目现有风格为准，优先最小改动。
- Patch 约定: HarmonyPatch 类尽量保持单一职责，避免跨模块耦合。
- 网络事件: 明确事件来源与接收端职责（谁发送/谁结算/谁广播）。

## 测试与验证
- `networkplugin/NetWorkPlugin.csproj` 可通过 `dotnet build` 验证（当前仍有既有 warnings）。
- 全仓库/解决方案的完整构建可能受外部依赖与工程配置影响；涉及运行时行为的改动仍建议进行手动联机验证。


## 附录B：旧版 wiki/overview.md（原文保留）

# 项目概述

## 模块索引
- `networkplugin`: 联机/同步相关补丁与网络层实现。
- `lbol`: 游戏源代码（用于参考与编译依赖）。

## 关键入口
- Harmony 补丁目录: `networkplugin/Patch/`
- 网络消息类型: `networkplugin/Network/Messages/NetworkMessageTypes.cs`


## 附录C：旧版 wiki/arch.md（原文保留）

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
- `IServerCore`：网络核心抽象接口，定义底层事件与生命周期，便于未来替换/扩展传输层实现。
- `ServerCore`：`IServerCore` 的 LiteNetLib 实现，负责生命周期、连接鉴权、入站消息队列（优先级/限流）、心跳/超时清理等公共能力。
- `BaseGameServer`：业务骨架层，统一接入 core 事件并维护会话映射（连接/断开/延迟/收包分流），为 Host/Relay 复用通用流程。
- Host 模式：`networkplugin/Network/Server/NetworkServer.cs`（继承 `BaseGameServer`，保持原有直连语义与消息兼容）。
- Relay 模式：`networkplugin/Network/Server/RelayServer.cs`（继承 `BaseGameServer`，多房间/中继转发，并补齐 Host 依赖的系统消息）。
