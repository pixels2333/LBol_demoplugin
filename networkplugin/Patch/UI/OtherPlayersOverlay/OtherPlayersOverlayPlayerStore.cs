using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network;
using NetworkPlugin.Utils;
using UnityEngine;

namespace NetworkPlugin.Patch.UI;

public static partial class OtherPlayersOverlayPatch
{
    #region 玩家缓存与位置快照

    internal static string ResolveDisplayName(string playerId, string preferredName = null, bool isLocal = false)
    {
        string preferred = NormalizeDisplayName(preferredName, playerId);
        if (!string.IsNullOrWhiteSpace(preferred))
        {
            return preferred;
        }

        bool isSelf = isLocal || (!string.IsNullOrWhiteSpace(playerId) && string.Equals(playerId, _selfPlayerId, StringComparison.Ordinal));
        if (isSelf)
        {
            string runtimeName = NormalizeDisplayName(GameStateUtils.GetCurrentPlayerName(), playerId);
            if (!string.IsNullOrWhiteSpace(runtimeName))
            {
                return runtimeName;
            }

            try
            {
                runtimeName = NormalizeDisplayName(GameStateUtils.GetCurrentPlayer()?.Name, playerId);
                if (!string.IsNullOrWhiteSpace(runtimeName))
                {
                    return runtimeName;
                }
            }
            catch
            {
            }
        }

        if (!string.IsNullOrWhiteSpace(playerId))
        {
            lock (_syncLock)
            {
                if (_players.TryGetValue(playerId, out PlayerSummary summary) && summary != null)
                {
                    string cachedName = NormalizeDisplayName(summary.PlayerName, playerId);
                    if (!string.IsNullOrWhiteSpace(cachedName))
                    {
                        return cachedName;
                    }
                }
            }
        }

        return string.IsNullOrWhiteSpace(playerId) ? string.Empty : playerId;
    }

    private static string NormalizeDisplayName(string playerName, string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            return null;
        }

