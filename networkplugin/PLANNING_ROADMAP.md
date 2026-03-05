# LBoL 联机MOD开发规划路线图 v2.5

## 📜 历史章节（保留）

### 版本演进快照

| 版本 | 日期 | 版本定位 | 当前说明 |
|------|------|---------|---------|
| v2.1 | 早期阶段 | 目标导向规划版 | 保留章节结构作为长期路线参考 |
| v2.2 | 中期阶段 | 架构扩展版 | 保留里程碑与分阶段推进框架 |
| v2.3 | 2026-03-04 | 代码实扫纠偏版 | 纠正了与代码不一致的完成度描述 |
| v2.4 | 2026-03-04 | 风格回归 + 实扫校正版 | 在 v2.3 事实基础上恢复 v2.1 风格排版 |
| v2.5 | 2026-03-05 | 方案库收尾同步版 | 同步 `~exec` 收尾：消息常量治理 + NAT/配置可观测收口 |

> 说明：v2.5 继续以当前代码为准，不回退到历史文档中的过高完成度结论。

---

## 📊 功能对比分析 - 更新 2026-03-04

### ✅ 已完成核心功能（主链路可用）

| 功能模块 | 当前实现 | 完成度 | 代码依据 |
|---------|---------|--------|---------|
| 玩家状态同步 | ✅ | 95% | `Patch/Network/PlayerStateSyncPatch.cs` |
| 敌人状态同步 | ✅ | 90% | `Patch/Network/EnemySyncPatch.cs` |
| 敌人意图同步 | ✅ | 85% | `Patch/Network/EnemyIntentSyncPatch.cs`, `Patch/Network/EnemyIntentReceivePatch.cs` |
| 回合管理同步 | ✅ | 92% | `Patch/Network/EndTurnSyncPatch.cs` |
| 能量管理同步 | ✅ | 88% | `Patch/Network/EnergySyncPatch.cs` |
| 地图/房间推进同步 | ✅ | 90% | `Patch/Network/RoomEntrySyncPatch.cs`, `Patch/Network/MapCheckpointSyncPatch.cs`, `Patch/Network/RoomStateSyncPatch.cs` |
| 交易系统同步 | ✅ | 88% | `Patch/Network/TradeSyncPatch.cs` |
| 复活状态同步 | ✅ | 85% | `Patch/Network/ResurrectSyncPatch.cs` |
| 战斗报告转发 | ✅ | 85% | `Patch/Network/BattleReportForwardPatch.cs` |
| 中途加入主链路 | ✅ | 82% | `Network/MidGameJoin/MidGameJoinManager.cs`, `Network/MidGameJoin/MapCatchUpOrchestrator.cs` |
| 断线重连主链路 | ✅ | 80% | `Network/Reconnection/ReconnectionManager.cs` |
| 聊天系统与状态指示 | ✅ | 85% | `UI/Components/ChatUI.cs`, `UI/Components/NetworkStatusIndicator.cs` |

### 🔄 进行中 / 部分完成

| 功能模块 | 当前实现 | 完成度 | 备注 |
|---------|---------|--------|------|
| NAT 穿透 | ⚠️ 部分完成 | 68% | `Network/Utils/NatTraversal.cs`：STUN/令牌路径可用；UPnP 已明确为配置禁用/未实现语义并可观测 |
| 宝物联机同步 | ⚠️ 部分完成 | 65% | `Patch/Network/ExhibitSyncPatch.cs` 目前是定向修复，不是全量矩阵 |
| 出牌远端落地 | ⚠️ 部分完成 | 70% | `Patch/Network/RemoteCardUsePatch.cs` 使用 reverse patch stub，需专项回归 |
| 工具牌同步 | ⚠️ 部分完成 | 72% | `Patch/Network/ToolCardSyncPatch.cs` 仍需更多场景覆盖 |
| 离线玩家处理 | ✅ 已移除 AI 接管 | 100% | 离线玩家不再进入 AI 代打路径，统一走断线重连/中途加入恢复 |

### ❌ 未实现 / 已废弃

