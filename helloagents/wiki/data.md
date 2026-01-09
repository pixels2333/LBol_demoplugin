# 数据模型（网络载荷）

## OnRemoteCardUse
- `RequestId`: string，唯一请求标识
- `Timestamp`: long
- `SenderPlayerId`: string
- `TargetPlayerId`: string
- `Card`: { `CardId`, `IsUpgraded`, `UpgradeCounter`, ... }
- `SenderStatusEffects`: array（状态效果快照）
- `Actions`: array（动作蓝图，目前覆盖 Damage/Heal/ApplyStatusEffect）

## OnRemoteCardResolved
- `RequestId`: string（对应 OnRemoteCardUse）
- `ResolveSeq`: long（目标端结算序号，用于乱序丢弃）
- `Timestamp`: long
- `TargetPlayerId`: string
- `Hp/MaxHp/Block/Shield`: int
- `StatusEffects`: array

