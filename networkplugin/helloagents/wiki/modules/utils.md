# Utils 模块

目录：`Utils/`

## 职责
- 为 Patch 与 Network 层提供“读取游戏状态/转换数据/缓冲事件”的通用工具，降低重复反射与硬编码。

## 关键文件
- `Utils/GameStateUtils.cs`：获取当前 GameRun/Battle/玩家/敌人、判断联机/是否房主等
- `Utils/ManaUtils.cs`：法力组与数组/字符串转换、差值计算
- `Utils/CardUtils.cs`：卡牌信息/区域统计辅助
- `Utils/UnitUtils.cs`：单位（玩家/敌人）数据提取与转换
- `Utils/NetworkEventBuffer.cs`：网络事件缓冲（与同步/回放相关）
- `Utils/NetworkIdentityTracker.cs`：追踪本地玩家 Id/是否房主等（解析网络消息中的身份字段）

