# 复活面板同步 v1 - 任务清单（归档）

状态标记: [x] 已完成, [-] 已跳过

- [x] 补齐 PlayerId：`INetworkPlayer.playerId` + `LocalNetworkPlayer.playerId`（来自 `NetworkIdentityTracker`）。
- [x] 新增死亡登记册：`DeathRegistry`，由网络事件汇总供 Gap UI 展示。
- [x] 新增复活同步：`ResurrectSyncPatch`（请求/失败/结果）。
- [x] UI 接入：`GapOptionsPanel_Patch` 构造 payload；`ResurrectPanel` 发起请求并在回调中扣费/提示。
- [x] 防回环与 Host 自身广播缺失修复：`DeathPatches` 增加 `SuppressNetworkSync` 并本地更新 `DeathRegistry`。
- [x] 构建验证：`dotnet build networkplugin/NetWorkPlugin.csproj -c Release` 成功。

- [-] 手动双开冒烟：本次未执行（可按需要补充）。
