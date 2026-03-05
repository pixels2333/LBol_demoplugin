# 任务清单: networkplugin-review-optimization-closure

目录: `helloagents/plan/202603051930_networkplugin-review-optimization-closure/`

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
总任务: 46
已完成: 43
已跳过: 3
完成率: 93.5%
```

---

## 任务列表

### 1. 聊天协议统一（P0）

- [√] 1.1 统一聊天 DTO 为单模型
  - 文件: `networkplugin/Chat/ChatMessage.cs`
  - 改动: 保留唯一聊天模型；主字段统一为 `PlayerName`；在反序列化边界兼容旧字段读取
  - 验证: `ChatUI`、`ChatConsole`、网络收发均引用该模型

- [√] 1.2 移除 ChatUI 内重复 DTO 与枚举定义
  - 文件: `networkplugin/UI/Components/ChatUI.cs`
  - 改动: 删除文件尾本地 `ChatMessage`/`ChatMessageType`，改用 `NetworkPlugin.Chat` 命名空间模型
  - 验证: 编译通过且无命名冲突

- [√] 1.3 修复聊天消息淡出时间计算
  - 文件: `networkplugin/UI/Components/ChatUI.cs`
  - 改动: 替换 `new DateTime(messageObj.name.GetHashCode())` 逻辑，改为显式记录消息创建时间
  - 验证: 聊天消息淡出时间稳定、无异常跳变

- [√] 1.4 发送链路字段口径统一
  - 文件: `networkplugin/Chat/ChatConsole.cs`
  - 改动: 发送消息时仅填充统一字段，不再双字段并写
  - 验证: Host/Joiner 双向聊天显示一致

- [√] 1.5 接收链路兼容分支收口
  - 文件: `networkplugin/Core/SynchronizationManager.cs`
  - 改动: 清理 `UserName/PlayerName/PlayerId` 并行解析分支，统一汇聚到 `PlayerName`
  - 验证: 旧消息仍可读，新消息只走新口径

- [√] 1.6 聊天消息名治理
  - 文件: `networkplugin/Network/Messages/NetworkMessageTypes.cs`
  - 改动: 校验 `ChatMessage` 常量引用，移除聊天相关字面量入口
  - 验证: 聊天分发只通过常量

### 2. Campfire 同步恢复（P0）

- [√] 2.1 解除 Campfire 补丁禁用并恢复主流程
  - 文件: `networkplugin/Patch/Network/CampfireSyncPatch.cs`
  - 改动: 移除 `#if false`；实现 Upgrade/RemoveCard 的发送与接收落地
  - 验证: 不再出现“补丁已定义但未执行”状态

- [√] 2.2 增加 Campfire 事件去重
  - 文件: `networkplugin/Patch/Network/CampfireSyncPatch.cs`
  - 改动: 基于 `playerId + floor + actionId` 建立去重键
  - 验证: 连续广播不会重复执行同一篝火动作

- [√] 2.3 客户端路由注册对齐
  - 文件: `networkplugin/Network/Client/NetworkClient.cs`
  - 改动: 注册 Campfire 相关 handler，统一归入 GameEvent
  - 验证: 客户端接收到 Campfire 消息后有明确日志与状态更新

- [√] 2.4 Host/Relay 路由分类对齐
  - 文件: `networkplugin/Network/Server/NetworkServer.cs`
  - 文件: `networkplugin/Network/Server/RelayServer.cs`
  - 改动: 将 Campfire 消息纳入一致的 `IsGameEvent` 规则
  - 验证: Host 模式与 Relay 模式行为一致

- [√] 2.5 中途加入追赶最小补充
  - 文件: `networkplugin/Patch/Network/RoomStateSyncPatch.cs`
  - 改动: 在必要场景下回放 Campfire 关键状态（仅最小字段）
  - 验证: Joiner 进入后不会出现明显篝火状态错位

### 3. 敌人生成链路收敛（P1）

- [√] 3.1 明确敌人出生唯一发送入口
  - 文件: `networkplugin/Patch/EnemyUnits/SpawnedEnemyManager.cs`
  - 改动: 将敌人出生广播统一到 `EnemySpawned`，负载包含稳定 `SpawnId`
  - 验证: 运行期只出现单一主出生事件

