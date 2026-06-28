using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.RoomSync;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Snapshot;
using NetworkPlugin.Utils;
using LBoL.Presentation;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 房间/战斗残局同步：
/// - EnterNode 后请求主机房间快照
/// - 战斗开始/回合结束/战斗结束时上传房间快照（由先进入者成为权威）
/// - 客机进入战斗时尽力把敌人状态调到与主机一致（HP/Block/Shield/存活）
/// </summary>
[HarmonyPatch]
public static class RoomStateSyncPatch
{
    public static readonly HashSet<string> PendingRequests = new(StringComparer.Ordinal);

    private static INetworkClient TryGetClient()
        => SendSyncHelper.TryGetClient();

    private static RoomSyncManager TryGetRoomSync()
        => ModService.ServiceProvider?.GetService<RoomSyncManager>();

    private static bool ShouldUploadRoomState(BattleController battle, out string roomKey)
    {
        roomKey = null;
        if (battle == null)
        {
            return false;
        }

        INetworkClient client = TryGetClient();
        if (client == null || !client.IsConnected)
        {
            return false;
        }

        if (battle.Player == null || battle.Player != GameStateUtils.GetCurrentPlayer())
        {
            return false;
        }

        roomKey = TryGetRoomSync()?.GetLastEnteredRoomKey();
        return !string.IsNullOrWhiteSpace(roomKey);
    }

    [HarmonyPatch(typeof(MapPanel), "RequestEnterNode")]
    [HarmonyPrefix]
    public static bool MapPanel_RequestEnterNode_Prefix(MapPanel __instance, MapNodeWidget enteringWidget)
    {
        try
        {
            INetworkClient client = TryGetClient();
            if (client == null || !client.IsConnected)
            {
                return true;
            }

            __instance.StartCoroutine(CustomRequestEnterNodeRunner(__instance, enteringWidget));
            return false;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[RoomStateSync] CustomRequestEnterNodeRunner failed to start: {ex}");
            return true;
        }
    }

    private static System.Collections.IEnumerator CustomRequestEnterNodeRunner(MapPanel mapPanel, MapNodeWidget enteringWidget)
    {
        try
        {
            mapPanel.EnterNode(enteringWidget);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[RoomStateSync] mapPanel.EnterNode failed: {ex}");
        }

        string roomKey = null;
        if (enteringWidget?.MapNode != null)
        {
            string stationType = enteringWidget.MapNode.StationType.ToString();
            roomKey = RoomSyncManager.BuildRoomKey(enteringWidget.MapNode.Act, enteringWidget.X, enteringWidget.Y, stationType);

            var roomSync = TryGetRoomSync();
            if (roomSync != null)
            {
                roomSync.SetLastEnteredNode(enteringWidget.MapNode.Act, enteringWidget.X, enteringWidget.Y, stationType);
                lock (PendingRequests)
                {
                    PendingRequests.Add(roomKey);
                }
                RoomStateSnapshot known = roomSync.TryGetClientRoomState(roomKey);
                roomSync.RequestRoomState(roomKey, known?.RoomVersion ?? 0);
            }
        }

        float elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += UnityEngine.Time.deltaTime;
            yield return null;
        }

        if (!string.IsNullOrWhiteSpace(roomKey))
        {
            float timeout = 0f;
            bool isPending = true;
            while (isPending && timeout < 3.0f)
            {
                lock (PendingRequests)
                {
                    isPending = PendingRequests.Contains(roomKey);
                }

                if (isPending)
                {
                    timeout += UnityEngine.Time.deltaTime;
                    yield return null;
                }
            }

            if (timeout >= 3.0f)
            {
                Plugin.Logger?.LogWarning($"[RoomStateSync] 等待房间快照超时 (roomKey={roomKey})，强制放行。");
                lock (PendingRequests)
                {
                    PendingRequests.Remove(roomKey);
                }
            }
        }

