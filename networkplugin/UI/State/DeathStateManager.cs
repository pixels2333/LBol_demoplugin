using System;
using System.Collections.Generic;
using LBoL.Core.Units;

namespace NetworkPlugin.UI.State;

public class DeathStateManager : IDisposable
{
        public event Action<PlayerUnit> OnPlayerFakeDeath;

        public event Action<PlayerUnit> OnPlayerResurrected;

        private readonly Dictionary<string, DeadPlayerInfo> _deadPlayers = [];

        private readonly Queue<PlayerUnit> _resurrectionQueue = [];

        private class DeadPlayerInfo
    {
                public PlayerUnit Player { get; set; }

                public long DeathTimestamp { get; set; }

                public int PreDeathMaxHp { get; set; }

                public bool CanResurrect { get; set; } = true;
    }

        public void RegisterFakeDeath(PlayerUnit player)
    {
        if (player == null) return;

        DeadPlayerInfo deadInfo = new DeadPlayerInfo
        {
            Player = player,
            DeathTimestamp = DateTime.UtcNow.Ticks,
            PreDeathMaxHp = player.MaxHp,
            CanResurrect = true
        };

        _deadPlayers[player.Id] = deadInfo;

        OnPlayerFakeDeath?.Invoke(player);

        Plugin.Logger?.LogInfo($"[DeathStateManager] Player {player.Id} registered as fake dead at {new DateTime(deadInfo.DeathTimestamp)}");
    }

        public void RegisterResurrection(PlayerUnit player)
    {
        if (player == null) return;

        if (_deadPlayers.Remove(player.Id))
        {

            OnPlayerResurrected?.Invoke(player);

            Plugin.Logger?.LogInfo($"[DeathStateManager] Player {player.Id} resurrected");
        }
    }

        public void EnqueueForResurrection(PlayerUnit player)
    {
        if (player != null && _deadPlayers.ContainsKey(player.Id))
        {
            _resurrectionQueue.Enqueue(player);
            Plugin.Logger?.LogDebug($"[DeathStateManager] Player {player.Id} queued for resurrection");
        }
    }

        public PlayerUnit DequeueForResurrection()
    {
        return _resurrectionQueue.Count > 0 ? _resurrectionQueue.Dequeue() : null;
    }

        public List<PlayerUnit> GetAllFakeDead()
    {
        List<PlayerUnit> result = new List<PlayerUnit>();
        foreach (var deadInfo in _deadPlayers.Values)
        {
            if (deadInfo.Player != null)
            {
                result.Add(deadInfo.Player);
            }
        }
        return result;
    }

        public int GetFakeDeadCount()
    {
        return _deadPlayers.Count;
    }

        public bool IsFakeDead(PlayerUnit player)
    {
        return player != null && _deadPlayers.ContainsKey(player.Id);
    }

        public double GetFakeDeathDuration(PlayerUnit player)
    {
        if (player == null || !_deadPlayers.TryGetValue(player.Id, out var deadInfo))
        {
            return 0;
        }

        TimeSpan duration = new TimeSpan(DateTime.UtcNow.Ticks - deadInfo.DeathTimestamp);
        return duration.TotalSeconds;
    }

        public void ClearAllFakeDead()
    {
        _deadPlayers.Clear();
        _resurrectionQueue.Clear();
        Plugin.Logger?.LogDebug("[DeathStateManager] All fake death records cleared");
    }

        public void Dispose()
    {
        ClearAllFakeDead();
        OnPlayerFakeDeath = null;
        OnPlayerResurrected = null;
    }
}

public static class DeathManagementService
{
        private static DeathStateManager _instance;

        public static DeathStateManager Instance
    {
        get
        {
            _instance ??= new DeathStateManager();
            return _instance;
        }
    }

        public static void NotifyFakeDeath(PlayerUnit player)
    {
        Instance.RegisterFakeDeath(player);
    }

        public static void NotifyResurrection(PlayerUnit player)
    {
        Instance.RegisterResurrection(player);
    }

        public static List<PlayerUnit> GetFakeDead()
    {
        return Instance.GetAllFakeDead();
    }

        public static int GetFakeDeadCount()
    {
        return Instance.GetFakeDeadCount();
    }
}
