# AGENTS.md — 工作区指南

## 项目概要

LBoL（Library of Babel）游戏的 BepInEx 联机 MOD，使用 LiteNetLib 实现 UDP 网络通信。C#/.NET 项目。

- **目标框架**: `netstandard2.1`（插件）/ `net9.0`（测试/基准/调试工具）
- **SDK 版本**: .NET 9.0（无 `global.json`）
- **插件入口**: `networkplugin/Plugin.cs` — `BaseUnityPlugin` + HarmonyX + DI

## 构建与测试

```bash
# 构建主插件
dotnet build networkplugin/NetWorkPlugin.csproj -v minimal

# 运行所有测试（50 个）
dotnet test networkplugin/Tests/NetworkPlugin.Tests.csproj

# 基准测试
dotnet run --project networkplugin/Benchmarks/NetworkPlugin.Benchmarks.csproj -c Release

# 部署到游戏（可选）
powershell -ExecutionPolicy Bypass -File copy_networkplugin_dll.ps1
```

## 项目结构

| 项目 | 框架 | 说明 |
|------|------|------|
| `networkplugin/` | netstandard2.1 | **主开发区** ~189 文件, ~60k 行 |
| `networkplugin/Tests/` | net9.0 | 50 个 xUnit 测试 (Moq, FluentAssertions) |
| `networkplugin/Benchmarks/` | net9.0 | BenchmarkDotNet |
| `MyFirstPlugin/` | netstandard2.1 | 示例插件 |
| `UiObjectQueryPlugin/` | netstandard2.1 | HTTP 调试 UI 工具 |
| `debugtools/` | net9.0 | 调试控制台工具 |
| `lbol/` | — | LBoL 游戏源码镜像（5 个子项目） |

## 架构要点

- **DI 容器**: `Microsoft.Extensions.DependencyInjection 9.0.9`，在 `Plugin.cs` 中注册为单例
- **网络**: LiteNetLib 0.8.3.1 UDP 库
- **补丁**: HarmonyX 2.14.0 运行时打补丁，`Plugin.ApplyHarmonyPatchesSafely` 逐个安全应用
- **同步引擎**: `SynchronizationManager` → `EventBufferManager` + `StateCacheManager` + `AvailabilityTracker`
- **三大类已物理拆分**:
  - `TradePanel` → 6 文件（主文件 873 行）
  - `TradeDetailDialog` → 7 文件（主文件 799 行）
  - `NetworkServer` → 3 文件（主文件 548 行）

## 关键目录

- `networkplugin/Core/` — 同步核心
- `networkplugin/Network/Server/` — 服务端 + 路由 + 广播
- `networkplugin/Network/Client/` — 客户端连接与重连
- `networkplugin/Patch/` — Harmony 补丁（占总代码量 46%）
- `networkplugin/UI/Panels/` — 交易面板（6 文件）
- `networkplugin/UI/Dialogs/` — 交易对话框（7 文件）

## 常用文档链接

- [代码质量改进计划](../plan/20260519_networkplugin_code_quality_improvement_plan.md) — 评估 76/100
- [规划路线图](../networkplugin/PLANNING_ROADMAP.md) — v2.7 功能概览
- [多人回归清单](../plan/MULTIPLAYER_REGRESSION_CHECKLIST.md) — 回归测试基线
- [路由回归清单](../plan/NETWORK_ROUTE_REGRESSION_CHECKLIST.md) — FullStateSync/RoomState
- [目录重构计划](../plan/DIRECTORY_REFACTOR_PLAN.md) — 命名空间对齐计划
- [工作区目录指南](../WORKSPACE_DIRECTORY_GUIDE.md) — 完整目录职责说明

## 常见注意事项

- **空 catch**: 项目中 16 处空 catch 已全部修复，新增代码请避免引入新的空 `catch { }`。
- **Harmony Reverse Patch**: `RemoteCardUsePatch.Card_GetActions_Original` 是 reverse patch 桩，误入会抛 `NotImplementedException`。
- **UI 线程**: Harmony 补丁可能运行在任意线程，UI 更新必须通过 `Plugin.RunOnMainThread`。
- **Partial 文件**: TradePanel/TradeDetailDialog/NetworkServer 已拆为多文件，新增方法请加到对应分片文件，不要集中到主文件。
- **魔数**: 已有 5 个常量文件（`NetworkConstants`, `SyncConstants`, `UIConstants`, `LogConstants`, `ServerConstants`），不要硬编码字面量。
- **配置文件漂移**: 同名配置项只在 `ConfigManager` 中绑定一次。
- **Harmony Patch 与 DI**: Harmony 通过反射实例化 Patch 类，**无法使用构造函数注入**。因此 Patch 类必须通过 `ModService.ServiceProvider?.GetService<T>()` 静态获取 DI 服务。这是 Harmony 机制决定的必要模式，不是反模式。非 Patch 类（普通服务/管理器/工具类）仍应使用构造函数注入，不要模仿 Patch 类的写法。


### 过程中的错误与修正

