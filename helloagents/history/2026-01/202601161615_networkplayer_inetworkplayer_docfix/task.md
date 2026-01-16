# task

目标：完善 `networkplugin/Network/NetworkPlayer/INetworkPlayer.cs` 的 TODO 注释与 XML 文档注释。

## 任务清单

### A. 注释补全（不改签名）
- [√] 为 `INetworkPlayer` 接口顶层补齐一段统一的 `<summary>` + `<remarks>`：
      - 说明这是“网络同步玩家状态”的抽象。
      - 明确 `updateServer` 参数的语义（是否需要向服务器广播/提交）。
      - 备注 lowerCamelCase 命名与协议一致性原因。

- [√] `stance`：在现有 `TODO:stance名称可能要改` 旁新增行注释或 `<remarks>`：
      - 解释当前 `stance` 表达的语义（战斗姿态/架势/状态标识）。
      - 说明若要更名需同步 DTO（例如 `dto/NetWorkPlayer.cs` 的 `stance` 字段）与网络消息字段。

- [√] `ultimatePower`：补充文档注释，统一为“终极技能可用/充能状态（bool）”。

- [√] `mana`：补充文档注释，明确：
      - 当前协议采用 `int[4]`（插件侧默认长度 4）。
      - 描述数组索引与颜色映射的约定（若代码中已有约定则以现有实现为准；若无，注释中标注“约定待统一”）。
      - 提醒原游戏存在更完整的法力系统（`LBoL.Base.ManaColor`/`ManaGroup`）。

- [√] `UpdateLocation(MapNode visitingnode, ...)`：补充注释：
      - `MapNode` 来自 `LBoL.Core`，包含 `X/Y/Act/StationType` 等信息。
      - 说明该方法用于同步“当前访问节点”，不是单纯坐标。

- [√] `GetMyself`：在 `//TODO:预计弃用` 旁补充说明：
      - 预计弃用原因（例如：接口不应暴露自引用/由外部维护本地玩家单例）。
      - 给出替代方式（例如：由玩家管理器/上下文获取本地玩家）。

### B. 可选：结构性清理（需要用户明确允许删除/合并现有注释行）
- [ ? ] 合并接口顶部重复 `<summary>`，并移除未绑定任何成员的残留注释段（“玩家终极能量”后空缺）。

## 验收标准
- [ ] `INetworkPlayer.cs` 所有成员均有完整、准确的 XML 文档注释。
- [ ] 所有 `TODO:` 均得到“可执行的解释/迁移提示”，不再是悬空提示。
- [ ] 不修改任何接口成员签名与行为。

## 执行指令
- 使用 `~exec` 执行本方案包：`helloagents/plan/202601161615_networkplayer_inetworkplayer_docfix/`
