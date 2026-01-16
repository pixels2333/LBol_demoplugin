# why

## 目标
为 `INetworkPlayer` 接口补齐/修正注释，重点“完成 TODO 注释”与消除语义歧义，使联机同步字段、资源系统与地图节点参数在阅读层面可被准确理解。

## 现状与问题
- `INetworkPlayer.cs` 存在 `TODO:`（例如 `stance` 命名可能要改、`GetMyself` 预计弃用），当前缺少明确的迁移/语义说明。
- 顶部出现重复的 `<summary>` 块，且存在“玩家终极能量”说明后未跟随任何成员的残留注释段，容易造成文档生成/阅读困惑。
- 接口字段采用 lowerCamelCase（如 `userName`, `endturn`, `location_X`），与常规 C# 约定不同；需要在注释中明确这是协议/序列化一致性选择，避免后续误改。
- `mana` 在插件侧以 `int[4]` 使用（见 `networkplugin` 现有实现/DTO），但原游戏法力系统更复杂（见 `LBoL.Base.ManaColor`/`ManaGroup`），需要在注释中明确“当前同步协议的简化映射”。
- `UpdateLocation(MapNode visitingnode, ...)` 依赖原游戏 `LBoL.Core.MapNode`（含 `X/Y/Act/StationType` 等），注释应引用其语义，避免把它当成纯坐标对。

## 约束与边界
- 本次目标聚焦“注释完善”，不改变接口签名、命名或行为。
- 若要彻底清理重复 `<summary>` 与残留注释段（涉及删除/重排注释），需要用户明确允许“删除/调整现有注释行”；否则仅采用“追加说明”的方式规避风险。

## 成功标准
- `INetworkPlayer.cs` 中所有公开成员（属性/方法）拥有准确的 XML 文档注释。
- 所有 `TODO:` 旁给出可执行的解释（为何存在、何时改、风险）。
- 对 `mana`、`stance`、`ultimatePower`、`MapNode` 的语义在注释层面达成一致，降低误用与后续重构成本。