| # | 错误 | 表现 | 修正 | 教训 |
|---|------|------|------|------|
| 1 | `UpdateMapIcons` 过滤条件排除本地玩家 | 本地玩家不出现在地图上 | 移除此过滤条件 | 新增功能时必须审查所有过滤条件 |
| 2 | `GetFallbackCharacterId()` 优先级反了且未处理空字符串 | `Id` 可能为 `""`，`??` 不触发回退，导致加载失败 | 优先 `ModelName`，用 `IsNullOrWhiteSpace` 检查 | C# `??` 不处理空字符串；Unity 字段可返回 `""` |
| 3 | 网络事件覆盖本地玩家的 `CharacterId` | 网络消息到达后覆盖 `EnsureSelfPlayer` 设置的值 | 赋值前保护：空 charId 时用 `GetFallbackCharacterId()` | 网络同步和本地状态存在竞争，所有写入点都要保护 |
| 4 | 无本地玩家缓存生命周期管理 | `_selfPlayerId` 变化后旧条目残留，重复或错位 | 新增 `EnsureSelfPlayer_NoThrow()` + 旧条目清理 | 动态 ID 需主动清理旧缓存 |
| 5 | Addressables 返回 1x1 占位符未过滤 | 显示为极小色块 | 增加 `width <= 1 && height <= 1` 过滤 | Addressables "加载成功"≠"资源有效" |
| 6 | `Image.color = Color.red` 诊断残留 | 本地玩家头像显示为红色方块 | 改回 `Color.white` | 诊断修改验证后必须及时清理 |
| 7 | 本地玩家图标尺寸差异化 `(240, 320)` | 本地图标比别人大一倍，重叠 | 移除尺寸覆盖，统一尺寸 | 差异化尺寸需配套调整间距 |
| 8 | `horizontalSpacing` 小于 root 宽度 | 同一节点多个头像重叠 | 最终 `rootSize=(100,140)`，`spacing=130` | 间距必须大于元素宽度 |
| 9 | 头像在 root 中锚定方式不一致 | 不同比例 sprite 视觉高低不一 | Avatar 改为居中锚定 `(0.5,0.5)` | UI 对齐应以边界框中心为基准 |
| 10 | `_selfPlayerId` 重置时机太晚 | `EnsureSelfPlayer` 使用旧 ID，导致找不到本地玩家 | 将 ID 同步移到注入函数之前 | 状态字段必须先同步再使用 |
| 11 | 本地玩家位置兜底链路太长 | 兜底函数依赖可能失败的中间查找 | fallback 直接使用 `CurrentMap.VisitingNode` | 兜底逻辑应该最直接、最少依赖 |
| 13 | 居中锚定后 `sizeDelta.x = 0` | 头像 Image 宽度为 0，完全不可见 | 改为 `sizeDelta = (100, 100)` | 居中锚定下两轴 sizeDelta 都必须非零 |
| 14 | 诊断代码未清理 | `GetRedSprite()`、详细位置日志等残留较多 | 已全部清理，高频 Log 移出，保留关键的 LogError 异常捕获 | 功能验证稳定后应及时清理调试日志，避免日志噪音 |
| 15 | 地图头像无圆形遮罩与边框 | 头像显示为普通矩形框，没有与战斗界面保持圆角和精美边框的一致性 | 引入 `AvatarMask` (使用圆形遮罩) + `Border` 边框，本地玩家金色边框，远程玩家白色边框 | 差异化 UI 需要统一考虑层级与风格对齐 |

> 完整过程记录见 [handoffs/handoff-local-player-avatar.md](../handoffs/handoff-local-player-avatar.md)

### 本地玩家头像关键代码路径

```
UpdateMapIcons()
  ├── _selfPlayerId = NetworkIdentityTracker.GetSelfPlayerId() ?? "__local__"  // 先同步 ID
  ├── EnsureVirtualAiDefaultPlayer_NoThrow()
  ├── EnsureSelfPlayer_NoThrow()              // 本地玩家注入 _players + 清理旧缓存
  ├── players 过滤 (IsConnected, LocationX/Y >= 0)
  ├── 兜底：_selfPlayerId 不在 players 中时，用 CurrentMap.VisitingNode 添加
  ├── 强制 Destroy 旧 self icon
  └── foreach player:
        ├── EnsureMapIcon(player)
        │     ├── TryGetAvatarSpriteForPlayer()
        │     ├── 本地玩家 fallback: 复制其他 icon → Koishi → WhiteSprite
        │     └── Root (100x140) 
        │           ├── AvatarMask (100x100，圆形遮罩) -> AvatarImage (100x100)
        │           ├── Border (100x100，圆形边框，金色/白色)
        │           └── Label (名字, isSelf 时带 "[我]" 前缀)
        └── anchoredPosition = nodePos + (startX + i * spacing, 0)
```

### 待办事项

- [x] 复制 DLL 并重启游戏，验证头像显示和对齐
- [x] 若本地玩家仍显示白色方块，诊断 `LoadCharacterAvatarSprite` 路径（`ModelName` 值、Addressables 路径）
- [x] 获取 BepInEx 日志中 `MapIcon`/`Group` 行，确认头像位置计算正确
- [x] 清理诊断代码：`GetRedSprite()`、`_redSprite`/`_redTexture`、详细位置日志、`catch (Exception ex)`
- [x] 参考远程玩家头像模板（圆角 mask、边框）优化本地玩家/所有玩家图标样式