| 功能模块 | 当前实现 | 完成度 | 备注 |
|---------|---------|--------|------|
| 篝火同步 | ⚠️ 已启用主链 | 55% | `Patch/Network/CampfireSyncPatch.cs` 已启用；双端回归仍需执行 |
| 存档 bytes 联机同步 | ⚠️ 已废弃 | 0% | `EnableSaveLoadSync` 已标记 Deprecated；改为本地恢复 + FullSnapshot |
| 成就同步 | ❌ 未落地 | 0% | 未发现独立成就联机同步链路 |

---

## 🎯 主要成就 - P0 核心模块（实扫确认）

### 1. ✅ 房间与会话主链路
**完成度**: 90%  
**代码位置**: `Network/Room/NetworkRoom.cs`, `Network/Server/PlayerSession.cs`  
**已实现**:
- [✅] 房间成员加入/离开
- [✅] 房主切换与广播
- [✅] 基础会话管理

### 2. ✅ 地图推进与状态追赶链路
**完成度**: 88%  
**代码位置**: `Patch/Network/RoomEntrySyncPatch.cs`, `Patch/Network/MapCheckpointSyncPatch.cs`, `Network/RoomSync/RoomSyncManager.cs`  
**已实现**:
- [✅] 节点进入同步
- [✅] 检查点同步
- [✅] 房间状态同步

### 3. ✅ 战斗核心状态同步链路
**完成度**: 86%  
**代码位置**: `Patch/Network/PlayerStateSyncPatch.cs`, `Patch/Network/EnergySyncPatch.cs`, `Patch/Network/EnemySyncPatch.cs`  
**已实现**:
- [✅] 玩家状态同步
- [✅] 能量同步
- [✅] 敌人状态/生成同步

### 4. ✅ 连接恢复链路（重连 + 中途加入）
**完成度**: 81%  
**代码位置**: `Network/Reconnection/ReconnectionManager.cs`, `Network/MidGameJoin/MidGameJoinManager.cs`  
**已实现**:
- [✅] FullSnapshot 恢复主流程
- [✅] 中途加入追赶编排
- [✅] 已移除离线 AI 接管路径

### 5. ✅ UI 基础链路（聊天 + 网络状态）
**完成度**: 83%  
**代码位置**: `UI/Components/ChatUI.cs`, `UI/Components/NetworkStatusIndicator.cs`  
**已实现**:
- [✅] 聊天消息展示
- [✅] 网络状态展示
- [⚠️] 多面板展示口径仍需统一

---

## 🚀 技术成就总结

### 核心技术进展
1. **LiteNetLib 房间架构落地**：客户端/中继/房间路由链路可用。
2. **FullSnapshot 追赶策略稳定**：中途加入与断线重连不再依赖存档 bytes 同步。
3. **战斗同步补丁体系成型**：玩家状态、能量、敌人状态与部分事件已覆盖。
4. **UI 可观测能力提升**：聊天与网络状态有独立组件。

### 当前系统能力（实扫口径）
- ✅ 可支撑多人基础流程（建房、战斗、推进、重连、中途加入）。
- ✅ 已形成可维护的同步补丁分层（Network / UI / Map / Battle）。
- ⚠️ 仍存在历史兼容层与未收敛模块（Campfire / NAT / DTO 重复定义）。

---

## 📌 第一优先级 (P1 - 稳定性闭环)

#### 1. 篝火同步恢复
**重要性**: ⭐⭐⭐⭐  
**状态**: ❌ 未启用  
**代码位置**: `Patch/Network/CampfireSyncPatch.cs`  
**完成度**: 15%

**任务清单**:
- [ ] 解除 `#if false` 并补齐收发事件。
- [ ] 增加主机权威验证与去重。
- [ ] 加入重连/中途加入后的篝火状态恢复。

---

#### 2. 玩家模型收敛
**重要性**: ⭐⭐⭐⭐  
**状态**: ⚠️ 进行中  
**代码位置**: `Network/NetworkPlayer/`  
**完成度**: 45%

**任务清单**:
- [ ] 梳理 `NetWorkPlayer` 与 `dto/NetWorkPlayer` 引用。
- [ ] 下线 `NotImplementedException` 相关旧接口。
- [ ] 统一 DTO 与运行时模型边界。

---

#### 3. 聊天模型统一
**重要性**: ⭐⭐⭐  
**状态**: ⚠️ 进行中  
**代码位置**: `Chat/ChatMessage.cs`, `UI/Components/ChatUI.cs`  
**完成度**: 55%

