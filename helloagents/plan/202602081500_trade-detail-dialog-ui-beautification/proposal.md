# 交易详情对话框 UI 美化与同步完善

## 目标
- 将 `TradeDetailDialog` 的 UI 风格从原始矩形块升级为游戏原生风格（沉浸式）。
- 完善交易同步逻辑，支持多类型资产（卡牌、金币、遗物）的安全交换。
- 增强交互反馈（音效、装饰）。

## 变更内容
- **背景**: 使用 `ResourcesHelper.LoadUiBackground("Adventure")`。
- **组件**: 集成 `RecordCardCell`、`ExhibitWidget`、`CommonButtonWidget`。
- **布局**: 使用 `ScrollRect` 和 `VerticalLayoutGroup` 模拟游戏内的列表。
- **同步**: 完善 `Preparing` 阶段握手与 `Completed` 阶段的本地结算逻辑。
- **音效**: 挂载 `AudioManager` 的确认、取消与点击音效。

## 验收项
- [x] 代码编译通过。
- [x] UI 元素均使用游戏素材。
- [x] 逻辑覆盖了卡牌、金币与遗物的增删。
