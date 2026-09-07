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
                if (!ShouldUploadRoomState(__instance, out string roomKey))
                {
                    return;
                }

                ApplyHostSnapshotIfAny(__instance, roomKey);

                TryGetRoomSync()?.UploadRoomState(BuildSnapshot(__instance, roomKey, RoomPhase.InBattle));
            }
            catch
            {

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
                if (!ShouldUploadRoomState(__instance, out string roomKey))
                {
                    return;
                }

                TryGetRoomSync()?.UploadRoomState(BuildSnapshot(__instance, roomKey, RoomPhase.InBattle));
            }
            catch
            {

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
                if (!ShouldUploadRoomState(__instance, out string roomKey))
                {
                    return;
                }

                TryGetRoomSync()?.UploadRoomState(BuildSnapshot(__instance, roomKey, RoomPhase.BattleFinished));
            }
            catch
            {

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

        }

        snapshot.Rewards = new BattleRewardSnapshot();
        snapshot.GapOptionsEvents = GapOptionsSyncPatch.GetRecentGapOptionsEvents(roomKey);
        return snapshot;
    }
}
