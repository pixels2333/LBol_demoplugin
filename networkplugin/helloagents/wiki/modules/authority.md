# Authority 模块

目录：`Authority/`

## 职责
- 房主权威（Host Authority）：对需要仲裁的操作做校验与最终裁决，避免多端同时做出不可并行的决定。
- 请求处理：接收客户端请求（ClientRequest），校验格式/权限/冲突并执行。
- 广播与一致性：将权威动作（AuthoritativeAction）广播给所有客户端，保证最终一致。
- 重连支持：提供重连快照与近期动作历史（用于恢复与追赶）。

## 关键文件
- `Authority/HostAuthorityManager.cs`

## 关键方法（以符号为准）
- `Initialize()`：初始化并获取 DI 依赖
- `ProcessClientRequest()` / `ValidateAndExecuteRequest()`：请求处理主入口
- `ApplyAuthoritativeAction()` / `BroadcastActionToClients()`：执行与广播
- `GetReconnectionSnapshot()`：重连快照生成