**任务清单**:
- [ ] 统一 `PlayerName/Username` 字段口径。
- [ ] 收敛为单一聊天消息模型。
- [ ] 验证历史 payload 兼容反序列化。

---

#### 4. 固定回归清单落地
**重要性**: ⭐⭐⭐⭐  
**状态**: ⚠️ 待完善  
**完成度**: 35%

**任务清单**:
- [ ] 房间生命周期（建房/入房/离房/主机切换）。
- [ ] 战斗一致性（回合/能量/敌人状态）。
- [ ] 功能链路（交易/复活/地图推进）。
- [ ] 连接恢复（重连/中途加入/FullSnapshot 追赶）。

---

## 📌 第二优先级 (P2 - 体验与一致性优化)

#### 1. NAT 能力补全或收口
**重要性**: ⭐⭐⭐  
**状态**: ⚠️ 部分完成  
**代码位置**: `Network/Utils/NatTraversal.cs`  
**完成度**: 60%

**任务清单**:
- [ ] 若保留 UPnP：补齐创建/释放映射。
- [ ] 若不保留 UPnP：下线误导入口并同步文档。
- [ ] 明确 NAT 类型到连接策略映射表。

---

#### 2. 敌人生成同步路径收敛
**重要性**: ⭐⭐⭐  
**状态**: ⚠️ 进行中  
**代码位置**: `Patch/Network/EnemySpawnSyncPatch.cs`, `Patch/Network/SpawnedEnemySyncPatch.cs`  
**完成度**: 70%

**任务清单**:
- [ ] 明确两条 patch 链路职责边界。
- [ ] 统一 SpawnId 生成与匹配规则。
- [ ] 清理重复广播路径。

---

#### 3. UI 展示口径统一
**重要性**: ⭐⭐⭐  
**状态**: ⚠️ 进行中  
**代码位置**: `Patch/UI/OtherPlayersOverlayPatch.cs`, `UI/Panels/`  
**完成度**: 58%

**任务清单**:
- [ ] 统一多人覆盖层与玩家状态来源。
- [ ] 收敛交易/复活面板状态显示逻辑。
- [ ] 减少网络状态提示抖动。

---

## 📌 第三优先级 (P3 - 锦上添花功能)

#### 1. 成就联机同步
**重要性**: ⭐⭐  
**状态**: ❌ 未开始  
**完成度**: 0%

#### 2. 观战模式
**重要性**: ⭐⭐  
**状态**: ❌ 未开始  
**完成度**: 0%

#### 3. 调试面板与性能可视化
**重要性**: ⭐⭐  
**状态**: ⚠️ 待规划  
**完成度**: 10%

---

## 📊 开发进度更新 (2026-03-04 - 最新)

### 当前进度: ~76%（按核心链路加权）

**已稳定的主链路**:
- ✅ 房间与会话管理
- ✅ 地图推进/检查点同步
- ✅ 战斗核心状态同步（玩家/能量/敌人）
- ✅ 交易与复活同步
- ✅ 重连与中途加入主流程
- ✅ 聊天与网络状态基础 UI

**进行中关键项**:
- ⚠️ NAT 穿透能力补全（UPnP 未落地）
- ⚠️ 宝物/远端出牌路径回归强化
- ⚠️ 玩家模型与聊天模型收敛

**当前技术债指标（静态扫描）**:
- `NetworkMessageTypes` 公共消息常量约 126 个
- `TODO` 标记约 8 处
- `NotImplementedException` 约 11 处
- `#if false` 约 3 处

### 2026-03-05 `~exec` 收尾增量

- ✅ 完成 `5.5`：补丁层高频消息字面量替换为 `NetworkMessageTypes` 常量（含 `DebutBonusSyncPatch`、`RemoteCardUsePatch`、`GameResultSyncPatch` 相关链路）。
- ✅ 完成 `5.6`：删除未接入的 `MessageCategories`，避免“定义但不使用”结构。
- ✅ 完成 `6.1~6.3`：`NatTraversal` 新增 UPnP 状态语义（DisabledByConfig / UnsupportedOrUnavailable / AvailableButNotImplemented）与 STUN 日志口径；`NetworkStatusIndicator` 增加 NAT/UPnP 可观测展示。
- ✅ 完成 `6.4`：清理 `ConfigManager.FeatureToggles.cs` / `ConfigManager.Performance.cs` 中 `#if false` 历史参考块。
- ⚠️ `7.2` 聊天回归按最新范围决策移出当前方案验收；`7.3~7.4` 因缺少双端环境顺延为手工回归项。

