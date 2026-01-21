# 事件/对话同步 v1 - Task List

状态标记: [ ] 待做, [~] 进行中, [x] 已完成, [-] 跳过

## 阶段 B：确定性落地（apply）
- [x] `OnEventSelection`：pending + apply + 幂等去重 + 清理 pending。
- [x] `OnDialogOptions`：接收端缓存 options；用于 OptionIndex->OptionId 映射补偿。

## 阶段 C：断线重连/中途加入追赶（v1 最小可用）
- [x] Host：缓存最近 options/selection，并在 options 阶段周期性重发 options。
- [x] Host：在 `PlayerJoined/Welcome/PlayerListUpdate` 到来时限频重发最小快照。

## 阶段 D：投票模式打磨
- [x] 投票启用策略：仅手动注册 `eventId`。
- [x] 非 Host：接收 `OnEventVotingResult` 保持为“等待 selection 推进”的诊断路径。

## 阶段 E：验证
- [x] `dotnet build networkplugin/NetWorkPlugin.csproj -c Release`
- [-] `mvn -B test/verify`：当前 bash 环境未安装 `mvn`，无法执行。

## 备注 / 约束
- [x] 允许非 Host 上报事件开始（权威模型 B），但最终选择仍由 Host 仲裁并广播。
- [x] 中途加入只关注地图节点恢复边界；进入关卡/关卡内同步由其他模块负责。
