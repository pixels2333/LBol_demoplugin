# 会话交接文档：本地玩家地图头像功能

## 任务目标
在 NetworkPlugin 的地图节点 overlay（`NetworkPlugin_RemotePlayerIconsOverlay`）中，除了显示远端玩家头像外，也显示**本地玩家自己的头像**，头像应显示在玩家自己所在的地图节点上。

## 当前状态：基本功能已实现，存在两个待解决问题

### 已完成
1. ✅ 本地玩家在地图节点上显示头像（不再缺失）
2. ✅ 本地玩家图标颜色正常（`Color.white`）
3. ✅ 名字前缀 `[我]` 显示正确
4. ✅ 编译通过，无错误
5. ✅ 已修复 `_selfPlayerId` 重置时机和本地玩家位置兜底逻辑（最后一次修改）

### 待解决问题

#### 问题 1：头像 sprite 加载失败（显示为白色方块）
- 所有 fallback 都失败，最终回退到 `GetWhiteSprite()`（1x1 白色纹理）
- `TryGetAvatarSpriteForPlayer()` 通过 `PlayerSummary.CharacterId` → `networkPlayer.chara` → `remoteView.CharacterId` → `GetFallbackCharacterId()` 链尝试加载
- `GetFallbackCharacterId()` 使用 `PlayerUnit.ModelName` / `PlayerUnit.Id` 获取角色名
- `ResourcesHelper.LoadCharacterAvatarSprite(characterName)` 使用 Addressables 加载：`Assets/AA/StandPicture/{characterName}.png[{characterName}_Avatar]`
- 可能原因：本地玩家的 `ModelName` 返回值不在 Addressables 的 StandPicture 目录中，或者 Addressables 尚未就绪

#### 问题 2：头像水平对齐问题（已尝试修复）
- 同一节点上的多个头像可能存在位置偏移/重叠
- **最终布局参数**：
  - `rootRect.sizeDelta = (100f, 140f)`
  - `horizontalSpacing = 130f`（30px 间隙，避免重叠）
  - Avatar `anchorMin/Max/pivot = (0.5, 0.5)`，在 root 内居中
  - Label `sizeDelta.y = 16f`，紧贴底部
- **仍缺少**：游戏内实际运行截图验证

## 修改的文件

### `networkplugin/UI/Components/OtherPlayersOverlay/OtherPlayersOverlayViewRegistry.cs`
- **`UpdateMapIcons()`**：添加本地玩家到 players 列表的兜底逻辑；强制重建本地玩家图标缓存；`_selfPlayerId` 使用 `NetworkIdentityTracker.GetSelfPlayerId() ?? "__local__"`
- **`EnsureSelfPlayer_NoThrow()`**：将本地玩家注入 `_players` 缓存，清理旧的 `__`/`local` 前缀条目
- **`GetFallbackCharacterId()`**：优先 `ModelName`，回退 `Id`，最终 `"Koishi"`
- **`EnsureMapIcon()`**：本地玩家的 sprite 兜底链（复制其他玩家 → Koishi fallback → `GetWhiteSprite()`）；缓存 icon 重置标准尺寸/锚点；新 icon 尺寸改为 `(100,140)`；Avatar 居中锚定
- **`TryLoadAvatarSpriteNoThrow()`**：过滤 1x1 占位符纹理
- **布局**：`icon.Image.color = Color.white`（所有玩家统一）；`isSelf` 时添加 `[我]` 前缀；`horizontalSpacing = 130f`
- **诊断日志**：详细位置日志（可后续清理）

### `networkplugin/Patch/UI/OtherPlayersOverlayPatch.cs`
- **`TryGetAvatarSpriteForPlayer()`**：获取玩家头像 sprite 的方法
- **`GetRedSprite()`**：诊断用红色 sprite（已添加但未在主流程使用，可清理）
- **`_redSprite`/`_redTexture`**：诊断用缓存字段（可清理）

## 关键代码路径（修正后）
```
UpdateMapIcons()
  ├── _selfPlayerId = NetworkIdentityTracker.GetSelfPlayerId() ?? "__local__"  // 先同步 ID
  ├── EnsureVirtualAiDefaultPlayer_NoThrow()  // debug 玩家
  ├── EnsureSelfPlayer_NoThrow()              // 本地玩家注入 _players（使用正确 ID）
  ├── players 过滤 (IsConnected, LocationX/Y >= 0)
  ├── 兜底：如果 _selfPlayerId 不在 players 中，直接用 CurrentMap.VisitingNode 添加
  ├── 强制 Destroy 旧 self icon
  └── foreach player:
        ├── EnsureMapIcon(player)              // 创建/更新 icon
        │     ├── TryGetAvatarSpriteForPlayer()
        │     │     ├── player.CharacterId
        │     │     ├── networkPlayer.chara
        │     │     ├── remoteView.CharacterId
        │     │     └── GetFallbackCharacterId()
        │     ├── 本地玩家 fallback: 复制其他 icon → Koishi → WhiteSprite
        │     └── 创建 Root (120x160) + Avatar Image + Label
        └── 设置 anchoredPosition = nodePos + (startX + i * spacing, 0)
```

