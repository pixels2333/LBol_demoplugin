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
| 1 | `UpdateMapIcons` 过滤条件 `.Where(p => p.PlayerId != _selfPlayerId)` 排除本地玩家 | 本地玩家不出现在地图上 | 移除此过滤条件 | 新增功能时必须审查所有过滤条件 |
| 2 | `GetFallbackCharacterId()` 写成 `local?.Id ?? local?.ModelName ?? "Koishi"`，且 `Id` 返回 `""` | `""` 不触发 `??`，传入空字符串加载失败 | 优先 `ModelName`，`IsNullOrWhiteSpace` 检查 | C# `??` 不处理空字符串；Unity 字段可返回 `""` |
| 3 | `EventBridge` 中 `ResolveCharacterId()` 对本地玩家返回空值 | 网络消息到达后覆盖了 `EnsureSelfPlayer` 设置的 `CharacterId` | 赋值前保护：空 charId 时用 `GetFallbackCharacterId()` | 网络同步和本地状态存在竞争，所有写入点都要保护 |
| 4 | 无本地玩家缓存生命周期管理 | `_selfPlayerId` 变化后旧条目残留，重复或错位 | 新增 `EnsureSelfPlayer_NoThrow()` + 旧条目清理 | 动态 ID 需主动清理旧缓存 |
| 5 | `TryLoadAvatarSpriteNoThrow` 未过滤 1x1 占位符 | Addressables 返回 1x1 假有效 Sprite，显示为极小色块 | 增加 `width <= 1 && height <= 1` 过滤 | Addressables "加载成功"≠"资源有效" |
| 6 | `Image.color = Color.red` 诊断残留未清理 | 红色方块（"解决了，但是是红色"） | 改回 `Color.white` | 诊断修改验证后必须及时清理 |
| 7 | `icon.RootRect.sizeDelta = (240, 320)` 差异化尺寸 | 本地图标比别人大一倍，重叠（"头像没对齐"） | 移除尺寸覆盖，统一 `(120, 160)` | 差异化尺寸需配套调整间距 |
| 8 | `horizontalSpacing=100` 小于 `rootSize.x=120` | 同节点 2 个头像必然重叠 | 增大到 `110` | 间距必须大于元素宽度 |
| 9 | 诊断代码未清理 | `GetRedSprite()`、详细日志等残留 | 标记为待清理 | 功能验证后立即清理诊断代码 |

### 待办事项

- [ ] 获取 BepInEx 日志中 `MapIcon`/`Group` 行，确认头像位置计算正确
- [ ] 诊断 `LoadCharacterAvatarSprite` 为何对本地玩家失败（`ModelName` 值？Addressables 路径？）
- [ ] 清理诊断代码：`GetRedSprite()`、`_redSprite`/`_redTexture`、详细位置日志
- [ ] 参考远程玩家头像模板（圆角 mask、边框）优化本地玩家图标样式
