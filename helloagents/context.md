# 项目上下文

## 1. 基本信息

```yaml
名称: LBol_demoplugin
描述: 以 `networkplugin/` 为核心的 LBoL 联机/同步 Mod 仓库
类型: LBoL Mod / 知识库驱动研发仓库
状态: 开发中
```

## 2. 技术上下文

```yaml
语言: C#
框架: Harmony + LiteNetLib
包管理器: .NET SDK
构建工具: dotnet build / mvn test（工作区附属项目）
```

### 主要依赖
| 依赖 | 版本 | 用途 |
|------|------|------|
| Harmony | (repo) | Patch 框架 |
| LiteNetLib | (repo) | 网络传输 |
| Microsoft.Extensions.DependencyInjection | (repo) | 插件内服务注册与解析 |

## 3. 项目概述

### 核心功能
- 联机同步：通过 `GameEvent` / 系统消息同步房间、战斗、地图与交易状态
- Harmony 补丁：拦截关键游戏行为并驱动联机状态收敛
- UI 扩展：补充多人入口、交易面板、远端玩家覆盖层等联机交互界面
- 知识库工作流：使用 `helloagents/` 维护上下文、模块文档、方案包与归档

### 项目边界
```yaml
范围内:
  - 让 LBoL 支持多人联机并同步关键状态
  - 为网络层、补丁层与方案包提供可追溯文档
范围外:
  - 不承诺兼容所有第三方 Mod
  - 不提供云存档、账号体系或通用联网平台能力
```

## 4. 开发约定

### 代码规范
```yaml
命名风格: 与现有代码一致，优先最小改动
文件命名: 与现有目录结构一致
目录组织: networkplugin/ 为主实现，helloagents/ 为知识库与方案管理
```

### 错误处理
```yaml
错误码格式: 以日志为主，必要时使用结构化 JSON 负载
日志级别: Info/Warn/Error
```

### 测试要求
```yaml
测试框架: 未统一；优先使用构建验证与针对性手工联机回归
覆盖率要求: 未设定
测试文件位置: 无固定路径，回归清单位于 networkplugin/*.md 与 debugtools/
```

### Git规范
```yaml
分支策略: 未强制约束
提交格式: 未强制约束
```

## 5. 当前约束（源自历史决策）

> 这些是当前生效的技术约束，详细决策过程见对应方案包。

| 约束 | 原因 | 决策来源 |
|------|------|---------|
| 远端队友目标出牌由目标端结算，再回传快照广播 | 保持动画一致并避免重复结算 | `archive/2026-01/202601090851_remote_target_card/proposal.md#D001` |
| 服务器骨架采用 `ServerCore + BaseGameServer + Host/Relay` 双模式 | 复用基础能力并统一消息链路 | `archive/2026-01/202601091556_unify_server_core_two_modes/proposal.md#D001` |

## 6. 已知技术债务

> 这里记录仍值得后续继续收敛的问题；更细节的设计背景散落在模块文档与方案包中。

| 债务描述 | 优先级 | 来源 | 建议处理时机 |
|---------|--------|------|-------------|
| 知识库中的 legacy 文档仍保留在 `project.md`、`wiki/`、`history/`，后续可按需继续折叠 | P2 | `~upgrade` 升级保留策略 | 后续文档整理时 |

## 7. 迁移来源（保留原文件）

- `project.md`：旧版项目技术约定原文
- `wiki/overview.md`：旧版项目概览
- `wiki/arch.md`：旧版架构说明
- `history/index.md`：旧版历史索引

以上文件仍保留用于追溯，但标准入口以本文件和 `INDEX.md` 为准。
