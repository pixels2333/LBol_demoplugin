# NetworkPlayer DTO Directory

该目录当前保留给未来的“专用协议 DTO / mapper”拆分使用，暂不承载运行时代码。

## 当前约定

- 现有线协议载体仍在 `networkplugin/Network/NetworkPlayer/NetWorkPlayer.cs`。
- `NetWorkPlayer` 中的 `username`、`location_X`、`location_Y` 等 lowerCamelCase / 下划线字段，视为**协议字段形状**，用于保持历史 JSON 兼容。
- 运行时代码若需要更清晰的命名，应优先使用 `NetWorkPlayer` 上的 PascalCase 别名属性或 `INetworkPlayer` 接口，而不是在新逻辑里继续扩散 legacy 字段名。

## 何时把 DTO 拆到这里

只有在出现以下任一场景时，再把 `NetWorkPlayer` 拆成 `Wire DTO + Runtime Model + Mapper`：

1. 线协议字段需要和运行时对象显式分叉；
2. 同一玩家模型要支持多种消息版本；
3. 需要在 Relay / Client / UI 之间引入稳定 mapper 层。