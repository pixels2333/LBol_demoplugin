# 任务清单: 内联单次调用私有方法（反过度抽象重构）

目录: `helloagents/plan/202605071730_inline-single-call-methods/`

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
总任务: 19
已完成: 14
完成率: 74%
说明: Phase 1-4 已完成（29个方法内联），Phase 5（Patch文件）待执行
```

---

## 任务列表

### 1. TradeDetailDialog.cs — 探查 + 内联（Phase 1）

- [ ] 1.1 探查 `networkplugin/UI/Dialogs/TradeDetailDialog.cs` 中所有仅 1 次调用的私有方法
  - 验证: 列出所有方法名、行号、唯一调用点行号、方法体行数

- [ ] 1.2 内联 P0（≤15 行）+ P1（16-50 行）单次调用方法
  - 依赖: 1.1
  - 验证: `dotnet build` → 0 error

### 2. ResurrectPanel.cs — 探查 + 内联（Phase 2）

- [ ] 2.1 探查 `networkplugin/UI/Panels/ResurrectPanel.cs` 中所有仅 1 次调用的私有方法
  - 验证: 列出所有方法名、行号、唯一调用点行号、方法体行数

- [ ] 2.2 内联 P0 + P1 单次调用方法
  - 依赖: 2.1
  - 验证: `dotnet build` → 0 error

### 3. 工厂文件 — 内联（Phase 3）

- [ ] 3.1 内联 `TradeDetailDialogRuntimeFactory.cs`：`TryPickButtonTemplate`(34行) + `TryPickTextTemplate`(3行)
  - 验证: `dotnet build` → 0 error

- [ ] 3.2 内联 `TradePanelRuntimeFactory.cs`：`IsCurrentRuntimePanel`(13行)、`CloneTextOrCreate`(14行)、`TryResolveCommonButtonWidget`(17行)、`DisableExtraButtons`(20行)、`DisableTooltipBehaviours`(18行)
  - 验证: `dotnet build` → 0 error

- [ ] 3.3 内联 `ResurrectPanelRuntimeFactory.cs`：`IsRuntimeCreatedPanel`(13行)、`IsCurrentRuntimePanel`(14行)、`CopyRectTransform`(15行)
  - 注意: `CreateSimpleEntryTemplate`(88行) 保留不内联
  - 验证: `dotnet build` → 0 error

- [ ] 3.4 内联 `GapSharedPanelTemplateFactory.cs`：`TryResolveCommonButtonWidget`(17行)、`TryPickTextTemplate`(21行)
  - 验证: `dotnet build` → 0 error

- [ ] 3.5 内联 `RuntimeSelectionPanelFactory.cs`：`ResolveColorBlock`(17行)、`TryFindCommonAncestorRect`(27行)、`CopyRectTransform`(14行)
  - 验证: `dotnet build` → 0 error

### 4. TradePanel.cs — 分级内联（Phase 4，重头戏）

- [ ] 4.1 内联 P0 方法（≤15 行）：`HasLocalOffer`、`HasRemoteOffer`、`BuildWhereText`、`ClearOfferPreview`、`SendTradeEvent`、`TrySendConfirm`、`TrySendCancel`、`ShowPickerEmpty`、`HidePickerEmpty`、`ShowExhibitPickerOverlay`（~12 个）
  - 验证: `dotnet build` → 0 error

- [ ] 4.2 内联 P1 方法（16-50 行）：`TryGetLocalizedStringQuiet`、`ShowCardPickerOverlay`、`TryEnsureNetworkConnected`、`TryUnsubscribeTradeEvents`、`PopulateLocalDebugRemoteOffer`、`CreateCardPickerCardWidget`、`CreateCardPickerSelectionMarker`、`GetCardPickerDisplayCards`、`ApplyCardPickerToggleStates`、`LockSlots`、`CopyRectTransform`、`TryFindCommonAncestorRect`（~12 个，部分可能评估后跳过）
  - 注意: 逐个评估内联后有意义的执行，不盲目全内联
  - 验证: `dotnet build` → 0 error

- [ ] 4.3 评估 P2 方法（51-100 行）：`SetupTradeSession`(85行)、`TryHandlePreparing`(107行)、`EnsureOfferActionsOverlay`(90行)、`OnCardPickerSelectionChanged`(47行)、`ApplyCardPickerSelection`(56行)、`RebuildExhibitPickerList`(49行)、`PruneOfferEditorExtraButtons`(69行)、`ApplyStateToUi`(71行)、`CreateExhibitRecordRow`(65行)
  - 决策: 逐个人工评估是否需要内联
  - 验证: `dotnet build` → 0 error

- [ ] 4.4 P3 方法（>100 行）确认不内联：`ApplyNetworkTradeAndClose`(184行)、`TryPickOfferEditorTemplates`(158行)、`EnsurePartnerPickerOverlay`(254行)、`RebuildCardPickerList`(101行)
  - 验证: 无需操作，仅记录

### 5. Patch 文件 — 探查 + 内联（Phase 5）

- [ ] 5.1 探查 `Patch/UI/OtherPlayersOverlayPatch.cs` 中单次调用私有方法
  - 验证: 列出清单

- [ ] 5.2 内联 P0 + P1 方法
  - 依赖: 5.1
  - 验证: `dotnet build` → 0 error

- [ ] 5.3 探查并内联其余 Patch 文件（`MainMenuMultiplayerEntryPatch.cs`、`GapOptionsPanel_Patch.cs`、`EventSyncPatch.cs`）
  - 注意: HarmonyX Patch 方法绝对不动
  - 验证: `dotnet build` → 0 error

### 6. 最终回归验证（Phase 6）

- [ ] 6.1 全量构建：`dotnet build networkplugin/NetWorkPlugin.csproj -c Debug -v minimal -p:LangVersion=preview -clp:ErrorsOnly`
  - 验证: 0 error，警告数 ≤243（基线值）

- [ ] 6.2 统计清理后的代码总行数
  - 验证: 与清理前基线（62,334 行）对比，预期减少 ≥500 行

---

## 执行备注

> 执行过程中的重要记录

| 任务 | 状态 | 备注 |
|------|------|------|