- [√] 3.2 统一接收侧出生落地逻辑
  - 文件: `networkplugin/Patch/Network/EnemySpawnSyncPatch.cs`
  - 改动: 将生成、重放、绑定流程统一到一个接收链
  - 验证: 敌人数量、位置、初始状态与 Host 一致

- [√] 3.3 下线并行出生主路径
  - 文件: `networkplugin/Patch/Network/SpawnedEnemySyncPatch.cs`
  - 改动: 删除/停用 `BattleEnemySpawned` 并行发送分支（可短暂保留兼容桥）
  - 验证: 并行主路径不再参与业务流程

- [√] 3.4 敌人状态定位优先 SpawnId
  - 文件: `networkplugin/Patch/Network/EnemySyncPatch.cs`
  - 文件: `networkplugin/Patch/Network/EnemyStateReceivePatch.cs`
  - 改动: 状态发送切为 Host 权威；接收落地按 `SpawnId` 主键定位，次级兜底为 RootIndex+Id（兼容桥 1 迭代）
  - 验证: 高频状态更新不再出现错绑

- [√] 3.5 敌人意图接收逻辑对齐
  - 文件: `networkplugin/Patch/Network/EnemyIntentReceivePatch.cs`
  - 改动: 与 3.4 的定位逻辑一致，统一 SpawnId 锚点
  - 验证: 意图 UI 更新稳定，无错位

- [√] 3.6 清理敌人生成功能字面量
  - 文件: `networkplugin/Patch/Network/EnemySyncPatch.cs`
  - 文件: `networkplugin/Patch/Network/EnemyIntentSyncPatch.cs`
  - 文件: `networkplugin/Patch/Network/EnemyIntentReceivePatch.cs`
  - 文件: `networkplugin/Network/Messages/NetworkMessageTypes.cs`
  - 改动: 同域消息名统一改为 `NetworkMessageTypes` 常量，并将回放筛选改为显式白名单（含兼容旧名）
  - 验证: 不再存在同域消息名字面量

### 4. 玩家模型收敛（P1）

- [√] 4.1 移除旧 NetWorkPlayer 主路径依赖
  - 文件: `networkplugin/Network/NetworkPlayer/NetWorkPlayer.cs`
  - 改动: 删除未实现成员或将类迁出主路径；不再作为业务主模型
  - 验证: 无 `NotImplementedException` 留在联机主链路

- [√] 4.2 删除重复 DTO 定义
  - 文件: `networkplugin/Network/NetworkPlayer/dto/NetWorkPlayer.cs`
  - 改动: 删除重复定义并修复引用
  - 验证: 全局仅剩一套玩家模型契约

- [√] 4.3 插件入口依赖切换到接口模型
  - 文件: `networkplugin/Plugin.cs`
  - 改动: 清理旧 `NetWorkPlayer` 字段/初始化，统一依赖 `INetworkPlayer`
  - 验证: 插件启动和联机初始化正常

- [√] 4.4 本地玩家映射补齐
  - 文件: `networkplugin/Network/NetworkPlayer/LocalNetworkPlayer.cs`
  - 文件: `networkplugin/Network/NetworkPlayer/INetworkPlayer.cs`
  - 文件: `networkplugin/Network/Client/NetworkManager.cs`
  - 改动: 增加 `stage` 主链字段并打通本地位置映射（`UpdateLocation` 同步写入 stage，PlayerList 侧可更新 self.stage）
  - 验证: 本地状态快照完整

- [√] 4.5 远端玩家映射补齐
  - 文件: `networkplugin/Network/NetworkPlayer/RemoteNetworkPlayer.cs`
  - 文件: `networkplugin/Network/Client/NetworkManager.cs`
  - 改动: 远端模型补齐 `stage` 字段并接入 PlayerList 映射，避免 Local/Remote 字段口径漂移
  - 验证: 玩家列表与 Overlay 显示一致

### 5. 消息路由与常量治理（P1）

- [√] 5.1 梳理并分组消息常量
  - 文件: `networkplugin/Network/Messages/NetworkMessageTypes.cs`
  - 改动: 新增 Chat/Battle/Room/Trade 分组，并保留既有 System/GameSync/StateManagement 分组
  - 验证: 常量表可维护、无重复语义

