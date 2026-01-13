// ============================================================================
// ApprovedJoin.cs
// 批准加入信息类
// 功能：表示游戏中途加入请求被批准后的相关信息
// ============================================================================

using System;

namespace NetworkPlugin.Network.MidGameJoin;

/// <summary>
/// 批准加入信息类
/// <para>表示游戏中途加入请求被批准后的相关信息，包含连接验证和状态同步所需的数据</para>
/// </summary>
public class ApprovedJoin
{
    #region 请求标识属性
    
    /// <summary>
    /// 请求ID
    /// <para>用于唯一标识加入请求的GUID或标识符</para>
    /// </summary>
    public string RequestId { get; set; } = string.Empty;
    
    /// <summary>
    /// 房间ID
    /// <para>玩家要加入的目标房间标识</para>
    /// </summary>
    public string RoomId { get; set; } = string.Empty;
    
    #endregion
    
    #region 玩家信息属性
    
    /// <summary>
    /// 玩家名称
    /// <para>加入玩家的显示名称</para>
    /// </summary>
    public string PlayerName { get; set; } = string.Empty;
    
    /// <summary>
    /// 主机玩家ID
    /// <para>房间主机玩家的唯一标识</para>
    /// </summary>
    public string HostPlayerId { get; set; } = string.Empty;
    
    /// <summary>
    /// 客户端玩家ID
    /// <para>加入玩家的唯一标识</para>
    /// </summary>
    public string ClientPlayerId { get; set; } = string.Empty;
    
    #endregion
    
    #region 安全验证属性
    
    /// <summary>
    /// 加入令牌
    /// <para>用于验证加入请求合法性的安全令牌</para>
    /// </summary>
    public string JoinToken { get; set; } = string.Empty;
    
    #endregion
    
    #region 时间戳属性
    
    /// <summary>
    /// 批准时间戳
    /// <para>请求被批准的时间（Unix时间戳，毫秒）</para>
    /// </summary>
    public long ApprovedAt { get; set; }
    
    /// <summary>
    /// 过期时间戳
    /// <para>加入令牌的过期时间（Unix时间戳，毫秒）</para>
    /// </summary>
    public long ExpiresAt { get; set; }
    
    #endregion
    
    #region 游戏状态属性
    
    /// <summary>
    /// 引导状态
    /// <para>玩家加入时需要同步的游戏状态信息</para>
    /// </summary>
    public PlayerBootstrappedState BootstrappedState { get; set; } = new();
    
    #endregion
    
    #region 辅助方法
    
    /// <summary>
    /// 检查加入令牌是否已过期
    /// </summary>
    /// <returns>如果已过期返回true，否则返回false</returns>
    public bool IsExpired()
    {
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return currentTime > ExpiresAt;
    }
    
    /// <summary>
    /// 获取剩余有效时间（毫秒）
    /// </summary>
    /// <returns>剩余有效时间，如果已过期则返回负数</returns>
    public long GetRemainingTime()
    {
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return ExpiresAt - currentTime;
    }
    
    #endregion
}
