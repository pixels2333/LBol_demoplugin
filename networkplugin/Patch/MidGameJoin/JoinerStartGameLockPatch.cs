using System;
using System.Collections.Generic;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.MidGameJoin;
using NetworkPlugin.Network.Snapshot;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.MidGameJoin;

/// <summary>
/// Joiner start-game lock:
/// - Joiner chooses character in StartGamePanel.
/// - Right before GameMaster.StartGame creates the run, override seed/difficulty/puzzles/mode/stages
///   to match host settings from the pending FullSnapshot.
///
/// Guard:
/// - Must be connected.
/// - Must be joiner (self is NOT host).
/// - Must have a pending FullSnapshot with the required host start config.
/// </summary>
[HarmonyPatch]
public static class JoinerStartGameLockPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static INetworkClient TryGetClient()
        => ServiceProvider?.GetService<INetworkClient>();

    private static MapCatchUpOrchestrator TryGetCatchUp()
        => ServiceProvider?.GetService<MapCatchUpOrchestrator>();

    // Patch the seed overload (the non-seed overload delegates to this one).
    /// <summary>
    /// 开始游戏前置：将加入者的种子/难度/开局配置对齐到主机的快照
    /// </summary>
    [HarmonyPatch(typeof(LBoL.Presentation.GameMaster), nameof(LBoL.Presentation.GameMaster.StartGame),
        new[]
        {
            typeof(ulong?),
            typeof(GameDifficulty),
            typeof(PuzzleFlag),
            typeof(PlayerUnit),
            typeof(PlayerType),
            typeof(Exhibit),
            typeof(int?),
            typeof(IEnumerable<Card>),
            typeof(IEnumerable<Stage>),
            typeof(Type),
            typeof(IEnumerable<JadeBox>),
            typeof(GameMode),
            typeof(bool),
        })]
    [HarmonyPrefix]
    public static void GameMaster_StartGame_Prefix(
        ref ulong? seed,
        ref GameDifficulty difficulty,
        ref PuzzleFlag puzzles,
        PlayerUnit player,
        PlayerType playerType,
        Exhibit initExhibit,
        int? initMoneyOverride,
        IEnumerable<Card> deck,
        ref IEnumerable<Stage> stages,
        ref Type debutAdventureType,
        IEnumerable<JadeBox> jadeBoxes,
        ref GameMode gameMode,
        ref bool showRandomResult)
    {
        try
        {
            INetworkClient client = TryGetClient();
            if (client == null || !client.IsConnected)
            {
                return;
            }

            NetworkIdentityTracker.EnsureSubscribed(client);

            // Do not affect the host.
            if (NetworkIdentityTracker.GetSelfIsHost())
            {
                return;
            }

            MapCatchUpOrchestrator catchUp = TryGetCatchUp();
            if (catchUp == null)
            {
                return;
            }

            if (!catchUp.TryGetPendingFullSnapshot(out FullStateSnapshot snapshot))
            {
                return;
            }

            if (snapshot?.GameState == null)
            {
                return;
            }

            // Require host config; otherwise we cannot guarantee deterministic alignment.
            if (snapshot.GameState.RootSeed == null ||
                snapshot.GameState.Difficulty == null ||
                snapshot.GameState.Puzzles == null ||
                snapshot.GameState.GameMode == null ||
                snapshot.GameState.StageTypeNames == null ||
                snapshot.GameState.StageTypeNames.Count == 0)
            {
                return;
            }

            // Lock run-level settings.
            seed = snapshot.GameState.RootSeed;
            difficulty = (GameDifficulty)snapshot.GameState.Difficulty.Value;
            puzzles = (PuzzleFlag)snapshot.GameState.Puzzles.Value;
            gameMode = (GameMode)snapshot.GameState.GameMode.Value;

            if (snapshot.GameState.ShowRandomResult != null)
            {
                showRandomResult = snapshot.GameState.ShowRandomResult.Value;
            }

            // Lock stages to host list.
            stages = BuildStages(snapshot.GameState.StageTypeNames, stages);

            // Debut adventure: optional, best-effort.
            if (!string.IsNullOrWhiteSpace(snapshot.GameState.DebutAdventureTypeName))
            {
                Type resolved = TryResolveDebutAdventureType(snapshot.GameState.DebutAdventureTypeName);
                if (resolved != null)
                {
                    debutAdventureType = resolved;
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private static IEnumerable<Stage> BuildStages(List<string> stageTypeNames, IEnumerable<Stage> fallback)
    {
        try
        {
            if (stageTypeNames == null || stageTypeNames.Count == 0)
            {
                return fallback;
            }

            List<Stage> stages = new();
            foreach (string name in stageTypeNames)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                Stage s = Library.CreateStage(name);
                if (s != null)
                {
                    stages.Add(s);
                }
            }

            return stages.Count > 0 ? stages : fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private static Type TryResolveDebutAdventureType(string typeName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return null;
            }

            // Safest path: try instantiate an adventure by ID, then use its runtime type.
            // This avoids referencing internal TypeFactory<>, which is not accessible from the plugin assembly.
            try
            {
                var adv = Library.TryCreateAdventure(typeName);
                if (adv != null)
                {
                    return adv.GetType();
                }
            }
            catch
            {
                // ignored
            }

            // Fallback: scan loaded assemblies by simple name.
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (Type t in asm.GetTypes())
                    {
                        if (t != null && string.Equals(t.Name, typeName, StringComparison.Ordinal))
                        {
                            return t;
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
