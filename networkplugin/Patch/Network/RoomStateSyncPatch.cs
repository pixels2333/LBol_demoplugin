using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.RoomSync;
using NetworkPlugin.Network.Snapshot;
using NetworkPlugin.Utils;

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
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static INetworkClient TryGetClient()
    {
        try
        {
            return ServiceProvider?.GetService<INetworkClient>();
        }
        catch
        {
            return null;
        }
    }

    [HarmonyPatch(typeof(GameMap), nameof(GameMap.EnterNode))]
    private static class GameMap_EnterNode_RequestRoomState
    {
        [HarmonyPostfix]
        public static void Postfix(MapNode node)
        {
            try
            {
                if (node == null)
                {
                    return;
                }

                var client = TryGetClient();
                if (client == null || !client.IsConnected)
                {
                    return;
                }

                // 记录本地“最后进入节点”，供后续上传/应用时构造 RoomKey。
                string stationType = node.StationType.ToString();
                RoomSyncManager.SetLastEnteredNode(node.Act, node.X, node.Y, stationType);

                // 每次进入节点都向主机请求一次该房间快照（LAN 下不做防刷）。
                string roomKey = RoomSyncManager.BuildRoomKey(node.Act, node.X, node.Y, stationType);
                RoomStateSnapshot known = RoomSyncManager.TryGetClientRoomState(roomKey);
                RoomSyncManager.RequestRoomState(roomKey, known?.RoomVersion ?? 0);
            }
            catch
            {
                // ignored
            }
        }
    }

    [HarmonyPatch(typeof(BattleController), "StartBattle")]
    private static class BattleController_StartBattle_UploadAndApply
    {
        [HarmonyPostfix]
        public static void Postfix(BattleController __instance)
        {
            try
            {
                if (__instance == null)
                {
                    return;
                }

                var client = TryGetClient();
                if (client == null || !client.IsConnected)
                {
                    return;
                }

                // 仅同步本地玩家的战斗。
                if (__instance.Player == null || __instance.Player != GameStateUtils.GetCurrentPlayer())
                {
                    return;
                }

                string roomKey = RoomSyncManager.GetLastEnteredRoomKey();
                if (string.IsNullOrWhiteSpace(roomKey))
                {
                    return;
                }

                // 客机：如果主机已缓存该房间为 InBattle，则尽力把敌人状态调到一致（不强行重建敌人）。
                ApplyHostSnapshotIfAny(__instance, roomKey);

                // 上传一次初始快照：怪物清单 + 初始状态。
                RoomSyncManager.UploadRoomState(BuildSnapshot(__instance, roomKey, RoomPhase.InBattle));
            }
            catch
            {
                // ignored
            }
        }
    }

    [HarmonyPatch(typeof(BattleController), nameof(BattleController.RequestEndPlayerTurn))]
    private static class BattleController_EndTurn_Upload
    {
        [HarmonyPostfix]
        public static void Postfix(BattleController __instance)
        {
            try
            {
                if (__instance == null)
                {
                    return;
                }

                var client = TryGetClient();
                if (client == null || !client.IsConnected)
                {
                    return;
                }

                if (__instance.Player == null || __instance.Player != GameStateUtils.GetCurrentPlayer())
                {
                    return;
                }

                string roomKey = RoomSyncManager.GetLastEnteredRoomKey();
                if (string.IsNullOrWhiteSpace(roomKey))
                {
                    return;
                }

                // 这里做一次节流：只在敌方回合结束/或每回合一次更合适；暂用 EndPlayerTurn 作为近似。
                RoomSyncManager.UploadRoomState(BuildSnapshot(__instance, roomKey, RoomPhase.InBattle));
            }
            catch
            {
                // ignored
            }
        }
    }

    [HarmonyPatch(typeof(BattleController), "EndBattle")]
    private static class BattleController_EndBattle_UploadFinished
    {
        [HarmonyPostfix]
        public static void Postfix(BattleController __instance)
        {
            try
            {
                if (__instance == null)
                {
                    return;
                }

                var client = TryGetClient();
                if (client == null || !client.IsConnected)
                {
                    return;
                }

                if (__instance.Player == null || __instance.Player != GameStateUtils.GetCurrentPlayer())
                {
                    return;
                }

                string roomKey = RoomSyncManager.GetLastEnteredRoomKey();
                if (string.IsNullOrWhiteSpace(roomKey))
                {
                    return;
                }

                RoomSyncManager.UploadRoomState(BuildSnapshot(__instance, roomKey, RoomPhase.BattleFinished));
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
            RoomStateSnapshot snapshot = RoomSyncManager.TryGetClientRoomState(roomKey);
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
                try { Traverse.Create(local).Property("Hp").SetValue(remote.Health); } catch { }
                try { Traverse.Create(local).Property("Block").SetValue(remote.Block); } catch { }
                try { Traverse.Create(local).Property("Shield").SetValue(remote.Shield); } catch { }
            }
        }
        catch
        {
            // ignored
        }
    }

    private static RoomStateSnapshot BuildSnapshot(BattleController battle, string roomKey, RoomPhase phase)
    {
        var snapshot = new RoomStateSnapshot
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
        return snapshot;
    }
}
