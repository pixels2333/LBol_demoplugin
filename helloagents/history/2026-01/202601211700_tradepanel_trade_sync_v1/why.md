# TradePanel TODO: Trade Sync v1 - WHY (executed)

目标：把 TradePanel 的 TODO 从“本地演示”升级为可用的联机交易。

- 采用 Host 权威裁决 + 广播，避免并发/分叉。
- 支持任意两玩家发起交易（v1 默认选择一个远端玩家，后续可加 UI 选择）。
- 完成后各自客户端只落地自己的 deck（模型A）。