        try
        {
            GameMaster.Instance.StartCoroutine(DelayEnterNode(enteringWidget));
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[RoomStateSync] RequestEnterMapNode failed: {ex}");
        }
    }

    private static System.Collections.IEnumerator DelayEnterNode(MapNodeWidget enteringWidget)
    {
        yield return null;
        try
        {
            GameMaster.RequestEnterMapNode(enteringWidget.X, enteringWidget.Y);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[RoomStateSync] GameMaster.RequestEnterMapNode failed: {ex}");
        }
    }

    /// <summary>
    /// 进入节点时记录房间元数据
    /// </summary>
    [HarmonyPatch(typeof(GameMap), nameof(GameMap.EnterNode))]
    private static class GameMap_EnterNode_RequestRoomState
    {
        [HarmonyPostfix]
        public static void Postfix(MapNode node, bool freeMove, bool forced)
        {
            try
            {
                if (node == null || forced)
                {
                    return;
                }

                var client = TryGetClient();
                if (client == null || !client.IsConnected)
                {
                    return;
                }

                string stationType = node.StationType.ToString();
                var roomSync = TryGetRoomSync();
                roomSync?.SetLastEnteredNode(node.Act, node.X, node.Y, stationType);
            }
            catch
            {
                // ignored
            }
        }
    }

    /// <summary>
    /// 战斗开始时上传房间快照并应用主机缓存（如果有）
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "StartBattle")]
    private static class BattleController_StartBattle_UploadAndApply
    {
        [HarmonyPostfix]
        public static void Postfix(BattleController __instance)
        {
            try
            {
                if (!ShouldUploadRoomState(__instance, out string roomKey))
                {
                    return;
                }

                // 客机：如果主机已缓存该房间为 InBattle，则尽力把敌人状态调到一致（不强行重建敌人）。
                ApplyHostSnapshotIfAny(__instance, roomKey);

                // 上传一次初始快照：怪物清单 + 初始状态。
                TryGetRoomSync()?.UploadRoomState(BuildSnapshot(__instance, roomKey, RoomPhase.InBattle));
            }
            catch
            {
                // ignored
            }
        }
    }

    /// <summary>
    /// 回合结束时上传房间状态
    /// </summary>
    [HarmonyPatch(typeof(BattleController), nameof(BattleController.RequestEndPlayerTurn))]
    private static class BattleController_EndTurn_Upload
    {
        [HarmonyPostfix]
        public static void Postfix(BattleController __instance)
        {
            try
            {
                if (!ShouldUploadRoomState(__instance, out string roomKey))
                {
                    return;
                }

                // 这里做一次节流：只在敌方回合结束/或每回合一次更合适；暂用 EndPlayerTurn 作为近似。
                TryGetRoomSync()?.UploadRoomState(BuildSnapshot(__instance, roomKey, RoomPhase.InBattle));
            }
            catch
            {
                // ignored
            }
        }
    }

    /// <summary>
    /// 战斗结束时上传最终快照
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "EndBattle")]
    private static class BattleController_EndBattle_UploadFinished
    {
        [HarmonyPostfix]
        public static void Postfix(BattleController __instance)
        {
            try
            {
                if (!ShouldUploadRoomState(__instance, out string roomKey))
                {
                    return;
                }

                TryGetRoomSync()?.UploadRoomState(BuildSnapshot(__instance, roomKey, RoomPhase.BattleFinished));
            }
            catch
            {
                // ignored
            }
        }
    }

    private static void ApplyHostSnapshotIfAny(BattleController battle, string roomKey)
    {
        try
        {
            RoomStateSnapshot snapshot = TryGetRoomSync()?.TryGetClientRoomState(roomKey);
            if (snapshot == null || snapshot.Phase != RoomPhase.InBattle || snapshot.Enemies == null || snapshot.Enemies.Count == 0)
            {
                return;
            }

            if (battle.EnemyGroup == null)
            {
                return;
            }

            // 按 Index 对齐；若数量不足则仅对齐交集。
            List<EnemyUnit> localEnemies = battle.EnemyGroup.Where(e => e != null).ToList();
            foreach (var remote in snapshot.Enemies.OrderBy(e => e.Index))
            {
                if (remote.Index < 0 || remote.Index >= localEnemies.Count)
                {
                    continue;
                }

                EnemyUnit local = localEnemies[remote.Index];
                if (local == null)
                {
                    continue;
                }

                // 只做基础状态对齐：避免强行改复杂字段导致崩溃。
                // 注意：LBoL 的 Unit.Hp/Block/Shield 的 setter 可能是 internal，需用反射/Traverse。
                try
                {
                    Traverse.Create(local).Property("Hp").SetValue(remote.Health);
                    Traverse.Create(local).Property("Block").SetValue(remote.Block);
                    Traverse.Create(local).Property("Shield").SetValue(remote.Shield);
                }
                catch (Exception ex) { Plugin.Logger?.LogWarning($"[RoomStateSync] Traverse 设置属性失败: {ex.Message}"); }
            }
        }
        catch
        {
            // ignored
        }
    }

    private static RoomStateSnapshot BuildSnapshot(BattleController battle, string roomKey, RoomPhase phase)
    {
        RoomStateSnapshot snapshot = new RoomStateSnapshot
        {
            RoomKey = roomKey,
            Phase = phase,
            BattleId = battle.GetHashCode().ToString(),
        };

        // 最后进入节点的元数据
        // RoomKey 已包含 Act/X/Y/StationType，但这里也填充一份，便于日志与调试。
        try
        {
            string[] parts = roomKey.Split(':');
            if (parts.Length >= 4)
            {
                snapshot.Act = int.TryParse(parts[0], out int act) ? act : 0;
                snapshot.X = int.TryParse(parts[1], out int x) ? x : 0;
                snapshot.Y = int.TryParse(parts[2], out int y) ? y : 0;
                snapshot.StationType = parts[3];
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            if (battle.EnemyGroup != null)
            {
                int idx = 0;
                foreach (EnemyUnit e in battle.EnemyGroup)
                {
                    if (e == null)
                    {
                        idx++;
                        continue;
                    }

                    snapshot.Enemies.Add(new EnemyStateSnapshot
                    {
                        EnemyId = e.Id,
                        EnemyName = e.Name,
                        EnemyType = e.GetType().Name,
                        Health = e.Hp,
                        MaxHealth = e.MaxHp,
                        Block = e.Block,
                        Shield = e.Shield,
                        Index = idx,
                        IsAlive = e.IsAlive,
                    });

                    idx++;
                }
            }
        }
        catch
        {
            // ignored
        }

        // Rewards: 当前阶段不强行生成/发放，只占位留给后续补齐。
        snapshot.Rewards = new BattleRewardSnapshot();
        snapshot.GapOptionsEvents = GapOptionsSyncPatch.GetRecentGapOptionsEvents(roomKey);
        return snapshot;
    }
}