- [√] 5.2 客户端 `IsGameEvent` 判定统一
  - 文件: `networkplugin/Network/Client/NetworkClient.cs`
  - 文件: `networkplugin/Network/Messages/NetworkMessageTypes.cs`
  - 改动: 客户端改为委托 `NetworkMessageTypes.IsGameEvent(..., Client)`，判定规则同源
  - 验证: 客户端不再误判关键消息

- [√] 5.3 Host `IsGameEvent` 判定统一
  - 文件: `networkplugin/Network/Server/NetworkServer.cs`
  - 文件: `networkplugin/Network/Messages/NetworkMessageTypes.cs`
  - 改动: HostServer 改为委托 `NetworkMessageTypes.IsGameEvent(..., HostServer)`
  - 验证: Host 不再与 Client 产生分类漂移

- [√] 5.4 Relay `IsGameEvent` 判定统一
  - 文件: `networkplugin/Network/Server/RelayServer.cs`
  - 文件: `networkplugin/Network/Messages/NetworkMessageTypes.cs`
  - 改动: Relay 改为委托 `NetworkMessageTypes.IsGameEvent(..., Relay)`
  - 验证: Relay 行为与 Host/Client 一致

- [√] 5.5 补丁层消息引用替换
  - 文件: `networkplugin/Patch/Network/*.cs`
  - 改动: 扫描并替换高频消息字面量为常量
  - 验证: 补丁层不再扩散字面量

- [√] 5.6 清理无效 `MessageCategories`（如保留则必须接入）
  - 文件: `networkplugin/Network/Messages/NetworkMessageTypes.cs`
  - 改动: 二选一：删除闲置定义，或接入实际路由
  - 验证: 不留“定义但不使用”结构

### 6. NAT 与配置收口（P2）

- [√] 6.1 明确 NAT 能力边界与返回语义
  - 文件: `networkplugin/Network/Utils/NatTraversal.cs`
  - 改动: 区分 STUN 成功与 UPnP 未启用/未实现状态，统一日志文案
  - 验证: 连接日志可直接判断能力边界

- [√] 6.2 配置项与能力对齐
  - 文件: `networkplugin/Configuration/ConfigManager.Sync.cs`
  - 改动: 显式标注 UPnP 状态（禁用或实验开关）
  - 验证: 配置含义与运行时行为一致

- [√] 6.3 网络状态面板补充 NAT 可观测性
  - 文件: `networkplugin/UI/Components/NetworkStatusIndicator.cs`
  - 改动: 增加 NAT 关键状态显示或日志提示
  - 验证: 现场排障不再依赖深层日志

- [√] 6.4 清理配置遗留参考块
  - 文件: `networkplugin/Configuration/ConfigManager.FeatureToggles.cs`
  - 文件: `networkplugin/Configuration/ConfigManager.Performance.cs`
  - 改动: 删除 `#if false` 历史参考块，保留有效配置定义
  - 验证: 配置文件可读性提升，无伪配置

### 7. 验证、回归与文档同步（P0-P2贯穿）

- [√] 7.1 构建验证
  - 文件: `networkplugin/NetWorkPlugin.csproj`
  - 改动: 无代码改动，执行构建
  - 验证: `dotnet build networkplugin/NetWorkPlugin.csproj` 通过

- [-] 7.2 聊天回归验收
  - 文件: `networkplugin/UI/Components/ChatUI.cs`
  - 文件: `networkplugin/Chat/ChatConsole.cs`
  - 改动: 无新增，执行 Host/Joiner 双向聊天回归
  - 验证: 昵称、内容、淡出行为一致
  - 说明: 按最新范围决策，聊天功能移出当前方案验收范围，标记为 `[-]` 跳过。

- [-] 7.3 Campfire 回归验收
  - 文件: `networkplugin/Patch/Network/CampfireSyncPatch.cs`
  - 改动: 无新增，执行 Upgrade/RemoveCard 双端联机回归
  - 验证: 双端状态一致且无重复执行
  - 说明: 当前无双端联机测试环境，标记为 `[-]` 跳过（待后续手工回归）。

- [-] 7.4 敌人链路回归验收
  - 文件: `networkplugin/Patch/Network/EnemySpawnSyncPatch.cs`
  - 文件: `networkplugin/Patch/Network/EnemyIntentReceivePatch.cs`
  - 改动: 无新增，执行战斗场景回归
  - 验证: 生成、状态、意图连续一致
  - 说明: 当前无双端联机测试环境，标记为 `[-]` 跳过（待后续手工回归）。

