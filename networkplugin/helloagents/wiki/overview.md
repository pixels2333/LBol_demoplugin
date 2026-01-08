# NetworkPlugin（LBoL 联机 MOD）

> 本仓库是一个 BepInEx 插件（目标进程 `LBoL.exe`），通过 Harmony Patch 将关键游戏流程“事件化”，并使用 LiteNetLib 在玩家之间同步，目标是实现可玩的多人联机体验。

## 1. 项目概述

### 目标与背景
- 目标：为 LBoL 提供多人联机能力，解决“各自本地随机/状态不同步”导致无法协同游玩的根本问题。
- 核心手段：HarmonyX 拦截游戏逻辑 → 生成网络事件/快照 → LiteNetLib 传输 → 远端回放/应用。

### 当前范围（按代码现状）
- 联机基础：客户端/服务器/中继（RelayServer）与房间（Room）管理；玩家实体（PlayerEntity）状态追踪。
- 同步核心：`ISynchronizationManager`（事件队列、远端事件缓冲、状态缓存、全量同步请求）。
- 游戏同步：卡牌、法力/能量、回合、战斗、地图、商店/遗物/事件等（以 `Patch/` 为主）。
- 辅助能力：断线重连（Reconnection）、中途加入（MidGameJoin）、聊天（Chat + UI）。
- UI：连接状态指示器、聊天窗口、交易/复活面板等。

### 非目标 / 未覆盖
- 通用联机大厅与账号体系、反作弊、跨版本兼容（目前不是核心目标）。
- NAT 穿透/UPnP 完整可用性：存在相关工具与 UI 字段，但实现与适配仍在演进。

### 已知限制（重要）
- 当前工程存在若干既有编译错误（例如重复类型/缺失类型/可访问性不一致等），会导致 `dotnet build` 失败；需要先修复这些问题再做完整构建验证。

## 2. 目录结构（代码）
- `Plugin.cs`：BepInEx 入口、DI 容器初始化、`harmony.PatchAll()` 装载补丁。
- `Configuration/`：配置项（功能开关、网络参数、性能参数、平衡参数）。
- `Core/`：同步管理（`ISynchronizationManager` / `SynchronizationManager`）。
- `Network/`：网络层（Client/Server/Relay/Room/Player/Snapshot/MidGameJoin/Reconnection）。
- `Patch/`：Harmony 补丁集合（按 Actions/Network/UI/Map/EnemyUnits 等拆分）。
- `UI/`：联机 UI 组件与面板。
- `Utils/`：游戏/网络辅助工具（读取当前战斗、法力转换、缓冲等）。

## 3. 模块索引（SSOT）

| 模块 | 职责 | 文档 |
|------|------|------|
| Core | 同步管理与统计、事件入队/出队、远端事件处理 | `modules/core.md` |
| Network | 客户端/服务端/中继/房间、玩家实体、快照、重连、中途加入 | `modules/network.md` |
| Patch | Harmony Patch 同步点（把游戏行为转为网络事件/回放） | `modules/patch.md` |
| Configuration | 配置项与开关（性能/网络/平衡/同步策略） | `modules/configuration.md` |
| Authority | 房主权威与请求校验（HostAuthorityManager） | `modules/authority.md` |
| UI | 聊天、连接状态、交易/复活等 UI | `modules/ui.md` |
| Utils | 游戏状态/法力/卡牌/单位等工具与缓冲 | `modules/utils.md` |
| Chat | 控制台聊天与聊天消息模型 | `modules/chat.md` |
| Commands | 命令系统接口与属性（可用于扩展控制台/调试） | `modules/commands.md` |

## 4. 快速链接
- 技术约定：`../project.md`
- 架构设计：`arch.md`
- API 手册：`api.md`
- 数据模型：`data.md`
- 变更历史：`../history/index.md`

## 5. 参考资料（非 SSOT）
- 开发规划：`../../PLANNING_ROADMAP.md`
- 功能总结：`../../完善功能总结.md`
- LiteNetLib 集成指南：`../../LiteNetLib集成指南.md`
