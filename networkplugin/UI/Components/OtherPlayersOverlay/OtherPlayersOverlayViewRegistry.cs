using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.Units;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Network.Snapshot;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.Patch.UI;

public static partial class OtherPlayersOverlayPatch
{
    #region 远端视图注册与地图图标

    private static ulong _lastMapIconLayoutFingerprint;

    private static void EnsureRemoteCharacters()
    {
        // 确保渲染时 _selfPlayerId 已同步，过滤逻辑才能正确排除本地玩家
        _selfPlayerId = NetworkIdentityTracker.GetSelfPlayerId();

        if (!ShouldRenderRemoteCharacters(IsMapPanelVisible()))
        {
            HideRemoteCharacters();
            return;
        }

        if (Singleton<GameDirector>.Instance == null || Singleton<GameDirector>.Instance.PlayerUnitView == null)
        {
            HideRemoteCharacters();
            return;
        }

        Transform unitRoot = TryGetGameDirectorTransform("unitRoot");
        Transform playerRoot = TryGetGameDirectorTransform("playerRoot");
        GameObject unitPrefab = TryGetGameDirectorUnitPrefab();
        if (unitRoot == null || playerRoot == null || unitPrefab == null)
        {
            return;
        }

        if (_remoteCharactersRoot == null)
        {
            GameObject rootGo = new("NetworkPlugin_RemoteCharacters");
            rootGo.transform.SetParent(unitRoot, false);
            _remoteCharactersRoot = rootGo.transform;
        }

        List<PlayerSummary> remotePlayers;
        lock (_syncLock)
        {
            remotePlayers = _players.Values
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.PlayerId))
                .Where(p => p.IsConnected)
                .Where(p => string.IsNullOrWhiteSpace(_selfPlayerId) || p.PlayerId != _selfPlayerId)
                .OrderByDescending(p => p.IsHost)
                .ThenBy(p => p.PlayerName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        if (remotePlayers.Count == 0)
        {
            HideRemoteCharacters();
            return;
        }

        _remoteCharactersRoot.gameObject.SetActive(true);

        HashSet<string> alive = new HashSet<string>(remotePlayers.Select(p => p.PlayerId));
        foreach (string existingId in _remoteCharacters.Keys.ToList())
        {
            if (!alive.Contains(existingId))
            {
                RemoveRemoteCharacter(existingId);
            }
        }

        foreach (PlayerSummary p in remotePlayers)
        {
            EnsureRemoteCharacterView(unitPrefab, p);
        }
    }

