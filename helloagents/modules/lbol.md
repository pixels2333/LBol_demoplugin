# lbol

## 职责

提供 LBoL 游戏代码镜像，作为 `networkplugin/` 联机补丁对接时的类型、流程、UI 结构与运行时语义参考来源。

## 目录分层

| 子目录 | 作用 |
|--------|------|
| `LBoL.Base/` | 基础类型、枚举、扩展与通用结构 |
| `LBoL.ConfigData/` | 卡牌、敌人、关卡、音效等配置数据模型 |
| `LBoL.Core/` | 战斗、地图、事件、回合、存档等核心玩法逻辑 |
| `LBoL.EntityLib/` | 卡牌、敌人、宝物、关卡等具体实体实现 |
| `LBoL.Presentation/` | UI、动画、音频与表现层相关逻辑 |

## 使用约定

- `lbol/` 主要用于查找真实游戏类型、流程入口和 UI 结构，不是当前联机功能的主改动目录。
- 遇到 `MapNode`、`GameRunController`、`BattleController`、`GapOptions`、`GameMaster` 等游戏原生行为时，应优先回到 `lbol/` 对照语义后再修改 `networkplugin/`。
- 需要知识库记录的，是“对接点”和“语义结论”，而不是在此重复维护整套游戏 API 清单。

## 典型对接点

- 地图/房间/站点流程：`LBoL.Core/`、`LBoL.Presentation/UI/Panels/`
- 战斗与回合逻辑：`LBoL.Core/Battle/`
- 角色、卡牌、展品、状态效果定义：`LBoL.EntityLib/` 与 `LBoL.ConfigData/`
- 联机 UI 克隆与模板参考：`LBoL.Presentation/`

## 当前知识库中的典型引用场景

- `modules/networkplugin.md`：对齐 Harmony Patch 的挂接点与 UI 模板来源
- `modules/networkplayer.md`：说明 `MapNode` 等原生类型的语义边界
- `modules/inrun-map-progress-sync.md`：定位地图追赶、阶段同步与 checkpoint 相关的原生流程

## 依赖关系

```yaml
依赖: 无
被依赖: networkplugin, networkplayer, inrun-map-progress-sync
```