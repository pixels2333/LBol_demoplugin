# 模块索引

> 通过此文件快速定位知识库中的模块文档。

## 模块清单

| 模块 | 职责 | 状态 | 文档 |
|------|------|------|------|
| networkplugin | 联机同步、UI 扩展与 Harmony 补丁主模块 | 🚧 | [networkplugin.md](./networkplugin.md) |
| networkplayer | 玩家模型、DTO 与兼容层约定 | ✅ | [networkplayer.md](./networkplayer.md) |
| protocol | 网络事件、载荷与数据模型约定 | ✅ | [protocol.md](./protocol.md) |
| inrun-map-progress-sync | 中途加入/断线重连的地图追赶专题文档 | 🚧 | [inrun-map-progress-sync.md](./inrun-map-progress-sync.md) |
| lbol | 游戏代码镜像与联机补丁对接参考 | ✅ | [lbol.md](./lbol.md) |

## 模块依赖关系

```text
networkplugin → protocol
networkplugin → networkplayer
networkplugin → lbol
networkplayer → protocol
networkplayer → lbol
inrun-map-progress-sync → networkplugin
inrun-map-progress-sync → protocol
inrun-map-progress-sync → lbol
```

## 状态说明
- ✅ 稳定
- 🚧 开发中
- 📝 规划中
