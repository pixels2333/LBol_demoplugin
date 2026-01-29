# 任务清单: inrun-map-progress-sync

目录: helloagents/plan/202601251728_inrun-map-progress-sync/

---

## 执行状态
```yaml
总任务: 22
已完成: 11
完成率: 50%
```

---

## 已完成

- [√] 1.* 协议清理：删除 HostSaveTransfer（主机存档分片传输）
- [√] 2.* FullStateSnapshot 聚焦“种子 + 地图进度”
- [√] 3.* 主机侧：生成权威 MapStateSnapshot + 关键提交点 checkpoint
- [√] 4.1 客户端侧：追赶执行器状态机骨架（MapState 消费/应用）
- [√] 4.2.1 扩充 FullSnapshot 开局对齐字段（Host -> Client）：`GameStateSnapshot.Difficulty/Puzzles/GameMode/ShowRandomResult/StageTypeNames/DebutAdventureTypeName`
- [√] 4.2.2 Joiner 选角但锁定主机参数：Patch `GameMaster.StartGame` 强制覆盖 `seed/difficulty/puzzles/mode/stages`
- [√] 4.2.3 Joiner StageIndex 对齐：在首次 `EnterNextStage` 前设置 `_stageIndex = hostStageIndex - 1`

相关实现索引见：`helloagents/modules/inrun-map-progress-sync.md`

## 待执行

- [√] 4.3 重连：优先恢复本地存档，再对齐 MapState（回主菜单重连场景）
- [√] 4.4 追赶路径：以 MapState.PathHistory 为准（逐节点追赶到 CurrentLocation）
- [√] 5.* 追赶时“已清节点领奖/结算”入口考古与实现
- [√] 6.* 正在战斗节点：追赶时 RoomState 请求时机与 Intent 表现
- [√] 7.1 SaveLoadSyncPatch：删除或改造为 checkpoint 广播（禁止存档同步）
	- 结论：不实现“联机存档 bytes 同步”。保留本地 RestoreGameRun 用于重连继续，但联机追赶以 FullSnapshot+checkpoint 为准。
	- 配置：`EnableSaveLoadSync` 标记为 Deprecated，并在运行时强制关闭（避免误用）。
- [ ] 8.1 路由与消息约束：单播/广播边界复核
- [ ] 9.* 测试与手工验收脚本
