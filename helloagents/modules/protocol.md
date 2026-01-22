# protocol

## 职责

定义网络事件/载荷/数据模型的文档约定（以代码实现为准）。

---

以下内容从旧版 wiki 迁移，保持原文：

## 来源：wiki/api.md

本项目的网络同步主要通过 `NetworkMessageTypes` 中定义的事件类型进行广播。

## 远端目标出牌
- `OnRemoteCardUse`: 发送端对远端队友出牌（目标端结算入口）。
- `OnRemoteCardResolved`: 目标端结算完成后广播状态快照（用于所有客户端更新远端玩家显示）。

## 来源：wiki/data.md

## OnRemoteCardUse
- `RequestId`: string，唯一请求标识
- `Timestamp`: long
- `SenderPlayerId`: string
- `TargetPlayerId`: string
- `Card`: 无
- `SenderStatusEffects`: array（状态效果快照）
- `Actions`: array（动作蓝图，目前覆盖 Damage/Heal/ApplyStatusEffect）

## OnRemoteCardResolved
- `RequestId`: string（对应 OnRemoteCardUse）
- `ResolveSeq`: long（目标端结算序号，用于乱序丢弃）
- `Timestamp`: long
- `TargetPlayerId`: string
- `Hp/MaxHp/Block/Shield`: int
- `StatusEffects`: array

## 接口定义（可选）

> 模块对外暴露的公共API和数据结构

### 公共API
| 函数/方法 | 参数 | 返回值 | 说明 |
|----------|------|--------|------|
| (见文档原文) | - | - | 模块接口以代码为准，文档记录关键约定 |

### 数据结构
| 字段 | 类型 | 说明 |
|------|------|------|
| - | - | - |

## 行为规范

> 描述模块的核心行为和业务规则

### 核心场景
**条件**: 需要联机连接（Host/Relay）
**行为**: 按模块约定发送/接收事件并保证一致性
**结果**: 本地与远端状态收敛一致

## 依赖关系

```yaml
依赖: 无
被依赖: networkplugin, networkplayer
```
