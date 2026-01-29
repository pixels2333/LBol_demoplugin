# 模块索引

> 通过此文件快速定位模块文档

## 模块清单

| 模块 | 职责 | 状态 | 文档 |
|------|------|------|------|
| 模块 | 职责 | 状态 | 文档 |
|------|------|------|------|
| networkplugin | 联机同步与 UI/补丁主模块 | 🚧 | [networkplugin.md](./networkplugin.md) |
| networkplayer | 玩家模型/DTO/兼容层 | ✅ | [networkplayer.md](./networkplayer.md) |
| protocol | 网络事件/载荷/数据模型 | ✅ | [protocol.md](./protocol.md) |
| inrun-map-progress-sync | 局内地图进度同步（中途加入/重连追赶） | 🚧 | [inrun-map-progress-sync.md](./inrun-map-progress-sync.md) |

## 模块依赖关系

```
无
networkplugin → protocol
networkplugin → networkplayer
networkplayer → protocol

```

## 状态说明
- ✅ 稳定
- 🚧 开发中
- 📝 规划中
