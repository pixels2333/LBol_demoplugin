# 任务清单: 修复 networkplugin 构建错误

目录: `helloagents/plan/202601091108_build_networkplugin/`

## 1. MidGameJoin（去重）
- [ ] 1.1 删除 `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs` 末尾重复的 `MidGameJoinConfig` 定义，保留 `JoinRequestStatus`。

## 2. ConfigManager（去重并保留必要 sync 配置）
- [ ] 2.1 在 `networkplugin/Configuration/ConfigManager.Sync.cs` 删除重复成员：`EnableCardSync/EnableManaSync/EnableBattleSync/EnableMapSync/MaxQueueSize/StateCacheExpiryMinutes`。
- [ ] 2.2 确认 `EnableStatusEffectSync` 仍被绑定且可被 `Patch/Actions/ApplyStatusEffectAction_Patch.cs` 读取。

## 3. SaveLoadSyncPatch（可访问性修复）
- [ ] 3.1 将 `networkplugin/Patch/Network/SaveLoadSyncPatch.cs` 中 4 个 Harmony patch 方法改为 `private static`（或调整 state 类型可见性）。

## 4. 验证
- [ ] 4.1 执行 `dotnet build .\networkplugin\NetWorkPlugin.csproj -c Debug --no-restore`，确认 11 个 error 归零。

## 5. 安全检查
- [ ] 5.1 检查本次改动仅涉及可见性/重复定义清理，不引入反序列化/反射调用的新增输入面。
