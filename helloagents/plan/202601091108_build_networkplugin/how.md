# 技术设计: 修复 networkplugin 构建错误

## 构建错误清单（本次 build 结果）
- `Network/MidGameJoin/MidGameJoinManager.cs`: CS0101 - 命名空间已包含 `MidGameJoinConfig` 定义。
- `Configuration/ConfigManager.Sync.cs`: CS0102 - `ConfigManager` 已包含以下成员定义：
  - `EnableCardSync`, `EnableManaSync`, `EnableBattleSync`, `EnableMapSync`
  - `MaxQueueSize`, `StateCacheExpiryMinutes`
- `Patch/Network/SaveLoadSyncPatch.cs`: CS0051 - 可访问性不一致：public patch 方法参数类型为 private `SaveSyncState` / `LoadSyncState`。

## 推荐修复方案（最小变更）
### 1) MidGameJoinConfig 重复定义（推荐：删除重复项）
- 保留 `networkplugin/Network/MidGameJoin/MidGameJoinConfig.cs` 中的 `MidGameJoinConfig`。
- 删除 `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs` 文件末尾重复的 `public class MidGameJoinConfig`（保留 `JoinRequestStatus` enum）。

### 2) ConfigManager 重复成员（推荐：Sync.cs 只保留“真正属于 Sync 的内容”）
- 已存在的唯一来源：
  - `ConfigManager.FeatureToggles.cs`：`EnableCardSync/EnableManaSync/EnableBattleSync/EnableMapSync`
  - `ConfigManager.Performance.cs`：`MaxQueueSize/StateCacheExpiryMinutes`
- `ConfigManager.Sync.cs` 处理建议：
  - 删除上述 6 个重复 `ConfigEntry<T>` 成员定义。
  - `BindSyncConfiguration` 只保留 sync 专属配置（当前仅看到 `EnableStatusEffectSync` 仍需要保留/绑定）。
  - `GetSyncConfiguration()` 可继续读取 FeatureToggles/Performance 中的成员（删除重复定义后仍可访问）。

### 3) SaveLoadSyncPatch 可访问性（推荐：降低 patch 方法可见性）
- 将下列 Harmony patch 方法从 `public static` 改为 `private static`（或至少 `internal static`），使其可访问性不高于 `private class SaveSyncState/LoadSyncState`：
  - `GameSaveSync.GameSave_Prefix(...)`
  - `GameSaveSync.GameSave_Postfix(...)`
  - `GameLoadSync.GameLoad_Prefix(...)`
  - `GameLoadSync.GameLoad_Postfix(...)`
- 备选：把 `SaveSyncState/LoadSyncState` 提升为 `internal`/`public`（但不推荐扩大可见性）。

## 风险与回滚
- 风险点集中在 ConfigManager 配置项迁移/删重：需确认 `EnableStatusEffectSync` 仍可被读取。
- 回滚：如出现运行期配置缺失，优先把遗漏的 ConfigEntry 恢复到最合适的 partial 文件（FeatureToggles/Performance/Network 等）。

## 验证
- `dotnet build .\networkplugin\NetWorkPlugin.csproj -c Debug --no-restore`
