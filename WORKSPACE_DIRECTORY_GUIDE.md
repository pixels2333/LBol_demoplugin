# 工作区目录说明

> 目的：快速说明当前 VS Code 多根工作区中，各主要目录的职责与用途。
> 范围：[LBol_demoplugin](file:///d:/programme/LBol_demoplugin) 与 [Together in Spire v6.4.20](file:///d:/programme/Together in Spire v6.4.20)。

## 1. 工作区根目录（多根）

### [LBol_demoplugin](file:///d:/programme/LBol_demoplugin)
以 C#/.NET 为主的联机插件开发主工程，包含插件源码、游戏反编译代码、依赖库、辅助调试工具和测试工程。

### [Together in Spire v6.4.20](file:///d:/programme/Together in Spire v6.4.20)
以 Java 为主的参考工程（Slay the Spire 联机相关模组代码与资源），用于对照联机架构与功能实现。

---

## 2. [LBol_demoplugin](file:///d:/programme/LBol_demoplugin) 目录说明

### 工程与配置
- [.git](file:///d:/programme/LBol_demoplugin/.git)：Git 版本控制数据。
- [.github](file:///d:/programme/LBol_demoplugin/.github)：仓库级自动化与协作配置（例如工作流、模板）。
- [.vscode](file:///d:/programme/LBol_demoplugin/.vscode)：VS Code 工作区/调试/任务相关配置。
- [simplesolution.sln](file:///d:/programme/LBol_demoplugin/simplesolution.sln)：Visual Studio 解决方案入口，聚合了整个工作区的所有项目：
  - [networkplugin](file:///d:/programme/LBol_demoplugin/networkplugin) (主插件)
  - [networkplugin/Tests](file:///d:/programme/LBol_demoplugin/networkplugin/Tests) (自动化单元测试)
  - [networkplugin/Benchmarks](file:///d:/programme/LBol_demoplugin/networkplugin/Benchmarks) (性能基准测试)
  - [debugtools](file:///d:/programme/LBol_demoplugin/debugtools) (独立控制台调试器)
  - [UiObjectQueryPlugin](file:///d:/programme/LBol_demoplugin/UiObjectQueryPlugin) (游戏内 UI 调试 HTTP 服务)
  - [AiSimClient](file:///d:/programme/LBol_demoplugin/AiSimClient) (独立 Avalonia AI 模拟客户端)
  - `lbol` 下的 5 个反编译镜像分层项目

### 业务与工具子项目
- [networkplugin](file:///d:/programme/LBol_demoplugin/networkplugin)：联机 MOD 的核心代码目录（当前主开发区）。
  - [Chat](file:///d:/programme/LBol_demoplugin/networkplugin/Chat)：聊天消息与聊天控制台。
  - [Configuration](file:///d:/programme/LBol_demoplugin/networkplugin/Configuration)：联机、同步、性能等配置管理。
  - [Core](file:///d:/programme/LBol_demoplugin/networkplugin/Core)：同步管理核心接口与实现。包含 [EventBufferManager](file:///d:/programme/LBol_demoplugin/networkplugin/Core/EventBufferManager.cs) (事件缓冲)、[StateCacheManager](file:///d:/programme/LBol_demoplugin/networkplugin/Core/StateCacheManager.cs) (状态缓存)、[AvailabilityTracker](file:///d:/programme/LBol_demoplugin/networkplugin/Core/AvailabilityTracker.cs) (可用性追踪)。
  - [Network](file:///d:/programme/LBol_demoplugin/networkplugin/Network)：网络通信主逻辑（客户端/服务端/路由/广播/协议/重连/中途加入/快照等）。
    - [Client](file:///d:/programme/LBol_demoplugin/networkplugin/Network/Client)：客户端连接与重连管理。
    - [Server](file:///d:/programme/LBol_demoplugin/networkplugin/Network/Server)：服务端及广播路由。其核心 [NetworkServer](file:///d:/programme/LBol_demoplugin/networkplugin/Network/Server/NetworkServer.cs) 已拆分为主文件 + `Routing` (路由) + `Broadcast` (广播)，以及 [RelayServer](file:///d:/programme/LBol_demoplugin/networkplugin/Network/Server/RelayServer.cs) (转发服务器)。
    - [Messages](file:///d:/programme/LBol_demoplugin/networkplugin/Network/Messages)：自定义通信协议网络包定义。
    - 其他子目录包括 `Room/` (房间)、`RoomSync/` (房间状态同步)、`Snapshot/` (网络快照)、`Sync/` (变量同步 `SyncVar`)。
  - [Patch](file:///d:/programme/LBol_demoplugin/networkplugin/Patch)：对游戏行为进行 Harmony 拦截与打补丁的同步逻辑。
    - 按游戏机制划分子目录，如 `Actions/` (卡牌与动作控制)、`EnemyUnits/` (敌方单位控制，含 [RemotePlayerProxyEnemy](file:///d:/programme/LBol_demoplugin/networkplugin/Patch/EnemyUnits/RemotePlayerProxyEnemy.cs))、`Map/` (地图同步)、`UI/` (界面行为同步) 等。
  - [UI](file:///d:/programme/LBol_demoplugin/networkplugin/UI)：联机功能界面组件、对话框、面板、控件。
    - [Components](file:///d:/programme/LBol_demoplugin/networkplugin/UI/Components)：公共 UI 组件，含 [OtherPlayersOverlay](file:///d:/programme/LBol_demoplugin/networkplugin/UI/Components/OtherPlayersOverlay) (其他玩家地图/状态覆盖层逻辑与视图注册表)。
    - [Panels](file:///d:/programme/LBol_demoplugin/networkplugin/UI/Panels)：主面板，包含已物理拆分为 6 个文件的 [TradePanel](file:///d:/programme/LBol_demoplugin/networkplugin/UI/Panels/TradePanel.cs)。
    - [Dialogs](file:///d:/programme/LBol_demoplugin/networkplugin/UI/Dialogs)：明细对话框，包含已物理拆分为 7 个文件的 [TradeDetailDialog](file:///d:/programme/LBol_demoplugin/networkplugin/UI/Dialogs/TradeDetailDialog.cs)。
    - [Prototypes](file:///d:/programme/LBol_demoplugin/networkplugin/UI/Prototypes)：HTML 交互原型页面，用于前端 UI 设计预览。
  - [Utils](file:///d:/programme/LBol_demoplugin/networkplugin/Utils)：卡牌、法力、状态、日志、事件缓冲等辅助工具。
  - [Tests](file:///d:/programme/LBol_demoplugin/networkplugin/Tests)：自动化单元测试工程（50+ 个基于 xUnit, Moq, FluentAssertions 的测试用例）。
  - [Benchmarks](file:///d:/programme/LBol_demoplugin/networkplugin/Benchmarks)：基于 BenchmarkDotNet 的联机性能测试工程。

- [debugtools](file:///d:/programme/LBol_demoplugin/debugtools)：独立调试控制台工具项目（用于文件存在性检查、运行状态验证、协议模拟等）。
- [UiObjectQueryPlugin](file:///d:/programme/LBol_demoplugin/UiObjectQueryPlugin)：独立 Unity 运行时 HTTP 调试插件。在游戏内开启 HTTP 服务，方便外部工具（如场景查询器）查询层级中 GameObject 的 Transform 信息。
- [AiSimClient](file:///d:/programme/LBol_demoplugin/AiSimClient)：基于 Avalonia UI 的独立 AI 模拟客户端。用于模拟大量虚拟客户端向服务端发起连接、执行并发操作以进行压力测试与行为模拟。
- [MyFirstPlugin](file:///d:/programme/LBol_demoplugin/MyFirstPlugin)：示例/实验插件工程。
- [lbol](file:///d:/programme/LBol_demoplugin/lbol)：LBoL 游戏代码镜像（5个分层项目，用于查找类型与事件对接点）。
- [lib](file:///d:/programme/LBol_demoplugin/lib)：外部依赖 DLL（Unity/LBoL 游戏本体/第三方库），供插件编译与运行时引用。

### 交付、文档与规划
- [handoffs](file:///d:/programme/LBol_demoplugin/handoffs)：交付与修正记录目录，保存历次开发过程的 Hand-off 文档（如 [handoff-local-player-avatar.md](file:///d:/programme/LBol_demoplugin/handoffs/handoff-local-player-avatar.md)），详细记录修复的 Bug、测试用例与设计决策。
- [plan](file:///d:/programme/LBol_demoplugin/plan)：项目方案包目录，用于跟踪重大功能计划与实施进展。
  - [20260519_networkplugin_code_quality_improvement_plan.md](file:///d:/programme/LBol_demoplugin/plan/20260519_networkplugin_code_quality_improvement_plan.md)：代码质量改进计划（评估 76/100）。
  - [DIRECTORY_REFACTOR_PLAN.md](file:///d:/programme/LBol_demoplugin/plan/DIRECTORY_REFACTOR_PLAN.md)：目录重构与命名空间对齐计划。
  - [MULTIPLAYER_REGRESSION_CHECKLIST.md](file:///d:/programme/LBol_demoplugin/plan/MULTIPLAYER_REGRESSION_CHECKLIST.md)：多人回归测试清单。
  - [NETWORK_ROUTE_REGRESSION_CHECKLIST.md](file:///d:/programme/LBol_demoplugin/plan/NETWORK_ROUTE_REGRESSION_CHECKLIST.md)：路由同步（FullStateSync/RoomState）回归清单。
- [PLANNING_ROADMAP.md](file:///d:/programme/LBol_demoplugin/networkplugin/PLANNING_ROADMAP.md)：v2.7 功能规划与开发路线图。
- 根目录下其他参考文档：[lbol-complete-project-doc.md](file:///d:/programme/LBol_demoplugin/lbol-complete-project-doc.md)、[lbol-methods-doc.md](file:///d:/programme/LBol_demoplugin/lbol-methods-doc.md)、[lbol-noncore-methods-doc.md](file:///d:/programme/LBol_demoplugin/lbol-noncore-methods-doc.md)。
- 调试与脚本：[chat.json](file:///d:/programme/LBol_demoplugin/chat.json)、[createproject.ps1](file:///d:/programme/LBol_demoplugin/createproject.ps1)、[copy_networkplugin_dll.ps1](file:///d:/programme/LBol_demoplugin/copy_networkplugin_dll.ps1)。

### 常见构建产物目录
- `bin/`、`obj/`（分布在各项目内）：编译输出与中间产物目录。

---

## 3. [Together in Spire v6.4.20](file:///d:/programme/Together in Spire v6.4.20) 目录说明

### 核心代码
- [spireTogether](file:///d:/programme/Together in Spire v6.4.20/spireTogether)：联机主模组核心代码（网络、战斗、地图、事件、UI、存档、补丁等）。
- [dLib](file:///d:/programme/Together in Spire v6.4.20/dLib)：通用库层（命令、补丁、UI 元素、工具类、兼容逻辑等基础能力）。
- [skindex](file:///d:/programme/Together in Spire v6.4.20/skindex)：皮肤/外观相关子系统（实体皮肤、注册、保存、解锁、兼容适配等）。

### 资源与元数据
- [spireTogetherResources](file:///d:/programme/Together in Spire v6.4.20/spireTogetherResources)：联机模组资源（图片、本地化、着色器）。
- [dLibResources](file:///d:/programme/Together in Spire v6.4.20/dLibResources)：dLib 资源（主要是 UI 图片）。
- [skindexResources](file:///d:/programme/Together in Spire v6.4.20/skindexResources)：skindex 资源（音频、图片、本地化）。
- `META-INF/`：JAR 元信息与 Maven 元数据（`MANIFEST.MF` 等）。
- [ModTheSpire.json](file:///d:/programme/Together in Spire v6.4.20/ModTheSpire.json)：模组加载入口配置。
- [IDCheckStringsDONT-EDIT-AT-ALL.json](file:///d:/programme/Together in Spire v6.4.20/IDCheckStringsDONT-EDIT-AT-ALL.json)：特定运行校验字符串配置文件（不应手改）。

---

## 4. 快速定位建议

- 想改联机行为：优先看 [networkplugin/Network/](file:///d:/programme/LBol_demoplugin/networkplugin/Network) + [networkplugin/Patch/](file:///d:/programme/LBol_demoplugin/networkplugin/Patch)。
- 想改联机界面：优先看 [networkplugin/UI/](file:///d:/programme/LBol_demoplugin/networkplugin/UI)。
- 想确认游戏底层类型/事件：优先看 [lbol/LBoL.Core/](file:///d:/programme/LBol_demoplugin/lbol/LBoL.Core) 与 [lbol/LBoL.Base/](file:///d:/programme/LBol_demoplugin/lbol/LBoL.Base)。
- 想参考成熟联机实现：优先看 [Together in Spire v6.4.20/spireTogether/](file:///d:/programme/Together in Spire v6.4.20/spireTogether) 与 [Together in Spire v6.4.20/dLib/](file:///d:/programme/Together in Spire v6.4.20/dLib)。
