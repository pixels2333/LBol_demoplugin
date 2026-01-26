# 任务清单: mainmenu-mp-entry

目录: `helloagents/plan/{YYYYMMDDHHMM}_mainmenu-mp-entry/`

---

## 任务状态符号说明

| 符号 | 状态 | 说明 |
|------|------|------|
| `[ ]` | pending | 待执行 |
| `[√]` | completed | 已完成 |
| `[X]` | failed | 执行失败 |
| `[-]` | skipped | 已跳过 |
| `[?]` | uncertain | 待确认 |

---

## 执行状态
```yaml
总任务: 8
已完成: 7
完成率: 88%
```

---

## 任务列表

### 1. 诊断与最小修复（主菜单入口）

- [√] 1.1 阅读并确认主菜单入口补丁现状（是否已存在、是否被 PatchAll 覆盖）
  - 文件: `networkplugin/Patch/UI/MainMenuMultiplayerEntryPatch.cs`
  - 验证: 现状为通过 `newGameButton` 克隆；若字段/时机不匹配将导致无法创建入口（表现为主菜单无按钮）

- [√] 1.2 为模板按钮定位实现“分层降级”策略（字段名 → 反射字段扫描 → 子节点按钮扫描）
  - 文件: `networkplugin/Patch/UI/MainMenuMultiplayerEntryPatch.cs`
  - 验证: `TryFindTemplateButton` 支持 3 级降级，失败时输出原因

- [√] 1.3 修复“重复创建/按钮引用失效”的边界情况（场景切换、面板刷新后对象被销毁）
  - 文件: `networkplugin/Patch/UI/MainMenuMultiplayerEntryPatch.cs`
  - 验证: 通过 Unity 的“被销毁即 null”语义判断，失效时会重新创建；未销毁则仅 SetActive(true)

- [√] 1.4 增强日志：当注入失败时输出具体失败点与候选数量（不刷屏，只在失败时输出）
  - 文件: `networkplugin/Patch/UI/MainMenuMultiplayerEntryPatch.cs`
  - 验证: 模板定位失败会记录 why；父节点为空等关键点也有 warning

### 2. 联机入口交互（Host/Join 最小可用）

- [√] 2.1 保持点击“多人游戏”可用：弹窗选择 做房主/加入房主 并触发对应流程
  - 文件: `networkplugin/Patch/UI/MainMenuMultiplayerEntryPatch.cs`
  - 验证: 已升级为“真正的面板 UI”（遮罩 + 容器 + 3 个按钮），并复用原 Host/Join 流程

- [√] 2.2 如主菜单初始化时机不足，补齐注入时机（例如 OnEnable 或其他主菜单显示相关方法）
  - 文件: `networkplugin/Patch/UI/MainMenuMultiplayerEntryPatch.cs`
  - 依赖: 1.2
  - 验证: `Awake/RefreshProfile` 都会更新 `_lastMainMenuPanel` 并 Ensure；入口点击时也可回退 `UiManager.GetPanel<MainMenuPanel>()`

### 3. 构建与回归验证

- [√] 3.1 构建 `networkplugin/NetWorkPlugin.csproj` 并确保无编译错误
  - 验证: `dotnet build` 成功（仓库既有 266 warnings，未新增 error）

- [ ] 3.2 进行一次最小运行验证清单（人工）：进入主菜单确认按钮可见与可点击
  - 验证: 入口出现且点击后弹出“多人游戏”遮罩面板（不要求实际联网成功）

---

## 执行备注

> 执行过程中的重要记录

| 任务 | 状态 | 备注 |
|------|------|------|
| 0 | completed | 运行时修复：`DeathStateSyncPatch` 由错误的 `PlayerUnit.Status` setter patch 改为 `Unit.Status` setter patch，避免 `Harmony.PatchAll()` 阶段崩溃（否则主菜单入口补丁无法生效）。 |
