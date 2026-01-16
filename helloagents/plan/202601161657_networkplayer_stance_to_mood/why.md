# why

## 背景
原游戏（`lbol/`）中与“心境/架势”相关的概念实际为 Mood（心境）及其循环特效（基于 `StatusEffect.UnitEffectName`）。在当前联机插件实现中：
- 心境同步补丁以 `UnitEffectName`（`ChaowoLoop`/`BenwoLoop`/`DunwuLoop`）为核心（见 `MoodEffectSyncPatch` / `MoodSwitchRenderPatches`）。
- 运行时玩家模型中已存在 `mood` 字段并绑定 JSON 字段名 `"mood"`（见 `NetWorkPlayer.cs`）。

但接口与部分 DTO 仍使用 `stance`：
- `INetworkPlayer` 暴露 `stance`/`UpdateStance`。
- `dto/NetWorkPlayer.cs` 仍以 `JsonPropertyName("stance")` 传输。

这会导致：
- 语义不一致（对开发者误导：以为游戏里有 stance）。
- 数据层不一致（同一“心境”字段在不同类型中叫 mood/stance）。
- 后续扩展/维护难度上升。

## 目标
- 统一语义：将“心境字段”统一命名为 `mood`。
- 统一协议：将 DTO 序列化字段从 `"stance"` 迁移为 `"mood"`。
- 控制风险：尽量采用兼容迁移策略，避免旧版本联机混用时直接断联/不同步。

## 成功标准
- 代码层：所有玩家接口/实现对外以 `mood` 表达心境，不再使用 `stance` 作为主语义。
- 数据层：网络消息/DTO 统一使用 `"mood"` 字段；如需要兼容，则可读取旧字段 `"stance"`。
- 构建验证：`dotnet build networkplugin/NetWorkPlugin.csproj -c Release` 成功。
