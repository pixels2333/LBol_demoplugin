namespace NetworkPlugin.Network.MidGameJoin;

public class MidGameJoinConfig
{
    #region 基本开关配置

        public bool AllowMidGameJoin { get; set; } = true;

    #endregion

    #region 请求管理配置

        public int JoinRequestTimeoutMinutes { get; set; } = 2;

        public int MaxJoinRequestsPerRoom { get; set; } = 5;

    #endregion

    #region 补偿机制配置

        public bool EnableCompensation { get; set; } = true;

    #endregion

    #region 性能优化配置

        public int CatchUpBatchSize { get; set; } = 50;

    #endregion

    #region 配置验证方法

        public bool Validate()
    {

        if (JoinRequestTimeoutMinutes <= 0)
            return false;

        if (MaxJoinRequestsPerRoom <= 0)
            return false;

        if (CatchUpBatchSize <= 0 || CatchUpBatchSize > 1000)
            return false;

        return true;
    }

        public override string ToString()
    {
        return $"MidGameJoinConfig: " +
               $"AllowMidGameJoin={AllowMidGameJoin}, " +
               $"JoinRequestTimeoutMinutes={JoinRequestTimeoutMinutes}, " +
               $"MaxJoinRequestsPerRoom={MaxJoinRequestsPerRoom}, " +
               $"EnableCompensation={EnableCompensation}, " +
               $"CatchUpBatchSize={CatchUpBatchSize}";
    }

    #endregion
}