## 过程中的错误与修正记录（共 14 项）

### 错误 1：过滤条件排除了本地玩家
- **错误代码**：`UpdateMapIcons()` 中 `.Where(p => string.IsNullOrWhiteSpace(_selfPlayerId) || p.PlayerId != _selfPlayerId)` 将本地玩家排除在渲染列表之外
- **表现**：本地玩家根本不出现在地图节点上
- **修正**：移除此过滤条件，允许本地玩家参与渲染
- **教训**：原设计是"只显示远端玩家"，新增本地玩家功能时必须审查所有过滤条件

### 错误 2：`GetFallbackCharacterId()` 优先级反了 + 空字符串未处理
- **错误代码**：`return local?.Id ?? local?.ModelName ?? "Koishi"`
- **表现**：返回的 `Id` 可能是空字符串（`""`），而非 `null`，导致 `??` 不触发回退，最终传入 `LoadCharacterAvatarSprite("")` 加载失败
- **修正**：改用 `string.IsNullOrWhiteSpace()` 检查，优先使用 `ModelName`（这才是角色立绘的标识符），回退 `Id`，最终 `"Koishi"`
- **教训**：C# 的 `??` 不处理空字符串，Unity 配置字段返回 `""` 而非 `null` 是常见陷阱

### 错误 3：网络事件覆盖本地玩家的 CharacterId
- **错误代码**：`OtherPlayersOverlayEventBridge` 中 `ResolveCharacterId(root)` 对本地玩家返回空值，直接赋给 `CharacterId`
- **表现**：即使 `EnsureSelfPlayer_NoThrow()` 正确设置了 `CharacterId`，网络消息到达后会将其覆盖为空
- **修正**：在 `EventBridge` 的两处 `_players[playerId] = new PlayerSummary` 赋值前，增加保护逻辑：如果 playerId 等于 `_selfPlayerId` 且 charId 为空，则使用 `GetFallbackCharacterId()`
- **教训**：网络同步和本地状态注入之间存在竞争，需要在所有写入点保护关键字段

### 错误 4：没有管理本地玩家的缓存生命周期
- **错误代码**：`_mapIcons` 和 `_players` 中没有对本地玩家的专门管理
- **表现**：`_selfPlayerId` 变化后（如从 `"__local__"` 变为真实 ID），旧条目残留在缓存中，导致重复或错位
- **修正**：新增 `EnsureSelfPlayer_NoThrow()` 方法，负责注入本地玩家到 `_players`、清理旧 `__`/`local` 前缀条目；`UpdateMapIcons()` 中强制 Destroy 旧 self icon 后再重建
- **教训**：动态 ID 变化需要主动清理旧缓存，不能假设 ID 永远不变

### 错误 5：Addressables 返回 1x1 占位符未过滤
- **错误代码**：`TryLoadAvatarSpriteNoThrow()` 直接返回 `ResourcesHelper.LoadCharacterAvatarSprite()` 的结果
- **表现**：Addressables 在资源未就绪时返回 1x1 的占位 Sprite（非 null），被当作有效 sprite 使用，显示为一个极小的色块
- **修正**：增加 1x1 纹理过滤：`if (sprite.texture != null && sprite.texture.width <= 1 && sprite.texture.height <= 1) return null`
- **教训**：Unity Addressables 的"加载成功"不代表"资源有效"，需要检查纹理实际尺寸

### 错误 6：误将 Image.color 设为 Color.red（诊断残留）
- **错误代码**：`icon.Image.color = isSelf ? Color.red : ...`
- **表现**：本地玩家头像显示为红色方块（用户反馈："解决了，但是是红色"）
- **修正**：改为 `icon.Image.color = isSelf ? Color.white : ...`
- **教训**：诊断用的临时修改必须在验证后及时清理，不能留到最终版本

### 错误 7：本地玩家图标尺寸设为 (240, 320) 导致对齐问题
- **错误代码**：`if (isSelf) { icon.RootRect.sizeDelta = new Vector2(240f, 320f); }`
- **表现**：本地玩家图标比其他人大一倍，与其他头像重叠（用户反馈："头像没对齐"）
- **修正**：移除此尺寸覆盖，所有玩家使用统一的 `(120, 160)`
- **教训**：差异化尺寸需要配套调整布局间距，否则必然重叠