    private static void UpdateRemoteCharactersLayout()
    {
        if (!ShouldRenderRemoteCharacters(IsMapPanelVisible()))
        {
            return;
        }

        if (_remoteCharactersRoot == null || !_remoteCharactersRoot.gameObject.activeInHierarchy)
        {
            return;
        }

        Transform playerRoot = TryGetGameDirectorTransform("playerRoot");
        if (playerRoot == null)
        {
            return;
        }

        List<PlayerSummary> remotePlayers;
        lock (_syncLock)
        {
            remotePlayers = _players.Values
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.PlayerId))
                .Where(p => p.IsConnected)
                .Where(p => string.IsNullOrWhiteSpace(_selfPlayerId) || p.PlayerId != _selfPlayerId)
                .OrderByDescending(p => p.IsHost)
                .ThenBy(p => p.PlayerName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        Vector3 basePos = playerRoot.localPosition;
        const float xStep = 1.4f;
        const float yStep = 0.5f;
        const float scale = 0.85f;

        for (int index = 0; index < remotePlayers.Count; index++)
        {
            PlayerSummary p = remotePlayers[index];
            if (!_remoteCharacters.TryGetValue(p.PlayerId, out RemoteCharacterView view) || view?.Root == null)
            {
                continue;
            }

            int row = index / 2;
            int col = index % 2;
            float x = (row + 1) * xStep;
            float y = col == 0 ? yStep : -yStep;

            view.Root.transform.localPosition = basePos + new Vector3(x, y, 0f);
            view.Root.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    private static void TickRemoteCharacters()
    {
        if (!ShouldRenderRemoteCharacters(IsMapPanelVisible()))
        {
            return;
        }

        if (_remoteCharacters.Count == 0)
        {
            return;
        }

        INetworkManager manager = TryGetNetworkManager();

        foreach (RemoteCharacterView rc in _remoteCharacters.Values)
        {
            if (rc?.View == null || rc.Root == null || !rc.Root.activeInHierarchy)
            {
                continue;
            }

            rc.View.Tick();

            PlayerSummary summary = null;
            lock (_syncLock)
            {
                _players.TryGetValue(rc.PlayerId, out summary);
            }

            if (summary != null && TryGetRemoteBattleState(summary, out RemoteBattleState battleState))
            {
                UpdateRemoteCharacterStatsValues(rc.View, battleState.Block, battleState.Shield, battleState.Health, battleState.MaxHealth);
            }
            else if (manager != null)
            {
                INetworkPlayer networkPlayer = manager.GetPlayer(rc.PlayerId);
                if (networkPlayer != null)
                {
                    UpdateRemoteCharacterStatsValues(rc.View, networkPlayer.block, networkPlayer.shield, networkPlayer.HP, networkPlayer.maxHP);
                }
            }
        }
    }

    private static void UpdateRemoteCharacterStats(UnitView view, INetworkPlayer networkPlayer)
    {
        if (networkPlayer == null) return;
        UpdateRemoteCharacterStatsValues(view, networkPlayer.block, networkPlayer.shield, networkPlayer.HP, networkPlayer.maxHP);
    }

    private static void UpdateRemoteCharacterStatsValues(UnitView view, int rawBlock, int rawShield, int rawHp, int rawMaxHp)
    {
        try
        {
            if (view == null || view.Unit == null)
            {
                return;
            }

            bool isShieldActive = rawShield > 0;
            bool isBlockActive = rawBlock > 0;

            var traverse = Traverse.Create(view);
            bool currentHasShield = traverse.Property<bool>("HasShield").Value;
            bool currentHasBlock = traverse.Property<bool>("HasBlock").Value;

            // 1. 如果盾的状态发生改变，调用属性 set 改变常驻外罩显示
            if (currentHasShield != isShieldActive)
            {
                traverse.Property<bool>("HasShield").Value = isShieldActive;
                if (isShieldActive)
                {
                    var method = AccessTools.Method(typeof(UnitView), "CreateLocalShieldEffect", new[] { typeof(string), typeof(bool) });
                    method?.Invoke(view, new object[] { "GainShield", true });
                }
            }

            // 2. 如果格挡状态发生改变，同理
            if (currentHasBlock != isBlockActive)
            {
                traverse.Property<bool>("HasBlock").Value = isBlockActive;
                if (isBlockActive)
                {
                    var method = AccessTools.Method(typeof(UnitView), "CreateLocalShieldEffect", new[] { typeof(string), typeof(bool) });
                    method?.Invoke(view, new object[] { "GainBlock", false });
                }
            }

            // 3. 对齐数值
            int oldBlock = view.Unit.Block;
            int oldShield = view.Unit.Shield;
            int oldHp = view.Unit.Hp;
            int oldMaxHp = view.Unit.MaxHp;

            int newBlock = Math.Max(0, rawBlock);
            int newShield = Math.Max(0, rawShield);
            int newHp = Math.Max(0, rawHp);
            int newMaxHp = Math.Max(1, rawMaxHp);

            bool needsHpTween = false;
            bool needsMaxHpRefresh = false;

            var unitTraverse = Traverse.Create(view.Unit);

            if (oldBlock != newBlock || oldShield != newShield)
            {
                unitTraverse.Property<int>("Block").Value = newBlock;
                unitTraverse.Property<int>("Shield").Value = newShield;
                needsHpTween = true;
            }

            if (oldHp != newHp)
            {
                unitTraverse.Property<int>("Hp").Value = newHp;
                needsHpTween = true;
            }

            if (oldMaxHp != newMaxHp)
            {
                unitTraverse.Property<int>("MaxHp").Value = newMaxHp;
                needsMaxHpRefresh = true;
            }

            // 4. 触发状态条 Widget UI 刷新与防定位组件销毁重置
            object statusWidgetObj = traverse.Field("_statusWidget").GetValue();
            if (statusWidgetObj != null)
            {
                var statusWidget = (LBoL.Presentation.UI.Widgets.UnitStatusWidget)statusWidgetObj;
                statusWidget.Alpha = 1f;
                
                // 重点：修复 ScenePositionTier 因没有 TargetTransform 自我销毁导致血条滞留左下角的 Bug
                var scenePositionTier = statusWidget.GetComponent<LBoL.Presentation.UI.ScenePositionTier>();
                if (scenePositionTier == null)
                {
                    scenePositionTier = statusWidget.gameObject.AddComponent<LBoL.Presentation.UI.ScenePositionTier>();
                    Transform hpBarPoint = Traverse.Create(view).Field("hpBarPoint").GetValue<Transform>();
                    scenePositionTier.TargetTransform = hpBarPoint;
                    
                    // 通过反射重新给 statusWidget 内部的 _scenePositionTier 字段设值
                    Traverse.Create(statusWidget).Field("_scenePositionTier").SetValue(scenePositionTier);
                }

                if (needsMaxHpRefresh)
                {
                    AccessTools.Method(statusWidgetObj.GetType(), "OnMaxHpChanged")?.Invoke(statusWidgetObj, null);
                }
                else if (needsHpTween)
                {
                    AccessTools.Method(statusWidgetObj.GetType(), "TweenHpBar")?.Invoke(statusWidgetObj, null);
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogWarning($"[OtherPlayersOverlay] UpdateRemoteCharacterStats 失败: {ex.Message}");
        }
    }

    private static void EnsureRemoteCharacterView(GameObject unitPrefab, PlayerSummary player)
    {
        if (player == null || string.IsNullOrWhiteSpace(player.PlayerId))
        {
            return;
        }

        string desiredCharacter = player.CharacterId;
        if (string.IsNullOrWhiteSpace(desiredCharacter))
        {
            desiredCharacter = GetFallbackCharacterId();
        }

        if (_remoteCharacters.TryGetValue(player.PlayerId, out RemoteCharacterView existing))
        {
            if (existing != null && existing.Root != null && string.Equals(existing.CharacterId, desiredCharacter, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            RemoveRemoteCharacter(player.PlayerId);
        }

        UnitStatusHud hud = UiManager.GetPanel<UnitStatusHud>();
        if (hud == null)
        {
            return;
        }

        PlayerUnit unit = TryCreatePlayerUnit(desiredCharacter);
        if (unit == null)
        {
            return;
        }

        unit.Initialize();

        GameObject container = new($"RemotePlayer_{player.PlayerId}");
        container.transform.SetParent(_remoteCharactersRoot, false);

        GameObject go = UnityEngine.Object.Instantiate(unitPrefab, container.transform);
        UnitView view = go.GetComponent<UnitView>();
        if (view == null)
        {
            UnityEngine.Object.Destroy(container);
            return;
        }

        view.Unit = unit;
        view.SetStatusWidget(hud.CreateStatusWidget(unit), 1f);
        view.SetInfoWidget(hud.CreateInfoWidget(unit), 1f);
        view.SetStatusVisible(true, true);

        unit.SetView(view);
        view.IsHidden = false;
        DisableRemoteCharacterInteractions(view);

        _remoteCharacters[player.PlayerId] = new RemoteCharacterView
        {
            PlayerId = player.PlayerId,
            CharacterId = desiredCharacter,
            Root = container,
            View = view,
        };

        try
        {
            LoadAndActivateRemoteModelAsync(view, unit, container).Forget();
        }
        catch
        {
        }
    }

    /// <summary>
    /// 异步加载远程玩家的 spine 模型，加载完成后通过一次 SetActive(false)→SetActive(true)
    /// 切换强制触发 OnEnable，使 SkeletonAnimation/SkeletonMecanim 组件重新初始化并渲染。
    /// 否则模型加载完成时 GameObject 已激活，OnEnable 不会再次调用，spine 不显示。
    /// </summary>
    private static async UniTaskVoid LoadAndActivateRemoteModelAsync(UnitView view, PlayerUnit unit, GameObject container)
    {
        try
        {
            await view.LoadUnitModelAsync(unit.ModelName, true, default(float?));
            
            // 等待直到远端角色根节点在层级中处于激活状态（意味着场景已加载完毕且Overlay处于可见状态）
            while (_remoteCharactersRoot == null || !_remoteCharactersRoot.gameObject.activeInHierarchy)
            {
                await UniTask.DelayFrame(1);
            }

            // 在根节点激活后，再等待几帧让 Unity 完成渲染管线的相关设置
            await UniTask.DelayFrame(5);

            if (container != null)
            {
                // 彻底禁用
                container.SetActive(false);
                
                // 延迟 2 帧，确保 Unity 状态机和 Spine 组件彻底卸载/重置
                await UniTask.DelayFrame(2);

                if (container != null)
                {
                    // 重新启用，强制触发 OnEnable 重建 Spine 动画渲染
                    container.SetActive(true);
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogWarning($"[OtherPlayersOverlay] LoadAndActivateRemoteModelAsync 失败: {ex}");
        }
    }
    private static void DisableRemoteCharacterInteractions(UnitView view)
    {
        if (view == null)
        {
            return;
        }

        if (view.BoxCollider != null) view.BoxCollider.enabled = false;

        try
        {
            Collider2D circle = Traverse.Create(view).Field("_circleCollider").GetValue<Collider2D>();
            if (circle != null) circle.enabled = false;
        }
        catch
        {
        }

        Collider selector = view.SelectorCollider;
        if (selector != null)
        {
            selector.enabled = false;
        }
    }

    public static void TriggerRemoteCharacterCardUseEffect(string playerId, string cardOrUsName, bool isUs, JsonElement? actions = null)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        Plugin.RunOnMainThread(() =>
        {
            try
            {
                if (!_remoteCharacters.TryGetValue(playerId, out RemoteCharacterView charView) || charView?.View == null)
                {
                    return;
                }

                UnitView view = charView.View;

                // 1. 头顶气泡与招式名称
                if (!string.IsNullOrWhiteSpace(cardOrUsName))
                {
                    string bubbleText = isUs ? $"【符卡】{cardOrUsName}" : cardOrUsName;
                    view.Chat(bubbleText, 2.0f);
                }

                // 2. 特效回放：如果有动作蓝图，调度 PlayVisualsCoroutine 播放卡牌专属弹幕与特效
                bool hasBlueprintActions = actions.HasValue && actions.Value.ValueKind == JsonValueKind.Array && actions.Value.GetArrayLength() > 0;
                if (hasBlueprintActions)
                {
                    BattleController battle = GameStateUtils.GetCurrentGameRun()?.Battle;
                    Singleton<GameDirector>.Instance?.StartCoroutine(RemoteCardUsePatch.PlayVisualsCoroutine(actions.Value.Clone(), battle, skipStateVisuals: true, defaultSenderPlayerId: playerId));
                }
                else
                {
                    // 兜底：通用动作与声光
                    string animName = isUs ? "spell" : "cast";
                    try
                    {
                        view.PlayAnimation(animName);
                    }
                    catch
                    {
                        try { view.PlayAnimation("spell"); } catch { }
                    }

                    try
                    {
                        view.PlayEffectOneShot(isUs ? "UsCast" : "CardCast", 0f);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogWarning($"[OtherPlayersOverlay] TriggerRemoteCharacterCardUseEffect 异常: {ex.Message}");
            }
        });
    }

    /// <summary>
    /// 触发敌人攻击队友小人的视觉效果（弹幕射击与擦弹/受击动作）。
    /// </summary>
    public static void TriggerRemoteEnemyAttackVisual(
        string playerId,
        string enemyId,
        string gunName,
        GunType gunType,
        bool isGrazed,
        bool isAccuracy,
        int damage)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        Plugin.RunOnMainThread(() =>
        {
            try
            {
                if (!_remoteCharacters.TryGetValue(playerId, out RemoteCharacterView charView) || charView?.View == null)
                {
                    return;
                }

                UnitView targetView = charView.View;
                if (targetView == null || targetView.gameObject == null || !targetView.gameObject.activeInHierarchy)
                {
                    return;
                }

                // 查找当前场景中的敌人 UnitView
                UnitView enemyView = FindMatchingEnemyUnitView(enemyId);

                DamageInfo info = DamageInfo.Attack((float)damage, isAccuracy);
                info.IsGrazed = isGrazed;

                if (enemyView != null && !string.IsNullOrEmpty(gunName) && gunName != "Empty" && gunName != "Instant")
                {
                    var pairs = new List<ValueTuple<UnitView, DamageInfo>>
                    {
                        new(targetView, info)
                    };

                    var method = Traverse.Create(typeof(GameDirector)).Method("GunShootAction", enemyView, pairs, gunName, gunType);
                    if (method.MethodExists())
                    {
                        System.Collections.IEnumerator coroutine = (System.Collections.IEnumerator)method.GetValue();
                        Singleton<GameDirector>.Instance?.StartCoroutine(coroutine);
                        return;
                    }
                    else
                    {
                        enemyView.PerformShoot(gunName);
                    }
                }

                // 兜底或即时动画
                if (isGrazed)
                {
                    targetView.PlayAnimation("graze");
                }
                else
                {
                    targetView.PlayAnimation("hit");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogWarning($"[OtherPlayersOverlay] TriggerRemoteEnemyAttackVisual 异常: {ex.Message}");
            }
        });
    }

    private static UnitView FindMatchingEnemyUnitView(string enemyId)
    {
        try
        {
            GameDirector gd = Singleton<GameDirector>.Instance;
            if (gd == null) return null;

            IReadOnlyList<UnitView> enemyViews = GameDirector.Enemies;
            if (enemyViews == null || enemyViews.Count == 0) return null;

            if (!string.IsNullOrWhiteSpace(enemyId))
            {
                foreach (UnitView ev in enemyViews)
                {
                    if (ev != null && ev.Unit != null && ev.Unit.IsAlive)
                    {
                        if (string.Equals(ev.Unit.Id, enemyId, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(ev.Unit.Name, enemyId, StringComparison.OrdinalIgnoreCase))
                        {
                            return ev;
                        }
                    }
                }
            }

            // 找不到匹配 ID 则回退到第一个存活敌人
            return enemyViews.FirstOrDefault(ev => ev != null && ev.Unit != null && ev.Unit.IsAlive) 
                   ?? enemyViews.FirstOrDefault(ev => ev != null);
        }
        catch
        {
            return null;
        }
    }

    internal static IEnumerable<UnitView> SnapshotRemoteCharacterUnitViews()
    {
        if (!ShouldRenderRemoteCharacters(IsMapPanelVisible()))
        {
            return Array.Empty<UnitView>();
        }

        if (_remoteCharacters.Count == 0)
        {
            return Array.Empty<UnitView>();
        }

        List<UnitView> list = new List<UnitView>(_remoteCharacters.Count);
        foreach (RemoteCharacterView rc in _remoteCharacters.Values)
        {
            if (rc?.View == null || rc.Root == null || !rc.Root.activeInHierarchy)
            {
                continue;
            }

            list.Add(rc.View);
        }
        return list;
    }

    internal static void SetRemoteCharacterTargetingEnabled(bool enabled)
    {
        if (!ShouldRenderRemoteCharacters(IsMapPanelVisible()))
        {
            return;
        }

        if (_remoteCharacters.Count == 0)
        {
            return;
        }

        foreach (RemoteCharacterView rc in _remoteCharacters.Values)
        {
            if (rc?.View == null)
            {
                continue;
            }

            SetSelectorColliderEnabled(rc.View, enabled);
        }
    }

    internal static bool TryGetPointedRemotePlayer(Vector2 screenPosition, out string playerId, out string playerName)
    {
        playerId = null;
        playerName = null;

        if (!ShouldRenderRemoteCharacters(IsMapPanelVisible()))
        {
            return false;
        }

        try
        {
            if (_remoteCharacters.Count == 0)
            {
                return false;
            }

            Ray ray = CameraController.MainCamera.ScreenPointToRay(screenPosition);
            foreach (RemoteCharacterView rc in _remoteCharacters.Values)
            {
                if (rc?.View == null || rc.Root == null || !rc.Root.activeInHierarchy)
                {
                    continue;
                }

                Collider selector = rc.View.SelectorCollider;
                if (selector == null)
                {
                    continue;
                }

                if (!selector.Raycast(ray, out _, float.PositiveInfinity))
                {
                    continue;
                }

                playerId = rc.PlayerId;
                lock (_syncLock)
                {
                    if (!string.IsNullOrWhiteSpace(playerId) && _players.TryGetValue(playerId, out PlayerSummary summary))
                    {
                        playerName = ResolveDisplayName(playerId, summary?.PlayerName);
                    }
                }

                return !string.IsNullOrWhiteSpace(playerId);
            }
        }
        catch
        {
        }

        playerId = null;
        playerName = null;
        return false;
    }

    internal static string GetPlayerIdFromUnit(PlayerUnit pu)
    {
        if (pu == null)
        {
            return null;
        }

        lock (_syncLock)
        {
            foreach (var kv in _remoteCharacters)
            {
                if (kv.Value?.View != null && ReferenceEquals(kv.Value.View.Unit, pu))
                {
                    return kv.Key;
                }
            }
        }
        return null;
    }

    internal static bool TryGetRemoteCharacterUnitView(string playerId, out UnitView view)
    {
        view = null;
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return false;
        }

        if (!ShouldRenderRemoteCharacters(IsMapPanelVisible()))
        {
            return false;
        }

        if (_remoteCharacters.TryGetValue(playerId, out RemoteCharacterView rc) && rc?.View != null && rc.Root != null && rc.Root.activeInHierarchy)
        {
            view = rc.View;
            return true;
        }

        view = null;
        return false;
    }

    private static void SetSelectorColliderEnabled(UnitView view, bool enabled)
    {
        if (view == null)
        {
            return;
        }

        Collider selector = view.SelectorCollider;
        if (selector == null)
        {
            return;
        }

        selector.enabled = enabled;
        selector.gameObject.SetActive(enabled);
    }

    private static void HideRemoteCharacters()
    {
        _remoteCharactersRoot?.gameObject.SetActive(false);
    }

    private static void ClearRemoteCharacters()
    {
        foreach (string playerId in _remoteCharacters.Keys.ToList())
        {
            RemoveRemoteCharacter(playerId);
        }

        if (_remoteCharactersRoot != null)
        {
            UnityEngine.Object.Destroy(_remoteCharactersRoot.gameObject);
            _remoteCharactersRoot = null;
        }
    }

    private static void RemoveRemoteCharacter(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        if (_remoteCharacters.TryGetValue(playerId, out RemoteCharacterView rc))
        {
            if (rc?.Root != null)
            {
                UnityEngine.Object.Destroy(rc.Root);
            }
            _remoteCharacters.Remove(playerId);
        }
    }

    private static Transform TryGetGameDirectorTransform(string fieldName)
    {
        try
        {
            return Traverse.Create(Singleton<GameDirector>.Instance).Field(fieldName).GetValue<Transform>();
        }
        catch
        {
            return null;
        }
    }

    private static GameObject TryGetGameDirectorUnitPrefab()
    {
        try
        {
            return Traverse.Create(Singleton<GameDirector>.Instance).Field("unitPrefab").GetValue<GameObject>();
        }
        catch
        {
            return null;
        }
    }

    private static string GetFallbackCharacterId()
    {
        try
        {
            PlayerUnit local = Singleton<GameDirector>.Instance.PlayerUnitView?.Unit as PlayerUnit;
            if (local != null)
            {
                string id = local.ModelName;
                if (!string.IsNullOrWhiteSpace(id))
                    return id;
                id = local.Id;
                if (!string.IsNullOrWhiteSpace(id))
                    return id;
            }
        }
        catch
        {
        }

        try
        {
            PlayerUnit player = GameStateUtils.GetCurrentPlayer();
            if (player != null)
            {
                string id = player.ModelName;
                if (!string.IsNullOrWhiteSpace(id))
                    return id;
                id = player.Id;
                if (!string.IsNullOrWhiteSpace(id))
                    return id;
            }
        }
        catch
        {
        }

        return "Koishi";
    }

    private static PlayerUnit TryCreatePlayerUnit(string characterId)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(characterId))
            {
                PlayerUnit unit = Library.TryCreatePlayerUnit(characterId);
                if (unit != null)
                {
                    return unit;
                }
            }
        }
        catch
        {
        }

        try
        {
            return Library.TryCreatePlayerUnit("Koishi") ?? Library.CreatePlayerUnit("Koishi");
        }
        catch
        {
            return null;
        }
    }

    private static string GetEffectiveSelfPlayerId()
    {
        if (!string.IsNullOrWhiteSpace(_selfPlayerId))
        {
            return _selfPlayerId;
        }

        string tracked = NetworkIdentityTracker.GetSelfPlayerId();
        if (!string.IsNullOrWhiteSpace(tracked))
        {
            return tracked;
        }

        // 单机模式下 NetworkIdentityTracker 也没有 ID，使用固定兜底
        return "__local__";
    }

    private static void EnsureSelfPlayer_NoThrow()
    {
        try
        {
            string effectiveSelfId = GetEffectiveSelfPlayerId();
            if (string.IsNullOrWhiteSpace(effectiveSelfId))
            {
                return;
            }

            int stage = -1;
            int x = -1;
            int y = -1;
            string locName = null;
            try
            {
                var run = GameStateUtils.GetCurrentGameRun();
                var node = run?.CurrentMap?.VisitingNode;
                if (node != null)
                {
                    stage = node.Act;
                    x = node.X;
                    y = node.Y;
                    locName = node.StationType.ToString();
                }
            }
            catch
            {
            }

            if (x < 0 || y < 0)
            {
                if (TryGetSelfLocation(out int selfStage, out int selfX, out int selfY, out string selfLocName))
                {
                    stage = selfStage;
                    x = selfX;
                    y = selfY;
                    locName = selfLocName;
                }
            }

            if (string.IsNullOrWhiteSpace(locName))
            {
                locName = "Map";
            }

            lock (_syncLock)
            {
                // 清理旧的本地玩家条目，避免 _selfPlayerId 变化后旧条目仍出现在地图上
                List<string> toRemove = null;
                foreach (var kvp in _players)
                {
                    if (kvp.Key != effectiveSelfId && (kvp.Key.StartsWith("__") || kvp.Key.StartsWith("local", StringComparison.OrdinalIgnoreCase)))
                    {
                        toRemove ??= new List<string>();
                        toRemove.Add(kvp.Key);
                    }
                }
                if (toRemove != null)
                {
                    foreach (string oldId in toRemove)
                    {
                        _players.Remove(oldId);
                        if (_mapIcons.TryGetValue(oldId, out MapIconUi oldIcon) && oldIcon?.Root != null)
                        {
                            UnityEngine.Object.Destroy(oldIcon.Root);
                            _mapIcons.Remove(oldId);
                        }
                    }
                }

                if (!_players.TryGetValue(effectiveSelfId, out PlayerSummary p) || p == null)
                {
                    p = new PlayerSummary { PlayerId = effectiveSelfId };
                    _players[effectiveSelfId] = p;
                }

                p.PlayerName = ResolveDisplayName(effectiveSelfId, null, isLocal: true);
                p.IsConnected = true;
                p.IsHost = NetworkIdentityTracker.GetSelfIsHost();
                p.CharacterId = GetFallbackCharacterId();
                p.Stage = stage;
                p.LocationX = x;
                p.LocationY = y;
                p.LocationName = locName;
                p.LastUpdateTime = Time.unscaledTime;
            }
        }
        catch
        {
        }
    }

    private static void UpdateMapIcons(MapPanel mapPanel)
    {
        if (mapPanel == null)
        {
            return;
        }

        INetworkClient client = TryGetNetworkClient();

        // 先同步 _selfPlayerId，确保 EnsureSelfPlayer_NoThrow 使用正确的 ID
        string tracked = NetworkIdentityTracker.GetSelfPlayerId();
        _selfPlayerId = !string.IsNullOrWhiteSpace(tracked) ? tracked : "__local__";

        EnsureVirtualAiDefaultPlayer_NoThrow();
        EnsureSelfPlayer_NoThrow();
        if (client == null || !client.IsConnected)
        {
            if (!IsVirtualAiDefaultEnabled())
            {
                HideAllMapIcons();
                return;
            }
        }

        MapNodeWidget[,] widgets;
        try
        {
            widgets = Traverse.Create(mapPanel).Field("_mapNodeWidgets").GetValue<MapNodeWidget[,]>();
        }
        catch
        {
            return;
        }

        if (widgets == null)
        {
            return;
        }

        RectTransform overlayRoot = EnsureMapIconsOverlayRoot(mapPanel);
        if (overlayRoot == null)
        {
            return;
        }

        // 尊重 Runtime Editor 手动 Active 开关：不在每帧强制开启。
        if (!overlayRoot.gameObject.activeSelf)
        {
            return;
        }

        overlayRoot.SetAsLastSibling();
        _defaultFont ??= FindDefaultFont(mapPanel.transform);

        if (_mapNodeIconsRoots.Count > 0)
        {
            foreach (RectTransform root in _mapNodeIconsRoots.Values)
            {
                if (root != null)
                {
                    UnityEngine.Object.Destroy(root.gameObject);
                }
            }

            _mapNodeIconsRoots.Clear();
        }

        List<PlayerSummary> players;
        lock (_syncLock)
        {
            players = _players.Values
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.PlayerId))
                .Where(p => p.IsConnected)
                .Where(p => p.LocationX >= 0 && p.LocationY >= 0)
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(_selfPlayerId) && players.All(p => p.PlayerId != _selfPlayerId))
        {
            int selfStage = -1;
            int selfX = -1;
            int selfY = -1;
            string selfLocationName = null;
            // 兜底：直接用当前访问节点
            try
            {
                var run = GameStateUtils.GetCurrentGameRun();
                var node = run?.CurrentMap?.VisitingNode;
                if (node != null)
                {
                    selfStage = node.Act;
                    selfX = node.X;
                    selfY = node.Y;
                    selfLocationName = node.StationType.ToString();
                }
            }
            catch
            {
            }

            if (selfX >= 0 && selfY >= 0)
            {
                players.Add(new PlayerSummary
                {
                    PlayerId = _selfPlayerId,
                    PlayerName = ResolveDisplayName(_selfPlayerId, null, isLocal: true),
                    IsHost = NetworkIdentityTracker.GetSelfIsHost(),
                    IsConnected = true,
                    CharacterId = GetFallbackCharacterId(),
                    LocationX = selfX,
                    LocationY = selfY,
                    Stage = selfStage,
                    LocationName = selfLocationName,
                });
            }
        }

        if (players.Count == 0)
        {
            HideAllMapIcons();
            return;
        }

        _lastMapIconLayoutFingerprint = 0;

        HashSet<string> alive = new HashSet<string>();
        foreach (var group in players.GroupBy(p => (X: p.LocationX, Y: p.LocationY)))
        {
            int x = group.Key.X;
            int y = group.Key.Y;

            if (x < widgets.GetLowerBound(0) || x > widgets.GetUpperBound(0) || y < widgets.GetLowerBound(1) || y > widgets.GetUpperBound(1))
            {
                continue;
            }

            MapNodeWidget widget = widgets[x, y];
            if (widget == null)
            {
                continue;
            }

            Vector2 nodeAnchoredPosition = GetMapNodeAnchoredPosition(widget);

            List<PlayerSummary> orderedPlayers = [.. group
                .OrderByDescending(p => p.IsHost)
                .ThenBy(p => p.PlayerName, StringComparer.OrdinalIgnoreCase)];


            const float baseY = 0f;
            const float horizontalSpacing = 130f;
            float startX = -((orderedPlayers.Count - 1) * horizontalSpacing * 0.5f);

            for (int i = 0; i < orderedPlayers.Count; i++)
            {
                PlayerSummary p = orderedPlayers[i];
                alive.Add(p.PlayerId);

                MapIconUi icon = EnsureMapIcon(p);
                if (icon.Root.transform.parent != overlayRoot)
                {
                    icon.Root.transform.SetParent(overlayRoot, false);
                }

                icon.Root.transform.SetAsLastSibling();
                icon.Root.hideFlags = HideFlags.None;
                icon.RootRect.anchorMin = new Vector2(0.5f, 0.5f);
                icon.RootRect.anchorMax = new Vector2(0.5f, 0.5f);
                icon.RootRect.pivot = new Vector2(0.5f, 0.5f);

                icon.RootRect.anchoredPosition = nodeAnchoredPosition + new Vector2(startX + i * horizontalSpacing, baseY);
                icon.Label.text = ResolveDisplayName(p.PlayerId, p.PlayerName);
                bool isSelf = !string.IsNullOrWhiteSpace(_selfPlayerId) && string.Equals(p.PlayerId, _selfPlayerId, StringComparison.Ordinal);

                icon.Image.color = isSelf ? Color.white : (p.IsHost ? new Color(1f, 0.95f, 0.4f, 1f) : Color.white);
                if (isSelf)
                {
                    icon.Label.text = "[我] " + icon.Label.text;
                }
            }
        }

        foreach (var kv in _mapIcons)
        {
            if (!alive.Contains(kv.Key))
            {
                kv.Value.Root.SetActive(false);
            }
        }

        overlayRoot.SetAsLastSibling();
    }

    private static Vector2 GetMapNodeAnchoredPosition(MapNodeWidget widget)
    {
        if (widget == null)
        {
            return Vector2.zero;
        }

        RectTransform rect = widget.transform as RectTransform;
        if (rect != null)
        {
            return rect.anchoredPosition;
        }

        Vector3 local = widget.transform.localPosition;
        return new Vector2(local.x, local.y);
    }

    private static RectTransform EnsureMapIconsOverlayRoot(MapPanel mapPanel)
    {
        if (mapPanel == null)
        {
            return null;
        }

        RectTransform nodeHolder = TryGetMapNodeHolder(mapPanel);
        if (nodeHolder == null)
        {
            return null;
        }

        if (_mapIconsRoot != null && _mapIconsRoot.transform.parent == nodeHolder)
        {
            _mapIconsRoot.hideFlags = HideFlags.None;
            _mapIconsRoot.SetAsLastSibling();
            return _mapIconsRoot;
        }

        if (_mapIconsRoot != null)
        {
            UnityEngine.Object.Destroy(_mapIconsRoot.gameObject);
            _mapIconsRoot = null;
        }

        GameObject root = new("NetworkPlugin_RemotePlayerIconsOverlay");
        root.hideFlags = HideFlags.None;
        root.transform.SetParent(nodeHolder, false);

        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
        rt.localEulerAngles = Vector3.zero;
        rt.SetAsLastSibling();

        _mapIconsRoot = rt;
        return rt;
    }

    private static RectTransform TryGetMapNodeHolder(MapPanel mapPanel)
    {
        if (mapPanel == null)
        {
            return null;
        }

        try
        {
            return Traverse.Create(mapPanel).Field("nodeHolder").GetValue<RectTransform>();
        }
        catch
        {
            return null;
        }
    }

    private static RectTransform EnsureMapIconsRoot(MapNodeWidget widget, RectTransform overlayRoot)
    {
        if (widget == null || overlayRoot == null)
        {
            return null;
        }

        if (_mapNodeIconsRoots.TryGetValue(widget, out RectTransform existing) && existing != null && existing.transform.parent == overlayRoot)
        {
            existing.localPosition = widget.transform.localPosition;
            existing.SetAsLastSibling();
            return existing;
        }

        if (existing != null)
        {
            UnityEngine.Object.Destroy(existing.gameObject);
        }

        GameObject root = new("NetworkPlugin_RemotePlayerIcons");
        root.hideFlags = HideFlags.None;
        root.transform.SetParent(overlayRoot, false);

        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localPosition = widget.transform.localPosition;
        rt.sizeDelta = new Vector2(140f, 140f);
        rt.SetAsLastSibling();
        _mapNodeIconsRoots[widget] = rt;
        return rt;
    }

    private static void CleanupMapIconRoots()
    {
        if (_mapNodeIconsRoots.Count == 0)
        {
            return;
        }

        List<MapNodeWidget> toRemove = null;
        foreach (var kv in _mapNodeIconsRoots)
        {
            MapNodeWidget widget = kv.Key;
            RectTransform root = kv.Value;
            if (widget != null && root != null && _mapIconsRoot != null && root.transform.parent == _mapIconsRoot)
            {
                root.localPosition = widget.transform.localPosition;
                continue;
            }

            toRemove ??= new List<MapNodeWidget>();
            toRemove.Add(widget);
            if (root != null)
            {
                UnityEngine.Object.Destroy(root.gameObject);
            }
        }

        if (toRemove == null)
        {
            return;
        }

        for (int i = 0; i < toRemove.Count; i++)
        {
            _mapNodeIconsRoots.Remove(toRemove[i]);
        }
    }

    private static MapIconUi EnsureMapIcon(PlayerSummary player)
    {
        bool isSelf = !string.IsNullOrWhiteSpace(_selfPlayerId) && string.Equals(player.PlayerId, _selfPlayerId, StringComparison.Ordinal);
        if (_mapIcons.TryGetValue(player.PlayerId, out MapIconUi ui) && ui?.Root != null)
        {
            Sprite sprite = TryGetAvatarSpriteForPlayer(player);
            // 本地玩家强制兜底：如果 sprite 可疑（1x1 或 null），直接复制其他玩家已加载的有效 sprite
            if (isSelf)
            {
                if (sprite == null || (sprite.texture != null && sprite.texture.width <= 1 && sprite.texture.height <= 1))
                {
                    Plugin.Logger?.LogWarning($"[NetworkPlugin] Self player cached icon sprite suspicious (null or 1x1), forcing copy from others.");
                    sprite = null;
                }
                if (sprite == null)
                {
                    foreach (var kvp in _mapIcons)
                    {
                        if (kvp.Key != _selfPlayerId && kvp.Value?.Image?.sprite != null && kvp.Value.Image.sprite != GetWhiteSprite())
                        {
                            Sprite otherSprite = kvp.Value.Image.sprite;
                            Plugin.Logger?.LogWarning($"[NetworkPlugin] Self player cached icon copying sprite from {kvp.Key}, sprite={otherSprite.name}, tex={otherSprite.texture?.width}x{otherSprite.texture?.height}");
                            sprite = otherSprite;
                            break;
                        }
                    }
                }
                if (sprite == null) sprite = TryGetAvatarSprite("Koishi");
                if (sprite == null)
                {
                    Debug.LogWarning($"[NetworkPlugin] Self player avatar failed: CharacterId={player.CharacterId}, fallback={GetFallbackCharacterId()}, Koishi also failed.");
                }
            }
            ui.Image.sprite = sprite ?? GetWhiteSprite();
            // 重置缓存 icon 的尺寸/锚点，避免历史脏数据导致对齐不一致
            ui.RootRect.sizeDelta = new Vector2(100f, 140f);
            ui.RootRect.anchorMin = new Vector2(0.5f, 0.5f);
            ui.RootRect.anchorMax = new Vector2(0.5f, 0.5f);
            ui.RootRect.pivot = new Vector2(0.5f, 0.5f);
            if (ui.BorderImage != null)
            {
                ui.BorderImage.color = isSelf ? new Color(1f, 0.84f, 0f, 1f) : Color.white;
            }
            return ui;
        }

        string safeId = string.IsNullOrWhiteSpace(player.PlayerId) ? "unknown" : player.PlayerId.Replace("/", "_").Replace("\\", "_");
        GameObject root = new($"RemoteIcon_{safeId}");
        root.hideFlags = HideFlags.None;
        root.transform.SetParent(null, false);

        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(100f, 140f);

        // 创建 AvatarMask 节点
        GameObject maskGo = new("AvatarMask");
        maskGo.transform.SetParent(root.transform, false);

        RectTransform maskRect = maskGo.AddComponent<RectTransform>();
        maskRect.anchorMin = new Vector2(0.5f, 0.5f);
        maskRect.anchorMax = new Vector2(0.5f, 0.5f);
        maskRect.pivot = new Vector2(0.5f, 0.5f);
        maskRect.anchoredPosition = new Vector2(0f, 10f);
        maskRect.sizeDelta = new Vector2(100f, 100f);

        Image maskImage = maskGo.AddComponent<Image>();
        maskImage.sprite = GetCircleMaskSprite();
        maskImage.color = Color.white;
        maskImage.raycastTarget = false;
        maskImage.preserveAspect = true;

        Mask mask = maskGo.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // 创建 AvatarImage 节点（放入 Mask 下面）
        GameObject avatarGo = new("AvatarImage");
        avatarGo.transform.SetParent(maskGo.transform, false);

        RectTransform avatarRect = avatarGo.AddComponent<RectTransform>();
        avatarRect.anchorMin = new Vector2(0.5f, 0.5f);
        avatarRect.anchorMax = new Vector2(0.5f, 0.5f);
        avatarRect.pivot = new Vector2(0.5f, 0.5f);
        avatarRect.anchoredPosition = Vector2.zero;
        avatarRect.sizeDelta = new Vector2(100f, 100f);

        Image avatar = avatarGo.AddComponent<Image>();
        avatar.raycastTarget = false;
        avatar.preserveAspect = true;

        Sprite avatarSprite = TryGetAvatarSpriteForPlayer(player);
        // 本地玩家强制兜底：如果 sprite 可疑（1x1 或 null），直接复制其他玩家已加载的有效 sprite
        if (isSelf)
        {
            if (avatarSprite == null || (avatarSprite.texture != null && avatarSprite.texture.width <= 1 && avatarSprite.texture.height <= 1))
            {
                Plugin.Logger?.LogWarning($"[NetworkPlugin] Self player new icon sprite suspicious (null or 1x1), forcing copy from others.");
                avatarSprite = null;
            }
            if (avatarSprite == null)
            {
                foreach (var kvp in _mapIcons)
                {
                    if (kvp.Key != _selfPlayerId && kvp.Value?.Image?.sprite != null && kvp.Value.Image.sprite != GetWhiteSprite())
                    {
                        Sprite otherSprite = kvp.Value.Image.sprite;
                        Plugin.Logger?.LogWarning($"[NetworkPlugin] Self player new icon copying sprite from {kvp.Key}, sprite={otherSprite.name}, tex={otherSprite.texture?.width}x{otherSprite.texture?.height}");
                        avatarSprite = otherSprite;
                        break;
                    }
                }
            }
            if (avatarSprite == null) avatarSprite = TryGetAvatarSprite("Koishi");
            if (avatarSprite == null)
            {
                Debug.LogWarning($"[NetworkPlugin] Self player avatar failed: CharacterId={player.CharacterId}, fallback={GetFallbackCharacterId()}, Koishi also failed.");
            }
        }
        avatar.sprite = avatarSprite ?? GetWhiteSprite();

        // 创建 Border 节点（放在 root 下，与 mask 节点平级，使其覆盖在头像之上）
        GameObject borderGo = new("Border");
        borderGo.transform.SetParent(root.transform, false);

        RectTransform borderRect = borderGo.AddComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0.5f, 0.5f);
        borderRect.anchorMax = new Vector2(0.5f, 0.5f);
        borderRect.pivot = new Vector2(0.5f, 0.5f);
        borderRect.anchoredPosition = new Vector2(0f, 10f);
        borderRect.sizeDelta = new Vector2(100f, 100f);

        Image borderImage = borderGo.AddComponent<Image>();
        borderImage.sprite = GetCircleBorderSprite();
        borderImage.color = isSelf ? new Color(1f, 0.84f, 0f, 1f) : Color.white;
        borderImage.raycastTarget = false;
        borderImage.preserveAspect = true;

        TextMeshProUGUI label = CreateTmpText(root.transform, "Name", player.PlayerName ?? player.PlayerId, 14f);
        label.text = ResolveDisplayName(player.PlayerId, player.PlayerName);
        label.alignment = TextAlignmentOptions.Center;
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 0f);
        labelRect.pivot = new Vector2(0.5f, 0f);
        labelRect.anchoredPosition = Vector2.zero;
        labelRect.sizeDelta = new Vector2(0f, 16f);

        MapIconUi icon = new MapIconUi
        {
            Root = root,
            RootRect = rootRect,
            Image = avatar,
            BorderImage = borderImage,
            Label = label,
            CharacterId = player.CharacterId,
        };

        _mapIcons[player.PlayerId] = icon;
        return icon;
    }

    private static void SetMapIconSprite(MapIconUi icon, string characterId)
    {
        string effectiveId = characterId;
        if (string.IsNullOrWhiteSpace(effectiveId))
        {
            effectiveId = GetFallbackCharacterId();
        }

        Sprite sprite = TryGetAvatarSprite(effectiveId);
        if (sprite == null)
        {
            string fallback = GetFallbackCharacterId();
            if (!string.Equals(effectiveId, fallback, StringComparison.OrdinalIgnoreCase))
            {
                sprite = TryGetAvatarSprite(fallback);
            }
        }

        icon.CharacterId = effectiveId;
        icon.Image.sprite = sprite ?? GetWhiteSprite();
    }

    internal static Sprite TryGetAvatarSprite(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return null;
        }

        string normalized = characterId.Trim().Trim('"');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        List<string> candidates = BuildAvatarCharacterCandidates(normalized);
        foreach (string candidate in candidates)
        {
            if (_avatarCache.TryGetValue(candidate, out Sprite cached) && cached != null)
            {
                return cached;
            }

            Sprite sprite = TryLoadAvatarSpriteNoThrow(candidate);
            if (sprite == null)
            {
                continue;
            }

            foreach (string alias in candidates)
            {
                _avatarCache[alias] = sprite;
            }

            return sprite;
        }

        return null;
    }

    private static List<string> BuildAvatarCharacterCandidates(string characterId)
    {
        List<string> candidates = new List<string>(6);
        AddAvatarCandidate(candidates, characterId);

        int slash = Math.Max(characterId.LastIndexOf('/'), characterId.LastIndexOf('\\'));
        if (slash >= 0 && slash + 1 < characterId.Length)
        {
            AddAvatarCandidate(candidates, characterId.Substring(slash + 1));
        }

        if (characterId.EndsWith("_Avatar", StringComparison.OrdinalIgnoreCase))
        {
            AddAvatarCandidate(candidates, characterId.Substring(0, characterId.Length - "_Avatar".Length));
        }

        if (TryResolveCharacterAliasViaLibrary(characterId, out string modelName, out string unitId))
        {
            AddAvatarCandidate(candidates, modelName);
            AddAvatarCandidate(candidates, unitId);
        }

        return candidates;
    }

    private static bool TryResolveCharacterAliasViaLibrary(string characterId, out string modelName, out string unitId)
    {
        modelName = null;
        unitId = null;

        try
        {
            PlayerUnit unit = Library.TryCreatePlayerUnit(characterId);
            if (unit == null)
            {
                return false;
            }

            modelName = unit.ModelName;
            unitId = unit.Id;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static Sprite TryLoadAvatarSpriteNoThrow(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return null;
        }

        try
        {
            Sprite sprite = ResourcesHelper.LoadCharacterAvatarSprite(characterId);
            if (sprite == null)
            {
                return null;
            }
            // 过滤掉 Addressables 可能返回的 1x1 占位符（视为加载失败）
            if (sprite.texture != null && sprite.texture.width <= 1 && sprite.texture.height <= 1)
            {
                Plugin.Logger?.LogWarning($"[NetworkPlugin] Avatar loaded but is 1x1 placeholder: {characterId}");
                return null;
            }
            return sprite;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[NetworkPlugin] TryLoadAvatarSpriteNoThrow exception for characterId='{characterId}': {ex}");
            return null;
        }
    }

    private static void AddAvatarCandidate(List<string> candidates, string characterId)
    {
        if (candidates == null || string.IsNullOrWhiteSpace(characterId))
        {
            return;
        }

        string normalized = characterId.Trim().Trim('"');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        if (candidates.Any(existing => string.Equals(existing, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        candidates.Add(normalized);
    }

    private static void HideMapIcon(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        if (_mapIcons.TryGetValue(playerId, out MapIconUi icon) && icon?.Root != null)
        {
            icon.Root.SetActive(false);
        }
    }

    private static void HideAllMapIcons()
    {
        foreach (MapIconUi icon in _mapIcons.Values)
        {
            if (icon?.Root != null)
            {
                icon.Root.SetActive(false);
            }
        }

        foreach (RectTransform root in _mapNodeIconsRoots.Values)
        {
            if (root != null)
            {
                root.gameObject.SetActive(false);
            }
        }

        // 保留总根节点 Active 状态，避免覆盖 Runtime Editor 的手动开关。
    }

    private static void ClearMapIcons()
    {
        foreach (MapIconUi icon in _mapIcons.Values)
        {
            if (icon?.Root != null)
            {
                UnityEngine.Object.Destroy(icon.Root);
            }
        }
        _mapIcons.Clear();

        if (_mapIconsRoot != null)
        {
            UnityEngine.Object.Destroy(_mapIconsRoot.gameObject);
            _mapIconsRoot = null;
        }

        foreach (RectTransform root in _mapNodeIconsRoots.Values)
        {
            if (root != null)
            {
                UnityEngine.Object.Destroy(root.gameObject);
            }
        }

        _mapNodeIconsRoots.Clear();
    }

    private static ulong ComputeMapIconLayoutFingerprint(IReadOnlyList<PlayerSummary> players)
    {
        if (players == null || players.Count == 0)
        {
            return 0;
        }

        ulong xor = 0;
        ulong sum = 0;

        for (int i = 0; i < players.Count; i++)
        {
            PlayerSummary player = players[i];
            string signature = string.Join("|",
                player.PlayerId ?? string.Empty,
                player.PlayerName ?? string.Empty,
                player.IsHost ? "1" : "0",
                player.LocationX.ToString(),
                player.LocationY.ToString(),
                player.CharacterId ?? string.Empty);

            ulong playerFingerprint = NetLogHelper.ComputeFnv1a64(signature);
            xor ^= playerFingerprint;
            sum += playerFingerprint;
        }

        return xor ^ sum ^ (ulong)players.Count;
    }

    private static bool AreMapIconRootsStable(MapNodeWidget[,] widgets, IReadOnlyList<PlayerSummary> players)
    {
        if (widgets == null || players == null || _mapIconsRoot == null)
        {
            return false;
        }

        int maxX = widgets.GetUpperBound(0);
        int maxY = widgets.GetUpperBound(1);

        for (int i = 0; i < players.Count; i++)
        {
            PlayerSummary player = players[i];
            if (string.IsNullOrWhiteSpace(player.PlayerId) || player.LocationX < 0 || player.LocationY < 0)
            {
                return false;
            }

            if (player.LocationX < widgets.GetLowerBound(0) || player.LocationX > maxX || player.LocationY < widgets.GetLowerBound(1) || player.LocationY > maxY)
            {
                return false;
            }

            MapNodeWidget widget = widgets[player.LocationX, player.LocationY];
            if (widget == null || !_mapNodeIconsRoots.TryGetValue(widget, out RectTransform widgetRoot) || widgetRoot == null || widgetRoot.transform.parent != _mapIconsRoot || !widgetRoot.gameObject.activeSelf)
            {
                return false;
            }

            if ((widgetRoot.localPosition - widget.transform.localPosition).sqrMagnitude > 0.01f)
            {
                return false;
            }

            if (!_mapIcons.TryGetValue(player.PlayerId, out MapIconUi icon) || icon?.Root == null || icon.Root.transform.parent != widgetRoot || !icon.Root.activeSelf)
            {
                return false;
            }
        }

        return true;
    }

    public static void ForceRefreshRemoteCharacters()
    {
        Plugin.RunOnMainThread(() =>
        {
            try
            {
                if (_remoteCharactersRoot != null && _remoteCharactersRoot.gameObject.activeInHierarchy)
                {
                    Plugin.Logger?.LogInfo("[OtherPlayersOverlay] Active Spine refresh triggered programmatically.");
                    _remoteCharactersRoot.gameObject.SetActive(false);
                    
                    UniTask.DelayFrame(3).ContinueWith(() =>
                    {
                        if (_remoteCharactersRoot != null)
                        {
                            _remoteCharactersRoot.gameObject.SetActive(true);
                        }
                    }).Forget();
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogWarning($"[OtherPlayersOverlay] ForceRefreshRemoteCharacters 异常: {ex}");
            }
        });
    }

    private static void ResetMapIconLayoutCache()
    {
        _lastMapIconLayoutFingerprint = 0;
    }

    private sealed class RemoteCharacterView
    {
        public string PlayerId { get; set; }
        public string CharacterId { get; set; }
        public GameObject Root { get; set; }
        public UnitView View { get; set; }
    }

    private sealed class MapIconUi
    {
        public GameObject Root { get; set; }
        public RectTransform RootRect { get; set; }
        public Image Image { get; set; }
        public Image BorderImage { get; set; }
        public TextMeshProUGUI Label { get; set; }
        public string CharacterId { get; set; }
    }

    public static void ApplyStatusEffectsToRemotePlayer(string playerId, List<RemoteStatusEffectInfo> remoteEffects)
    {
        Plugin.RunOnMainThread(() =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(playerId) || remoteEffects == null) return;
                if (!_remoteCharacters.TryGetValue(playerId, out RemoteCharacterView remoteChar) || remoteChar?.View == null) return;

                Unit unit = remoteChar.View.Unit;
                if (unit == null) return;

                var currentEffectsList = unit.StatusEffects;
                if (currentEffectsList == null) return;
                var currentEffects = currentEffectsList.Where(se => se != null).ToList();

                HashSet<string> remoteTypes = new(StringComparer.Ordinal);
                foreach (var rInfo in remoteEffects)
                {
                    if (string.IsNullOrWhiteSpace(rInfo.Type)) continue;

                    string typeName = rInfo.Type;
                    remoteTypes.Add(typeName);

                    LBoL.Core.StatusEffects.StatusEffect existing = currentEffects.FirstOrDefault(se => string.Equals(se.GetType().Name, typeName, StringComparison.Ordinal));
                    if (existing != null)
                    {
                        if (existing.HasLevel) existing.Level = rInfo.Level;
                        else if (existing.HasCount) existing.Count = rInfo.Level;
                        if (existing.HasDuration) existing.Duration = rInfo.Duration;

                        if (remoteChar?.View != null && !string.IsNullOrEmpty(existing.UnitEffectName))
                        {
                            remoteChar.View.SendEffectMessage(existing.UnitEffectName, "OnPropertyChanged", existing);
                        }
                    }
                    else
                    {
                        LBoL.Core.StatusEffects.StatusEffect newEffect = Library.TryCreateStatusEffect(typeName);
                        if (newEffect != null)
                        {
                            Traverse.Create(newEffect).Property("GameRun").SetValue(unit.GameRun ?? unit.Battle?.GameRun);

                            if (newEffect.HasLevel && rInfo.Level > 0) newEffect.Level = rInfo.Level;
                            else if (newEffect.HasCount && rInfo.Level > 0) newEffect.Count = rInfo.Level;
                            if (newEffect.HasDuration && rInfo.Duration > 0) newEffect.Duration = rInfo.Duration;

                            Traverse.Create(unit).Method("TryAddStatusEffect", newEffect).GetValue();

                            if (!string.IsNullOrEmpty(newEffect.UnitEffectName) && remoteChar?.View != null)
                            {
                                remoteChar.View.TryPlayEffectLoop(newEffect.UnitEffectName);
                                remoteChar.View.SendEffectMessage(newEffect.UnitEffectName, "OnPropertyChanged", newEffect);
                            }
                        }
                    }
                }

                List<LBoL.Core.StatusEffects.StatusEffect> toRemove = currentEffects.Where(se => !remoteTypes.Contains(se.GetType().Name)).ToList();
                foreach (var se in toRemove)
                {
                    if (!string.IsNullOrEmpty(se.UnitEffectName) && remoteChar?.View != null)
                    {
                        remoteChar.View.EndEffectLoop(se.UnitEffectName, true);
                    }

                    Traverse.Create(unit).Method("TryRemoveStatusEffect", se).GetValue();
                }

                var widgetObj = Traverse.Create(remoteChar.View).Field("_statusWidget").GetValue();
                if (widgetObj != null)
                {
                    Traverse.Create(widgetObj).Method("SetStatusEffects").GetValue();
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogWarning($"[OtherPlayersOverlay] ApplyStatusEffectsToRemotePlayer 异常: {ex.Message}");
            }
        });
    }

    #endregion
}