---

## 🎮 可玩性里程碑（完成度百分比块）

### MVP（基础多人可玩）完成度: 88%
- ✅ 建房/入房
- ✅ 战斗主流程同步
- ✅ 地图推进同步
- ✅ 交易与复活基础流程
- ⚠️ 篝火节点尚未打通

### Alpha（稳定性增强）完成度: 66%
- ✅ 重连与中途加入主链路
- ✅ 房间状态追赶能力
- ⚠️ 模型收敛与异常链路清理中
- ⚠️ 回归脚本仍需固化

### Beta（体验优化）完成度: 48%
- ✅ 聊天与网络状态 UI 可用
- ⚠️ UI 展示口径仍有分歧
- ⚠️ NAT 策略需明确收口方案

### 正式版本（扩展能力）完成度: 28%
- ❌ 成就同步未开始
- ❌ 观战模式未开始
- ⚠️ 调试可视化未成体系

---

## 🚀 推荐开发顺序（修订版）

### 第一阶段（稳定性闭环）
1. 打通 `CampfireSyncPatch` 全链路。
2. 收敛 `NetworkPlayer` 历史兼容层。
3. 统一聊天 DTO。
4. 固化回归清单并形成固定复测流程。

### 第二阶段（一致性优化）
5. 决策并落实 NAT 方案（补全 UPnP 或明确移除）。
6. 收敛敌人生成同步双路径。
7. 统一 UI 多处玩家状态展示口径。

### 第三阶段（扩展评估）
8. 成就联机同步可行性评估。
9. 观战模式与调试可视化是否立项。

---

## 📦 模块依赖关系

```
核心层 (P1):
├─ 房间/会话管理
├─ 战斗状态同步
├─ 地图推进与检查点
├─ 连接恢复（重连/中途加入）
└─ 基础 UI（聊天/网络状态）

优化层 (P2):
├─ NAT 能力补全/收口
├─ 模型收敛（Player/Chat）
├─ 敌人生成路径收敛
└─ UI 口径统一

扩展层 (P3):
├─ 成就联机同步
├─ 观战模式
└─ 调试与性能可视化
```

---

## 📝 关键 TODO 清单

### 立即行动项（本周）
1. [ ] 启用并补齐 `CampfireSyncPatch`。
2. [ ] 清理 `NetWorkPlayer` 系列未实现接口。
3. [ ] 聊天消息模型统一改造。
4. [ ] 完成第一版固定回归脚本。

### 短期目标（1-2 周）
5. [ ] 明确 NAT 策略并落地。
6. [ ] 收敛敌人生成同步双路径。
7. [ ] 统一 UI 覆盖层状态来源。

### 中期目标（2-4 周）
8. [ ] 完成 P1/P2 剩余项并回归验证。
9. [ ] 输出一致性问题清单与修复闭环。
10. [ ] 评估 P3 立项优先级。

---

## ⚠️ 风险与技术债

### 高风险
1. `CampfireSyncPatch` 未启用导致流程断层。
2. 历史玩家模型仍含 `NotImplementedException`。
3. 聊天 DTO 双定义存在协议漂移风险。

### 中风险
4. `SaveSync` 历史常量仍保留，易引起认知偏差。
5. 敌人生成双路径并存提高维护成本。
6. NAT 文档认知与实现存在偏差。

---

## ✅ 回归验收清单（发布前）

1. 房间生命周期：建房、入房、离房、主机切换。
2. 战斗一致性：回合、能量、敌人状态、玩家状态。
3. 功能链路：交易、复活、地图进入与房间状态同步。
4. 连接恢复：断线重连、中途加入、FullSnapshot 追赶。
5. UI 反馈：聊天、网络状态、多人覆盖层显示稳定。

---

**最后更新**: 2026-03-05  
**文档版本**: v2.5  
**更新方式**: 保留 v2.1 风格章节 + 基于当前代码的完成度纠偏