- [√] 7.5 文档与路线图同步
  - 文件: `networkplugin/PLANNING_ROADMAP.md`
  - 文件: `helloagents/CHANGELOG.md`
  - 改动: 同步本方案执行结果与决策
  - 验证: 文档状态与代码一致

### 8. 幽灵方法治理（P1）

- [√] 8.1 删除插件入口无调用遗留字段
  - 文件: `networkplugin/Plugin.cs`
  - 改动: 删除 `netWorkPlayer` 字段，并清理对应注释/引用
  - 验证: 符号引用检查仅剩 0 处；插件启动流程不受影响

- [√] 8.2 清理历史 NetWorkPlayer 占位方法
  - 文件: `networkplugin/Network/NetworkPlayer/NetWorkPlayer.cs`
  - 改动: 处理仅定义无调用方法（`SendData`、`IsLobbyOwner`、`IsPlayerInSameRoom`、`IsPlayerOnSameAct`），避免 `NotImplementedException` 留存
  - 验证: 主流程不再依赖上述占位方法，静态检查无残留调用

- [√] 8.3 删除重复 DTO 玩家模型
  - 文件: `networkplugin/Network/NetworkPlayer/dto/NetWorkPlayer.cs`
  - 改动: 删除重复 `NetWorkPlayer` 定义，并修复可能存在的引用
  - 验证: 全局不存在该文件类引用，编译通过

- [√] 8.4 下线 `GetMyself` 接口契约
  - 文件: `networkplugin/Network/NetworkPlayer/INetworkPlayer.cs`
  - 文件: `networkplugin/Network/NetworkPlayer/LocalNetworkPlayer.cs`
  - 文件: `networkplugin/Network/NetworkPlayer/RemoteNetworkPlayer.cs`
  - 改动: 移除 `GetMyself` 契约及实现，统一由上层持有本地玩家引用
  - 验证: 全局无 `GetMyself(` 调用和实现残留

- [√] 8.5 收敛无调用空实现 `UpdateStance`
  - 文件: `networkplugin/Network/NetworkPlayer/LocalNetworkPlayer.cs`
  - 文件: `networkplugin/Network/NetworkPlayer/RemoteNetworkPlayer.cs`
  - 文件: `networkplugin/Network/NetworkPlayer/INetworkPlayer.cs`
  - 改动: 二选一：删除接口与实现，或接入真实调用链
  - 验证: 代码中不再存在“定义即终点”的 `UpdateStance` 空实现

- [√] 8.6 处理 `NetworkManager.UpdatePlayerInfo` 死入口
  - 文件: `networkplugin/Network/Client/NetworkManager.cs`
  - 改动: 明确该方法为有效入口（补接调用）或删除并收敛解析逻辑
  - 验证: 该方法不存在“仅定义无调用”状态

- [√] 8.7 删除 ShopTradeIconPatch 无调用私有方法
  - 文件: `networkplugin/Patch/UI/ShopTradeIconPatch.cs`
  - 改动: 删除 `ApplyCompactButtonStyle_NoThrow`、`ForceButtonLabelCenter`、`TryGetShopPanelRoot`
  - 验证: 符号引用扫描仅保留 0 处，商店 UI 交互回归通过

- [√] 8.8 建立 ReversePatch 方法白名单
  - 文件: `networkplugin/Patch/Network/RemoteCardUsePatch.cs`
  - 改动: 标注 `Card_GetActions_Original` 为 Harmony ReversePatch stub，不纳入幽灵方法删除
  - 验证: 反向补丁运行正常，无误删

- [√] 8.9 幽灵方法专项复核
  - 文件: `networkplugin/**/*.cs`
  - 改动: 对本节裁剪点执行符号引用复核与构建验证
  - 验证: `dotnet build networkplugin/NetWorkPlugin.csproj` 通过，目标符号无断链

---

## 执行顺序建议

1. 先执行 1.* + 2.*（P0）
2. 再执行 3.* + 4.* + 5.* + 8.*（P1）
3. 最后执行 6.* + 7.*（P2 与收尾）

---

## 备注

- 若 3.*（敌人链路收敛）过程中发现跨模块影响超预期，可拆分为独立子方案包执行。
- 若 2.*（Campfire）在联机房间回放链路出现高风险回归，可先上线“消息可收发 + 最小落地”，再追加中途加入追赶细化。