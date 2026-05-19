namespace NetworkPlugin.UI;

/// <summary>
/// UI 模块常量定义
/// </summary>
public static class UIConstants
{
    /// <summary>默认最大交易槽位数</summary>
    public const int DefaultMaxTradeSlots = 3;

    /// <summary>最大金钱出价</summary>
    public const int MaxMoneyOffer = 999;

    /// <summary>交易完成等待时间（秒）</summary>
    public const float TradeCompleteWaitTime = 2f;

    /// <summary>UI 刷新帧间隔</summary>
    public const int UiRefreshFrameInterval = 5;

    /// <summary>远程玩家头像基础宽度</summary>
    public const float AvatarEntryBaseWidth = 260f;

    /// <summary>远程玩家头像基础高度</summary>
    public const float AvatarEntryBaseHeight = 520f;

    /// <summary>日志字符串截断阈值</summary>
    public const int StringTruncateThreshold = 200;

}
