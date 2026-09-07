using System;

namespace NetworkPlugin.Network.MidGameJoin;

public class ApprovedJoin
{
    #region 请求标识属性

        public string RequestId { get; set; } = string.Empty;

        public string RoomId { get; set; } = string.Empty;

    #endregion

    #region 玩家信息属性

        public string PlayerName { get; set; } = string.Empty;

        public string HostPlayerId { get; set; } = string.Empty;

        public string ClientPlayerId { get; set; } = string.Empty;

    #endregion

    #region 安全验证属性

        public string JoinToken { get; set; } = string.Empty;

    #endregion

    #region 时间戳属性

        public long ApprovedAt { get; set; }

        public long ExpiresAt { get; set; }

    #endregion

    #region 游戏状态属性

        public PlayerBootstrappedState BootstrappedState { get; set; } = new();

    #endregion

    #region 辅助方法

        public bool IsExpired()
    {
        long currentTime = DateTime.UtcNow.Ticks;
        return currentTime > ExpiresAt;
    }

        public long GetRemainingTime()
    {
        long currentTime = DateTime.UtcNow.Ticks;
        long remainingTicks = ExpiresAt - currentTime;
        return remainingTicks / TimeSpan.TicksPerMillisecond;
    }

    #endregion
}