        string trimmed = playerName.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(playerId) && string.Equals(trimmed, playerId, StringComparison.Ordinal))
        {
            return null;
        }

        return trimmed;
    }

    private static ConfigManager TryGetConfig()
    {
        try
        {
            return ServiceProvider?.GetService<ConfigManager>();
        }
        catch
        {
            return null;
        }
    }

    private static bool ShouldInjectTradeDebugPlayers()
    {
        try
        {
            return TryGetConfig()?.DebugFakePlayersForTrade?.Value == true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsVirtualAiDefaultEnabled()
    {
        try
        {
            var cfg = TryGetConfig();
            return cfg?.DebugVirtualPlayerAiDefault?.Value == true;
        }
        catch
        {
            return false;
        }
    }

    private static void EnsureVirtualAiDefaultPlayer_NoThrow()
    {
        try
        {
            if (!IsVirtualAiDefaultEnabled())
            {
                lock (_syncLock)
                {
                    _players.Remove("aidefault");
                }
                return;
            }

            string characterId = GetFallbackCharacterId();
            string name = "AI Default";

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
                locName = "Trade";
            }

            lock (_syncLock)
            {
                if (!_players.TryGetValue("aidefault", out PlayerSummary p) || p == null)
                {
                    p = new PlayerSummary { PlayerId = "aidefault" };
                    _players["aidefault"] = p;
                }

                p.PlayerName = name;
                p.IsConnected = true;
                p.IsHost = false;
                p.CharacterId = characterId;
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

    private static void InjectTradeDebugPlayersDetailed(List<(string PlayerId, string PlayerName, bool IsConnected, bool IsHost, int Stage, int LocationX, int LocationY, string LocationName, string CharacterId)> list)
    {
        if (list == null)
        {
            return;
        }

        int stage = -1;
        int x = -1;
        int y = -1;
        string loc = "Shop";
        if (TryGetSelfLocation(out int selfStage, out int selfX, out int selfY, out string selfLocName))
        {
            stage = selfStage;
            x = selfX;
            y = selfY;
            if (!string.IsNullOrWhiteSpace(selfLocName))
            {
                loc = selfLocName;
            }
        }
        string characterId = GetFallbackCharacterId();

        if (list.All(p => !string.Equals(p.PlayerId, "aidefault", StringComparison.Ordinal)))
        {
            list.Add(("aidefault", "AI Default", true, false, stage, x, y, loc, characterId));
        }
    }

    private static void InjectTradeDebugPlayers(List<(string PlayerId, string PlayerName, bool IsConnected, bool IsHost)> list)
    {
        if (list == null)
        {
            return;
        }

        if (list.All(p => !string.Equals(p.PlayerId, "aidefault", StringComparison.Ordinal)))
        {
            list.Add(("aidefault", "AI Default", true, false));
        }
    }

    internal static List<(string PlayerId, string PlayerName, bool IsConnected, bool IsHost)> SnapshotPlayers()
    {
        try
        {
            EnsureVirtualAiDefaultPlayer_NoThrow();

            lock (_syncLock)
            {
                var list = _players.Values
                    .Where(p => p != null && !string.IsNullOrWhiteSpace(p.PlayerId))
                    .OrderByDescending(p => p.IsHost)
                    .ThenByDescending(p => p.IsConnected)
                    .ThenBy(p => p.PlayerName, StringComparer.OrdinalIgnoreCase)
                    .Select(p => (p.PlayerId, ResolveDisplayName(p.PlayerId, p.PlayerName), p.IsConnected, p.IsHost))
                    .ToList();

                if (ShouldInjectTradeDebugPlayers())
                {
                    InjectTradeDebugPlayers(list);
                }

                if (!string.IsNullOrWhiteSpace(_selfPlayerId) && list.All(p => p.PlayerId != _selfPlayerId))
                {
                    list.Insert(0, (_selfPlayerId, ResolveDisplayName(_selfPlayerId, null, isLocal: true), true, false));
                }

                return list;
            }
        }
        catch
        {
            return new List<(string PlayerId, string PlayerName, bool IsConnected, bool IsHost)>();
        }
    }

    internal static List<(string PlayerId, string PlayerName, bool IsConnected, bool IsHost, int Stage, int LocationX, int LocationY, string LocationName, string CharacterId)> SnapshotPlayersDetailed()
    {
        try
        {
            EnsureVirtualAiDefaultPlayer_NoThrow();

            lock (_syncLock)
            {
                var list = _players.Values
                    .Where(p => p != null && !string.IsNullOrWhiteSpace(p.PlayerId))
                    .OrderByDescending(p => p.IsHost)
                    .ThenByDescending(p => p.IsConnected)
                    .ThenBy(p => p.PlayerName, StringComparer.OrdinalIgnoreCase)
                    .Select(p => (
                        p.PlayerId,
                        ResolveDisplayName(p.PlayerId, p.PlayerName),
                        p.IsConnected,
                        p.IsHost,
                        p.Stage,
                        p.LocationX,
                        p.LocationY,
                        p.LocationName,
                        p.CharacterId))
                    .ToList();

                if (ShouldInjectTradeDebugPlayers())
                {
                    InjectTradeDebugPlayersDetailed(list);
                }

                if (!string.IsNullOrWhiteSpace(_selfPlayerId) && list.All(p => p.PlayerId != _selfPlayerId))
                {
                    list.Insert(0, (_selfPlayerId, ResolveDisplayName(_selfPlayerId, null, isLocal: true), true, false, -1, -1, -1, null, null));
                }

                return list;
            }
        }
        catch
        {
            return new List<(string PlayerId, string PlayerName, bool IsConnected, bool IsHost, int Stage, int LocationX, int LocationY, string LocationName, string CharacterId)>();
        }
    }

    internal static bool TryGetSelfLocation(out int stage, out int locationX, out int locationY, out string locationName)
    {
        stage = -1;
        locationX = -1;
        locationY = -1;
        locationName = null;

        try
        {
            string selfIdToLookup = _selfPlayerId;
            if (string.IsNullOrWhiteSpace(selfIdToLookup))
            {
                selfIdToLookup = NetworkIdentityTracker.GetSelfPlayerId();
            }

            if (!string.IsNullOrWhiteSpace(selfIdToLookup))
            {
                lock (_syncLock)
                {
                    if (_players.TryGetValue(selfIdToLookup, out PlayerSummary p) && p != null && p.LocationX >= 0 && p.LocationY >= 0)
                    {
                        stage = p.Stage;
                        locationX = p.LocationX;
                        locationY = p.LocationY;
                        locationName = p.LocationName;
                        return true;
                    }
                }
            }

            var run = GameStateUtils.GetCurrentGameRun();
            var node = run?.CurrentMap?.VisitingNode;
            if (node != null)
            {
                stage = node.Act;
                locationX = node.X;
                locationY = node.Y;
                locationName = node.StationType.ToString();
                return true;
            }
        }
        catch
        {
        }

        return false;
    }

    internal static bool TryResolvePlayerIdByName(string playerName, out string playerId)
    {
        playerId = null;
        if (string.IsNullOrWhiteSpace(playerName))
        {
            return false;
        }

        try
        {
            foreach (PlayerSummary p in _players.Values)
            {
                if (p != null && string.Equals(ResolveDisplayName(p.PlayerId, p.PlayerName), playerName, StringComparison.OrdinalIgnoreCase))
                {
                    playerId = p.PlayerId;
                    return !string.IsNullOrWhiteSpace(playerId);
                }
            }
        }
        catch
        {
        }

        playerId = null;
        return false;
    }

    internal static bool TryGetSelfPlayer(out string playerId, out string playerName)
    {
        playerId = null;
        playerName = null;

        if (string.IsNullOrWhiteSpace(_selfPlayerId))
        {
            return false;
        }

        playerId = _selfPlayerId;

        try
        {
            if (_players.TryGetValue(_selfPlayerId, out PlayerSummary p) && p != null)
            {
                playerName = ResolveDisplayName(p.PlayerId, p.PlayerName, isLocal: true);
            }
        }
        catch
        {
        }

        return !string.IsNullOrWhiteSpace(playerId);
    }

    #endregion
}