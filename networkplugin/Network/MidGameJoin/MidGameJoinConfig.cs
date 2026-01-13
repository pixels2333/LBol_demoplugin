// ============================================================================
// MidGameJoinConfig.cs
// 中途加入配置类
// 功能：配置游戏中途加入功能的各种参数和开关
// ============================================================================

namespace NetworkPlugin.Network.MidGameJoin;

/// <summary>
/// 中途加入配置类
/// <para>包含游戏中途加入功能的所有配置参数，用于控制加入行为、超时设置和性能优化</para>
/// </summary>
public class MidGameJoinConfig
{
    #region 基本开关配置
    
    /// <summary>
    /// 是否允许中途加入
    /// <para>控制是否启用游戏中途加入功能</para>
    /// <value>默认值：true（启用）</value>
    /// </summary>
    public bool AllowMidGameJoin { get; set; } = true;
    
    #endregion
    
    #region 请求管理配置
    
    /// <summary>
    /// 加入请求超时时间（分钟）
    /// <para>加入请求的有效期，超过此时间未处理的请求将被自动拒绝</para>
    /// <value>默认值：2分钟</value>
    /// </summary>
    public int JoinRequestTimeoutMinutes { get; set; } = 2;
    
    /// <summary>
    /// 每个房间最大加入请求数
    /// <para>限制单个房间同时处理的加入请求数量，防止资源耗尽</para>
    /// <value>默认值：5个请求</value>
    /// </summary>
    public int MaxJoinRequestsPerRoom { get; set; } = 5;
    
    #endregion
    
    #region AI控制配置
    
    /// <summary>
    /// AI控制超时时间（分钟）
    /// <para>玩家离开后AI控制角色的最大持续时间，超过此时间AI控制将自动结束</para>
    /// <value>默认值：10分钟</value>
    /// </summary>
    public int AIControlTimeoutMinutes { get; set; } = 10;
    
    /// <summary>
    /// 是否启用AI直通模式
    /// <para>控制AI是否可以直接接管玩家角色而不需要等待</para>
    /// <value>默认值：true（启用）</value>
    /// </summary>
    public bool EnableAIPassthrough { get; set; } = true;
    
    #endregion
    
    #region 补偿机制配置
    
    /// <summary>
    /// 是否启用补偿机制
    /// <para>控制是否对中途加入的玩家进行资源补偿（如金币、经验等）</para>
    /// <value>默认值：true（启用）</value>
    /// </summary>
    public bool EnableCompensation { get; set; } = true;
    
    #endregion
    
    #region 性能优化配置
    
    /// <summary>
    /// 追赶批次大小
    /// <para>控制状态同步时每次发送的游戏状态更新数量，影响网络流量和同步速度</para>
    /// <value>默认值：50条记录</value>
    /// </summary>
    public int CatchUpBatchSize { get; set; } = 50;
    
    #endregion
    
    #region 配置验证方法
    
    /// <summary>
    /// 验证配置的有效性
    /// </summary>
    /// <returns>如果配置有效返回true，否则返回false</returns>
    /// <remarks>
    /// 检查配置参数是否在合理范围内，防止无效配置导致运行时错误
    /// </remarks>
    public bool Validate()
    {
        // 超时时间必须为正数
        if (JoinRequestTimeoutMinutes <= 0)
            return false;
            
        // AI控制超时时间必须为正数
        if (AIControlTimeoutMinutes <= 0)
            return false;
            
        // 最大请求数必须为正数
        if (MaxJoinRequestsPerRoom <= 0)
            return false;
            
        // 批次大小必须在合理范围内
        if (CatchUpBatchSize <= 0 || CatchUpBatchSize > 1000)
            return false;
            
        return true;
    }
    
    /// <summary>
    /// 获取配置的字符串表示形式
    /// </summary>
    /// <returns>包含所有配置值的格式化字符串</returns>
    public override string ToString()
    {
        return $"MidGameJoinConfig: " +
               $"AllowMidGameJoin={AllowMidGameJoin}, " +
               $"JoinRequestTimeoutMinutes={JoinRequestTimeoutMinutes}, " +
               $"MaxJoinRequestsPerRoom={MaxJoinRequestsPerRoom}, " +
               $"AIControlTimeoutMinutes={AIControlTimeoutMinutes}, " +
               $"EnableCompensation={EnableCompensation}, " +
               $"EnableAIPassthrough={EnableAIPassthrough}, " +
               $"CatchUpBatchSize={CatchUpBatchSize}";
    }
    
    #endregion
}
