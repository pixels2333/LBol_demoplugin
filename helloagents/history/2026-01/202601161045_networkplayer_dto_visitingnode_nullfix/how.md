# 怎么修复

## 设计原则
- DTO 只负责承载可序列化数据；同步机制/运行时对象放在其他层（例如 `PlayerEntity`）。
- 初始化时不要读取可能为 `null` 的外部引用。
- 兼容现有数据协议：不改字段名、不改字段类型。

## 方案对比

### 方案 A（推荐）：构造函数内对 `VisitingNode` 做空值回退
在构造函数设置 `location_X`/`location_Y` 时：
- 若 `VisitingNode != null`：使用 `VisitingNode.X/Y`。
- 否则：将 `location_X`/`location_Y` 设为 0（或保留已有默认值）。

优点：
- 改动最小，风险最低。
- 不改变外部调用方式；任何地方 `new NetWorkPlayer()` 都不再崩溃。

缺点：
- 坐标默认值可能不是业务最优（但至少稳定）。

### 方案 B：移除构造函数对 `VisitingNode` 的依赖，提供显式初始化入口
将坐标初始化逻辑从构造函数移出：
- 构造函数只做纯默认值。
- 新增 `InitializeLocationFromVisitingNode(MapNode node)` 或类似方法，由调用者在拿到 `VisitingNode` 后调用。

优点：
- 职责更清晰，避免“隐式读取外部状态”。

缺点：
- 需要改动所有调用方，容易漏改；与“快速修复稳定性”目标不匹配。

## 推荐决策
采用方案 A。

## 注意事项
- `networkplugin/Network/NetworkPlayer/NetWorkPlayer.cs` 也存在类似注释提示（该文件也在构造函数末尾访问 `VisitingNode.X/Y`）。需要在执行阶段确认是否同样需要修复，以免出现“修了 dto 但运行时用的是另一个同名类”的情况。
