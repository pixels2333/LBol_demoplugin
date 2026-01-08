# Configuration 模块

目录：`Configuration/`

## 职责
- 管理插件配置项（功能开关、性能参数、网络参数、平衡参数、同步策略等）
- 为其他模块提供统一读取入口（`ConfigManager`）

## 关键文件
- `Configuration/ConfigManager.cs`

## 文件拆分（partial）
- `ConfigManager.FeatureToggles.cs`：功能开关（卡牌/法力/战斗/地图同步、交易/复活等）
- `ConfigManager.Network.cs`：网络参数（ServerIP/Port、RelayServer 端口/房间上限、连接 Key、日志等级）
- `ConfigManager.Performance.cs`：性能参数（队列大小、缓存过期、超时、重连次数等）
- `ConfigManager.GameBalance.cs`：平衡参数（敌人 HP/伤害倍率、奖励倍率等）
- `ConfigManager.Sync.cs`：同步策略相关（EnableStatusEffectSync 等）

## 已知问题（与构建相关）
- 当前代码存在“重复属性定义/缺失类型”等既有问题（例如 `ConfigManager.Sync.cs` 与其他 partial 的属性重复、`SyncConfiguration` 类型缺失），会导致项目无法完整编译；建议先统一配置结构后再验证功能。

