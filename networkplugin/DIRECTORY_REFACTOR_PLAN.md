# 重构 networkplugin 目录结构

## 目录

1. [目标](#目标)
2. [原则](#原则)
3. [Phase 1: 低风险单文件移动](#phase-1-低风险单文件移动)
4. [Phase 2: OtherPlayersOverlay 批量移动](#phase-2-otherplayersoverlay-批量移动)
5. [Phase 3: Network 根级文件归位](#phase-3-network-根级文件归位)
6. [Phase 4: HTML 原型 + 空目录清理](#phase-4-html-原型--空目录清理)
7. [Phase 5: .csproj 同步 + 编译验证](#phase-5-csproj-同步--编译验证)
8. [执行顺序总览](#执行顺序总览)

---

## 目标

将 `networkplugin/` 下所有放到错目录的文件移动到语义正确的路径，**namespace 同步为路径对齐的命名空间**，清理空目录。

## 原则

- 每个 .cs 文件的物理路径 ≡ namespace
- 修改源码：移动 + 改 namespace + 同步所有引用者的 using
- 不移动刚重构过的文件（`TurnAction_Patch.cs`、`TurnBoundaryReceivePatch.cs`、`NetworkEventHelper.cs`、`SendSyncHelper.cs`）
- 最终 `dotnet build` 0 错误

---

## Phase 1: 低风险单文件移动

三步无相互依赖，可并行。

### 1.1 RemotePlayerProxyEnemy.cs

| 维度 | 值 |
|---|---|
| 从 | `Patch/UI/RemotePlayerProxyEnemy.cs` |
| 到 | `Patch/EnemyUnits/RemotePlayerProxyEnemy.cs` |
| namespace 旧 | `NetworkPlugin.Patch.UI` |
| namespace 新 | `NetworkPlugin.Patch.EnemyUnits` |
| 被引用者 | `Patch/UI/PlayerTargeterPatch.cs` |

**操作**：
1. 改 namespace → `NetworkPlugin.Patch.EnemyUnits`
2. 在 `PlayerTargeterPatch.cs` 中添加 `using NetworkPlugin.Patch.EnemyUnits;`
3. 移动物理文件

### 1.2 RuntimeGapOption.cs

| 维度 | 值 |
|---|---|
| 从 | `Patch/UI/RuntimeGapOption.cs` |
| 到 | `UI/Models/RuntimeGapOption.cs` |
| namespace 旧 | `NetworkPlugin.Patch.UI` |
| namespace 新 | `NetworkPlugin.UI.Models` |
| 被引用者 | `Patch/UI/GapOptionsPanel_Patch.cs` |

### 1.3 TradeUiMessages.cs

| 维度 | 值 |
|---|---|
| 从 | `Patch/UI/TradeUiMessages.cs` |
| 到 | `UI/Components/TradeUiMessages.cs` |
| namespace 旧 | `NetworkPlugin.Patch.UI` |
| namespace 新 | `NetworkPlugin.UI.Components` |
| 被引用者 | `Patch/UI/ShopTradeIconPatch.cs`、`UI/Panels/TradePanel.cs` |

---

## Phase 2: OtherPlayersOverlay 批量移动

依赖 Phase 1。

### 2.1 5 个文件整体移出 Patch

| 维度 | 值 |
|---|---|
| 从 | `Patch/UI/OtherPlayersOverlay/` |
| 到 | `UI/Components/OtherPlayersOverlay/` |
| namespace 旧 | `NetworkPlugin.Patch.UI` |
| namespace 新 | `NetworkPlugin.UI.Components` |
| 被引用者 | `Patch/UI/OtherPlayersOverlayPatch.cs` |

涉及文件：
- `OtherPlayersOverlayBattleState.cs`
- `OtherPlayersOverlayEventBridge.cs`
- `OtherPlayersOverlayPlayerStore.cs`
- `OtherPlayersOverlaySceneLifecycle.cs`
- `OtherPlayersOverlayViewRegistry.cs`

**操作**：
1. 创建 `UI/Components/OtherPlayersOverlay/`
2. 每个文件改 namespace → `NetworkPlugin.UI.Components`
3. 在 `OtherPlayersOverlayPatch.cs` 中添加 using
4. 批量移动 5 个文件
5. 删除空的 `Patch/UI/OtherPlayersOverlay/`

---

## Phase 3: Network 根级文件归位

依赖 Phase 1-2。

### 3.1 SyncVar.cs（引用少，先做）

| 从 | `Network/SyncVar.cs` |
| 到 | `Network/Sync/SyncVar.cs` |
| namespace 新 | `NetworkPlugin.Network.Sync` |
| 被引用者 | `Network/PlayerEntity.cs` 等 |

### 3.2 PlayerEntity.cs

| 从 | `Network/PlayerEntity.cs` |
| 到 | `Network/NetworkPlayer/PlayerEntity.cs` |
| namespace 新 | `NetworkPlugin.Network.NetworkPlayer` |
| 被引用者 | `RemoteNetworkPlayer.cs`、`ReconnectionManager.cs` 等 |

### 3.3 ModService.cs（50+ 引用，最后做）

| 从 | `Network/ModService.cs` |
| 到 | `Network/Services/ModService.cs` |
| namespace 新 | `NetworkPlugin.Network.Services` |
| 被引用者 | **50+ 个文件** |

---

## Phase 4: HTML 原型 + 空目录清理

依赖 Phase 1-3。

### 4.1 HTML 原型文件归位

创建 `UI/Prototypes/`，移动 4 个文件：
- `UI/Components/NetworkStatusIndicator-UI-Interactive.html`
- `UI/Dialogs/TradeDetailDialog-UI-Interactive.html`
- `UI/Panels/ResurrectPanel-UI-Interactive.html`
- `UI/Panels/TradePanel-UI-Interactive.html`

### 4.2 删除 8 个空 Shared/ 目录

```
Core/Shared/
Configuration/Shared/
UI/Factories/Shared/
UI/Panels/Shared/
Patch/Actions/Shared/
Patch/Network/Shared/
Network/Client/Shared/
Network/Server/Shared/
```

---

## Phase 5: .csproj 同步 + 编译验证

1. 更新 `NetWorkPlugin.csproj` 中 `<Compile Include="...">` 路径
2. `dotnet build networkplugin/NetWorkPlugin.csproj`

---

## 执行顺序总览

```
Phase 1 (并行)
├── 1.1 RemotePlayerProxyEnemy.cs
├── 1.2 RuntimeGapOption.cs
└── 1.3 TradeUiMessages.cs
        │
Phase 2  ▼
└── 2.1 OtherPlayersOverlay (5 文件)
        │
Phase 3  ▼ (建议: SyncVar → PlayerEntity → ModService)
├── 3.1 SyncVar.cs
├── 3.2 PlayerEntity.cs
└── 3.3 ModService.cs (50+ 引用)
        │
Phase 4  ▼
├── 4.1 HTML 原型
└── 4.2 空目录清理
        │
Phase 5  ▼
└── .csproj + 编译验证
```
