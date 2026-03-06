# LBol_demoplugin 知识库

> 本文件是知识库入口，优先读取本文件后再进入模块、方案或历史归档。

## 快速导航

| 需要了解 | 读取文件 |
|---------|---------|
| 项目概况、技术栈、开发约定 | [context.md](context.md) |
| 模块索引 | [modules/_index.md](modules/_index.md) |
| 某个模块的职责和接口 | `modules/<模块名>.md` |
| 项目变更历史 | [CHANGELOG.md](CHANGELOG.md) |
| 历史方案索引 | [archive/_index.md](archive/_index.md) |
| 当前待执行的方案 | [plan/](plan/) |

## 知识库状态

```yaml
最后更新: 2026-03-06 17:02
模块数量: 5
待执行方案: 7
归档月份: 2
```

## 读取指引

```yaml
启动任务:
  1. 读取本文件获取导航
  2. 读取 context.md 获取项目上下文
  3. 检查 plan/ 是否有进行中方案包

任务相关:
  - 涉及特定模块: 读取 `modules/<模块名>.md`
  - 需要历史决策: 搜索 CHANGELOG.md → 读取对应 `archive/<YYYY-MM>/<方案包>/proposal.md`
  - 继续之前任务: 读取 `plan/<方案包>/*`
```

## 迁移说明

- 标准结构以 `INDEX.md`、`context.md`、`modules/`、`archive/`、`plan/` 为准。
- 旧版 `project.md`、`wiki/`、`history/` 仍保留，作为迁移来源和历史追溯材料，不再作为主入口。

### Legacy 映射表

| legacy 文件/目录 | 标准落点 | 状态 |
|------------------|----------|------|
| `project.md` | `context.md` | 已折叠 |
| `wiki/overview.md` | `context.md`、`modules/lbol.md` | 已折叠 |
| `wiki/arch.md` | `context.md`、`modules/networkplugin.md` | 已折叠 |
| `wiki/api.md`、`wiki/data.md` | `modules/protocol.md` | 已折叠 |
| `wiki/modules/networkplugin.md` | `modules/networkplugin.md` | 已折叠 |
| `wiki/modules/networkplayer.md` | `modules/networkplayer.md` | 已折叠 |
| `history/index.md` | `archive/_index.md` | 已折叠 |

- legacy 文件当前仅保留用于历史追溯、旧链接兼容与人工比对，不再作为日常维护入口。