### 错误 8：horizontalSpacing=100 小于 rootSize.x=120
- **错误代码**：`const float horizontalSpacing = 100f` 但 `rootRect.sizeDelta = new Vector2(120f, 160f)`
- **表现**：同一节点上 2 个头像（宽度 120）间距只有 100，必然重叠
- **修正**：最终改为 `rootRect.sizeDelta = new Vector2(100f, 140f)`，`horizontalSpacing = 130f`，留出 30px 间隙，避免任何重叠
- **教训**：间距必须大于元素宽度，否则相邻元素必然重叠；调整尺寸时应同时调整间距

### 错误 9：头像在 root 中锚定方式不一致
- **错误代码**：Avatar 使用 `anchorMin=(0,1), anchorMax=(1,1), pivot=(0.5,1)` 顶部锚定
- **表现**：不同比例的头像 sprite 在视觉上可能高低不一，且 root 边界框和 sprite 内容不一致
- **修正**：改为 `anchorMin=(0.5,0.5), anchorMax=(0.5,0.5), pivot=(0.5,0.5)` 居中锚定，使 sprite 在 root 内水平和垂直居中
- **教训**：UI 对齐应以边界框中心为基准，而不是以内容顶部为基准，才能保证视觉一致

### 错误 10：`_selfPlayerId` 重置时机太晚
- **错误代码**：`UpdateMapIcons()` 中先调用 `EnsureSelfPlayer_NoThrow()`，后重置 `_selfPlayerId = NetworkIdentityTracker.GetSelfPlayerId() ?? "__local__"`
- **表现**：`EnsureSelfPlayer_NoThrow()` 使用旧的 `_selfPlayerId`（如上一次的 `"__local__"`）注入 `_players`，导致后续渲染时以新的真实 ID 查找时找不到本地玩家，本地玩家图标完全不生成
- **修正**：将 `_selfPlayerId` 重置移到 `EnsureVirtualAiDefaultPlayer_NoThrow()` / `EnsureSelfPlayer_NoThrow()` 之前
- **教训**：状态字段必须先同步再使用，缓存注入函数依赖它时必须保证其最新

### 错误 11：本地玩家位置兜底链路太长且可能失败
- **错误代码**：`UpdateMapIcons()` 中本地玩家 fallback 先调用 `TryGetSelfLocation()`，失败后再尝试 `GameStateUtils.GetCurrentGameRun()?.CurrentMap?.VisitingNode`
- **表现**：`TryGetSelfLocation()` 内部会按 `_selfPlayerId` 查 `_players`，若未注入或 ID 不一致则返回 false，导致兜底路径被跳过
- **修正**：在 fallback 中直接使用当前访问节点，不再依赖 `TryGetSelfLocation()` 的中间查找
- **教训**：兜底逻辑应该最直接、最少依赖，避免"兜底函数调用另一个可能失败的函数"

### 错误 13：居中锚定后误把 sizeDelta.x 设为 0，导致头像消失
- **错误代码**：`avatarRect.sizeDelta = new Vector2(0f, 100f)`
- **表现**：头像 Image 在 `anchorMin/Max=(0.5,0.5)` 居中模式下宽度为 0，整个头像完全不可见（用户反馈："图像消失了"）
- **修正**：改为 `avatarRect.sizeDelta = new Vector2(100f, 100f)`，使 Avatar 在 root 内水平和垂直方向都有实际尺寸
- **教训**：当使用居中锚定时，`sizeDelta.x` 和 `sizeDelta.y` 都必须非零，不能只设置高度

### 错误 14：诊断代码未清理（持续）
- **残留代码**：
  - `GetRedSprite()` 方法和 `_redSprite`/`_redTexture` 字段
  - 详细位置诊断日志 `Plugin.Logger?.LogInfo($"[NetworkPlugin] MapIcon playerId=...")`
  - `OtherPlayersOverlayPatch.cs` 中 catch 块改为 `catch (Exception ex)` 并添加 LogError
- **状态**：待清理
- **教训**：诊断代码应在功能验证通过后立即清理，避免代码膨胀和日志噪音

## 当前状态
- ✅ 本地玩家头像可正常显示
- ✅ 三个头像水平排列，尺寸统一为 `100x140`，间距 `130px`
- ✅ 头像在 root 内水平和垂直居中
- ✅ 编译通过，0 错误
- ⏳ 等待游戏内最终验证

## 下一步建议
1. **复制 DLL 并重启游戏验证**：运行 `copy_networkplugin_dll.ps1`，确认头像显示正常且对齐
2. **诊断 sprite 加载**：如果本地玩家仍显示白色方块，在 `TryLoadAvatarSpriteNoThrow` 中添加日志打印 `characterName` 和 Addressables 路径
3. **清理诊断代码**：功能稳定后移除 `GetRedSprite()`、详细位置日志、`catch (Exception ex)` 等诊断代码
4. **考虑 UI 样式**：参考远程玩家头像模板，为本地玩家添加圆角 mask、边框等视觉效果