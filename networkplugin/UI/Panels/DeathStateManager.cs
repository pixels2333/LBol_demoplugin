using System;
using System.Collections.Generic;
using LBoL.Core.Units;
using NetworkPlugin.Patch;

namespace NetworkPlugin.UI.Panels;

/// <summary>
/// 死亡状态管理类
/// 用于在死亡补丁中维护和管理玩家的死亡状态信息
/// 与 DeathPatches 补丁类配合工作，提供 UI 层的状态显示
/// </summary>
/// <remarks>
/// 这个类作为死亡补丁的辅助类，维护以下信息：
/// 1. 当前假死的玩家列表
/// 2. 玩家的死亡时间戳
/// 3. 待复活的玩家队列
/// 4. 死亡事件通知系统
///
/// 用途场景：
/// - 在 UI 上显示死亡玩家列表
/// - 追踪死亡状态的时间
/// - 管理复活队列，便于 Gap 等特殊事件复活玩家
/// </remarks>
public class DeathStateManager : IDisposable
{
    /// <summary>
    /// 当玩家进入假死状态时触发的事件
    /// </summary>
    public event Action<PlayerUnit> OnPlayerFakeDeath;

    /// <summary>
    /// 当玩家复活时触发的事件
    /// </summary>
    public event Action<PlayerUnit> OnPlayerResurrected;

    /// <summary>
    /// 当前处于假死状态的玩家列表
    /// </summary>
    private readonly Dictionary<string, DeadPlayerInfo> _deadPlayers = [];

    /// <summary>
    /// 待复活的玩家队列（FIFO）
    /// </summary>
    private readonly Queue<PlayerUnit> _resurrectionQueue = [];

    /// <summary>
    /// 死亡玩家的信息结构
    /// </summary>
    private class DeadPlayerInfo
    {
        /// <summary>
        /// 玩家单位
        /// </summary>
        public PlayerUnit Player { get; set; }

        /// <summary>
        /// 死亡时间戳（UTC Ticks）
        /// </summary>
        public long DeathTimestamp { get; set; }

        /// <summary>
        /// 死亡前的最大生命值（用于复活时计算恢复量）
        /// </summary>
        public int PreDeathMaxHp { get; set; }

        /// <summary>
        /// 是否可以被复活（某些特殊死亡可能不可被复活）
        /// </summary>
        public bool CanResurrect { get; set; } = true;
    }

    /// <summary>
    /// 记录玩家进入假死状态
    /// 由 DeathPatches 中的 HandleFakeDeath 调用
    /// </summary>
    /// <param name="player">进入假死的玩家</param>
    public void RegisterFakeDeath(PlayerUnit player)
    {
        if (player == null) return;

        var deadInfo = new DeadPlayerInfo
        {
            Player = player,
            DeathTimestamp = DateTime.UtcNow.Ticks,
            PreDeathMaxHp = player.MaxHp,
            CanResurrect = true
        };

        _deadPlayers[player.Id] = deadInfo;

        // 触发假死事件
        OnPlayerFakeDeath?.Invoke(player);

        Plugin.Logger?.LogInfo($"[DeathStateManager] Player {player.Id} registered as fake dead at {new DateTime(deadInfo.DeathTimestamp)}");
    }

    /// <summary>
    /// 记录玩家复活
    /// 由 DeathPatches 中的 HandleResurrection 调用
    /// </summary>
    /// <param name="player">复活的玩家</param>
    public void RegisterResurrection(PlayerUnit player)
    {
        if (player == null) return;

        if (_deadPlayers.Remove(player.Id))
        {
            // 触发复活事件
            OnPlayerResurrected?.Invoke(player);

            Plugin.Logger?.LogInfo($"[DeathStateManager] Player {player.Id} resurrected");
        }
    }

    /// <summary>
    /// 将玩家加入复活队列
    /// 用于管理哪些玩家需要被复活
    /// </summary>
    /// <param name="player">要复活的玩家</param>
    public void EnqueueForResurrection(PlayerUnit player)
    {
        if (player != null && _deadPlayers.ContainsKey(player.Id))
        {
            _resurrectionQueue.Enqueue(player);
            Plugin.Logger?.LogDebug($"[DeathStateManager] Player {player.Id} queued for resurrection");
        }
    }

    /// <summary>
    /// 从复活队列中移出玩家
    /// </summary>
    /// <returns>下一个需要复活的玩家，如果队列为空则返回 null</returns>
    public PlayerUnit DequeueForResurrection()
    {
        return _resurrectionQueue.Count > 0 ? _resurrectionQueue.Dequeue() : null;
    }

    /// <summary>
    /// 获取所有假死玩家的列表
    /// </summary>
    /// <returns>当前所有假死玩家的列表</returns>
    public List<PlayerUnit> GetAllFakeDead()
    {
        var result = new List<PlayerUnit>();
        foreach (var deadInfo in _deadPlayers.Values)
        {
            if (deadInfo.Player != null)
            {
                result.Add(deadInfo.Player);
            }
        }
        return result;
    }

    /// <summary>
    /// 获取假死玩家的数量
    /// </summary>
    /// <returns>当前假死玩家数量</returns>
    public int GetFakeDeadCount()
    {
        return _deadPlayers.Count;
    }

    /// <summary>
    /// 检查玩家是否处于假死状态
    /// </summary>
    /// <param name="player">要检查的玩家</param>
    /// <returns>如果玩家处于假死状态则返回 true</returns>
    public bool IsFakeDead(PlayerUnit player)
    {
        return player != null && _deadPlayers.ContainsKey(player.Id);
    }

    /// <summary>
    /// 获取玩家的假死持续时间（秒）
    /// </summary>
    /// <param name="player">要查询的玩家</param>
    /// <returns>假死持续时间，如果玩家未假死则返回 0</returns>
    public double GetFakeDeathDuration(PlayerUnit player)
    {
        if (player == null || !_deadPlayers.TryGetValue(player.Id, out var deadInfo))
        {
            return 0;
        }

        var duration = new TimeSpan(DateTime.UtcNow.Ticks - deadInfo.DeathTimestamp);
        return duration.TotalSeconds;
    }

    /// <summary>
    /// 清空所有死亡玩家记录
    /// 通常在游戏结束或战斗结束时调用
    /// </summary>
    public void ClearAllFakeDead()
    {
        _deadPlayers.Clear();
        _resurrectionQueue.Clear();
        Plugin.Logger?.LogDebug("[DeathStateManager] All fake death records cleared");
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        ClearAllFakeDead();
        OnPlayerFakeDeath = null;
        OnPlayerResurrected = null;
    }
}

/// <summary>
/// 死亡管理服务
/// 提供全局的死亡状态管理接口
/// </summary>
public static class DeathManagementService
{
    /// <summary>
    /// 全局死亡状态管理器实例
    /// </summary>
    private static DeathStateManager _instance;

    /// <summary>
    /// 获取或创建死亡状态管理器实例
    /// </summary>
    public static DeathStateManager Instance
    {
        get
        {
            _instance ??= new DeathStateManager();
            return _instance;
        }
    }

    /// <summary>
    /// 通知玩家进入假死状态
    /// </summary>
    /// <param name="player">进入假死的玩家</param>
    public static void NotifyFakeDeath(PlayerUnit player)
    {
        Instance.RegisterFakeDeath(player);
    }

    /// <summary>
    /// 通知玩家复活
    /// </summary>
    /// <param name="player">复活的玩家</param>
    public static void NotifyResurrection(PlayerUnit player)
    {
        Instance.RegisterResurrection(player);
    }

    /// <summary>
    /// 获取所有假死玩家
    /// </summary>
    /// <returns>假死玩家列表</returns>
    public static List<PlayerUnit> GetFakeDead()
    {
        return Instance.GetAllFakeDead();
    }

    /// <summary>
    /// 获取假死玩家数量
    /// </summary>
    /// <returns>假死玩家数量</returns>
    public static int GetFakeDeadCount()
    {
        return Instance.GetFakeDeadCount();
    }
}
